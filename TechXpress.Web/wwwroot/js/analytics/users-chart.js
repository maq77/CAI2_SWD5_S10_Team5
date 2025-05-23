document.addEventListener("DOMContentLoaded", function () {
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    $.get("/Admin/Analytics/GetUserChartData", function (data) {
        new Chart(document.getElementById("usersChart"), {
            type: "bar",
            data: {
                labels: data.map(d => monthNames[d.month-1]),
                datasets: [{
                    label: "Users",
                    data: data.map(x => x.count),
                    backgroundColor: "#17a2b8"
                }]
            }
        });
    });
});
