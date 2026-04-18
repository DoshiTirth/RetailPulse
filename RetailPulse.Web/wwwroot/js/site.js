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