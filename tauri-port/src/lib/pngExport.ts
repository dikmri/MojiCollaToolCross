import type { CanvasData, ImageData } from './types';
import { argbToRgba } from './color';

function imgDisplaySize(img: ImageData) {
  return {
    w: img.modifiedWidth  || img.width,
    h: img.modifiedHeight || img.height,
  };
}

function img2Position(cd: CanvasData) {
  const { w: w1, h: h1 } = imgDisplaySize(cd.imageData1);
  const { w: w2, h: h2 } = imgDisplaySize(cd.imageData2);
  const x1 = cd.imageMarginLeft;
  const y1 = cd.imageMarginTop;
  switch (cd.image2LocatePosition) {
    case 'left':   return { x: x1 - w2, y: y1,      w: w2, h: h2 };
    case 'right':  return { x: x1 + w1, y: y1,      w: w2, h: h2 };
    case 'top':    return { x: x1,      y: y1 - h2, w: w2, h: h2 };
    case 'bottom': return { x: x1,      y: y1 + h1, w: w2, h: h2 };
  }
}

function loadImage(dataUrl: string): Promise<HTMLImageElement> {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload  = () => resolve(img);
    img.onerror = () => reject(new Error('image load failed'));
    img.src = dataUrl;
  });
}

/**
 * キャンバス（画像 + SVGテキスト）をPNG data URLとして返す。
 */
export async function renderToPngDataUrl(
  svgEl: SVGSVGElement,
  canvasData: CanvasData,
): Promise<string> {
  const { canvasWidth: cw, canvasHeight: ch } = canvasData;
  const ns = 'http://www.w3.org/2000/svg';

  const canvas = document.createElement('canvas');
  canvas.width  = cw;
  canvas.height = ch;
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('Canvas 2D context unavailable');

  // 背景色
  ctx.fillStyle = argbToRgba(canvasData.canvasColor);
  ctx.fillRect(0, 0, cw, ch);

  // 画像1
  if (canvasData.imageData1.dataUrl) {
    const img = await loadImage(canvasData.imageData1.dataUrl);
    const { w, h } = imgDisplaySize(canvasData.imageData1);
    ctx.drawImage(img, canvasData.imageMarginLeft, canvasData.imageMarginTop, w, h);
  }

  // 画像2
  if (canvasData.imageData2.dataUrl) {
    const img = await loadImage(canvasData.imageData2.dataUrl);
    const pos = img2Position(canvasData);
    ctx.drawImage(img, pos.x, pos.y, pos.w, pos.h);
  }

  // SVGテキストレイヤー（クローンして新規SVGに移す）
  const exportSvg = document.createElementNS(ns, 'svg');
  exportSvg.setAttribute('xmlns', ns);
  exportSvg.setAttribute('width',   String(cw));
  exportSvg.setAttribute('height',  String(ch));
  exportSvg.setAttribute('viewBox', `0 0 ${cw} ${ch}`);
  exportSvg.style.overflow = 'hidden';

  const clone = svgEl.cloneNode(true) as SVGSVGElement;
  while (clone.firstChild) exportSvg.appendChild(clone.firstChild);

  const serializer = new XMLSerializer();
  const svgStr = serializer.serializeToString(exportSvg);
  const blob = new Blob([svgStr], { type: 'image/svg+xml;charset=utf-8' });
  const url  = URL.createObjectURL(blob);

  try {
    await new Promise<void>((resolve, reject) => {
      const img = new Image();
      img.onload  = () => { ctx.drawImage(img, 0, 0); resolve(); };
      img.onerror = () => reject(new Error('SVG→Canvas render failed'));
      img.src = url;
    });
  } finally {
    URL.revokeObjectURL(url);
  }

  return canvas.toDataURL('image/png');
}
