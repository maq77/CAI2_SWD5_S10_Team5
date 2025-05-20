$(document).ready(function () {
    // Star rating functionality in the review form
    $('.stars input').change(function () {
        var rating = $(this).val();

        // Update the stars display
        $('.stars label').css('color', '#e4e7ed');
        for (var i = rating; i >= 1; i--) {
            $('label[for="star' + i + '"]').css('color', '#f9ba48');
        }
    });

    // Initialize the stars based on the default selected value
    var initialRating = $('.stars input:checked').val() || 5;
    for (var i = initialRating; i >= 1; i--) {
        $('label[for="star' + i + '"]').css('color', '#f9ba48');
    }

    // Show reviews tab when clicking on review link
    $('.review-link').on('click', function (e) {
        e.preventDefault();

        // Activate reviews tab
        $('#reviews-tab').tab('show');

        // Scroll to reviews section
        $('html, body').animate({
            scrollTop: $('#reviews-tab').offset().top - 100
        }, 500);
    });

    // Show active tab from ViewBag if set
    if (typeof activeTab !== 'undefined' && activeTab === 'reviews') {
        $('#reviews-tab').tab('show');
    }

    // Handle review edit functionality
    $('.edit-review-btn').on('click', function () {
        const reviewId = $(this).data('review-id');
        const rating = $(this).data('rating');
        const comment = $('#review-content-' + reviewId).text().trim();

        // Populate the edit form
        $('#edit-review-id').val(reviewId);
        $('#edit-rating-' + rating).prop('checked', true);
        $('#edit-comment').val(comment);

        // Show the edit modal
        $('#editReviewModal').modal('show');
    });

    // Confirmation for review deletion
    $('.delete-review-btn').on('click', function (e) {
        if (!confirm('Are you sure you want to delete this review?')) {
            e.preventDefault();
        }
    });

    // Form validation
    $('#review-form form').on('submit', function (e) {
        const comment = $('textarea[name="Comment"]').val().trim();

        if (comment.length < 10) {
            e.preventDefault();
            alert('Please enter a review comment (minimum 10 characters).');
            return false;
        }

        if (!$('input[name="Rating"]:checked').val()) {
            e.preventDefault();
            alert('Please select a rating.');
            return false;
        }

        return true;
    });

    // Pagination functionality
    $('.reviews-pagination a').on('click', function (e) {
        e.preventDefault();

        const page = $(this).text();
        const productId = new URLSearchParams(window.location.search).get('id');

        // If it's the next button
        if ($(this).find('.fa-angle-right').length) {
            const currentPage = $('.reviews-pagination li.active').text();
            window.location.href = '?id=' + productId + '&page=' + (parseInt(currentPage) + 1) + '#reviews';
        } else {
            window.location.href = '?id=' + productId + '&page=' + page + '#reviews';
        }
    });
});