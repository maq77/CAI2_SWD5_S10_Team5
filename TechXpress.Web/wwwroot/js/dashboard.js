/**
 * TechXpress Admin JavaScript - Complete Version
 * Enhanced admin panel functionality with animations, charts, and interactive features
 */

// Global variables
let salesChart = null;
let categoryChart = null;
let dashboardRefreshInterval = null;

document.addEventListener('DOMContentLoaded', function () {
    // Initialize all components
    initializeSidebar();
    initializeTooltips();
    initializeNotifications();
    initializeDashboard();
    initializeDataTables();
    initializeModalHandlers();
    initializeFormValidation();
    initializeSearchFunctionality();
    setupEventListeners();

    console.log("TechXpress Admin Panel Initialized");
});

/**
 * Initialize sidebar functionality
 */
function initializeSidebar() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.admin-sidebar');
    const mainContent = document.querySelector('.flex-grow-1');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            if (mainContent) {
                mainContent.classList.toggle('sidebar-collapsed');
            }

            // Save sidebar state to localStorage
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        });

        // Restore sidebar state from localStorage
        const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (sidebarCollapsed) {
            sidebar.classList.add('collapsed');
            if (mainContent) {
                mainContent.classList.add('sidebar-collapsed');
            }
        }
    }

    // Highlight active menu item
    highlightActiveMenuItem();
}

/**
 * Highlight active menu item based on current URL
 */
function highlightActiveMenuItem() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.sidebar-nav .nav-link');

    navLinks.forEach(link => {
        link.classList.remove('active');
        if (link.getAttribute('href') && currentPath.includes(link.getAttribute('href'))) {
            link.classList.add('active');
        }
    });
}

/**
 * Initialize Bootstrap tooltips
 */
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

/**
 * Initialize notification system
 */
function initializeNotifications() {
    updateNotificationBadge();

    // Check for new notifications every 30 seconds
    setInterval(updateNotificationBadge, 30000);

    // Handle notification dropdown
    const notificationDropdown = document.getElementById('notificationDropdown');
    if (notificationDropdown) {
        notificationDropdown.addEventListener('click', function () {
            loadNotifications();
        });
    }
}

/**
 * Initialize dashboard components
 */
function initializeDashboard() {
    if (document.querySelector('.dashboard-container')) {
        initCounters();
        initializeCharts();
        loadRecentActivity();

        // Auto-refresh dashboard every 5 minutes
        dashboardRefreshInterval = setInterval(refreshDashboardData, 300000);
    }
}

/**
 * Initialize animated counters for dashboard stats
 */
function initCounters() {
    document.querySelectorAll('.stat-number').forEach(function (el) {
        const target = parseFloat(el.getAttribute('data-target') || 0);
        const prefix = el.textContent.trim().startsWith('$') ? '$' : '';
        const suffix = el.textContent.trim().endsWith('%') ? '%' : '';
        let current = 0;
        const increment = target / 80;
        const duration = 2000; // ms
        const stepTime = duration / 80;

        // Add loading animation
        el.style.opacity = '0.5';

        const timer = setInterval(function () {
            current += increment;
            if (current >= target) {
                el.textContent = prefix + formatNumber(target) + suffix;
                el.style.opacity = '1';
                clearInterval(timer);

                // Add completion animation
                el.style.transform = 'scale(1.1)';
                setTimeout(() => {
                    el.style.transform = 'scale(1)';
                }, 200);
            } else {
                el.textContent = prefix + formatNumber(Math.floor(current)) + suffix;
            }
        }, stepTime);
    });
}

/**
 * Format numbers with commas
 */
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

/**
 * Initialize charts for dashboard
 */
function initializeCharts() {
    //initializeSalesChart();
    //initializeCategoryChart();
    //initializeRevenueChart();
    //initializeOrdersChart();
}

/**
 * Initialize sales chart
 */
