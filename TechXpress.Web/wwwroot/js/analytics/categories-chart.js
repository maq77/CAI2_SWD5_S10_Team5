document.addEventListener("DOMContentLoaded", function () {
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    $.get("/Admin/Analytics/GetCategoryChartData", function (data) {
        new Chart(document.getElementById("categoriesChart"), {
            type: "bar",
            data: {
                labels: data.map(d => monthNames[d.month-1]),
                datasets: [{
                    label: "Categories",
                    data: data.map(x => x.count),
                    backgroundColor: "#17a2b8"
                }]
            }
        });
    });
});
