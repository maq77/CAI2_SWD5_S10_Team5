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

    //document.getElementById('cartDropdown').addEventListener('click', function (e) {
    //    e.preventDefault();
    //    const dropdown = this.nextElementSibling;
    //    dropdown.classList.toggle('show');
    //});

    window.addEventListener('click', function (e) {
        const dropdown = document.querySelector('.cart-dropdown');
        const toggle = document.getElementById('cartDropdown');

        if (!dropdown.contains(e.target) && !toggle.contains(e.target)) {
            dropdown.classList.remove('show');
        }
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
                alert("An error occurred User Not Authorized. Please try again.");
                alert(response.message);
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
    function checkIfInWishlist(productId, button) {
        $.ajax({
            url: "/Customer/Wishlist/IsinWishlist",
            type: "GET",
            data: { productId: productId },
            success: function (response) {
                var icon = button.find("i");
                var tooltip = button.find(".tooltipp");

                if (response.success) {
                    icon.removeClass("fa-heart-o").addClass("fa-heart text-danger");
                    tooltip.text("Remove from wishlist");
                } else {
                    icon.removeClass("fa-heart text-danger").addClass("fa-heart-o");
                    tooltip.text("Add to wishlist");
                }
            },
            error: function () {
                console.error("Error checking wishlist status.");
            }
        });
    }

    $(document).ready(function () {
        updateWishlistCount();

        /*// Sync all wishlist buttons with backend state
        $(".wishlist-toggle").each(function () {
            var button = $(this);
            var productId = button.data("product-id");
            checkIfInWishlist(productId, button);
        });*/
    });


});
