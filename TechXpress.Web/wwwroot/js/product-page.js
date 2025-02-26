$(function () {  // ✅ Ensures jQuery is Ready
    let pageNumber = 1; // ✅ Track current page
    let pageSize = 5;
    let isLoading = false;
    let hasMoreProducts = true;

    // ✅ Live Search (Separate AJAX for searching)
    $("#searchBox").on("keyup", function () {
        clearTimeout($.data(this, 'timer'));
        let wait = setTimeout(resetProducts, 300); // ✅ Add delay to prevent excessive requests
        $(this).data('timer', wait);
    });

    // ✅ Sorting
    $(".sort-button").on("click", function (e) {
        e.preventDefault();
        $(".sort-button").removeClass("active");
        $(this).addClass("active");
        searchProducts();
    });

    // ✅ Category Filter
    $("#categoryFilter").on("change", function () {
        searchProducts();
    });

    // ✅ Search on Button Click
    $("#searchButton").on("click", function () {
        searchProducts();
    });

    // ✅ Auto Load More on Scroll
    /*$(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() >= $(document).height() - 100) {
            if (!isLoading) {
                isLoading = true;
                loadProducts();
            }
        }
    });*/
    ////////////////////////////// lazy loading for image
    document.addEventListener("DOMContentLoaded", function () {
        let lazyImages = document.querySelectorAll(".lazy-image");

        if ("IntersectionObserver" in window) {
            let observer = new IntersectionObserver(function (entries, observer) {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        let img = entry.target;
                        img.src = img.dataset.src;
                        img.classList.remove("lazy-image");
                        observer.unobserve(img);
                    }
                });
            });

            lazyImages.forEach(img => {
                observer.observe(img);
            });
        } else {
            // Fallback for older browsers
            lazyImages.forEach(img => {
                img.src = img.dataset.src;
            });
        }
    });


    $(document).on("click", ".pagination-link", function (e) {
        e.preventDefault();
        pageNumber = $(this).data("pageNumber"); // ✅ Get page number from button
        loadProducts(false); // ✅ Replace product list
    });

    $("#loadMoreButton").on("click", function () {
        pageNumber++;
        loadProducts(true); // ✅ Append products instead of replacing
    });

    // ✅ Reset Products & Load New Data
    function resetProducts() {
        pageNumber = 1; // ✅ Reset to first page
        hasMoreProducts = true;
        $("#productList").html(""); // ✅ Clear previous products
        $("#paginationControls").html(""); // ✅ Reset pagination
        $("#loadMoreButton").show(); // ✅ Show Load More button
        loadProducts(false);
    }

    // ✅ Search Products (Separate AJAX Call)
    function searchProducts() {
        if (!hasMoreProducts || isLoading) return;
        if (!pageNumber) pageNumber = 1;
        var searchTerm = $("#searchBox").val();
        var sortOrder = $(".sort-button.active").data("sort") || "";
        var categoryId = $("#categoryFilter").val() || 0;

        $.ajax({
            url: "/Product/Search",
            type: "GET",
            data: { pageNumber: pageNumber, pageSize: pageSize, searchTerm: searchTerm, categoryId: categoryId, sortOrder: sortOrder },
            beforeSend: function () {
                $("#productList").html("<p class='text-center'>Loading...</p>"); // Show loading
                isLoading = true;
            },
            success: function (data) {
                if (data.trim() === "") {
                    $("#productList").html("<p class='text-center text-muted'>No products found.</p>");
                } else {
                    $("#productList").html(data);
                }
                isLoading = false;
            },
            error: function () {
                alert("Error loading products. Please try again.");
                isLoading = false;
            }
        });
    }

    // ✅ Load More Products (Separate AJAX Call)
    function loadProducts(append = false) {
        if (!hasMoreProducts || isLoading) return;

        let searchTerm = $("#searchBox").val();
        let categoryId = $("#categoryFilter").val() || 0;
        let sortOrder = $(".sort-button.active").data("sort") || "";

        $.ajax({
            url: "/Product/LoadMoreProducts",
            type: "GET",
            data: { pageNumber: pageNumber, pageSize: pageSize, searchTerm: searchTerm, categoryId: categoryId, sortOrder: sortOrder },
            beforeSend: function () {
                $("#loadMoreButton").text("Loading...").prop("disabled", true);
                isLoading = true;
            },
            success: function (data) {
                if (data.trim() === "") {
                    hasMoreProducts = false;
                    $("#loadMoreButton").hide();
                } else {
                    if (append) {
                        $("#productList").append(data); // ✅ Append products
                    } else {
                        $("#productList").html(data); // ✅ Replace products
                    }
                    $("#loadMoreButton").text("Load More").prop("disabled", false);
                    updatePaginationControls();
                }
                isLoading = false;
            },
            error: function () {
                alert("Error loading products. Please try again.");
                isLoading = false;
            }
        });
    }
    function updatePaginationControls() {
        $.ajax({
            url: "/Product/GetPagination",
            type: "GET",
            data: { pageNumber: pageNumber, pageSize: pageSize },
            success: function (paginationHtml) {
                $("#paginationControls").html(paginationHtml); // ✅ Update pagination UI
            }
        });
    }
    loadProducts(false);
});
