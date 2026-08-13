export function renderSalesChart(id, labels, values) {
    const canvas = document.getElementById(id);
    if (!canvas || typeof Chart === 'undefined') return;
    Chart.getChart(canvas)?.destroy();
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
            animation: { duration: 1050, easing: 'easeOutQuart' },
            plugins: { legend: { display: false }, tooltip: { backgroundColor: '#17231d', titleColor: '#aebbb4', bodyColor: '#fff', cornerRadius: 9, padding: 12, displayColors: false, callbacks: { label: ctx => `Revenue  $${ctx.parsed.y.toLocaleString()}` } } },
            scales: {
                x: { grid: { display: false }, border: { display: false }, ticks: { color: '#87938d', font: { family: 'Segoe UI', size: 10, weight: 600 } } },
                y: { beginAtZero: true, border: { display: false }, grid: { color: '#edf1ef', drawTicks: false }, ticks: { padding: 10, color: '#87938d', font: { family: 'Segoe UI', size: 10 }, callback: value => `$${value}` } }
            }
        }
    });
}

export function printPage() { window.print(); }

const motionSelector = [
    '.dashboard-hero', '.page-heading', '.summary-strip', '.metric-card',
    '.panel', '.data-panel', '.form-card', '.pos-context', '.catalog-panel',
    '.cart-panel', '.report-kpi', '.profile-card', '.invoice', '.auth-card',
    '.product-tile', '.alert', '.admin-hero', '.admin-kpi',
    '.admin-orders-panel', '.admin-management-panel', '.admin-health-panel',
    '.admin-activity-panel', '.access-guide'
].join(',');

function initializeMotion() {
    if (window.stockFlowMotionInitialized) return;
    window.stockFlowMotionInitialized = true;

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduceMotion || !('IntersectionObserver' in window)) return;

    const registered = new WeakSet();
    const revealObserver = new IntersectionObserver(entries => {
        for (const entry of entries) {
            if (!entry.isIntersecting) continue;
            entry.target.classList.add('motion-visible');
            revealObserver.unobserve(entry.target);
        }
    }, { threshold: .08, rootMargin: '0px 0px -24px' });

    const register = root => {
        const candidates = [];
        if (root instanceof Element && root.matches(motionSelector)) candidates.push(root);
        if (root.querySelectorAll) candidates.push(...root.querySelectorAll(motionSelector));

        candidates.forEach((element, index) => {
            if (registered.has(element)) return;
            registered.add(element);
            element.dataset.motion = '';
            element.style.setProperty('--motion-delay', `${Math.min(index * 38, 228)}ms`);
            revealObserver.observe(element);
        });
    };

    register(document);
    document.documentElement.classList.add('motion-ready');

    new MutationObserver(mutations => {
        for (const mutation of mutations) {
            for (const node of mutation.addedNodes) {
                if (node instanceof Element) register(node);
            }
        }
    }).observe(document.body, { childList: true, subtree: true });
}

initializeMotion();

if (!window.stockFlowKeyboardBound) {
    window.stockFlowKeyboardBound = true;
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
}

if (!window.stockFlowPasswordToggleBound) {
    window.stockFlowPasswordToggleBound = true;
    document.addEventListener('click', event => {
        const button = event.target.closest('[data-password-toggle]');
        if (!button) return;
        const input = document.getElementById(button.dataset.passwordToggle);
        if (!(input instanceof HTMLInputElement)) return;
        const revealing = input.type === 'password';
        input.type = revealing ? 'text' : 'password';
        button.textContent = revealing ? 'Hide' : 'Show';
        button.setAttribute('aria-pressed', String(revealing));
        input.focus();
    });
}
