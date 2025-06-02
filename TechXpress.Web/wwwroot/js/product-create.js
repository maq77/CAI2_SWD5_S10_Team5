$(function () {
    $("#productForm").on("submit",function (e) {
        e.preventDefault(); // Prevent default form submission

        let formData = new FormData(this); // Collect form data (including files)

        $.ajax({
            url: "/Admin/Product/Create",
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                $("#successMessage").removeClass("d-none").text("Product added successfully!");
                $("#errorMessage").addClass("d-none");
                $("#productForm")[0].reset(); // Clear form after success
                alert(response.message);
                window.location.href = "/Admin/Product/Index";
            },
            error: function (xhr) {
                let errorText = "Error adding product.";
                if (xhr.responseJSON) {
                    errorText += " " + xhr.responseJSON.message;
                }
                $("#errorMessage").removeClass("d-none").text(errorText);
            }
        });
    });
});
