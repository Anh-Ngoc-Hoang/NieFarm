// Generates src/NieFarm.Web/Content/VietnamMapGeometry.g.cs — the three coffee-region outlines,
// label anchors and archipelago points behind the home page's "Vietnam Coffee Stories" map.
//
//   npm ci && npm run build                         (downloads the pinned source once)
//   node build-map.mjs --source <path.geojson>      (use a local copy of the same file)
//   node build-map.mjs --source <path> --allow-unpinned   (a different dataset, e.g. Natural Earth)
//
// A one-off developer tool; nothing in `dotnet build` runs it. See README.md.

import { createHash } from 'node:crypto';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import mapshaper from 'mapshaper';
import { PROVINCES, REGIONS } from './regions.mjs';
import { ARCHIPELAGOS, SEA_LABEL } from './islands.mjs';

const here = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(here, '..', '..');

const SOURCE_URL =
  'https://github.com/wmgeolab/geoBoundaries/raw/9469f09/releaseData/gbOpen/VNM/ADM1/geoBoundaries-VNM-ADM1.geojson';
const SOURCE_SHA256 = '25dbc2fec9862016710118fc98042d3e803830c72361c91c4e864ae668fa9541';
const CACHE_PATH = join(here, 'data', 'geoBoundaries-VNM-ADM1.geojson');
const OUT_CS = join(repoRoot, 'src', 'NieFarm.Web', 'Content', 'VietnamMapGeometry.g.cs');
const OUT_PREVIEW = join(here, 'out', 'preview.svg');

// Projection and frame (plan D5): equirectangular, cos(lat) correction at 16N.
const LON_MIN = 102.0;
const LON_MAX = 115.3;
// 7.0, not the plan's 7.4: the dashed ellipse round Trường Sa reaches past 7.4N below An Bang (7.87N).
const LAT_MIN = 7.0;
const LAT_MAX = 23.5;
const WIDTH = 600;
const K = Math.cos((16 * Math.PI) / 180);
const S = WIDTH / ((LON_MAX - LON_MIN) * K);
const HEIGHT = Math.round((LAT_MAX - LAT_MIN) * S);

const WARN_BYTES = 15 * 1024;
const FAIL_BYTES = 30 * 1024;

const fail = (msg) => {
  console.error(`build-map: ${msg}`);
  process.exit(1);
};

// ---- args ----
const args = process.argv.slice(2);
const argValue = (name) => {
  const i = args.indexOf(name);
  return i >= 0 ? args[i + 1] : undefined;
};
const sourceArg = argValue('--source');
const allowUnpinned = args.includes('--allow-unpinned');

// ---- 1. source ----
async function loadSource() {
  let path = sourceArg ? resolve(sourceArg) : CACHE_PATH;
  if (!sourceArg && !existsSync(CACHE_PATH)) {
    console.log(`Downloading ${SOURCE_URL}`);
    const res = await fetch(SOURCE_URL);
    if (!res.ok) fail(`download failed: HTTP ${res.status}`);
    mkdirSync(dirname(CACHE_PATH), { recursive: true });
    writeFileSync(CACHE_PATH, Buffer.from(await res.arrayBuffer()));
  }
  if (!existsSync(path)) fail(`source not found: ${path}`);

  const bytes = readFileSync(path);
  const sha = createHash('sha256').update(bytes).digest('hex');
  if (sha !== SOURCE_SHA256) {
    if (!allowUnpinned) fail(`SHA-256 mismatch for ${path}\n  expected ${SOURCE_SHA256}\n  actual   ${sha}`);
    console.warn(`WARNING: unpinned source ${path} (sha256 ${sha}); re-check its ISO codes against regions.mjs.`);
  }
  console.log(`Source ${path} (${bytes.length} bytes, sha256 ${sha})`);
  return JSON.parse(bytes.toString('utf8'));
}

// ---- 2. assign regions by ISO code ----
function annotate(geo) {
  const byIso = new Map(PROVINCES.map((p) => [p.iso, p]));
  if (byIso.size !== PROVINCES.length) fail('regions.mjs has a duplicate ISO code');
  for (const p of PROVINCES) if (!REGIONS.includes(p.region)) fail(`unknown region ${p.region} for ${p.iso}`);

  const used = new Set();
  const unassigned = [];
  for (const f of geo.features) {
    const iso = (f.properties.shapeISO ?? '').trim();
    const row = byIso.get(iso);
    if (!row) {
      unassigned.push(`${iso || '(no ISO)'} ${String(f.properties.shapeName).trim()}`);
      continue;
    }
    used.add(iso);
    f.properties = { region: row.region };
  }
  if (unassigned.length) fail(`unassigned source features:\n  ${unassigned.join('\n  ')}`);
  const unused = PROVINCES.filter((p) => !used.has(p.iso)).map((p) => `${p.iso} ${p.name}`);
  if (unused.length) fail(`regions.mjs rows with no source feature:\n  ${unused.join('\n  ')}`);
  console.log(`Assigned ${geo.features.length} features from ${PROVINCES.length} provinces`);
  return geo;
}

