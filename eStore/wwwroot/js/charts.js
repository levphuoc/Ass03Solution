window.renderSalesChart = (labels, data, chartType) => {
    const ctx = document.getElementById('salesChart').getContext('2d');

    if (window.salesChartInstance) {
        window.salesChartInstance.destroy();
    }

    const colors = labels.map(() => {
        const hue = Math.floor(Math.random() * 360);
        return `hsl(${hue}, 70%, 65%)`;
    });

    window.salesChartInstance = new Chart(ctx, {
        type: chartType,
        data: {
            labels: labels,
            datasets: [{
                label: 'Revenue ($)',
                data: data,
                backgroundColor: chartType === 'pie' ? colors : 'rgba(54, 162, 235, 0.7)',
                borderColor: chartType === 'pie' ? '#ffffff' : 'rgba(54, 162, 235, 1)',
                borderWidth: 1.5,
                hoverOffset: chartType === 'pie' ? 8 : 0,
                tension: chartType === 'line' ? 0.3 : 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                duration: 1000,
                easing: 'easeOutQuart'
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0,0,0,0.7)',
                    titleFont: { size: 16, weight: 'bold' },
                    bodyFont: { size: 14 },
                    footerFont: { size: 12 },
                    callbacks: {
                        label: (context) => {
                            const label = context.label || '';
                            const value = context.raw.toLocaleString('en-US', { style: 'currency', currency: 'USD' });
                            return `${label}: ${value}`;
                        }
                    }
                }
            },
            scales: chartType === 'pie' ? {} : {
                y: {
                    title: {
                        display: true,
                        text: 'Revenue ($)',
                        font: { size: 14, weight: 'bold' }
                    },
                    beginAtZero: true,
                    ticks: {
                        callback: (value) => `$${value}`
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Products',
                        font: { size: 14, weight: 'bold' }
                    }
                }
            }
        }
    });
};
