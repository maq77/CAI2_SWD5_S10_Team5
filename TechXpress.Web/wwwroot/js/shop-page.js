$(document).ready(function () {
    // Handle search button click
    $('#searchButton').on('click', function () {
        var searchQuery = $('#searchBox').val();
        performSearch(searchQuery);
    });
    //$('#searchButton').on('click', function () {
    //    // Get search parameters
    //    var searchQuery = $('#searchBox').val();
    //    var categoryId = getUrlParameter('categoryId') || 0;
    //    var sortOrder = getUrlParameter('sortOrder') || '';

    //    // Make AJAX call
    //    loadProducts(searchQuery, categoryId, sortOrder, 1);

    //    // Update browser URL
    //    updateUrl(searchQuery, categoryId, sortOrder, 1);
    //});

    //$.ajax({
    //    url: '/Shop/Search',
    //    type: 'GET',
    //    data: {
    //        searchQuery: $('#searchBox').val(),
    //        categoryId: categoryId,
    //        sortOrder: sortOrder,
    //        page: page
    //    },
    //    success: function (data) {
    //        $('#productList').html(data);
    //    },
    //    error: function () {
    //        alert('Error loading products.');
    //    }
    //});
    // Handle search on Enter key
    $('#searchBox').on('keypress', function (e) {
        if (e.which === 13) {
            $('#searchButton').click();
            return false;
        }
    });

    // Add to cart functionality
    $(document).on('click', '.add-to-cart', function () {
        var productId = $(this).data('product-id');
        addToCart(productId);
    });

    // Handle AJAX pagination
    $(document).on('click', '.pagination a', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');

        // Only handle AJAX pagination if it's not a direct link
        if (url && !$(this).hasClass('disabled')) {
            loadProductsFromUrl(url);
        }

        return false;
    });

    // Helper function for search
    function performSearch(searchQuery) {
        // Get current URL parameters
        var urlParams = new URLSearchParams(window.location.search);

        // Update search parameter
        if (searchQuery) {
            urlParams.set('searchQuery', searchQuery);
        } else {
            urlParams.delete('searchQuery');
        }

        // Reset to page 1 when searching
        urlParams.set('page', 1);

        // Navigate to new URL
        window.location.href = window.location.pathname + '?' + urlParams.toString();
    }

    // Function to add product to cart via AJAX
    function addToCart(productId) {
        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { productId: productId, quantity: 1 },
            success: function (response) {
                if (response.success) {
                    // Show success message
                    showNotification('Product added to cart!', 'success');

                    // Update cart counter in navigation if it exists
                    if ($('#cartCounter').length) {
                        $('#cartCounter').text(response.cartItemCount);
                    }
                } else {
                    showNotification('Failed to add product to cart. ' + response.message, 'danger');
                }
            },
            error: function () {
                showNotification('Failed to add product to cart. Please try again.', 'danger');
            }
        });
    }

    // Function to load products from URL via AJAX
    function loadProductsFromUrl(url) {
        $('#productList').addClass('loading');

        $.ajax({
            url: url,
            type: 'GET',
            success: function (data) {
                $('#productList').html(data);

                // Update URL without page refresh using history API
                history.pushState(null, '', url);
            },
            error: function () {
                showNotification('Error loading products. Please try again.', 'danger');
            },
            complete: function () {
                $('#productList').removeClass('loading');
            }
        });
    }

    // Function to show notifications
    function showNotification(message, type) {
        // Create notification element
        var notification = $('<div class="alert alert-' + type + ' alert-dismissible fade show position-fixed" style="top: 20px; right: 20px; z-index: 1050;">' +
            message +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>');

        // Add to body
        $('body').append(notification);

        // Auto-dismiss after 3 seconds
        setTimeout(function () {
            notification.alert('close');
        }, 3000);
    }
});