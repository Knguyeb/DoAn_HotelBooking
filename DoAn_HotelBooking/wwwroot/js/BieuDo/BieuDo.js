// ==================== BIỂU ĐỒ DOANH THU ====================

// Lấy dữ liệu từ Razor view
const labels = window.labelsData || [];
const values = window.valuesData || [];
const total = values.reduce((a, b) => a + b, 0);

// Sinh màu đôi lập
const colors = [];
for (let i = 0; i < labels.length; i++) {
    const hue = ((i * 137.5) % 360);
    colors.push(`hsl(${hue}, 75%, 55%)`);
}

let currentChart = null;
const ctx = document.getElementById('mainChart')?.getContext('2d');

// Biểu đồ tròn
function initializePieChart() {
    if (currentChart) currentChart.destroy();
    currentChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels,
            datasets: [{
                data: values,
                backgroundColor: colors,
                borderColor: '#000',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: context => {
                            const value = context.raw;
                            const percent = ((value / total) * 100).toFixed(1);
                            return `${context.label}: ${value.toLocaleString('vi-VN')} VNĐ (${percent}%)`;
                        }
                    }
                }
            }
        }
    });
}

// Biểu đồ cột
function createBarChart() {
    if (currentChart) currentChart.destroy();
    currentChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: 'Doanh thu',
                data: values,
                backgroundColor: colors,
                borderColor: '#fff',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: ctx => ctx.raw.toLocaleString('vi-VN') + " VNĐ"
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        color: '#fff',
                        callback: val => val.toLocaleString('vi-VN') + ' VNĐ'
                    },
                    grid: { color: 'rgba(255,255,255,0.2)' },
                    border: { color: '#fff', width: 2 }
                },
                x: {
                    display: false,
                    grid: { display: false },
                    border: { color: '#fff', width: 2 }
                }
            }
        }
    });
}

// Biểu đồ đường
function createLineChart() {
    if (currentChart) currentChart.destroy();
    currentChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels,
            datasets: [{
                label: 'Doanh thu',
                data: values,
                backgroundColor: colors[0],
                borderColor: colors[0],
                borderWidth: 3,
                fill: false,
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: ctx => ctx.raw.toLocaleString('vi-VN') + " VNĐ"
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        color: '#fff',
                        callback: val => val.toLocaleString('vi-VN') + ' VNĐ'
                    },
                    grid: { color: 'rgba(255,255,255,0.2)' },
                    border: { color: '#fff', width: 2 }
                },
                x: {
                    display: false,
                    grid: { display: false },
                    border: { color: '#fff', width: 2 }
                }
            }
        }
    });
}

// Biểu đồ Doughnut
function createDoughnutChart() {
    if (currentChart) currentChart.destroy();
    currentChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels,
            datasets: [{
                data: values,
                backgroundColor: colors,
                borderColor: '#000',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: ctx => {
                            const value = ctx.raw;
                            const percent = ((value / total) * 100).toFixed(1);
                            return `${ctx.label}: ${value.toLocaleString('vi-VN')} VNĐ (${percent}%)`;
                        }
                    }
                }
            }
        }
    });
}

// Cập nhật chú thích
function updateLegend() {
    let legendHTML = "";
    labels.forEach((label, i) => {
        const percent = ((values[i] / total) * 100).toFixed(1);
        legendHTML += `
            <div class="d-flex align-items-center gap-2 mb-1">
                <div style="width:15px;height:15px;background:${colors[i]};border-radius:3px;"></div>
                <span>${label} (${percent}%)</span>
            </div>
        `;
    });
    document.getElementById("chartLegend").innerHTML = legendHTML;
}

// Xử lý chuyển đổi loại biểu đồ
document.querySelectorAll('.chart-type-btn').forEach(btn => {
    btn.addEventListener('click', e => {
        e.preventDefault();
        document.querySelectorAll('.chart-type-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        const chartType = btn.getAttribute('data-type');
        switch (chartType) {
            case 'pie':
                initializePieChart();
                document.getElementById('chartTypeDropdown').innerHTML = '<i class="bi bi-pie-chart-fill"></i> Biểu đồ tròn';
                break;
            case 'bar':
                createBarChart();
                document.getElementById('chartTypeDropdown').innerHTML = '<i class="bi bi-bar-chart-fill"></i> Biểu đồ cột';
                break;
            case 'line':
                createLineChart();
                document.getElementById('chartTypeDropdown').innerHTML = '<i class="bi bi-graph-up"></i> Biểu đồ đường';
                break;
            case 'doughnut':
                createDoughnutChart();
                document.getElementById('chartTypeDropdown').innerHTML = '<i class="bi bi-circle"></i> Biểu đồ Doughnut';
                break;
        }
    });
});

// Hiệu ứng đếm số
function animateCounter(element, target, duration = 1200, isPercent = false) {
    let start = 0;
    const step = Math.max(1, Math.ceil(target / (duration / 30)));
    const interval = setInterval(() => {
        start += step;
        if (start >= target) {
            element.textContent = isPercent
                ? target.toFixed(1) + "%"
                : target.toLocaleString('vi-VN') + " VNĐ";
            clearInterval(interval);
        } else {
            element.textContent = isPercent
                ? start.toFixed(1) + "%"
                : start.toLocaleString('vi-VN') + " VNĐ";
        }
    }, 30);
}

// Kích hoạt hiệu ứng khi load
document.addEventListener('DOMContentLoaded', () => {
    initializePieChart();
    updateLegend();

    const tongDoanhThuEl = document.querySelector('.report-card:first-child .card-text');
    if (tongDoanhThuEl) {
        const value = parseInt(tongDoanhThuEl.textContent.replace(/\D/g, ''));
        animateCounter(tongDoanhThuEl, value);
    }

    const topDoanhThuEl = document.querySelector('.report-card:nth-child(2) .card-text b.text-danger');
    if (topDoanhThuEl) {
        const value = parseInt(topDoanhThuEl.textContent.replace(/\D/g, ''));
        animateCounter(topDoanhThuEl, value);
    }

    const topPercentEl = document.querySelector('.report-card:nth-child(2) .card-text span.text-danger');
    if (topPercentEl) {
        const value = parseFloat(topPercentEl.textContent.replace('%', '').trim());
        animateCounter(topPercentEl, value, 1000, true);
    }

    document.querySelectorAll('#tableCustomer tbody tr td:nth-child(3)').forEach(td => {
        const value = parseInt(td.textContent.replace(/\D/g, ''));
        if (!isNaN(value)) {
            td.textContent = '0 VNĐ';
            animateCounter(td, value, 1000 + Math.random() * 800);
        }
    });
});