function initializeSalesChart() {
    const ctx = document.getElementById('salesChart');
    if (!ctx) return;

    // Destroy existing chart if it exists
    if (salesChart) {
        salesChart.destroy();
    }

    const gradient1 = ctx.getContext('2d').createLinearGradient(0, 0, 0, 400);
    gradient1.addColorStop(0, 'rgba(102, 126, 234, 0.3)');
    gradient1.addColorStop(1, 'rgba(102, 126, 234, 0)');

    const gradient2 = ctx.getContext('2d').createLinearGradient(0, 0, 0, 400);
    gradient2.addColorStop(0, 'rgba(86, 171, 47, 0.3)');
    gradient2.addColorStop(1, 'rgba(86, 171, 47, 0)');

    salesChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [{
                label: 'Sales',
                data: [12000, 19000, 15000, 25000, 22000, 30000, 28000, 35000, 32000, 40000, 38000, 45000],
                borderColor: 'rgb(102, 126, 234)',
                backgroundColor: gradient1,
                tension: 0.4,
                fill: true,
                pointBackgroundColor: 'rgb(102, 126, 234)',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 8
            }, {
                label: 'Revenue',
                data: [8000, 12000, 10000, 18000, 16000, 22000, 20000, 26000, 24000, 30000, 28000, 35000],
                borderColor: 'rgb(86, 171, 47)',
                backgroundColor: gradient2,
                tension: 0.4,
                fill: true,
                pointBackgroundColor: 'rgb(86, 171, 47)',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                intersect: false,
                mode: 'index'
            },
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        padding: 20
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0,0,0,0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: 'rgba(255,255,255,0.1)',
                    borderWidth: 1,
                    cornerRadius: 8,
                    callbacks: {
                        label: function (context) {
                            return context.dataset.label + ': $' + context.parsed.y.toLocaleString();
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0,0,0,0.05)'
                    },
                    ticks: {
                        callback: function (value) {
                            return '$' + value.toLocaleString();
                        }
                    }
                }
            },
            animation: {
                duration: 2000,
                easing: 'easeInOutQuart'
            }
        }
    });
}

/**
 * Initialize category chart
 */
function initializeCategoryChart() {
    const ctx = document.getElementById('categoryChart');
    if (!ctx) return;

    if (categoryChart) {
        categoryChart.destroy();
    }

    categoryChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Smartphones', 'Laptops', 'Accessories', 'Tablets', 'Gaming'],
            datasets: [{
                data: [35, 25, 20, 12, 8],
                backgroundColor: [
                    'rgba(102, 126, 234, 0.8)',
                    'rgba(86, 171, 47, 0.8)',
                    'rgba(247, 151, 30, 0.8)',
                    'rgba(255, 65, 108, 0.8)',
                    'rgba(79, 172, 254, 0.8)'
                ],
                borderWidth: 0,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20,
                        usePointStyle: true
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0,0,0,0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: 'rgba(255,255,255,0.1)',
                    borderWidth: 1,
                    cornerRadius: 8,
                    callbacks: {
                        label: function (context) {
                            return context.label + ': ' + context.parsed + '%';
                        }
                    }
                }
            },
            animation: {
                animateRotate: true,
                duration: 2000
            }
        }
    });
}

/**
 * Initialize revenue chart (mini chart)
 */
function initializeRevenueChart() {
    const ctx = document.getElementById('revenueChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
            datasets: [{
                data: [8500, 12000, 9500, 15000],
                backgroundColor: 'rgba(102, 126, 234, 0.8)',
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                x: {
                    display: false
                },
                y: {
                    display: false
                }
            }
        }
    });
}

/**
 * Initialize orders chart (mini chart)
 */
function initializeOrdersChart() {
    const ctx = document.getElementById('ordersChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
            datasets: [{
                data: [23, 45, 32, 67, 49, 72, 55],
                borderColor: 'rgba(86, 171, 47, 1)',
                backgroundColor: 'rgba(86, 171, 47, 0.1)',
                tension: 0.4,
                fill: true,
                pointRadius: 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                x: {
                    display: false
                },
                y: {
                    display: false
                }
            }
        }
    });
}

