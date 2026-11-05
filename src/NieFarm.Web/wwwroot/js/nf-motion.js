// NieFarm 2.0 — scroll reveal, self-drawing line art, sticky header + reading
// progress, and the /quy-trinh journey rail.
//
// Ported from wwwroot/design-preview/nf2.js sections 0-2 only. Sections 3-6
// (toggle groups, gallery thumbs, quantity stepper, accordion) are dropped —
// they correspond to existing Blazor @onclick / component state (selected
// category chips, product options, gallery thumbnail, quantity stepper,
// accordion open/closed) and are re-expressed there instead, so this module
// never reads or writes application state. See
// docs/plans/public-site-redesign-nf2.md §2.2 and §6 for the full reasoning.
//
// Loaded as a plain <script type="module"> at the end of <body> (see
// Components/App.razor), not via IJSRuntime interop from a component:
//   1. Prerendering — [data-reveal] elements start at opacity:0; a module
//      that only runs once the circuit is live would leave the prerendered
//      first paint blank until the WebSocket connects. A body-tag module
//      runs at DOM-parse time instead.
//   2. Lifetime — these observers live as long as the document, not as long
//      as any one component/circuit, so there is no natural IAsyncDisposable
//      owner.
//   3. Precedent — Components/Layout/ReconnectModal.razor and quill-editor.js
//      already load this way.
//
// The re-binding trap: Blazor Server's router patches the DOM in place on
// internal navigation (no full document reload), and ordinary
// StateHasChanged re-renders do the same. An IntersectionObserver / cached
// element reference populated once at load would never see content that
// arrives afterwards — e.g. navigating cua-hang -> san-pham/{slug} by
// clicking a link, or Shop's filter chips re-rendering the product grid.
// Fix: everything below is re-derived by scan(), which is idempotent (the
// data-nf-bound marker) and is invoked by DOMContentLoaded/immediate call, a
// MutationObserver on document.body (coalesced through one requestAnimationFrame
// so a burst of mutations only re-scans once), and Blazor's own
// "enhancedload" event (the non-interactive Razor Page hops).
//
// Rule for markup authors (documented in the plan, repeated here): never put
// data-reveal or .draw on an element whose class attribute contains a Razor
// expression. Blazor only rewrites the "class" attribute when its *rendered*
// value changes; a static class string keeps the "is-in" class this module
// adds, but a dynamic one wipes it out on the next re-render.
(() => {
    const boundAttr = 'data-nf-bound';

    let reduced = matchMedia('(prefers-reduced-motion: reduce)').matches;

    /* 0. Normalise hand-written line art -----------------------------------
       pathLength="1" lets one CSS rule (nf.css's ".draw path, …") draw any
       shape via stroke-dashoffset, and a per-shape delay stages the strokes.
       NfIcon.razor already emits both directly in markup when Draw="true",
       so this loop is only a fallback for SVG that isn't NfIcon-authored —
       it no-ops (via the pathLength check) on anything already stamped. */
    function normaliseDraw(svg) {
        svg.querySelectorAll('path, circle, line, polyline, rect').forEach((el, i) => {
            if (el.hasAttribute('pathLength')) return;
            el.setAttribute('pathLength', '1');
            el.style.setProperty('--d', Math.min(i * 0.11, 1.2) + 's');
        });
    }

    /* 1. Reveal on scroll ---------------------------------------------------- */
    // Observed element -> the reveal elements it releases. Usually an element
    // stands for itself; a mask reveal is watched through its parent (see watch()).
    const released = new WeakMap();

    const io = ('IntersectionObserver' in window)
        ? new IntersectionObserver((entries) => {
            for (const entry of entries) {
                if (!entry.isIntersecting) continue;
                (released.get(entry.target) || []).forEach(el => el.classList.add('is-in'));
                released.delete(entry.target);
                io.unobserve(entry.target);
            }
        }, { rootMargin: '0px 0px -12% 0px', threshold: 0.12 })
        : null;

    // Chromium measures the intersection after applying the target's own
    // clip-path, and a mask reveal starts fully clipped (inset 0 0 100% 0) — so
    // observed directly it reports zero visible area forever and never reveals.
    // Its parent stands in for it instead.
    function watch(el) {
        const anchor = el.getAttribute('data-reveal') === 'mask' && el.parentElement
            ? el.parentElement
            : el;
        let list = released.get(anchor);
        if (!list) {
            list = [];
            released.set(anchor, list);
            io.observe(anchor);
        }
        list.push(el);
    }

    /* 2. Sticky header + reading progress + journey rail --------------------- */
    let hdr = null;
    let bar = null;
    let journey = null;
    let rail = [];
    let stages = [];
    let pct = null;

    let ticking = false;
    function onScroll() {
        if (ticking) return;
        ticking = true;
        requestAnimationFrame(() => {
            const y = window.scrollY;
            hdr?.classList.toggle('is-stuck', y > 24);

            if (bar) {
                const max = document.documentElement.scrollHeight - innerHeight;
                bar.style.setProperty('--p', max > 0 ? (y / max).toFixed(4) : 0);
            }

            if (journey) {
                // Progress through the tour: 0 when its top reaches mid-viewport,
                // 1 when the last stage is centred.
                const r = journey.getBoundingClientRect();
                const span = r.height - innerHeight * 0.6;
                const p = Math.min(1, Math.max(0, (innerHeight * 0.4 - r.top) / Math.max(span, 1)));
                journey.style.setProperty('--p', p.toFixed(4));
                if (pct) pct.textContent = Math.round(p * 100) + '%';

                // Active stage = the one nearest the upper third of the viewport
                let active = 0;
                stages.forEach((s, i) => {
                    if (s.getBoundingClientRect().top < innerHeight * 0.45) active = i;
                });
                rail.forEach((li, i) => {
                    li.classList.toggle('is-active', i === active);
                    li.classList.toggle('is-done', i < active);
                });
            }
            ticking = false;
        });
    }
    addEventListener('scroll', onScroll, { passive: true });
    addEventListener('resize', onScroll);

    /* Bind every not-yet-bound [data-reveal]/.draw element, and refresh the
       cached scroll targets above — both go stale the instant Blazor's
       router (or a StateHasChanged re-render) patches the DOM, since neither
       fires a real "load" event. Idempotent via the data-nf-bound marker. */
    function scan() {
        document.querySelectorAll(`[data-reveal]:not([${boundAttr}]), .draw:not([${boundAttr}])`).forEach(el => {
            el.setAttribute(boundAttr, '');

            if (el.classList.contains('draw')) normaliseDraw(el);

            if (reduced || !io) {
                el.classList.add('is-in');
                return;
            }

            // Anything already on screen when it is bound is revealed straight
            // away instead of waiting for the observer's first callback. Under
            // InteractiveServer the prerendered DOM is patched during hydration,
            // so elements can be (re)bound after the observer has already
            // reported on the nodes they replaced — above-the-fold content then
            // never gets a callback at all and stays at opacity:0 forever. The
            // --d delays still stage the entrance.
            const box = el.getBoundingClientRect();
            if (box.top < innerHeight && box.bottom > 0) {
                el.classList.add('is-in');
                return;
            }
            watch(el);
        });

        // CheckoutLayout and /admin render no .hdr/.progressbar, so landing there
        // first and then navigating into PublicLayout must re-find them.
        hdr = document.querySelector('.hdr');
        bar = document.querySelector('.progressbar');
        journey = document.querySelector('.journey');
        rail = document.querySelectorAll('.rail__list li');
        stages = document.querySelectorAll('.stage');
        pct = document.querySelector('.rail__pct');

        onScroll();
    }

    /* Reduced-motion can change live (OS setting flipped mid-session): make
       everything already bound visible and stop animating immediately. */
    matchMedia('(prefers-reduced-motion: reduce)').addEventListener('change', (e) => {
        reduced = e.matches;
        if (!reduced) return;
        document.querySelectorAll(`[${boundAttr}]`).forEach(el => el.classList.add('is-in'));
    });

    let scanQueued = false;
    function queueScan() {
        if (scanQueued) return;
        scanQueued = true;
        requestAnimationFrame(() => {
            scanQueued = false;
            scan();
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => scan(), { once: true });
    } else {
        scan();
    }

    // Blazor Server router navigation + ordinary StateHasChanged re-renders.
    new MutationObserver(queueScan).observe(document.body, { childList: true, subtree: true });

    // Non-interactive Razor Page hops (/pages/tai-khoan/dang-nhap, …). Guarded:
    // Blazor's script may not have registered this event on every page.
    window.Blazor?.addEventListener?.('enhancedload', () => scan());
})();
