// Hoàng Sa and Trường Sa for the home page's coffee-regions map.
//
// Approximate, decorative. Positions are rounded to 0.01 degrees and are only good enough to
// place a dot on a 600px-wide map; this is not survey data.
//
// Why a curated list instead of the source polygons (plan D6): geoBoundaries carries the
// islets as polygons inside Đà Nẵng (Hoàng Sa) and Khánh Hòa (Trường Sa), but they are
// sub-pixel at this scale and the Trường Sa set is incomplete (Trường Sa Lớn, 8.64N 111.92E,
// is missing). build-map.mjs drops every source part east of 110E and draws these instead.
//
// The small near-shore islands the 20 km2 filter removes (Bạch Long Vĩ, Lý Sơn, Cồn Cỏ) are
// deliberately not added back (plan section 10, resolution 4).

export const ARCHIPELAGOS = {
  hoangSa: {
    label: 'Quần đảo Hoàng Sa',
    // Where the HTML label is centred, chosen by eye to clear the dots, the dashed ellipse
    // and the offshore "Miền Trung" label at every width.
    labelAt: { lat: 17.85, lon: 112.0 },
    islands: [
      { name: 'Hoàng Sa', lat: 16.53, lon: 111.61 },
      { name: 'Phú Lâm', lat: 16.83, lon: 112.34 },
      { name: 'Linh Côn', lat: 16.67, lon: 112.73 },
      { name: 'Tri Tôn', lat: 15.78, lon: 111.2 },
      { name: 'Quang Hòa', lat: 16.45, lon: 111.71 },
      { name: 'Đảo Cây', lat: 16.98, lon: 112.26 },
    ],
  },
  truongSa: {
    label: 'Quần đảo Trường Sa',
    labelAt: { lat: 12.35, lon: 113.2 },
    islands: [
      { name: 'Trường Sa Lớn', lat: 8.64, lon: 111.92 },
      { name: 'Song Tử Tây', lat: 11.43, lon: 114.33 },
      { name: 'Nam Yết', lat: 10.18, lon: 114.36 },
      { name: 'Sinh Tồn', lat: 9.88, lon: 114.33 },
      { name: 'Sơn Ca', lat: 10.38, lon: 114.48 },
      { name: 'An Bang', lat: 7.87, lon: 112.92 },
    ],
  },
};

// The italic "Biển Đông" sea label (plan section 10, resolution 3): open water between the
// mainland coast and the two archipelagos.
export const SEA_LABEL = { lat: 13.4, lon: 112.4 };
