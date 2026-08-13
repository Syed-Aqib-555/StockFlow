export function renderSalesChart(id, labels, values) {
    const canvas = document.getElementById(id);
    if (!canvas || typeof Chart === 'undefined') return;
    const context = canvas.getContext('2d');
    const gradient = context.createLinearGradient(0, 0, 0, 260);
    gradient.addColorStop(0, 'rgba(177, 208, 63, .28)');
    gradient.addColorStop(1, 'rgba(177, 208, 63, .01)');
    new Chart(context, {
        type: 'line',
        data: { labels, datasets: [{ data: values, borderColor: '#90ad28', backgroundColor: gradient, fill: true, tension: .38, borderWidth: 2.5, pointRadius: 0, pointHoverRadius: 5 }] },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { display: false }, tooltip: { backgroundColor: '#17231d', padding: 11, displayColors: false, callbacks: { label: ctx => `$${ctx.parsed.y.toLocaleString()}` } } },
            scales: {
                x: { grid: { display: false }, border: { display: false }, ticks: { color: '#87938d', font: { size: 11 } } },
                y: { beginAtZero: true, border: { display: false }, grid: { color: '#edf0ee' }, ticks: { color: '#87938d', font: { size: 11 }, callback: value => `$${value}` } }
            }
        }
    });
}

export function printPage() { window.print(); }
