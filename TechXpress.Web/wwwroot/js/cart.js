$(function () {
    //Update Cart
    $(".cart-quantity").on("change", function () {
        let row = $(this).closest("tr");
        let productId = row.data("product-id");
        let quantity = $(this).val();

        $.ajax({
            url: "/Customer/Cart/UpdateCart",
            type: "POST",
            data: { productId: productId, quantity: quantity },
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            success: function (response) {
                location.reload();
            }
        });
    });

    // Remove Cart Item
    $(".remove-from-cart").on("click", function () {
        let row = $(this).closest("tr");
        let productId = row.data("product-id");

        $.ajax({
            url: "/Customer/Cart/RemoveFromCart",
            type: "POST",
            data: { productId: productId },
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            success: function () {
                location.reload();
            }
        });
    });

    // Update Entire Cart
    $("#update-cart").on("click", function () {
        let updatedCart = [];

        $("#cart-items tr").each(function () {
            let productId = $(this).data("product-id");
            let quantity = $(this).find(".cart-quantity").val();
            updatedCart.push({ productId: productId, quantity: quantity });
        });

        $.ajax({
            url: "/Customer/Cart/UpdateCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(updatedCart),
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            success: function () {
                location.reload();
            }
        });
    });

    $("#cartDropdown").on("click", function (e) {
        e.preventDefault();
        let cartDropdown = $(".cart-dropdown");

        if (cartDropdown.hasClass("show")) {
            cartDropdown.removeClass("show").hide();
        } else {
            cartDropdown.addClass("show").show();
        }
    });

    // Close dropdown when clicking outside
    $(document).on("click", function (event) {
        if (!$(event.target).closest(".dropdown").length) {
            $(".cart-dropdown").removeClass("show").hide();
        }
    });

    function removeFromCart(productId) {
        $.ajax({
            url: '/Customer/Cart/RemoveFromCart',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                // Call updateCartSummary and maintain visibility
                updateCartSummary(true);
            },
            error: function () {
                alert("Error removing item from cart.");
            }
        });
    }

    function updateCartSummary(keepOpen) {
        $.get('/Customer/Cart/GetCartSummary', function (data) {
            // Store the current state - if it's visible or not
            var isVisible = $(".cart-dropdown").hasClass("show");

            // Update the content
            $(".cart-dropdown").html(data);

            // If it was visible before or we want to keep it open, make it visible again
            if (isVisible || keepOpen === true) {
                $(".cart-dropdown").addClass("show").css("display", "block");
            }
        });
    }

    //$(".cart-dropdown").addClass("show").css("display", "block");
});
