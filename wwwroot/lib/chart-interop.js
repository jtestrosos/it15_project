// Chart.js interop functions for Blazor
window.chartInterop = {
    // Create a bar chart
    createBarChart: function (canvasId, labels, data, label) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        
        // Destroy existing chart if any
        if (ctx.chart) ctx.chart.destroy();
        
        ctx.chart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: label,
                    data: data,
                    backgroundColor: 'rgba(124, 58, 237, 0.6)',
                    borderColor: 'rgba(124, 58, 237, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });
    },

    // Create a line chart
    createLineChart: function (canvasId, labels, data, label) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        
        if (ctx.chart) ctx.chart.destroy();
        
        ctx.chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: label,
                    data: data,
                    borderColor: 'rgba(124, 58, 237, 1)',
                    backgroundColor: 'rgba(124, 58, 237, 0.1)',
                    tension: 0.3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    },

    // Create a doughnut/pie chart
    createDoughnutChart: function (canvasId, labels, data) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        
        if (ctx.chart) ctx.chart.destroy();
        
        ctx.chart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: [
                        'rgba(124, 58, 237, 0.8)',
                        'rgba(34, 197, 94, 0.8)',
                        'rgba(245, 158, 11, 0.8)',
                        'rgba(239, 68, 68, 0.8)',
                        'rgba(6, 182, 212, 0.8)'
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    },

    // Destroy a chart
    destroyChart: function (canvasId) {
        const ctx = document.getElementById(canvasId);
        if (ctx && ctx.chart) {
            ctx.chart.destroy();
        }
    }
};
