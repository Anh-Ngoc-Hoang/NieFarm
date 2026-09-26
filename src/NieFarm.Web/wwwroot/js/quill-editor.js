// Quill rich-text interop for the admin forms.
//
// Quill is pulled from the CDN on first use rather than from App.razor, because App.razor is the
// single host page for the public storefront as well — loading an editor only admins open would
// cost every customer page. ensureQuill() caches its promise, so two editors on one page share a
// single injected <script>.

window.quillEditors = {};

const QUILL_VERSION = '2.0.3';
const QUILL_CSS = `https://cdn.jsdelivr.net/npm/quill@${QUILL_VERSION}/dist/quill.snow.css`;
const QUILL_JS = `https://cdn.jsdelivr.net/npm/quill@${QUILL_VERSION}/dist/quill.js`;

let quillLoader = null;

function ensureQuill() {
    if (window.Quill) return Promise.resolve();
    if (quillLoader) return quillLoader;

    quillLoader = new Promise((resolve, reject) => {
        if (!document.querySelector(`link[data-quill]`)) {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = QUILL_CSS;
            link.setAttribute('data-quill', '');
            document.head.appendChild(link);
        }

        const existing = document.querySelector(`script[data-quill]`);
        if (existing) {
            existing.addEventListener('load', () => resolve());
            existing.addEventListener('error', () => reject(new Error('Quill failed to load')));
            return;
        }

        const script = document.createElement('script');
        script.src = QUILL_JS;
        script.setAttribute('data-quill', '');
        script.onload = () => resolve();
        script.onerror = () => reject(new Error('Quill failed to load'));
        document.head.appendChild(script);
    });

    // A failed load must not be cached, so a later open can retry.
    quillLoader.catch(() => { quillLoader = null; });

    return quillLoader;
}

const TOOLBARS = {
    full: [
        [{ header: [2, 3, false] }],
        ['bold', 'italic', 'underline', 'strike'],
        [{ color: [] }, { background: [] }],
        [{ list: 'ordered' }, { list: 'bullet' }],
        ['blockquote'],
        ['link'],
        ['clean']
    ],
    minimal: [
        ['bold', 'italic'],
        ['link'],
        ['clean']
    ]
};

// Returns true when the editor is live, false when Quill could not be loaded — the component
// falls back to a plain textarea on false rather than leaving the admin unable to type.
window.quillInit = async function (editorId, dotnetRef, initialHtml, toolbar, placeholder) {
    const container = document.getElementById(editorId);
    if (!container) return false;

    try {
        await ensureQuill();
    } catch {
        return false;
    }

    // The element may have been torn down while the CDN request was in flight.
    if (!document.getElementById(editorId)) return false;

    const quill = new Quill(container, {
        theme: 'snow',
        placeholder: placeholder || '',
        modules: { toolbar: TOOLBARS[toolbar] || TOOLBARS.full }
    });

    if (initialHtml) {
        quill.clipboard.dangerouslyPasteHTML(initialHtml, 'silent');
    }

    quill.on('text-change', function () {
        // Quill leaves "<p><br></p>" behind when the user clears the box; report it as empty so
        // an untouched field does not count as content.
        const html = quill.getText().trim().length === 0 ? '' : quill.root.innerHTML;
        dotnetRef.invokeMethodAsync('OnContentChanged', html);
    });

    window.quillEditors[editorId] = quill;
    return true;
};

window.quillDispose = function (editorId) {
    const quill = window.quillEditors[editorId];
    if (quill) {
        // Drop the toolbar Quill injected as a sibling; Blazor never rendered it and so will
        // not clean it up itself.
        const toolbar = quill.getModule('toolbar');
        if (toolbar && toolbar.container && toolbar.container.parentNode) {
            toolbar.container.parentNode.removeChild(toolbar.container);
        }
    }
    delete window.quillEditors[editorId];
};
