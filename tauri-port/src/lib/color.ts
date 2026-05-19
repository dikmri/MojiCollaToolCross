/** "#AARRGGBB" → "rgba(r,g,b,a)" */
export function argbToRgba(hex: string): string {
  const h = hex.replace('#', '');
  if (h.length !== 8) return hex;
  const a = (parseInt(h.slice(0, 2), 16) / 255).toFixed(3);
  const r = parseInt(h.slice(2, 4), 16);
  const g = parseInt(h.slice(4, 6), 16);
  const b = parseInt(h.slice(6, 8), 16);
  return `rgba(${r},${g},${b},${a})`;
}

/** "rgba(r,g,b,a)" or "#RRGGBB" → "#AARRGGBB" */
export function cssToArgb(css: string, alpha = 1): string {
  if (css.startsWith('#')) {
    const h = css.replace('#', '');
    const a = Math.round(alpha * 255).toString(16).padStart(2, '0').toUpperCase();
    return `#${a}${h.padStart(6, '0').toUpperCase()}`;
  }
  const m = css.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*([\d.]+))?\)/);
  if (!m) return `#FF000000`;
  const r = parseInt(m[1]).toString(16).padStart(2, '0');
  const g = parseInt(m[2]).toString(16).padStart(2, '0');
  const b = parseInt(m[3]).toString(16).padStart(2, '0');
  const a = Math.round((parseFloat(m[4] ?? '1')) * 255).toString(16).padStart(2, '0');
  return `#${a}${r}${g}${b}`.toUpperCase();
}
