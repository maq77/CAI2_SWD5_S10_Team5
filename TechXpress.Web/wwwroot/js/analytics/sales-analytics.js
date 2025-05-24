document.addEventListener("DOMContentLoaded", function () {
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    const ctx = document.getElementById("salesChart").getContext("2d");
    let salesChart;

    function loadSalesChart(from, to) {
        $.ajax({
            url: "/Admin/Analytics/GetSalesChartData",
            method: "GET",
            data: { from: from, to: to },
            success: function (data) {
                const labels = data.map(x => monthNames[x.month-1]);
                const sales = data.map(x => x.sales);
                const revenue = data.map(x => x.revenue);

                if (salesChart) salesChart.destroy();

                salesChart = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: labels,
                        datasets: [
                            {
                                label: "Sales",
                                data: sales,
                                borderColor: "#007bff",
                                backgroundColor: "rgba(0,123,255,0.1)",
                                fill: true,
                                tension: 0.3
                            },
                            {
                                label: "Revenue",
                                data: revenue,
                                borderColor: "#28a745",
                                backgroundColor: "rgba(40,167,69,0.1)",
                                fill: true,
                                tension: 0.3
                            }
                        ]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            legend: { position: 'top' },
                            title: { display: true, text: 'Monthly Sales & Revenue' }
                        }
                    }
                });
            }
        });
    }

    // Apply Filters
    $("#applyFilters").click(function () {
        const from = $("#fromDate").val();
        const to = $("#toDate").val();
        loadSalesChart(from, to);
    });

    // Export
    $(".export-btn").click(function () {
        const type = $(this).data("type");
        const from = $("#fromDate").val();
        const to = $("#toDate").val();
        const url = `/Admin/Analytics/ExportSalesData?type=${type}&from=${from}&to=${to}`;
        window.location.href = url;
    });

    // Initial Load
    loadSalesChart();
});