const readOut = (out, name) => JSON.parse(Buffer.isBuffer(out[name]) ? out[name].toString('utf8') : out[name]);

// ---- 3-5. dissolve, simplify, label anchors ----
async function buildRegions(annotated) {
  const out = await mapshaper.applyCommands(
    '-i in.json -explode -each "cx=this.centroidX" -filter "cx < 110" ' +
      '-dissolve region -filter-islands min-area=20km2 -simplify 10% keep-shapes ' +
      '-o out.json format=geojson precision=0.0001',
    { 'in.json': JSON.stringify(annotated) },
  );
  const regions = readOut(out, 'out.json');

  const names = regions.features.map((f) => f.properties.region).sort();
  if (regions.features.length !== 3 || names.join(',') !== 'central,north,south')
    fail(`expected exactly north/central/south, got [${names.join(', ')}]`);

  const south = regions.features.find((f) => f.properties.region === 'south');
  const southParts = south.geometry.type === 'MultiPolygon' ? south.geometry.coordinates.length : 1;
  if (southParts < 3) fail(`south has ${southParts} parts; expected mainland + Phú Quốc + Côn Đảo`);

  const anchorsOut = await mapshaper.applyCommands(
    '-i regions.json -dissolve region -points inner -o anchors.json format=geojson precision=0.0001',
    { 'regions.json': JSON.stringify(regions) },
  );
  const anchors = readOut(anchorsOut, 'anchors.json');
  const anchorByRegion = Object.fromEntries(
    anchors.features.map((f) => [f.properties.region, f.geometry.coordinates]),
  );
  for (const r of REGIONS) if (!anchorByRegion[r]) fail(`no label anchor for ${r}`);

  return { regions, anchorByRegion };
}

// ---- 6. project ----
const project = ([lon, lat]) => [(lon - LON_MIN) * K * S, (LAT_MAX - lat) * S];
const r1 = (v) => Number(v.toFixed(1));
const fmt = (v) => String(r1(v));

function ringToPath(ring) {
  const pts = [];
  // GeoJSON rings repeat the first point at the end; Z closes it instead.
  for (const c of ring.slice(0, -1)) {
    const [x, y] = project(c).map(r1);
    const prev = pts[pts.length - 1];
    if (!prev || prev[0] !== x || prev[1] !== y) pts.push([x, y]);
  }
  if (pts.length < 3) return '';
  return 'M' + pts.map(([x, y]) => `${fmt(x)} ${fmt(y)}`).join('L') + 'Z';
}

function geometryToPath(geometry) {
  const polys = geometry.type === 'MultiPolygon' ? geometry.coordinates : [geometry.coordinates];
  return polys.map((poly) => poly.map(ringToPath).join('')).join('');
}

const point = (lon, lat) => project([lon, lat]).map(r1);

// ---- 7. write ----
const csDouble = (v) => (Number.isInteger(v) ? `${v}.0` : String(v));
const csPoint = ([x, y]) => `new(${csDouble(x)}, ${csDouble(y)})`;
const csPoints = (pts) => `[\n        ${pts.map(csPoint).join(',\n        ')},\n    ]`;

const csEllipse = (e) =>
  `new(${csDouble(e.cx)}, ${csDouble(e.cy)}, ${csDouble(e.rx)}, ${csDouble(e.ry)}, ${csDouble(e.angle)})`;

