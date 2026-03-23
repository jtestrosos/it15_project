/**
 * SignalR Real-time Notification Service
 * Handles Hub connection, events, and UI toast display
 */
window.notificationService = {
    connection: null,
    dotNetRefs: [],

    start: function () {
        if (this.connection) return; // Already started

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        // Listen for new notifications from the server
        this.connection.on("ReceiveNotification", (notification) => {
            console.log("Real-time notification received:", notification);
            
            // Show a toast message to the user
            this.showToast(notification);
            
            // Notify Blazor components (like the bell) to update
            this.dotNetRefs.forEach(ref => {
                if (ref) ref.invokeMethodAsync('OnNotificationReceived');
            });

            // Fallback for non-blazor listeners
            const event = new CustomEvent('notification-received', { detail: notification });
            window.dispatchEvent(event);
        });

        this.connection.start()
            .then(() => console.log("SignalR: NotificationHub Connected"))
            .catch(err => console.error("SignalR: NotificationHub Connection Error: ", err));
    },

    showToast: function (notification) {
        // Create container if it doesn't exist
        let container = document.getElementById('notification-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'notification-toast-container';
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            container.style.zIndex = '2000';
            document.body.appendChild(container);
        }

        const toastId = 'toast-' + Date.now();
        const toastEl = document.createElement('div');
        toastEl.id = toastId;
        toastEl.className = 'toast align-items-center show border-0 shadow-lg mb-2';
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');
        
        // Define color scheme based on severity
        let bgClass = 'bg-primary text-white';
        let icon = 'bi-bell-fill';
        let borderClass = 'border-primary';

        if (notification.severity === 'Critical') {
            bgClass = 'bg-danger text-white';
            icon = 'bi-exclamation-triangle-fill';
            borderClass = 'border-danger';
        } else if (notification.severity === 'Warning') {
            bgClass = 'bg-warning text-dark';
            icon = 'bi-exclamation-circle-fill';
            borderClass = 'border-warning';
        }

        const closeBtnClass = notification.severity === 'Warning' ? '' : 'btn-close-white';

        toastEl.innerHTML = `
            <div class="toast-header ${bgClass} border-bottom-0">
                <i class="bi ${icon} me-2"></i>
                <strong class="me-auto">${notification.title}</strong>
                <small class="${notification.severity === 'Warning' ? 'text-muted' : 'text-white-50'}">Just now</small>
                <button type="button" class="btn-close ${closeBtnClass}" data-bs-dismiss="toast" aria-label="Close" onclick="event.stopPropagation(); this.closest('.toast').remove()"></button>
            </div>
            <div class="toast-body bg-white text-dark rounded-bottom ${notification.linkUrl ? 'toast-clickable' : ''}" 
                 ${notification.linkUrl ? `onclick="window.location.href='${notification.linkUrl}'"` : ''}>
                <div class="d-flex flex-column">
                    <span class="mb-2">${notification.message}</span>
                    ${notification.linkUrl ? `
                        <div class="mt-1">
                            <span class="text-primary fw-bold" style="font-size: 0.75rem;">
                                View Details <i class="bi bi-chevron-right ms-1"></i>
                            </span>
                        </div>` : ''}
                </div>
            </div>
        `;

        container.appendChild(toastEl);
        
        // Auto-remove after 8 seconds
        setTimeout(() => {
            const el = document.getElementById(toastId);
            if (el) {
                el.classList.add('fade-out'); // Optional transition if defined in CSS
                setTimeout(() => el.remove(), 600);
            }
        }, 8000);
    },

    registerListener: function (dotNetRef) {
        this.dotNetRefs.push(dotNetRef);
    }
};
