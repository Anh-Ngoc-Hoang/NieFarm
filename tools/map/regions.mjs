// Province -> coffee-region table for the home page's "Vietnam Coffee Stories" map.
//
// One row per pre-2025 province (63 rows), keyed by its ISO 3166-2 code, which is what
// geoBoundaries carries in `shapeISO`. Match on the code, never the name: the source's
// "Hà Nội" has a trailing tab, and "Ho Chi Minh" is unaccented.
//
// Why the pre-2025 units (plan D2): the 1 July 2025 merge (63 -> 34 provinces) joins several
// Tây Nguyên provinces to coastal ones (Kon Tum -> Quảng Ngãi, Bình Định -> Gia Lai,
// Phú Yên -> Đắk Lắk, Đắk Nông + Bình Thuận -> Lâm Đồng). Putting Tây Nguyên in the South with
// the 34-province set would drag the central coast into the South too. The coffee regions are
// agro-ecological, not administrative, and the older units follow the plateau edge. The
// national outline is identical either way.
//
// `successor2025` is documentation only; the build does not read it.
//
// build-map.mjs fails if any source feature is unassigned or any row here is unused.

export const REGIONS = ['north', 'central', 'south'];

export const PROVINCES = [
  // ---- North (25) ----
  { iso: 'VN-01', name: 'Lai Châu', region: 'north', successor2025: 'Lai Châu' },
  { iso: 'VN-02', name: 'Lào Cai', region: 'north', successor2025: 'Lào Cai' },
  { iso: 'VN-03', name: 'Hà Giang', region: 'north', successor2025: 'Tuyên Quang' },
  { iso: 'VN-04', name: 'Cao Bằng', region: 'north', successor2025: 'Cao Bằng' },
  { iso: 'VN-05', name: 'Sơn La', region: 'north', successor2025: 'Sơn La' },
  { iso: 'VN-06', name: 'Yên Bái', region: 'north', successor2025: 'Lào Cai' },
  { iso: 'VN-07', name: 'Tuyên Quang', region: 'north', successor2025: 'Tuyên Quang' },
  { iso: 'VN-09', name: 'Lạng Sơn', region: 'north', successor2025: 'Lạng Sơn' },
  { iso: 'VN-13', name: 'Quảng Ninh', region: 'north', successor2025: 'Quảng Ninh' },
  { iso: 'VN-14', name: 'Hòa Bình', region: 'north', successor2025: 'Phú Thọ' },
  { iso: 'VN-18', name: 'Ninh Bình', region: 'north', successor2025: 'Ninh Bình' },
  { iso: 'VN-20', name: 'Thái Bình', region: 'north', successor2025: 'Hưng Yên' },
  { iso: 'VN-53', name: 'Bắc Kạn', region: 'north', successor2025: 'Thái Nguyên' },
  { iso: 'VN-54', name: 'Bắc Giang', region: 'north', successor2025: 'Bắc Ninh' },
  { iso: 'VN-56', name: 'Bắc Ninh', region: 'north', successor2025: 'Bắc Ninh' },
  { iso: 'VN-61', name: 'Hải Dương', region: 'north', successor2025: 'Hải Phòng' },
  { iso: 'VN-63', name: 'Hà Nam', region: 'north', successor2025: 'Ninh Bình' },
  { iso: 'VN-66', name: 'Hưng Yên', region: 'north', successor2025: 'Hưng Yên' },
  { iso: 'VN-67', name: 'Nam Định', region: 'north', successor2025: 'Ninh Bình' },
  { iso: 'VN-68', name: 'Phú Thọ', region: 'north', successor2025: 'Phú Thọ' },
  { iso: 'VN-69', name: 'Thái Nguyên', region: 'north', successor2025: 'Thái Nguyên' },
  { iso: 'VN-70', name: 'Vĩnh Phúc', region: 'north', successor2025: 'Phú Thọ' },
  { iso: 'VN-71', name: 'Điện Biên', region: 'north', successor2025: 'Điện Biên' },
  { iso: 'VN-HN', name: 'Hà Nội', region: 'north', successor2025: 'Hà Nội' },
  { iso: 'VN-HP', name: 'Hải Phòng', region: 'north', successor2025: 'Hải Phòng' },

  // ---- Central (14): the coastal strip from Thanh Hóa to Bình Thuận ----
  { iso: 'VN-21', name: 'Thanh Hóa', region: 'central', successor2025: 'Thanh Hóa' },
  { iso: 'VN-22', name: 'Nghệ An', region: 'central', successor2025: 'Nghệ An' },
  { iso: 'VN-23', name: 'Hà Tĩnh', region: 'central', successor2025: 'Hà Tĩnh' },
  { iso: 'VN-24', name: 'Quảng Bình', region: 'central', successor2025: 'Quảng Trị' },
  { iso: 'VN-25', name: 'Quảng Trị', region: 'central', successor2025: 'Quảng Trị' },
  { iso: 'VN-26', name: 'Thừa Thiên Huế', region: 'central', successor2025: 'Huế' },
  { iso: 'VN-DN', name: 'Đà Nẵng', region: 'central', successor2025: 'Đà Nẵng' },
  { iso: 'VN-27', name: 'Quảng Nam', region: 'central', successor2025: 'Đà Nẵng' },
  { iso: 'VN-29', name: 'Quảng Ngãi', region: 'central', successor2025: 'Quảng Ngãi' },
  { iso: 'VN-31', name: 'Bình Định', region: 'central', successor2025: 'Gia Lai' },
  { iso: 'VN-32', name: 'Phú Yên', region: 'central', successor2025: 'Đắk Lắk' },
  { iso: 'VN-34', name: 'Khánh Hòa', region: 'central', successor2025: 'Khánh Hòa' },
  { iso: 'VN-36', name: 'Ninh Thuận', region: 'central', successor2025: 'Khánh Hòa' },
  { iso: 'VN-40', name: 'Bình Thuận', region: 'central', successor2025: 'Lâm Đồng' },

  // ---- South (24) ----
  // Tây Nguyên
  { iso: 'VN-28', name: 'Kon Tum', region: 'south', successor2025: 'Quảng Ngãi' },
  { iso: 'VN-30', name: 'Gia Lai', region: 'south', successor2025: 'Gia Lai' },
  { iso: 'VN-33', name: 'Đắk Lắk', region: 'south', successor2025: 'Đắk Lắk' },
  { iso: 'VN-72', name: 'Đắk Nông', region: 'south', successor2025: 'Lâm Đồng' },
  { iso: 'VN-35', name: 'Lâm Đồng', region: 'south', successor2025: 'Lâm Đồng' },
  // South-east (VN-43 has two source features: the mainland and Côn Đảo)
  { iso: 'VN-58', name: 'Bình Phước', region: 'south', successor2025: 'Đồng Nai' },
  { iso: 'VN-37', name: 'Tây Ninh', region: 'south', successor2025: 'Tây Ninh' },
  { iso: 'VN-57', name: 'Bình Dương', region: 'south', successor2025: 'Hồ Chí Minh' },
  { iso: 'VN-39', name: 'Đồng Nai', region: 'south', successor2025: 'Đồng Nai' },
  { iso: 'VN-43', name: 'Bà Rịa–Vũng Tàu', region: 'south', successor2025: 'Hồ Chí Minh' },
  { iso: 'VN-SG', name: 'Hồ Chí Minh', region: 'south', successor2025: 'Hồ Chí Minh' },
  // Mekong delta
  { iso: 'VN-41', name: 'Long An', region: 'south', successor2025: 'Tây Ninh' },
  { iso: 'VN-46', name: 'Tiền Giang', region: 'south', successor2025: 'Đồng Tháp' },
  { iso: 'VN-50', name: 'Bến Tre', region: 'south', successor2025: 'Vĩnh Long' },
  { iso: 'VN-51', name: 'Trà Vinh', region: 'south', successor2025: 'Vĩnh Long' },
  { iso: 'VN-49', name: 'Vĩnh Long', region: 'south', successor2025: 'Vĩnh Long' },
  { iso: 'VN-45', name: 'Đồng Tháp', region: 'south', successor2025: 'Đồng Tháp' },
  { iso: 'VN-44', name: 'An Giang', region: 'south', successor2025: 'An Giang' },
  { iso: 'VN-47', name: 'Kiên Giang', region: 'south', successor2025: 'An Giang' },
  { iso: 'VN-CT', name: 'Cần Thơ', region: 'south', successor2025: 'Cần Thơ' },
  { iso: 'VN-73', name: 'Hậu Giang', region: 'south', successor2025: 'Cần Thơ' },
  { iso: 'VN-52', name: 'Sóc Trăng', region: 'south', successor2025: 'Cần Thơ' },
  { iso: 'VN-55', name: 'Bạc Liêu', region: 'south', successor2025: 'Cà Mau' },
  { iso: 'VN-59', name: 'Cà Mau', region: 'south', successor2025: 'Cà Mau' },
];
