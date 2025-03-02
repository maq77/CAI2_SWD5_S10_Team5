$(function () {
    // ✅ Add to Cart
    $(".add-to-cart").on("click",function (e) {
        e.preventDefault();
        let productId = $(this).data("product-id");
        $.post("/Cart/AddToCart", { productId: productId, quantity: 1 }, function (response) {
            if (response.success) {
                updateCartSummary();
                alert(response.message);
            }
        });
    });

    // ✅ Update Cart
    $("#update-cart").on("click",function () {
        let updatedCart = [];
        $("#cart-items tr").each(function () {
            let productId = $(this).data("product-id");
            let quantity = $(this).find(".cart-quantity").val();
            updatedCart.push({ ProductId: productId, Quantity: parseInt(quantity) });
        });

        $.post("/Cart/UpdateCart", { updatedCart: updatedCart }, function (response) {
            if (response.success) {
                updateCartSummary();
                location.reload();
            }
        });
    });

    // ✅ Remove Item from Cart
    $(".remove-from-cart").on("click",function () {
        let productId = $(this).closest("tr").data("product-id");

        $.post("/Cart/RemoveFromCart", { productId: productId }, function (response) {
            if (response.success) {
                updateCartSummary();
                location.reload();
            }
        });
    });

    // ✅ Refresh Cart Summary
    function updateCartSummary() {
        $.get("/Cart/GetCartSummary", function (data) {
            $("#cart-summary").html(data);
        });
    }
});