/**
 * Initialize DataTables for admin lists
 */
function initializeDataTables() {
    // Products table
    if (document.getElementById('productsTable')) {
        $('#productsTable').DataTable({
            responsive: true,
            pageLength: 25,
            order: [[0, 'desc']],
            columnDefs: [
                { orderable: false, targets: -1 } // Disable ordering on actions column
            ],
            language: {
                search: "Search products:",
                lengthMenu: "Show _MENU_ products per page",
                info: "Showing _START_ to _END_ of _TOTAL_ products"
            }
        });
    }

    // Orders table
    if (document.getElementById('ordersTable')) {
        $('#ordersTable').DataTable({
            responsive: true,
            pageLength: 25,
            order: [[0, 'desc']],
            columnDefs: [
                { orderable: false, targets: -1 }
            ],
            language: {
                search: "Search orders:",
                lengthMenu: "Show _MENU_ orders per page",
                info: "Showing _START_ to _END_ of _TOTAL_ orders"
            }
        });
    }

    // Users table
    if (document.getElementById('usersTable')) {
        $('#usersTable').DataTable({
            responsive: true,
            pageLength: 25,
            order: [[1, 'asc']],
            columnDefs: [
                { orderable: false, targets: -1 }
            ],
            language: {
                search: "Search users:",
                lengthMenu: "Show _MENU_ users per page",
                info: "Showing _START_ to _END_ of _TOTAL_ users"
            }
        });
    }
}

/**
 * Initialize modal handlers
 */
function initializeModalHandlers() {
    // Generic modal closer
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('modal')) {
            const modal = bootstrap.Modal.getInstance(e.target);
            if (modal) modal.hide();
        }
    });

    // Reset forms when modals are hidden
    document.querySelectorAll('.modal').forEach(modal => {
        modal.addEventListener('hidden.bs.modal', function () {
            const form = this.querySelector('form');
            if (form) {
                form.reset();
                form.classList.remove('was-validated');
            }
        });
    });
}

/**
 * Initialize form validation
 */
function initializeFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // Real-time validation
    document.querySelectorAll('input, textarea, select').forEach(input => {
        input.addEventListener('blur', function () {
            if (this.checkValidity()) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            } else {
                this.classList.remove('is-valid');
                this.classList.add('is-invalid');
            }
        });
    });
}

/**
 * Initialize search functionality
 */
function initializeSearchFunctionality() {
    const globalSearch = document.getElementById('globalSearch');
    if (globalSearch) {
        let searchTimeout;
        globalSearch.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                performGlobalSearch(this.value);
            }, 300);
        });
    }
}

/**
 * Setup event listeners
 */
function setupEventListeners() {
    // Card hover effects
    document.querySelectorAll('.stat-card').forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-10px) scale(1.02)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });

    // Quick action buttons
    document.querySelectorAll('.action-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            // Add ripple effect
            const ripple = document.createElement('span');
            ripple.classList.add('ripple');
            this.appendChild(ripple);

            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });

    // File upload preview
    document.querySelectorAll('input[type="file"]').forEach(input => {
        input.addEventListener('change', handleFilePreview);
    });

    // Auto-save drafts
    document.querySelectorAll('textarea, input[type="text"]').forEach(input => {
        if (input.dataset.autosave) {
            let saveTimeout;
            input.addEventListener('input', function () {
                clearTimeout(saveTimeout);
                saveTimeout = setTimeout(() => {
                    saveDraft(input.name, input.value);
                }, 2000);
            });
        }
    });
}

/**
 * Update notification badge count
 */
function updateNotificationBadge() {
    fetch('/Admin/GetNotificationCount')
        .then(response => response.json())
        .then(data => {
            const badge = document.querySelector('.notification-badge');
            if (badge) {
                badge.textContent = data.count;
                badge.style.display = data.count > 0 ? 'inline-block' : 'none';
            }
        })
        .catch(error => {
            console.log('Notification update failed:', error);
        });
}

/**
 * Load notifications
 */
