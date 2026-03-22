(function () {
    // 1. MutationObserver to automatically add .sortable class to table headers
    // so they show the pointer and arrows, even when tables are dynamically rendered by Blazor.
    function initSortableHeaders() {
        document.querySelectorAll('table:not(.no-sort) thead th:not(.no-sort)').forEach(th => {
            // Skip headers that have no text (like checkboxes or pure action button columns)
            if (!th.textContent.trim() && th.children.length === 0) return;

            if (!th.classList.contains('sortable')) {
                th.classList.add('sortable');
                if (!th.hasAttribute('data-sort-dir')) {
                    th.setAttribute('data-sort-dir', 'none');
                }
            }
        });
    }

    // Run once on initial load
    initSortableHeaders();

    // Re-run safely whenever the DOM changes (Blazor dynamic rendering)
    const observer = new MutationObserver((mutations) => {
        let shouldInit = false;
        for (const mut of mutations) {
            if (mut.addedNodes.length > 0) {
                shouldInit = true;
                break;
            }
        }
        if (shouldInit) {
            initSortableHeaders();
        }
    });

    observer.observe(document.body, { childList: true, subtree: true });

    // 2. Global click event listener for sorting (Event Delegation)
    document.addEventListener('click', function (e) {
        // Find if a sortable header was clicked
        const th = e.target.closest('th.sortable');
        if (!th) return;

        const table = th.closest('table');
        if (!table || table.classList.contains('no-sort')) return;

        const tbody = table.querySelector('tbody');
        if (!tbody) return;

        // Get index of clicked column
        const headers = Array.from(th.parentNode.children);
        const colIndex = headers.indexOf(th);
        if (colIndex === -1) return;

        // Store original order on first sort (assigning data-original-index)
        if (!tbody.hasAttribute('data-original-order')) {
            Array.from(tbody.rows).forEach((row, idx) => {
                if (!row.hasAttribute('data-original-index')) {
                    row.setAttribute('data-original-index', idx);
                }
            });
            tbody.setAttribute('data-original-order', 'true');
        }

        // Determine next sort direction (none -> asc -> desc -> none)
        const currentDir = th.getAttribute('data-sort-dir') || 'none';
        let nextDir = 'asc';

        if (currentDir === 'none') nextDir = 'asc';
        else if (currentDir === 'asc') nextDir = 'desc';
        else if (currentDir === 'desc') nextDir = 'none';

        // Reset all other headers in the table row
        headers.forEach(h => {
            if (h !== th) {
                h.setAttribute('data-sort-dir', 'none');
                h.classList.remove('sort-asc', 'sort-desc');
            }
        });

        // Set new state on clicked header
        th.setAttribute('data-sort-dir', nextDir);
        th.classList.remove('sort-asc', 'sort-desc');
        if (nextDir !== 'none') {
            th.classList.add(`sort-${nextDir}`);
        }

        const rows = Array.from(tbody.rows);

        if (nextDir === 'none') {
            // Restore original order
            rows.sort((a, b) => {
                return parseInt(a.getAttribute('data-original-index')) - parseInt(b.getAttribute('data-original-index'));
            });
        } else {
            // Sort active
            rows.sort((a, b) => {
                const aCell = a.children[colIndex];
                const bCell = b.children[colIndex];

                if (!aCell || !bCell) return 0;

                // Extract text, ignoring hidden or purely visual elements inside td if necessary
                const aText = aCell.textContent.trim();
                const bText = bCell.textContent.trim();

                // Clean for numbers (removing symbols like '$', '₱', ',', '%', '#')
                const cleanText = (t) => t.replace(/[^\d.-]/g, '');
                
                // Helper to check if string is primarily numeric (including currency/negative/decimals)
                const isNumeric = (t) => /^[\$€£₱#]?\s*-?[\d,]+(\.\d+)?\s*%?$/.test(t) || /^-\s*[\$€£₱#]?[\d,]+(\.\d+)?\s*%?$/.test(t);

                if (isNumeric(aText) && isNumeric(bText)) {
                    let aNum = parseFloat(cleanText(aText));
                    let bNum = parseFloat(cleanText(bText));
                    if (isNaN(aNum)) aNum = 0;
                    if (isNaN(bNum)) bNum = 0;
                    return nextDir === 'asc' ? aNum - bNum : bNum - aNum;
                }

                // Check for dates (heuristics to avoid false positives on plain numbers like IDs)
                const aDate = Date.parse(aText);
                const bDate = Date.parse(bText);

                if (!isNaN(aDate) && !isNaN(bDate) && aText.length > 5 && bText.length > 5 && !aText.match(/^\d+$/)) {
                    return nextDir === 'asc' ? aDate - bDate : bDate - aDate;
                }

                // Default string compare (natural sorting to handle alphanumeric properly e.g. "Item 2" vs "Item 10")
                return nextDir === 'asc' 
                    ? aText.localeCompare(bText, undefined, { numeric: true, sensitivity: 'base' }) 
                    : bText.localeCompare(aText, undefined, { numeric: true, sensitivity: 'base' });
            });
        }

        // Reattach to DOM efficiently (using DocumentFragment)
        const frag = document.createDocumentFragment();
        rows.forEach(r => frag.appendChild(r));
        tbody.appendChild(frag);
    });
})();
