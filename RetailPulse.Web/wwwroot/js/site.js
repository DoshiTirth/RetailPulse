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

// Dark / Light Mode Toggle
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

//  Custom Confirmation Modal
function createModal() {
    if (document.getElementById('confirm-modal')) return;

    const modal = document.createElement('div');
    modal.id = 'confirm-modal';
    modal.innerHTML = `
        <div id="confirm-backdrop" style="
            position: fixed; inset: 0; background: rgba(0,0,0,0.6);
            backdrop-filter: blur(4px); z-index: 1000;
            display: flex; align-items: center; justify-content: center;
            opacity: 0; transition: opacity 0.2s ease;">
            <div style="
                background: var(--bg-surface);
                border: 1px solid var(--border);
                border-radius: var(--radius-lg);
                padding: 28px 32px;
                min-width: 360px;
                max-width: 440px;
                box-shadow: var(--shadow);
                transform: translateY(12px);
                transition: transform 0.2s ease;">
                <div id="confirm-icon" style="margin-bottom: 14px;"></div>
                <div id="confirm-title" style="
                    font-size: 16px; font-weight: 700;
                    color: var(--text-primary); margin-bottom: 8px;"></div>
                <div id="confirm-message" style="
                    font-size: 13px; color: var(--text-secondary);
                    line-height: 1.6; margin-bottom: 24px;"></div>
                <div style="display: flex; gap: 10px; justify-content: flex-end;">
                    <button id="confirm-cancel" class="btn btn-ghost">Cancel</button>
                    <button id="confirm-ok" class="btn btn-primary">Confirm</button>
                </div>
            </div>
        </div>`;
    document.body.appendChild(modal);
}

function showConfirm({ title, message, confirmText = 'Confirm', danger = false, onConfirm }) {
    createModal();
    const backdrop = document.getElementById('confirm-backdrop');
    const box = backdrop.querySelector('div');

    document.getElementById('confirm-title').textContent = title;
    document.getElementById('confirm-message').textContent = message;

    const okBtn = document.getElementById('confirm-ok');
    okBtn.textContent = confirmText;
    okBtn.className = `btn ${danger ? 'btn-danger' : 'btn-primary'}`;

    // Icon
    const iconEl = document.getElementById('confirm-icon');
    iconEl.innerHTML = danger
        ? `<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="var(--red)" stroke-width="2">
               <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
               <line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>
           </svg>`
        : `<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="var(--accent)" stroke-width="2">
               <circle cx="12" cy="12" r="10"/>
               <line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
           </svg>`;

    // Show
    backdrop.style.display = 'flex';
    requestAnimationFrame(() => {
        backdrop.style.opacity = '1';
        box.style.transform = 'translateY(0)';
    });

    function close() {
        backdrop.style.opacity = '0';
        box.style.transform = 'translateY(12px)';
        setTimeout(() => backdrop.style.display = 'none', 200);
    }

    okBtn.onclick = () => { close(); onConfirm(); };
    document.getElementById('confirm-cancel').onclick = close;
    backdrop.onclick = (e) => { if (e.target === backdrop) close(); };
}

// Wire up all confirmation forms
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('form[data-confirm]').forEach(form => {
        form.addEventListener('submit', e => {
            e.preventDefault();
            const title = form.dataset.confirmTitle || 'Are you sure?';
            const message = form.dataset.confirmMessage || 'This action cannot be undone.';
            const btnText = form.dataset.confirmBtn || 'Confirm';
            const danger = form.dataset.confirmDanger !== 'false';

            showConfirm({
                title, message, confirmText: btnText, danger,
                onConfirm: () => form.submit()
            });
        });
    });
});

// Global Search 
const globalSearch = document.getElementById('globalSearch');
const searchResults = document.getElementById('searchResults');

if (globalSearch) {
    let searchTimeout;

    globalSearch.addEventListener('input', () => {
        clearTimeout(searchTimeout);
        const query = globalSearch.value.trim();

        if (query.length < 2) {
            searchResults.style.display = 'none';
            return;
        }

        searchTimeout = setTimeout(async () => {
            try {
                const res = await fetch(`/Search?q=${encodeURIComponent(query)}`);
                const data = await res.json();

                if (data.length === 0) {
                    searchResults.innerHTML = `
                        <div style="padding: 16px; text-align: center;
                                    color: var(--color-text-faint); font-size: 13px;">
                            No results for "${query}"
                        </div>`;
                } else {
                    searchResults.innerHTML = data.map(item => `
                        <a href="${item.url}" style="
                            display: flex; align-items: center; gap: 12px;
                            padding: 10px 16px;
                            color: var(--color-text-primary);
                            text-decoration: none;
                            transition: background var(--transition-fast);
                            border-bottom: 1px solid var(--color-border);"
                           onmouseover="this.style.background='var(--color-secondary)'"
                           onmouseout="this.style.background='transparent'">
                            <div style="
                                width: 28px; height: 28px;
                                background: var(--color-accent-dim);
                                border-radius: var(--radius-sm);
                                display: flex; align-items: center; justify-content: center;
                                font-size: 10px; font-weight: 700;
                                color: var(--color-accent);
                                font-family: var(--font-heading);
                                flex-shrink: 0;">
                                ${item.type.substring(0, 2).toUpperCase()}
                            </div>
                            <div>
                                <div style="font-size: 13px; font-weight: 600;">
                                    ${item.name}
                                </div>
                                <div style="font-size: 11px;
                                            color: var(--color-text-faint);
                                            font-family: var(--font-mono);">
                                    ${item.type} ${item.meta ? '— ' + item.meta : ''}
                                </div>
                            </div>
                        </a>`).join('');
                }

                searchResults.style.display = 'block';
            } catch (e) {
                console.error('Search error:', e);
            }
        }, 300);
    });

    // Close on click outside
    document.addEventListener('click', (e) => {
        if (!globalSearch.contains(e.target) && !searchResults.contains(e.target)) {
            searchResults.style.display = 'none';
        }
    });

    // Keyboard navigation
    globalSearch.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            searchResults.style.display = 'none';
            globalSearch.blur();
        }
    });
}