function writeCs({ paths, labels, hoangSa, truongSa, areas }) {
  const cs = `// <auto-generated/>
// Generated by tools/map/build-map.mjs — do not edit; run tools/map (npm ci && npm run build).
//
// Source: geoBoundaries gbOpen VNM ADM1, commit 9469f09
//   ${SOURCE_URL}
//   sha256 ${SOURCE_SHA256}
// Licence: Public Domain (geoBoundaries, derived from Wikimedia Commons).
// Citation: Runfola, D. et al. (2020) geoBoundaries: A global database of political
//   administrative boundaries. PLOS ONE 15(4): e0231866.
//
// Pre-2025 provinces merged into three coffee regions by ISO code (tools/map/regions.mjs);
// Hoàng Sa / Trường Sa points are curated and approximate (tools/map/islands.mjs).
// Projection: equirectangular, cos(16°) correction; lon ${LON_MIN}–${LON_MAX}, lat ${LAT_MIN}–${LAT_MAX};
// absolute viewBox coordinates rounded to 0.1.

namespace NieFarm.Web.Content;

public static class VietnamMapGeometry
{
    public const int Width = ${WIDTH};
    public const int Height = ${HEIGHT};

    public const string NorthPath = "${paths.north}";

    public const string CentralPath = "${paths.central}";

    public const string SouthPath = "${paths.south}";

    /// <summary>A point inside each region (mapshaper "inner" point), in viewBox units.</summary>
    public static readonly MapPoint NorthLabel = ${csPoint(labels.north)};
    public static readonly MapPoint CentralLabel = ${csPoint(labels.central)};
    public static readonly MapPoint SouthLabel = ${csPoint(labels.south)};

    /// <summary>Archipelago label positions and the "Biển Đông" sea label, in viewBox units.</summary>
    public static readonly MapPoint HoangSaLabel = ${csPoint(labels.hoangSa)};
    public static readonly MapPoint TruongSaLabel = ${csPoint(labels.truongSa)};
    public static readonly MapPoint SeaLabel = ${csPoint(labels.sea)};

    public static readonly MapPoint[] HoangSaIslands = ${csPoints(hoangSa)};

    public static readonly MapPoint[] TruongSaIslands = ${csPoints(truongSa)};

    /// <summary>The dashed hairline ellipse drawn around each archipelago's dots.</summary>
    public static readonly MapEllipse HoangSaArea = ${csEllipse(areas.hoangSa)};
    public static readonly MapEllipse TruongSaArea = ${csEllipse(areas.truongSa)};
}

public readonly record struct MapPoint(double X, double Y);

/// <summary>An ellipse in viewBox units, rotated by <see cref="Angle"/> degrees about its centre.</summary>
public readonly record struct MapEllipse(double Cx, double Cy, double Rx, double Ry, double Angle);
`;
  writeFileSync(OUT_CS, cs, 'utf8');
}

function writePreview({ paths, labels, hoangSa, truongSa, areas }) {
  const colour = { north: '#2C4A34', central: '#D4A359', south: '#6B1313' };
  const dots = [...hoangSa, ...truongSa].map(([x, y]) => `<circle cx="${x}" cy="${y}" r="2.4" fill="#555"/>`);
  for (const e of Object.values(areas))
    dots.push(`<ellipse cx="${e.cx}" cy="${e.cy}" rx="${e.rx}" ry="${e.ry}" transform="rotate(${e.angle} ${e.cx} ${e.cy})" fill="none" stroke="#555" stroke-dasharray="3 3"/>`);
  const marks = Object.entries(labels).map(
    ([k, [x, y]]) => `<circle cx="${x}" cy="${y}" r="3" fill="#000"/><text x="${x + 5}" y="${y + 4}" font-size="12">${k}</text>`,
  );
  const svg = [
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${WIDTH} ${HEIGHT}" width="${WIDTH}" height="${HEIGHT}" style="background:#FBFAF7">`,
    ...REGIONS.map(
      (r) => `<path d="${paths[r]}" fill="${colour[r]}" fill-opacity=".15" stroke="#14110F" stroke-width="1" stroke-linejoin="round"/>`,
    ),
    ...dots,
    ...marks,
    '</svg>',
  ].join('\n');
  mkdirSync(dirname(OUT_PREVIEW), { recursive: true });
  writeFileSync(OUT_PREVIEW, svg, 'utf8');
}

// ---- main ----
const geo = annotate(await loadSource());
const { regions, anchorByRegion } = await buildRegions(geo);

const paths = Object.fromEntries(
  regions.features.map((f) => [f.properties.region, geometryToPath(f.geometry)]),
);
const labels = {
  north: project(anchorByRegion.north).map(r1),
  central: project(anchorByRegion.central).map(r1),
  south: project(anchorByRegion.south).map(r1),
  hoangSa: point(ARCHIPELAGOS.hoangSa.labelAt.lon, ARCHIPELAGOS.hoangSa.labelAt.lat),
  truongSa: point(ARCHIPELAGOS.truongSa.labelAt.lon, ARCHIPELAGOS.truongSa.labelAt.lat),
  sea: point(SEA_LABEL.lon, SEA_LABEL.lat),
};
const hoangSa = ARCHIPELAGOS.hoangSa.islands.map((i) => point(i.lon, i.lat));
const truongSa = ARCHIPELAGOS.truongSa.islands.map((i) => point(i.lon, i.lat));

