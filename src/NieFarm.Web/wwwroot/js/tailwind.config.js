// Design tokens exported from the Stitch project "NieFarm" (id 3649571720089026821).
// Loaded after the Tailwind CDN script in App.razor.
tailwind.config = {
    darkMode: "class",
    theme: {
        extend: {
            colors: {
                // Brand palette used across the screens
                "deep-red": "#6B1313",
                "amber-gold": "#D4A359",
                "forest-green": "#2C4A34",
                "cream-ivory": "#F9F6F0",
                "cream-shade": "#EFECE5",

                // Material tokens carried over from the Stitch theme
                "background": "#f9f9f9",
                "on-background": "#1a1c1c",
                "surface": "#f9f9f9",
                "surface-bright": "#f9f9f9",
                "surface-dim": "#dadada",
                "surface-variant": "#e2e2e2",
                "surface-container-lowest": "#ffffff",
                "surface-container-low": "#f3f3f4",
                "surface-container": "#eeeeee",
                "surface-container-high": "#e8e8e8",
                "surface-container-highest": "#e2e2e2",
                "on-surface": "#1a1c1c",
                "on-surface-variant": "#554337",
                "inverse-surface": "#2f3131",
                "inverse-on-surface": "#f0f1f1",
                "outline": "#887365",
                "outline-variant": "#dbc2b2",
                "primary": "#934b00",
                "on-primary": "#ffffff",
                "primary-container": "#da7b26",
                "on-primary-container": "#482200",
                "inverse-primary": "#ffb781",
                "primary-fixed": "#ffdcc5",
                "primary-fixed-dim": "#ffb781",
                "secondary": "#5f5e5e",
                "on-secondary": "#ffffff",
                "secondary-container": "#e4e2e1",
                "on-secondary-container": "#656464",
                "tertiary": "#6a5c49",
                "on-tertiary": "#ffffff",
                "tertiary-container": "#a2917b",
                "on-tertiary-container": "#352a1a",
                "error": "#ba1a1a",
                "on-error": "#ffffff",
                "error-container": "#ffdad6",
                "on-error-container": "#93000a",
                "slate-gray": "#5E5E5E",
                "soft-border": "#E5E7EB",
                "success-forest": "#15803D",
                "alert-rust": "#B45309",
                "system-slate": "#475569"
            },
            // The screens lean on a 250ms rhythm, which is not in Tailwind's default scale.
            transitionDuration: {
                250: "250ms"
            },
            borderRadius: {
                DEFAULT: "0.25rem",
                lg: "0.5rem",
                xl: "0.75rem",
                full: "9999px"
            },
            spacing: {
                gutter: "1.5rem",
                "section-padding-y": "120px",
                "card-padding": "2rem",
                "element-gap": "1rem",
                "container-max": "1320px"
            },
            maxWidth: {
                "container-max": "1320px"
            },
            fontFamily: {
                "headline-lg": ["Inter", "sans-serif"],
                "headline-lg-mobile": ["Inter", "sans-serif"],
                "headline-md": ["Inter", "sans-serif"],
                "headline-sm": ["Inter", "sans-serif"],
                "body-lg": ["Inter", "sans-serif"],
                "body-sm": ["Inter", "sans-serif"],
                "label-bold": ["Inter", "sans-serif"]
            },
            fontSize: {
                "headline-lg": ["60px", { lineHeight: "1.15", letterSpacing: "-0.02em", fontWeight: "700" }],
                "headline-lg-mobile": ["40px", { lineHeight: "1.2", fontWeight: "700" }],
                "headline-md": ["40px", { lineHeight: "1.25", fontWeight: "600" }],
                "headline-sm": ["24px", { lineHeight: "1.35", fontWeight: "500" }],
                "body-lg": ["18px", { lineHeight: "1.6", fontWeight: "400" }],
                "body-sm": ["14px", { lineHeight: "1.5", fontWeight: "400" }],
                "label-bold": ["14px", { lineHeight: "1", fontWeight: "600" }]
            }
        }
    }
};
