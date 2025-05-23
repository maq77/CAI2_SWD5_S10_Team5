document.addEventListener('DOMContentLoaded', function () {
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    // SALES CHART
    fetch('/Admin/Dashboard/GetSalesChartData')
        .then(res => res.json())
        .then(data => {
            const ctx = document.getElementById('salesChart');
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: data.map(d => monthNames[d.month - 1]),
                    datasets: [
                        {
                            label: 'Total Sales',
                            data: data.map(d => d.sales),
                            borderColor: '#36a2eb',
                            backgroundColor: 'rgba(54,162,235,0.1)',
                            tension: 0.3,
                            fill: true
                        },
                        {
                            label: 'Revenue',
                            data: data.map(d => d.revenue),
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
                        legend: {
                            position: 'top'
                        },
                        title: {
                            display: true,
                            text: 'Monthly Sales and Revenue'
                        }
                    }
                }
            });
        });

    // CATEGORY CHART
    fetch('/Admin/Dashboard/GetCategoryChartData')
        .then(res => res.json())
        .then(data => {
            const ctx = document.getElementById('categoryChart');
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

    // USER REGISTRATIONS CHART
    fetch('/Admin/Dashboard/GetUserChartData')
        .then(res => res.json())
        .then(data => {
            const ctx = document.getElementById('userChart');
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

    // PRODUCTS ADDED CHART
    fetch('/Admin/Dashboard/GetProductChartData')
        .then(res => res.json())
        .then(data => {
            const ctx = document.getElementById('productChart');
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

    // Animate Stat Numbers
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
