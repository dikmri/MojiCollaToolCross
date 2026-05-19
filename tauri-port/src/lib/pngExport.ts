import { argbToRgba } from './color';

/**
 * キャンバスSVGをPNG data URLとして返す。
 * svgEl: ページ上の <svg> 要素（MojiPanelたちを含む）
 * canvasColor: ARGB形式の背景色
 */
export async function renderToPngDataUrl(
  svgEl: SVGSVGElement,
  canvasWidth: number,
  canvasHeight: number,
  canvasColor: string,
): Promise<string> {
  const ns = 'http://www.w3.org/2000/svg';

  // エクスポート用SVG作成（サイズ明示、overflow:hidden でクリップ）
  const exportSvg = document.createElementNS(ns, 'svg');
  exportSvg.setAttribute('xmlns', ns);
  exportSvg.setAttribute('width',  String(canvasWidth));
  exportSvg.setAttribute('height', String(canvasHeight));
  exportSvg.setAttribute('viewBox', `0 0 ${canvasWidth} ${canvasHeight}`);
  exportSvg.style.overflow = 'hidden';

  // 背景矩形
  const bg = document.createElementNS(ns, 'rect');
  bg.setAttribute('width',  String(canvasWidth));
  bg.setAttribute('height', String(canvasHeight));
  bg.setAttribute('fill', argbToRgba(canvasColor));
  exportSvg.appendChild(bg);

  // MojiPanel群をディープクローン（ヒット矩形＝interaction overlayも含むがPNG上は透明）
  const clone = svgEl.cloneNode(true) as SVGSVGElement;
  while (clone.firstChild) exportSvg.appendChild(clone.firstChild);

  // SVG文字列にシリアライズ
  const serializer = new XMLSerializer();
  const svgStr = serializer.serializeToString(exportSvg);
  const blob = new Blob([svgStr], { type: 'image/svg+xml;charset=utf-8' });
  const url  = URL.createObjectURL(blob);

  try {
    const canvas = document.createElement('canvas');
    canvas.width  = canvasWidth;
    canvas.height = canvasHeight;
    const ctx = canvas.getContext('2d');
    if (!ctx) throw new Error('Canvas 2D context unavailable');

    await new Promise<void>((resolve, reject) => {
      const img = new Image();
      img.onload  = () => { ctx.drawImage(img, 0, 0); resolve(); };
      img.onerror = () => reject(new Error('SVG→Canvas render failed'));
      img.src = url;
    });

    return canvas.toDataURL('image/png');
  } finally {
    URL.revokeObjectURL(url);
  }
}
