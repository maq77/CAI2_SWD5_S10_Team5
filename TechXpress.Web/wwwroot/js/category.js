$(function () {

    // Create Category
    $("#createCategoryForm").on("submit", function (event) {
        event.preventDefault();

        var formData = {
            Name: $("#Name").val()
        };

        $.ajax({
            url: "/Admin/Category/Create",
            type: "POST",
            data: formData,
            success: function (response) {
                if (response.success) {
                    alert("Category created successfully!");
                    window.location.href = "/Admin/Category/Index";
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert("Error creating category.");
            }
        });
    });

    // Edit Category
    $("#editCategoryForm").on("submit", function (event) {
        event.preventDefault();

        var formData = {
            Id: $("#Id").val(),
            Name: $("#Name").val()
        };

        $.ajax({
            url: "/Admin/Category/Edit",
            type: "POST",
            data: formData,
            success: function (response) {
                if (response.success) {
                    alert("Category updated successfully!");
                    window.location.href = "/Category/Index";
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert("Error updating category.");
            }
        });
    });

    // Delete Category
    $(".delete-category").on("click", function () {
        var categoryId = $(this).data("id");

        if (confirm("Are you sure you want to delete this category?")) {
            $.ajax({
                url: "/Admin/Category/Delete",
                type: "POST",
                data: { id: categoryId },
                success: function (response) {
                    if (response.success) {
                        alert("Category deleted successfully.");
                        $("#row-" + categoryId).remove();
                    } else {
                        alert("Failed to delete category.");
                    }
                },
                error: function () {
                    alert("Error deleting category.");
                }
            });
        }
    });

});
