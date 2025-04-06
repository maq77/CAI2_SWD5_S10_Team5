$(function () {
    let page = 1;

    $("#loadMorBtn").on("click", function () {
        page++;
        $.get("/Home/LoadMoreProducts", { page: page, pageSize: 6 }, function (data) {
            if (data.trim() === "") {
                $("#loadMoreBtn").hide();
            } else {
                $("#productList").append(data);
            }
        });
    });

    function updateWishlistCount() {
        $.ajax({
            url: "/Customer/Wishlist/GetWishlistCount",
            type: "GET",
            success: function (response) {
                $(".wishlist-count").text(response.count); // Update wishlist count dynamically
            },
            error: function () {
                console.error("Failed to load wishlist count.");
            }
        });
    }

    $(document).off("click", ".wishlist-toggle").on("click", ".wishlist-toggle", function () {
        var button = $(this);
        var productId = button.data("product-id");
        var icon = button.find("i");
        var isWished = icon.hasClass("fa-heart"); // Check if it's already in wishlist

        if (!productId) {
            alert("Invalid product ID.");
            return;
        }

        button.prop("disabled", true); // Prevent multiple clicks

        $.ajax({
            url: isWished ? "/Customer/Wishlist/RemoveFromWishlist" : "/Customer/Wishlist/AddToWishlist",
            type: "POST",
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    // Toggle the icon class
                    icon.toggleClass("fa-heart fa-heart-o text-danger");

                    // Update the tooltip text dynamically
                    button.find(".tooltipp").text(isWished ? "Add to wishlist" : "Remove from wishlist");
                } else {
                    alert(response.message);
                }
                updateWishlistCount();
            },
            error: function () {
                alert("An error occurred. Please try again.");
            },
            complete: function () {
                button.prop("disabled", false); // Re-enable button
            }
        });
    });
    //Remove Wish
    $("[id^=remove-from-wishlist-]").on("click", function () {
        var productId = $(this).data("product-id");
        var row = $("#wishlist-item-" + productId);

        $.ajax({
            url: "/Customer/Wishlist/RemoveFromWishlist",
            type: "POST",
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    $("#wishlist-message")
                        .removeClass("alert-danger")
                        .addClass("alert-success")
                        .text(response.message)
                        .fadeIn().delay(2000).fadeOut();

                    row.fadeOut(500, function () { $(this).remove(); });
                } else {
                    $("#wishlist-message")
                        .removeClass("alert-success")
                        .addClass("alert-danger")
                        .text("Error removing from wishlist!")
                        .fadeIn().delay(2000).fadeOut();
                }
                updateWishlistCount();
            },
            error: function () {
                $("#wishlist-message")
                    .removeClass("alert-success")
                    .addClass("alert-danger")
                    .text("Error removing from wishlist!")
                    .fadeIn().delay(2000).fadeOut();
            }
        });
    });

    $(document).ready(function () {
        updateWishlistCount();
    });


});
