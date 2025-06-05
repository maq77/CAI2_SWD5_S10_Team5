
var ErrorLog = {
    // Configuration
    config: {
        urls: {
            index: '',
            details: '',
            delete: '',
            bulkDelete: '',
            cleanup: '',
            getStats: ''
        },
        currentPage: 1,
        pageSize: 25,
        selectedIds: [],
        refreshInterval: null
    },

    // Initialize the error log functionality
    init: function (indexUrl, detailsUrl, deleteUrl, bulkDeleteUrl, cleanupUrl, getStatsUrl) {
        this.config.urls.index = indexUrl;
        this.config.urls.details = detailsUrl;
        this.config.urls.delete = deleteUrl;
        this.config.urls.bulkDelete = bulkDeleteUrl;
        this.config.urls.cleanup = cleanupUrl;
        this.config.urls.getStats = getStatsUrl;

        this.bindEvents();
        this.setupAutoRefresh();
    },

    // Bind all event handlers
    bindEvents: function () {
        // Filter form submission
        $('#filtersForm').on('change', 'select, input', function () {
            ErrorLog.applyFilters();
        });

        // Search input with debounce
        let searchTimeout;
        $('#searchInput').on('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                ErrorLog.applyFilters();
            }, 500);
        });

        // Page size change
        $(document).on('change', '#pageSizeSelect', function () {
            ErrorLog.config.pageSize = parseInt($(this).val());
            ErrorLog.config.currentPage = 1;
            ErrorLog.loadData();
        });

        // Select all checkbox
        $(document).on('change', '#selectAll', function () {
            const isChecked = $(this).prop('checked');
            $('.row-checkbox').prop('checked', isChecked);
            ErrorLog.updateSelectedCount();
        });

        // Individual row checkboxes
        $(document).on('change', '.row-checkbox', function () {
            ErrorLog.updateSelectedCount();

            // Update select all checkbox state
            const totalCheckboxes = $('.row-checkbox').length;
            const checkedCheckboxes = $('.row-checkbox:checked').length;

            $('#selectAll').prop('indeterminate', checkedCheckboxes > 0 && checkedCheckboxes < totalCheckboxes);
            $('#selectAll').prop('checked', checkedCheckboxes === totalCheckboxes);
        });

        // Modal event handlers
        $('#confirmDeleteBtn').on('click', function () {
            const errorId = $(this).data('error-id');
            ErrorLog.deleteError(errorId);
        });

        $('#confirmBulkDeleteBtn').on('click', function () {
            ErrorLog.bulkDeleteErrors();
        });

        $('#confirmCleanupBtn').on('click', function () {
            const days = parseInt($('#cleanupDays').val());
            ErrorLog.cleanupOldLogs(days);
        });

        $('#deleteFromModal').on('click', function () {
            const errorId = $(this).data('error-id');
            $('#errorDetailsModal').modal('hide');
            ErrorLog.confirmDelete(errorId);
        });

        // Prevent form submission on enter
        $('#filtersForm').on('submit', function (e) {
            e.preventDefault();
            ErrorLog.applyFilters();
        });
    },

    // Setup auto-refresh functionality
    setupAutoRefresh: function () {
        // Refresh every 30 seconds if on first page with no filters
        if (this.hasNoFilters() && this.config.currentPage === 1) {
            this.config.refreshInterval = setInterval(() => {
                this.refreshStats();
            }, 30000);
        }
    },

    // Check if any filters are applied
    hasNoFilters: function () {
        return !$('#severityFilter').val() &&
            !$('#dateFrom').val() &&
            !$('#dateTo').val() &&
            !$('#searchInput').val();
    },

    // Apply current filters and reload data
    applyFilters: function () {
        this.config.currentPage = 1;
        this.loadData();
    },

    // Clear all filters
    clearFilters: function () {
        $('#severityFilter').val('');
        $('#dateFrom').val('');
        $('#dateTo').val('');
        $('#searchInput').val('');
        this.applyFilters();
    },

    // Load error log data
    loadData: function () {
        this.showLoading();

        const params = {
            page: this.config.currentPage,
            pageSize: this.config.pageSize,
            severity: $('#severityFilter').val(),
            dateFrom: $('#dateFrom').val(),
            dateTo: $('#dateTo').val(),
            search: $('#searchInput').val()
        };

        $.ajax({
            url: this.config.urls.index,
            type: 'GET',
            data: params,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: (response) => {
                $('#errorTableContainer').html(response);
                this.hideLoading();
                this.updateSelectedCount();
                this.refreshStats();
            },
            error: (xhr, status, error) => {
                this.hideLoading();
                this.showError('Failed to load error logs: ' + error);
            }
        });
    },

    // Show loading spinner
    showLoading: function () {
        $('#loadingSpinner').removeClass('d-none');
        $('#errorTableContainer').addClass('opacity-50');
    },

    // Hide loading spinner
    hideLoading: function () {
        $('#loadingSpinner').addClass('d-none');
        $('#errorTableContainer').removeClass('opacity-50');
    },

    // Change page
    changePage: function (page) {
        if (page < 1) return;
        this.config.currentPage = page;
        this.loadData();
    },

    // View error details
    viewErrorDetails: function (errorId) {
        $.ajax({
            url: this.config.urls.details,
            type: 'GET',
            data: { id: errorId },
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: (response) => {
                $('#errorDetailsContent').html(response);
                $('#deleteFromModal').data('error-id', errorId);
                $('#errorDetailsModal').modal('show');
            },
            error: (xhr, status, error) => {
                this.showError('Failed to load error details: ' + error);
            }
        });
    },

    // Show delete confirmation
    confirmDelete: function (errorId) {
        $('#confirmDeleteBtn').data('error-id', errorId);
        $('#deleteConfirmModal').modal('show');
    },

    // Delete single error
    deleteError: function (errorId) {
        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: this.config.urls.delete,
            type: 'POST',
            data: {
                id: errorId,
                __RequestVerificationToken: token
            },
            success: (response) => {
                if (response.success) {
                    this.showSuccess(response.message);
                    $('#deleteConfirmModal').modal('hide');
                    this.loadData();
                } else {
                    this.showError(response.message);
                }
            },
            error: (xhr, status, error) => {
                this.showError('Failed to delete error log: ' + error);
            }
        });
    },

    // Show bulk delete confirmation
    confirmBulkDelete: function () {
        const selectedIds = this.getSelectedIds();
        if (selectedIds.length === 0) {
            this.showWarning('Please select at least one error log to delete.');
            return;
        }

        $('#bulkDeleteCount').text(selectedIds.length);
        $('#bulkDeleteConfirmModal').modal('show');
    },

    // Bulk delete errors
    bulkDeleteErrors: function () {
        const selectedIds = this.getSelectedIds();
        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: this.config.urls.bulkDelete,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(selectedIds),
            headers: {
                'RequestVerificationToken': token,
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: (response) => {
                if (response.success) {
                    this.showSuccess(response.message);
                    $('#bulkDeleteConfirmModal').modal('hide');
                    this.loadData();
                } else {
                    this.showError(response.message);
                }
            },
            error: (xhr, status, error) => {
                this.showError('Failed to delete error logs: ' + error);
            }
        });
    },

    // Show cleanup modal
    showCleanupModal: function () {
        $('#cleanupModal').modal('show');
    },

    // Cleanup old logs
    cleanupOldLogs: function (days) {
        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: this.config.urls.cleanup,
            type: 'POST',
            data: {
                daysToKeep: days,
                __RequestVerificationToken: token
            },
            success: (response) => {
                if (response.success) {
                    this.showSuccess(response.message);
                    $('#cleanupModal').modal('hide');
                    this.loadData();
                } else {
                    this.showError(response.message);
                }
            },
            error: (xhr, status, error) => {
                this.showError('Failed to cleanup old logs: ' + error);
            }
        });
    },

    // Get selected error IDs
    getSelectedIds: function () {
        const selectedIds = [];
        $('.row-checkbox:checked').each(function () {
            selectedIds.push(parseInt($(this).val()));
        });
        return selectedIds;
    },

    // Update selected count display
    updateSelectedCount: function () {
        const selectedCount = $('.row-checkbox:checked').length;
        $('#selectedCount').text(`${selectedCount} selected`);
        $('#bulkDeleteBtn').prop('disabled', selectedCount === 0);
    },

    // Refresh data
    refreshData: function () {
        this.loadData();
    },

    // Refresh statistics only
    refreshStats: function () {
        $.ajax({
            url: this.config.urls.getStats,
            type: 'GET',
            success: (response) => {
                if (response.success !== false) {
                    $('#errorCount').text(response.errorCount);
                    $('#warningCount').text(response.warningCount);
                    $('#infoCount').text(response.infoCount);
                    $('#totalCount').text(response.totalCount);
                }
            },
            error: (xhr, status, error) => {
                console.log('Failed to refresh stats:', error);
            }
        });
    },

    // Show success message
    showSuccess: function (message) {
        this.showToast(message, 'success');
    },

    // Show error message
    showError: function (message) {
        this.showToast(message, 'danger');
    },

    // Show warning message
    showWarning: function (message) {
        this.showToast(message, 'warning');
    },

    // Show info message
    showInfo: function (message) {
        this.showToast(message, 'info');
    },

    // Generic toast notification
    showToast: function (message, type) {
        // Remove existing toasts
        $('.toast').remove();

        const toastHtml = `
            <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true" style="position: fixed; top: 20px; right: 20px; z-index: 9999;">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas fa-${this.getToastIcon(type)} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;

        $('body').append(toastHtml);

        const toast = new bootstrap.Toast($('.toast').last()[0], {
            autohide: true,
            delay: type === 'danger' ? 5000 : 3000
        });

        toast.show();

        // Remove toast element after it's hidden
        $('.toast').last().on('hidden.bs.toast', function () {
            $(this).remove();
        });
    },

    // Get appropriate icon for toast type
    getToastIcon: function (type) {
        switch (type) {
            case 'success': return 'check-circle';
            case 'danger': return 'exclamation-circle';
            case 'warning': return 'exclamation-triangle';
            case 'info': return 'info-circle';
            default: return 'info-circle';
        }
    },

    // Format date for display
    formatDate: function (dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
    },

    // Cleanup on page unload
    cleanup: function () {
        if (this.config.refreshInterval) {
            clearInterval(this.config.refreshInterval);
        }
    }
};

// Initialize when page loads
$(document).ready(function () {
    // Add cleanup on page unload
    $(window).on('beforeunload', function () {
        ErrorLog.cleanup();
    });

    // Add keyboard shortcuts
    $(document).on('keydown', function (e) {
        // Ctrl+R or F5 for refresh
        if ((e.ctrlKey && e.keyCode === 82) || e.keyCode === 116) {
            e.preventDefault();
            ErrorLog.refreshData();
        }

        // Escape to close modals
        if (e.keyCode === 27) {
            $('.modal').modal('hide');
        }
    });

    // Add responsive table scrolling indicator
    $('.table-responsive').on('scroll', function () {
        const scrollLeft = $(this).scrollLeft();
        const scrollWidth = $(this)[0].scrollWidth;
        const clientWidth = $(this)[0].clientWidth;

        $(this).toggleClass('scrolled-left', scrollLeft > 0);
        $(this).toggleClass('scrolled-right', scrollLeft < scrollWidth - clientWidth);
    });
});

// Export for use in other scripts if needed
window.ErrorLog = ErrorLog;