// The dashed hairline around each archipelago: the minimum-area ellipse enclosing its dots
// (Khachiyan's algorithm), grown by a margin in viewBox units. The Trường Sa points run SW-NE,
// so an axis-aligned or bounding-box ellipse would be far too big and spill off the frame.
function enclosingEllipse(pts, margin = 9) {
  const n = pts.length;
  let u = new Array(n).fill(1 / n);
  const inv3 = (m) => {
    const [a, b, c, d, e, f, g, h, i] = m.flat();
    const A = e * i - f * h, B = -(d * i - f * g), C = d * h - e * g;
    const det = a * A + b * B + c * C;
    return [
      [A / det, -(b * i - c * h) / det, (b * f - c * e) / det],
      [B / det, (a * i - c * g) / det, -(a * f - c * d) / det],
      [C / det, -(a * h - b * g) / det, (a * e - b * d) / det],
    ];
  };
  for (let iter = 0; iter < 20000; iter++) {
    const X = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
    pts.forEach(([x, y], k) => {
      const q = [x, y, 1];
      for (let r = 0; r < 3; r++) for (let c = 0; c < 3; c++) X[r][c] += u[k] * q[r] * q[c];
    });
    const Xi = inv3(X);
    const M = pts.map(([x, y]) => {
      const q = [x, y, 1];
      let s = 0;
      for (let r = 0; r < 3; r++) for (let c = 0; c < 3; c++) s += q[r] * Xi[r][c] * q[c];
      return s;
    });
    const j = M.indexOf(Math.max(...M));
    const step = (M[j] - 3) / (3 * (M[j] - 1));
    const next = u.map((v, k) => (1 - step) * v + (k === j ? step : 0));
    const change = Math.hypot(...next.map((v, k) => v - u[k]));
    u = next;
    if (change < 1e-9) break;
  }
  const cx = pts.reduce((a, [x], k) => a + u[k] * x, 0);
  const cy = pts.reduce((a, [, y], k) => a + u[k] * y, 0);
  let sxx = 0, syy = 0, sxy = 0;
  pts.forEach(([x, y], k) => {
    sxx += u[k] * x * x;
    syy += u[k] * y * y;
    sxy += u[k] * x * y;
  });
  sxx -= cx * cx;
  syy -= cy * cy;
  sxy -= cx * cy;
  // Shape matrix A = inv(cov) / 2; its eigenvalues give the semi-axes as 1/sqrt(lambda), i.e.
  // sqrt(2 * eigenvalue(cov)).
  const tr = sxx + syy, det = sxx * syy - sxy * sxy;
  const disc = Math.sqrt(Math.max(0, (tr * tr) / 4 - det));
  const l1 = tr / 2 + disc, l2 = tr / 2 - disc;
  const angle = Math.abs(sxy) < 1e-12 ? (sxx >= syy ? 0 : Math.PI / 2) : Math.atan2(l1 - sxx, sxy);
  return {
    cx: r1(cx),
    cy: r1(cy),
    rx: r1(Math.sqrt(2 * l1) + margin),
    ry: r1(Math.sqrt(2 * Math.max(l2, 0)) + margin),
    angle: r1((angle * 180) / Math.PI),
  };
}
const areas = { hoangSa: enclosingEllipse(hoangSa), truongSa: enclosingEllipse(truongSa) };
for (const [name, e] of Object.entries(areas)) {
  const t = (e.angle * Math.PI) / 180;
  const hx = Math.hypot(e.rx * Math.cos(t), e.ry * Math.sin(t));
  const hy = Math.hypot(e.rx * Math.sin(t), e.ry * Math.cos(t));
  if (e.cx - hx < 0 || e.cx + hx > WIDTH || e.cy - hy < 0 || e.cy + hy > HEIGHT)
    fail(`the ${name} ellipse runs off the frame (${JSON.stringify(e)})`);
  console.log(`  ${name} ellipse x ${r1(e.cx - hx)}–${r1(e.cx + hx)}, y ${r1(e.cy - hy)}–${r1(e.cy + hy)}`);
}

for (const [name, [x, y]] of Object.entries(labels))
  if (x < 0 || x > WIDTH || y < 0 || y > HEIGHT) fail(`label ${name} (${x}, ${y}) is outside the frame`);
for (const [x, y] of [...hoangSa, ...truongSa])
  if (x < 0 || x > WIDTH || y < 0 || y > HEIGHT) fail(`island (${x}, ${y}) is outside the frame`);

const geometry = { paths, labels, hoangSa, truongSa, areas };
writeCs(geometry);
writePreview(geometry);

// ---- 8. budget ----
let total = 0;
for (const r of REGIONS) {
  const bytes = Buffer.byteLength(paths[r], 'utf8');
  total += bytes;
  console.log(`  ${r.padEnd(8)} ${bytes.toLocaleString('en-US').padStart(7)} bytes`);
}
console.log(`  ${'total'.padEnd(8)} ${total.toLocaleString('en-US').padStart(7)} bytes  (viewBox 0 0 ${WIDTH} ${HEIGHT})`);
console.log(`Wrote ${OUT_CS}\nWrote ${OUT_PREVIEW}`);
if (total > FAIL_BYTES) fail(`path text ${total} bytes exceeds the ${FAIL_BYTES}-byte limit`);
if (total > WARN_BYTES) console.warn(`WARNING: path text ${total} bytes is above the ${WARN_BYTES}-byte budget`);
