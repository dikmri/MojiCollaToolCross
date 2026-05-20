<script lang="ts">
  import type { MojiData } from '$lib/types';
  import { argbToRgba } from '$lib/color';

  interface Props {
    moji: MojiData;
    selected: boolean;
    zoomScale: number;
    onSelect: () => void;
    onMove: (x: number, y: number) => void;
  }

  let { moji, selected, zoomScale, onSelect, onMove }: Props = $props();

  const lines = $derived(moji.fullText.split('\n'));
  const isVertical = $derived(moji.textDirection === 'tategaki');
  const hasBorder = $derived(moji.borderThickness > 0);
  const hasSecondBorder = $derived(moji.secondBorderThickness > 0);
  const fontWeight = $derived(moji.isBold ? 'bold' : 'normal');
  const fontStyle = $derived(moji.isItalic ? 'italic' : 'normal');
  const filterId1 = $derived(`mf1-${moji.id}`);
  const filterId2 = $derived(`mf2-${moji.id}`);

  // CSS identifiers cannot contain '+', spaces, etc. — always quote non-generic names
  const GENERIC_FONTS = new Set(['serif', 'sans-serif', 'monospace', 'cursive', 'fantasy', 'system-ui']);
  const fontFamily = $derived(
    GENERIC_FONTS.has(moji.fontFamilyName)
      ? moji.fontFamilyName
      : `'${moji.fontFamilyName.replace(/'/g, "\\'")}'`
  );

  // Line x position: vertical text columns go right-to-left
  function lineX(i: number): number {
    return isVertical ? (lines.length - 1 - i) * (moji.fontSize + moji.lineMargin) : 0;
  }
  // Line y position: horizontal text rows go top-to-bottom
  function lineY(i: number): number {
    return isVertical ? 0 : i * (moji.fontSize + moji.lineMargin);
  }

  const textAnchor = $derived(
    isVertical ? 'start' :
    moji.textAlign === 'center' ? 'middle' :
    moji.textAlign === 'right'  ? 'end'    : 'start'
  );

  // CSS style string per <text> element
  const textStyle = $derived(
    (isVertical ? 'writing-mode: vertical-rl; text-orientation: upright; ' : '') +
    'dominant-baseline: hanging; ' +
    `letter-spacing: ${moji.characterMargin}px;`
  );

  // Measure text bounding box after render, for background box and selection overlay
  let textGroupEl: SVGGElement | undefined = $state(undefined);
  let bbox = $state({ x: 0, y: 0, w: 10, h: 10 });

  $effect(() => {
    // Track layout-affecting properties as dependencies
    void (moji.fullText + moji.fontSize + moji.fontFamilyName +
          moji.textDirection + moji.isBold + moji.isItalic +
          moji.characterMargin + moji.lineMargin);
    requestAnimationFrame(() => {
      if (!textGroupEl) return;
      try {
        const b = textGroupEl.getBBox();
        if (b.width > 0 || b.height > 0) {
          bbox = { x: b.x, y: b.y, w: b.width, h: b.height };
        }
      } catch { /* SVG not yet in DOM */ }
    });
  });

  // Drag
  let hitEl: SVGRectElement | undefined = $state(undefined);
  let dragStart: { px: number; py: number; mx: number; my: number } | null = null;

  function handlePointerDown(e: PointerEvent) {
    if (e.button !== 0) return;
    e.preventDefault();
    e.stopPropagation();
    onSelect();
    dragStart = { px: e.clientX, py: e.clientY, mx: moji.x, my: moji.y };
    hitEl?.setPointerCapture(e.pointerId);
  }

  // click イベントの伝播を止めて、親要素の全体デセレクト処理をブロックする
  function handleClick(e: MouseEvent) {
    e.stopPropagation();
  }

  function handlePointerMove(e: PointerEvent) {
    if (!dragStart) return;
    const dx = (e.clientX - dragStart.px) / zoomScale;
    const dy = (e.clientY - dragStart.py) / zoomScale;
    onMove(dragStart.mx + dx, dragStart.my + dy);
  }

  function handlePointerUp() {
    dragStart = null;
  }

  // Rotation center is the bbox center
  const rotateCx = $derived(bbox.x + bbox.w / 2);
  const rotateCy = $derived(bbox.y + bbox.h / 2);