function loadNotifications() {
    fetch('/Admin/GetNotifications')
        .then(response => response.json())
        .then(data => {
            const container = document.getElementById('notificationsList');
            if (container) {
                container.innerHTML = '';
                data.notifications.forEach(notification => {
                    const item = createNotificationItem(notification);
                    container.appendChild(item);
                });
            }
        })
        .catch(error => {
            console.error('Failed to load notifications:', error);
        });
}

/**
 * Create notification item
 */
function createNotificationItem(notification) {
    const item = document.createElement('div');
    item.className = 'notification-item';
    item.innerHTML = `
        <div class="notification-icon ${notification.type}">
            <i class="fas ${getNotificationIcon(notification.type)}"></i>
        </div>
        <div class="notification-content">
            <div class="notification-title">${notification.title}</div>
            <div class="notification-message">${notification.message}</div>
            <div class="notification-time">${formatRelativeTime(notification.timestamp)}</div>
        </div>
    `;
    return item;
}

/**
 * Get notification icon based on type
 */
function getNotificationIcon(type) {
    const icons = {
        'order': 'fa-shopping-cart',
        'user': 'fa-user',
        'product': 'fa-box',
        'system': 'fa-cog',
        'warning': 'fa-exclamation-triangle',
        'success': 'fa-check-circle'
    };
    return icons[type] || 'fa-bell';
}

/**
 * Load recent activity
 */
function loadRecentActivity() {
    fetch('/Admin/GetRecentActivity')
        .then(response => response.json())
        .then(data => {
            const container = document.querySelector('.recent-activity .activity-list');
            if (container) {
                container.innerHTML = '';
                data.activities.forEach(activity => {
                    const item = createActivityItem(activity);
                    container.appendChild(item);
                });
            }
        })
        .catch(error => {
            console.log('Recent activity load failed:', error);
        });
}

/**
 * Create activity item
 */
function createActivityItem(activity) {
    const item = document.createElement('div');
    item.className = 'activity-item';
    item.innerHTML = `
        <div class="activity-icon" style="background: ${getActivityColor(activity.type)};">
            <i class="fas ${getActivityIcon(activity.type)}"></i>
        </div>
        <div class="activity-content">
            <strong>${activity.title}</strong><br>
            <small class="text-muted">${activity.description} • ${formatRelativeTime(activity.timestamp)}</small>
        </div>
    `;
    return item;
}

/**
 * Get activity icon based on type
 */
function getActivityIcon(type) {
    const icons = {
        'order': 'fa-shopping-cart',
        'user': 'fa-user-plus',
        'product': 'fa-box',
        'review': 'fa-star'
    };
    return icons[type] || 'fa-circle';
}

/**
 * Get activity color based on type
 */
function getActivityColor(type) {
    const colors = {
        'order': 'var(--gradient-2)',
        'user': 'var(--gradient-1)',
        'product': 'var(--gradient-3)',
        'review': 'var(--gradient-4)'
    };
    return colors[type] || 'var(--gradient-1)';
}

/**
 * Refresh dashboard data
 */
function refreshDashboardData() {
    fetch('/Admin/GetDashboardData')
        .then(response => response.json())
        .then(data => {
            // Update stat cards
            updateStatCard('totalProducts', data.totalProducts);
            updateStatCard('totalOrders', data.totalOrders);
            updateStatCard('totalUsers', data.totalUsers);
            updateStatCard('totalRevenue', data.totalRevenue);

            // Update charts
            if (salesChart && data.salesData) {
                salesChart.data.datasets[0].data = data.salesData;
                salesChart.update('none');
            }

            // Reload recent activity
            //loadRecentActivity();
        })
        .catch(error => {
            console.log('Dashboard refresh failed:', error);
        });
}

/**
 * Update stat card value
 */
function updateStatCard(id, newValue) {
    const element = document.querySelector(`[data-stat="${id}"] .stat-number`);
    if (element) {
        const currentValue = parseInt(element.textContent.replace(/[^\d]/g, ''));
        if (currentValue !== newValue) {
            element.style.color = '#28a745';
            element.textContent = newValue.toLocaleString();
            setTimeout(() => {
                element.style.color = '';
            }, 2000);
        }
    }
}

