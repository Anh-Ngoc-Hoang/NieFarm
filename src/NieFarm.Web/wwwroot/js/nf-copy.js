// Copy-to-clipboard for the bank transfer panel (BankTransferPanel.razor).
//
// A delegated document click handler for [data-copy]: it survives Blazor's DOM patching, and the
// data-copied attribute is added here, not in Razor markup, so Blazor never rewrites it. This is a
// plain body-tag module, not IJSRuntime interop (same rationale as nf-motion.js).
//
// With no Clipboard API (insecure origin, old browser) the click does nothing: the values stay
// visible and selectable as text, so there is no failure state.
(() => {
    const RESET_MS = 2000;
    const timers = new WeakMap();
    let live = null;

    // Created once at load (some screen readers miss a region that first appears with its text).
    function ensureLive() {
        if (live && live.isConnected) return live;
        if (!document.body) return null;
        live = document.createElement('span');
        live.className = 'sr-only';
        live.setAttribute('role', 'status');
        live.setAttribute('aria-live', 'polite');
        document.body.appendChild(live);
        return live;
    }

    if (document.body) ensureLive();
    else document.addEventListener('DOMContentLoaded', ensureLive, { once: true });

    function announce(text) {
        if (!ensureLive()) return;
        // Clear first so repeating the same message is announced again.
        live.textContent = '';
        requestAnimationFrame(() => { live.textContent = text; });
    }

    document.addEventListener('click', async (event) => {
        const button = event.target instanceof Element ? event.target.closest('[data-copy]') : null;
        if (!button || !navigator.clipboard?.writeText) return;

        try {
            await navigator.clipboard.writeText(button.getAttribute('data-copy') ?? '');
        } catch {
            return;
        }

        button.setAttribute('data-copied', '');
        announce('Đã sao chép');

        clearTimeout(timers.get(button));
        timers.set(button, setTimeout(() => button.removeAttribute('data-copied'), RESET_MS));
    });
})();
