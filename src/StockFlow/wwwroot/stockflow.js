export function renderSalesChart(id, labels, values) {
    const canvas = document.getElementById(id);
    if (!canvas || typeof Chart === 'undefined') return;
    const context = canvas.getContext('2d');
    const gradient = context.createLinearGradient(0, 0, 0, 270);
    gradient.addColorStop(0, 'rgba(172, 202, 61, .24)');
    gradient.addColorStop(.7, 'rgba(172, 202, 61, .05)');
    gradient.addColorStop(1, 'rgba(172, 202, 61, 0)');
    new Chart(context, {
        type: 'line',
        data: { labels, datasets: [{ data: values, borderColor: '#91aa35', backgroundColor: gradient, fill: true, tension: .38, borderWidth: 2.25, pointRadius: 0, pointHoverRadius: 5, pointHoverBackgroundColor: '#17231d', pointHoverBorderColor: '#d8f064', pointHoverBorderWidth: 2 }] },
        options: {
            responsive: true, maintainAspectRatio: false,
            interaction: { intersect: false, mode: 'index' },
            plugins: { legend: { display: false }, tooltip: { backgroundColor: '#17231d', titleColor: '#aebbb4', bodyColor: '#fff', cornerRadius: 9, padding: 12, displayColors: false, callbacks: { label: ctx => `Revenue  $${ctx.parsed.y.toLocaleString()}` } } },
            scales: {
                x: { grid: { display: false }, border: { display: false }, ticks: { color: '#87938d', font: { family: 'Segoe UI', size: 10, weight: 600 } } },
                y: { beginAtZero: true, border: { display: false }, grid: { color: '#edf1ef', drawTicks: false }, ticks: { padding: 10, color: '#87938d', font: { family: 'Segoe UI', size: 10 }, callback: value => `$${value}` } }
            }
        }
    });
}

export function printPage() { window.print(); }

document.addEventListener('keydown', event => {
    const target = event.target;
    const isTyping = target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement || target instanceof HTMLSelectElement || target?.isContentEditable;

    if (event.key === '/' && !isTyping) {
        const search = document.querySelector('.topbar-search input');
        if (search) {
            event.preventDefault();
            search.focus();
        }
    }

    if (event.key === 'Escape' && target instanceof HTMLInputElement && target.closest('.topbar-search')) {
        target.value = '';
        target.blur();
    }
});
