document.addEventListener('DOMContentLoaded', function () {
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    // ========== Sales Chart (Filtered by Date) ==========
    const ctx = document.getElementById('salesChart').getContext('2d');
    let salesChart;

    async function fetchAndRenderChart(from, to) {
        let url = '/Admin/Analytics/GetSalesChartData?';

        if (from) url += `from=${encodeURIComponent(from)}&`;
        if (to) url += `to=${encodeURIComponent(to)}&`;

        const response = await fetch(url);
        const data = await response.json();

        const labels = data.map(d => monthNames[d.month - 1]);
        const sales = data.map(d => d.sales);
        const revenue = data.map(d => d.revenue);

        if (salesChart) {
            salesChart.data.labels = labels;
            salesChart.data.datasets[0].data = sales;
            salesChart.data.datasets[1].data = revenue;
            salesChart.update();
        } else {
            salesChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Total Sales',
                            data: sales,
                            borderColor: '#36a2eb',
                            backgroundColor: 'rgba(54,162,235,0.1)',
                            tension: 0.3,
                            fill: true
                        },
                        {
                            label: 'Revenue',
                            data: revenue,
                            borderColor: '#4bc0c0',
                            backgroundColor: 'rgba(75,192,192,0.1)',
                            tension: 0.3,
                            fill: true
                        }
                    ]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { position: 'top' },
                        title: { display: true, text: 'Monthly Sales and Revenue' }
                    }
                }
            });
        }
    }

    // Initial load without filters
    fetchAndRenderChart();

    document.getElementById('applyFilters')?.addEventListener('click', () => {
        const from = document.getElementById('fromDate').value;
        const to = document.getElementById('toDate').value;
        fetchAndRenderChart(from, to);
    });

    // ========== Category Chart ==========
    fetch('/Admin/Analytics/GetCategoryChartData')
        .then(res => res.json())
        .then(data => {
            const ctx = document.getElementById('categoriesChart');
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: data.map(x => x.category),
                    datasets: [{
                        label: 'Products per Category',
                        data: data.map(x => x.count),
                        backgroundColor: [
                            '#36a2eb', '#ff6384', '#ffcd56', '#4bc0c0', '#9966ff', '#ff9f40'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Products by Category'
                        }
                    }
                }
            });
        });

    // ========== User Registrations ==========
    fetch('/Admin/Analytics/GetUserChartData')
        .then(res => res.json())
        .then(data => {
            const ctx = document.getElementById('usersChart');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.map(d => monthNames[d.month - 1]),
                    datasets: [{
                        label: 'User Registrations',
                        data: data.map(d => d.count),
                        backgroundColor: '#36a2eb'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Monthly User Registrations'
                        }
                    }
                }
            });
        });

    // ========== Products Uploaded ==========
    fetch('/Admin/Analytics/GetProductChartData')
        .then(res => res.json())
        .then(data => {
            const ctx = document.getElementById('productsChart');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.map(d => monthNames[d.month - 1]),
                    datasets: [{
                        label: 'Products Added',
                        data: data.map(d => d.count),
                        backgroundColor: '#ff6384'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Monthly Product Uploads'
                        }
                    }
                }
            });
        });

    // ========== Stat Count Animation ==========
    document.querySelectorAll('.stat-number').forEach(function (el) {
        const target = parseInt(el.getAttribute('data-target'));
        let current = 0;
        const increment = target / 50;
        const timer = setInterval(function () {
            current += increment;
            el.textContent = Math.floor(current);
            if (current >= target) {
                el.textContent = target;
                clearInterval(timer);
            }
        }, 30);
    });
});