/**
 * Show a toast notification
 * @param {string} message - The message to display
 * @param {string} type - The notification type (success, warning, danger, info)
 * @param {number} duration - Duration in milliseconds (default: 5000)
 */
function showNotification(message, type = 'info', duration = 5000) {
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0 show`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="fas ${getToastIcon(type)} me-2"></i>
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    toastContainer.appendChild(toast);

    const bsToast = new bootstrap.Toast(toast, { delay: duration });
    bsToast.show();

    toast.addEventListener('hidden.bs.toast', function () {
        toast.remove();
    });
}

/**
 * Get toast icon based on type
 */
function getToastIcon(type) {
    const icons = {
        'success': 'fa-check-circle',
        'danger': 'fa-exclamation-circle',
        'warning': 'fa-exclamation-triangle',
        'info': 'fa-info-circle'
    };
    return icons[type] || 'fa-bell';
}

/**
 * Handle data deletion with confirmation
 * @param {string} url - The URL to send the delete request to
 * @param {string} itemName - Name of the item being deleted
 * @param {function} callback - Function to call after successful deletion
 */
function confirmDelete(url, itemName, callback) {
    if (confirm(`Are you sure you want to delete ${itemName}?\nThis action cannot be undone.`)) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');

        fetch(url, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token ? token.value : ''
            }
        })
            .then(response => {
                if (response.ok) {
                    showNotification(`${itemName} deleted successfully.`, 'success');
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                } else {
                    throw new Error('Delete failed');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('Failed to delete. Please try again.', 'danger');
            });
    }
}

/**
 * Handle file upload preview
 */
function handleFilePreview(event) {
    const file = event.target.files[0];
    if (file && file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const preview = document.getElementById(event.target.dataset.preview);
            if (preview) {
                preview.src = e.target.result;
                preview.style.display = 'block';
            }
        };
        reader.readAsDataURL(file);
    }
}

/**
 * Save draft to localStorage
 */
function saveDraft(key, value) {
    try {
        localStorage.setItem(`draft_${key}`, value);
        showNotification('Draft saved', 'info', 2000);
    } catch (error) {
        console.error('Failed to save draft:', error);
    }
}

/**
 * Load draft from localStorage
 */
function loadDraft(key) {
    try {
        return localStorage.getItem(`draft_${key}`);
    } catch (error) {
        console.error('Failed to load draft:', error);
        return null;
    }
}

/**
 * Format relative time (e.g., "2 minutes ago")
 */
function formatRelativeTime(timestamp) {
    const now = new Date();
    const time = new Date(timestamp);
    const diffInSeconds = Math.floor((now - time) / 1000);

    if (diffInSeconds < 60) return 'Just now';
    if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} minutes ago`;
    if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} hours ago`;
    return `${Math.floor(diffInSeconds / 86400)} days ago`;
}

/**
 * Perform global search
 */
function performGlobalSearch(query) {
    if (query.length < 2) return;

    fetch(`/Admin/Search?q=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(data => {
            displaySearchResults(data);
        })
        .catch(error => {
            console.error('Search failed:', error);
        });
}

/**
 * Display search results
 */
function displaySearchResults(results) {
    const container = document.getElementById('searchResults');
    if (!container) return;

    container.innerHTML = '';
    if (results.length === 0) {
        container.innerHTML = '<div class="text-muted">No results found</div>';
        return;
    }

    results.forEach(result => {
        const item = document.createElement('a');
        item.href = result.url;
        item.className = 'list-group-item list-group-item-action';
        item.innerHTML = `
            <div class="d-flex w-100 justify-content-between">
                <h6 class="mb-1">${result.title}</h6>
                <small class="text-muted">${result.type}</small>
            </div>
            <p class="mb-1">${result.description}</p>
        `;
        container.appendChild(item);
    });
}
