$(function () {
    //Update Cart
    $(".cart-quantity").on("change", function () {
        let row = $(this).closest("tr");
        let productId = row.data("product-id");
        let quantity = $(this).val();

        $.ajax({
            url: "/Cart/UpdateCart",
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
            url: "/Cart/RemoveFromCart",
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
            url: "/Cart/UpdateCart",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(updatedCart),
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            success: function () {
                location.reload();
            }
        });
    });
});
});
