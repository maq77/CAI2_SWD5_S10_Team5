$(function () {
    //const token = $('input[name="__RequestVerificationToken"]').val();

    $(document).off("click", ".add-to-cart-1-btn").on("click", ".add-to-cart-1-btn", function () {
        var button = $(this);
        var productId = button.data("product-id");
        var quantityInput = button.closest('.add-to-cart-1').find('.product-quantity');
        var quantity = parseInt(quantityInput.val());
        //alert("Function hit!");


        if (!productId) {
            alert("Invalid product ID.");
            return;
        }
        if (quantity < 1) {
            alert("Quantity must be at least 1.");
            quantityInput.val(1);
            return;
        }
        //// Optional: check max if it's set
        var max = parseInt(quantityInput.attr("max"));
        if (max && quantity > max) {
            alert("Only " + max + " items in stock.");
            quantityInput.val(max);
            return;
        }

        button.prop("disabled", true); // Prevent multiple clicks

        $.ajax({
            url: "/Customer/Cart/AddToCart",
            type: "POST",
            data: { productId: productId, quantity: quantity },
            success: function (response) {
                alert(response.message);
                updateCartSummary(true);
            },
            error: function () {
                alert("An error occurred. Please try again.");
            },
            complete: function () {
                button.prop("disabled", false); // Re-enable button
            }
        });
    });
    

    //Update Cart
    $(".cart-quantity").on("change", function () {
        let row = $(this).closest("tr");
        let productId = row.data("product-id");
        let quantity = $(this).val();

        $.ajax({
            url: "/Customer/Cart/UpdateQuantity",
            type: "POST",
            data: { productId: productId, quantity: quantity },
            success: function (response) {
                if (response.success) {
                    alert(`Updated Quantity: ${quantity}`);
                    row.find(".cart-item-total").text(response.itemTotal);
                    $("#cart-total").text(response.cartTotal); 
                }
            },
            error: function () {
                alert("Failed to update quantity.");
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
            success: function () {
                location.reload();
            }
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".cart-toggle").forEach(function (btn) {
        btn.addEventListener("click", function () {
            const target = document.querySelector(this.dataset.target);
            if (target) target.classList.toggle("show");
        });
    });
});


////////////////////
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
