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
});
