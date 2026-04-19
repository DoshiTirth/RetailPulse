document.addEventListener('DOMContentLoaded', () => {

    // Counter animation for KPI values
    document.querySelectorAll('.kpi-value[data-target]').forEach(el => {
        const target = parseFloat(el.dataset.target);
        const prefix = el.dataset.prefix || '';
        const suffix = el.dataset.suffix || '';
        const isDecimal = el.dataset.decimal === 'true';
        const duration = 900;
        const start = performance.now();

        function update(now) {
            const elapsed = now - start;
            const progress = Math.min(elapsed / duration, 1);
            const ease = 1 - Math.pow(1 - progress, 3);
            const current = target * ease;
            el.textContent = prefix + (isDecimal
                ? current.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',')
                : Math.floor(current).toLocaleString()) + suffix;
            if (progress < 1) requestAnimationFrame(update);
        }
        requestAnimationFrame(update);
    });

    // Staggered fade-in for table rows
    document.querySelectorAll('tbody tr').forEach((row, i) => {
        row.style.opacity = '0';
        row.style.transform = 'translateY(6px)';
        row.style.transition = 'opacity 0.25s ease, transform 0.25s ease';
        setTimeout(() => {
            row.style.opacity = '1';
            row.style.transform = 'translateY(0)';
        }, 40 + i * 30);
    });

    // Animate cards
    document.querySelectorAll('.animate-in').forEach((el, i) => {
        el.style.animationDelay = `${i * 0.07}s`;
    });

});

// ─── Dark / Light Mode Toggle ─────────────────────────
const themeToggle = document.getElementById('theme-toggle');
const iconDark = document.getElementById('icon-dark');
const iconLight = document.getElementById('icon-light');
const toggleLabel = document.getElementById('toggle-label');
const savedTheme = localStorage.getItem('retailpulse-theme');

function applyTheme(isLight) {
    document.body.classList.toggle('light-mode', isLight);
    if (iconDark) iconDark.style.display = isLight ? 'none' : 'block';
    if (iconLight) iconLight.style.display = isLight ? 'block' : 'none';
    if (toggleLabel) toggleLabel.textContent = isLight ? 'Dark' : 'Light';
}

applyTheme(savedTheme === 'light');

if (themeToggle) {
    themeToggle.addEventListener('click', () => {
        const isLight = !document.body.classList.contains('light-mode');
        applyTheme(isLight);
        localStorage.setItem('retailpulse-theme', isLight ? 'light' : 'dark');
    });
}