</script>

<!-- Positioned within the canvas SVG -->
<g transform="translate({moji.x}, {moji.y}) rotate({moji.rotateAngle}, {rotateCx}, {rotateCy})">
  <defs>
    {#if hasBorder && moji.borderBlurRadius > 0}
      <filter id={filterId1} x="-100%" y="-100%" width="300%" height="300%">
        <feGaussianBlur stdDeviation={moji.borderBlurRadius} />
      </filter>
    {/if}
    {#if hasSecondBorder && moji.secondBorderBlurRadius > 0}
      <filter id={filterId2} x="-100%" y="-100%" width="300%" height="300%">
        <feGaussianBlur stdDeviation={moji.secondBorderBlurRadius} />
      </filter>
    {/if}
  </defs>

  <!-- Background box (drawn first, behind text) -->
  {#if moji.isBackgroundBoxExists && bbox.w > 0}
    <rect
      x={bbox.x - moji.backgroundBoxPadding}
      y={bbox.y - moji.backgroundBoxPadding}
      width={bbox.w + moji.backgroundBoxPadding * 2}
      height={bbox.h + moji.backgroundBoxPadding * 2}
      fill={argbToRgba(moji.backgroundBoxColor)}
      stroke={moji.backgroundBoxBorderThickness > 0
        ? argbToRgba(moji.backgroundBoxBorderColor)
        : 'none'}
      stroke-width={moji.backgroundBoxBorderThickness}
      rx={moji.backgroundBoxCornerRadius}
    />
  {/if}

  <!-- Second border layer (outer outline) -->
  {#if hasSecondBorder}
    <g
      font-family={fontFamily}
      font-size={moji.fontSize}
      font-weight={fontWeight}
      font-style={fontStyle}
      fill={argbToRgba(moji.secondBorderColor)}
      stroke={argbToRgba(moji.secondBorderColor)}
      stroke-width={moji.secondBorderThickness * 2}
      stroke-linejoin="round"
      style="paint-order: stroke fill"
      filter={moji.secondBorderBlurRadius > 0 ? `url(#${filterId2})` : undefined}
    >
      {#each lines as line, i}
        <text x={lineX(i)} y={lineY(i)} text-anchor={textAnchor} style={textStyle}>{line}</text>
      {/each}
    </g>
  {/if}

  <!-- First border layer (inner outline) -->
  {#if hasBorder}
    <g
      font-family={fontFamily}
      font-size={moji.fontSize}
      font-weight={fontWeight}
      font-style={fontStyle}
      fill={argbToRgba(moji.borderColor)}
      stroke={argbToRgba(moji.borderColor)}
      stroke-width={moji.borderThickness * 2}
      stroke-linejoin="round"
      style="paint-order: stroke fill"
      filter={moji.borderBlurRadius > 0 ? `url(#${filterId1})` : undefined}
    >
      {#each lines as line, i}
        <text x={lineX(i)} y={lineY(i)} text-anchor={textAnchor} style={textStyle}>{line}</text>
      {/each}
    </g>
  {/if}

  <!-- Foreground text (measure bbox from this group) -->
  <g
    bind:this={textGroupEl}
    font-family={fontFamily}
    font-size={moji.fontSize}
    font-weight={fontWeight}
    font-style={fontStyle}
    fill={argbToRgba(moji.foreColor)}
  >
    {#each lines as line, i}
      <text x={lineX(i)} y={lineY(i)} style={textStyle}>{line}</text>
    {/each}
  </g>

  <!-- Interaction overlay: drag handle + selection border -->
  <rect
    bind:this={hitEl}
    data-hit-rect="true"
    x={bbox.x - 4}
    y={bbox.y - 4}
    width={bbox.w + 8}
    height={bbox.h + 8}
    fill="transparent"
    stroke={selected ? '#0078d4' : 'transparent'}
    stroke-width="1"
    stroke-dasharray={selected ? '4 2' : undefined}
    rx="2"
    vector-effect="non-scaling-stroke"
    style="cursor: move;"
    onpointerdown={handlePointerDown}
    onpointermove={handlePointerMove}
    onpointerup={handlePointerUp}
    onclick={handleClick}
  />
</g>
