# tools/map — the "Vietnam Coffee Stories" map generator

A one-off developer tool that produces
`src/NieFarm.Web/Content/VietnamMapGeometry.g.cs`: the three coffee-region outlines (North,
Central, South), their label anchors, and the Hoàng Sa / Trường Sa points shown in the home
page's "Vietnam Coffee Stories" section. Only that generated C# file is used at runtime; nothing
here runs as part of `dotnet build`, and mapshaper is never shipped.

Plan and reasoning: `docs/plans/home-vietnam-coffee-stories.md` (D1–D6).

## Regenerate

```
cd tools/map
npm ci
npm run build
```

The first run downloads the pinned source into `data/` (gitignored) and checks its SHA-256. The
build prints the path size per region (budget: warn above 15 KB total, fail above 30 KB) and
writes `out/preview.svg` (gitignored) for a quick visual check: Tây Nguyên must sit in South, and
the internal borders must have no slivers or gaps.

Without network access, supply the same file locally:

```
node build-map.mjs --source path/to/geoBoundaries-VNM-ADM1.geojson
```

Never hand-edit `VietnamMapGeometry.g.cs`; change the inputs here and rebuild.

## Source and licence

- **Dataset:** geoBoundaries gbOpen, VNM ADM1, pinned to commit `9469f09`
  - URL: `https://github.com/wmgeolab/geoBoundaries/raw/9469f09/releaseData/gbOpen/VNM/ADM1/geoBoundaries-VNM-ADM1.geojson`
  - Size: 723,311 bytes
  - SHA-256: `25dbc2fec9862016710118fc98042d3e803830c72361c91c4e864ae668fa9541`
  - 64 features: the 63 pre-2025 provinces, with Côn Đảo as a second VN-43 feature.
- **Licence:** Public Domain (geoBoundaries metadata `boundaryLicense`, source Wikimedia Commons).
  No on-page attribution is required; it is credited here, in the generated file header and in
  CLAUDE.md anyway.
- **Citation:** Runfola, D. et al. (2020) *geoBoundaries: A global database of political
  administrative boundaries.* PLOS ONE 15(4): e0231866.
- **GADM is deliberately not used:** its licence forbids commercial redistribution, and this is an
  e-commerce site.
- **Tooling:** mapshaper `0.6.121`, pinned exactly (MPL-2.0, build-time only).

### Fallback source

If GitHub is unreachable, Natural Earth 10m admin-1 (`ne_10m_admin_1_states_provinces`, also
public domain, 63 provinces) can be used:

```
node build-map.mjs --source path/to/ne_vnm.geojson --allow-unpinned
```

Its ISO fields must be re-checked against `regions.mjs` first. The build fails on any unassigned
feature or unused table row, so a mismatch cannot pass silently.

## Why the pre-2025 provinces (plan D2)

The 1 July 2025 merge (63 → 34 provinces) does not change the national outline, only the internal
lines, but several merged units run from the plateau down to the coast: Kon Tum joined Quảng Ngãi,
Bình Định joined Gia Lai, Phú Yên joined Đắk Lắk, and Đắk Nông and Bình Thuận joined Lâm Đồng.
Putting Tây Nguyên in the South with the 34-province set would drag the central coast (Quảng
Ngãi, Quy Nhơn, Tuy Hòa, Phan Thiết) into the South as well. The coffee regions are
agro-ecological, not administrative, and the older units follow the plateau edge. On-page area
notes use the current names where they differ, for example "Đắk Nông (nay thuộc Lâm Đồng)".

## Province to region (plan D3)

`regions.mjs` has one row per pre-2025 province, keyed by ISO 3166-2 code (never by name: the
source's "Hà Nội" has a trailing tab), with its 2025 successor for documentation:

- **North (25):** the provinces north of Thanh Hóa.
- **Central (14):** the coastal strip, Thanh Hóa to Bình Thuận.
- **South (24):** the five Tây Nguyên provinces, the south-east, and the Mekong delta.

## Pipeline

1. Assign `region` to each feature by `shapeISO`, asserting full coverage both ways.
2. mapshaper: `-explode`, then drop parts whose centroid is east of 110°E (the source's incomplete
   islet polygons). Then `-dissolve region -filter-islands min-area=20km2`, then
   `-simplify 10% keep-shapes`. Simplification is topology-aware, so shared region borders stay
   seamless.
3. Assert exactly three regions, and that South keeps at least 3 parts (mainland, Phú Quốc, Côn
   Đảo).
4. `-points inner` gives one label anchor per region.
5. Project: equirectangular with a cos(16°) correction, lon 102.0–115.3, lat 7.0–23.5, width 600
   (height 774). Absolute coordinates are rounded to 0.1. The plan's southern edge was 7.4°N; it
   was moved to 7.0°N because the dashed ellipse round Trường Sa reaches below 7.4°N under An Bang
   (7.87°N).

## Archipelagos (plan D6)

The source's islet polygons are sub-pixel at this scale, and the Trường Sa set is incomplete
(Trường Sa Lớn is missing). `islands.mjs` instead lists approximate, decorative positions of
named features, drawn as dots at their true positions in the same projection, with no inset box.
Each group is enclosed in a dashed ellipse: the minimum-area enclosing ellipse
(Khachiyan's algorithm) plus a margin. `islands.mjs` also holds the curated label positions for
"Quần đảo Hoàng Sa", "Quần đảo Trường Sa" and "Biển Đông".

The small near-shore islands the 20 km² filter removes (Bạch Long Vĩ, Lý Sơn, Cồn Cỏ) are
deliberately not added back.
