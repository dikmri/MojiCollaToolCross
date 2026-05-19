<script lang="ts">
  import { palette, paletteToArgb } from '$lib/colorPalette';
  import { argbToRgba } from '$lib/color';

  interface Props {
    value: string;        // #AARRGGBB — 初期値のみ参照
    beforeColor: string;
    history: string[];
    onChange: (v: string) => void;         // スライダーリリース・確定時
    onPickColor: (v: string) => void;      // パレット・履歴クリック時
  }

  let { value, beforeColor, history, onChange, onPickColor }: Props = $props();

  function parseArgb(hex: string) {
    const h = hex.replace('#', '').padEnd(8, 'F');
    return {
      a: parseInt(h.slice(0, 2), 16) || 0,
      r: parseInt(h.slice(2, 4), 16) || 0,
      g: parseInt(h.slice(4, 6), 16) || 0,
      b: parseInt(h.slice(6, 8), 16) || 0,
    };
  }

  function toArgb(a: number, r: number, g: number, b: number): string {
    return '#' + [a, r, g, b]
      .map(v => Math.max(0, Math.min(255, Math.round(v))).toString(16).padStart(2, '0').toUpperCase())
      .join('');
  }

  // 初期値（value prop）から内部状態を初期化（一度だけ）
  const _init = parseArgb(value);
  let ca = $state(_init.a);
  let cr = $state(_init.r);
  let cg = $state(_init.g);
  let cb = $state(_init.b);

  // リアルタイムプレビュー用（ストアには流さない）
  const currentColor = $derived(toArgb(ca, cr, cg, cb));

  // Hex入力テキスト（スライダー操作中は変化しない、リリース時に同期）
  let hexInput = $state(value.replace('#', ''));

  // スライダーリリース時のみストアへ反映（onchange = マウスリリース時）
  function commitFromSliders() {
    hexInput = currentColor.replace('#', '');
    onChange(currentColor);
  }

  function applyHex() {
    const h = hexInput.toUpperCase().replace('#', '');
    if (!/^[0-9A-F]{8}$/.test(h)) return;
    const c = parseArgb('#' + h);
    ca = c.a; cr = c.r; cg = c.g; cb = c.b;
    onChange('#' + h);
  }

  function pickColor(argb: string) {
    const c = parseArgb(argb);
    ca = c.a; cr = c.r; cg = c.g; cb = c.b;
    hexInput = argb.replace('#', '');
    onChange(argb);
    onPickColor(argb);
  }

  // スライダー背景のグラデーション（リアルタイムプレビュー用）
  const bgR = $derived(`linear-gradient(to right,rgba(0,${cg},${cb},${ca/255}),rgba(255,${cg},${cb},${ca/255}))`);
  const bgG = $derived(`linear-gradient(to right,rgba(${cr},0,${cb},${ca/255}),rgba(${cr},255,${cb},${ca/255}))`);
  const bgB = $derived(`linear-gradient(to right,rgba(${cr},${cg},0,${ca/255}),rgba(${cr},${cg},255,${ca/255}))`);
  const bgA = $derived(`linear-gradient(to right,rgba(${cr},${cg},${cb},0),rgba(${cr},${cg},${cb},1))`);
</script>

<div class="color-picker">
  <!-- 変更前 / 現在のプレビュー -->
  <div class="preview-row">
    <span class="preview-label">変更前</span>
    <div class="swatch" style="background:{argbToRgba(beforeColor)}"></div>
    <div class="swatch" style="background:{argbToRgba(currentColor)}"></div>
    <span class="preview-label">現在</span>
  </div>

  <!-- RGBAスライダー（bind:value で即時プレビュー、onchange でコミット） -->
  {#each [
    { label: 'R', val: cr, setter: (v: number) => { cr = v; }, bg: bgR },
    { label: 'G', val: cg, setter: (v: number) => { cg = v; }, bg: bgG },
    { label: 'B', val: cb, setter: (v: number) => { cb = v; }, bg: bgB },
    { label: 'A', val: ca, setter: (v: number) => { ca = v; }, bg: bgA },
  ] as ch}
    <div class="slider-row">
      <span class="slider-label">{ch.label}</span>
      <div class="slider-track" style="background:{ch.bg}">
        <input
          type="range" min="0" max="255"
          value={ch.val}
          oninput={e => ch.setter(Number((e.target as HTMLInputElement).value))}
          onchange={commitFromSliders}
        />
      </div>
      <span class="slider-val">{ch.val}</span>
    </div>
  {/each}

  <!-- Hex入力 -->
  <div class="hex-row">
    <span class="slider-label">#</span>
    <input
      class="hex-input"
      type="text"
      maxlength="8"
      bind:value={hexInput}
      onkeydown={e => e.key === 'Enter' && applyHex()}
      onblur={applyHex}
    />
    <button class="apply-btn" onclick={applyHex}>確定</button>
  </div>

  <!-- マテリアルデザインパレット（14行×19列） -->
  <div class="palette-grid">
    {#each Array.from({ length: 14 }, (_, row) => row) as row}
      <div class="palette-row">
        {#each palette as family}
          <button
            class="palette-cell"
            style="background:{family[row]}"
            title={family[row]}
            onclick={() => pickColor(paletteToArgb(family[row]))}
          ></button>
        {/each}
      </div>
    {/each}
  </div>

  <!-- 色履歴 -->
  {#if history.length > 0}
    <div class="history-label">履歴</div>
    <div class="history-row">
      {#each history as color}
        <button
          class="palette-cell"
          style="background:{argbToRgba(color)}"
          title={color}
          onclick={() => pickColor(color)}
        ></button>
      {/each}
    </div>
  {/if}
</div>

<style>
  .color-picker {
    padding: 6px;
    display: flex;
    flex-direction: column;
    gap: 5px;
    background: var(--panel-bg);
    border: 1px solid var(--border);
    border-radius: 4px;
  }

  .preview-row {
    display: flex;
    align-items: center;
    gap: 4px;
  }
  .preview-label { font-size: 10px; color: var(--text); opacity: 0.6; }
  .swatch {
    width: 36px;
    height: 18px;
    border: 1px solid var(--border);
    border-radius: 2px;
    background-image:
      linear-gradient(45deg,#ccc 25%,transparent 25%),
      linear-gradient(-45deg,#ccc 25%,transparent 25%),
      linear-gradient(45deg,transparent 75%,#ccc 75%),
      linear-gradient(-45deg,transparent 75%,#ccc 75%);
    background-size: 6px 6px;
    background-position: 0 0,0 3px,3px -3px,-3px 0;
    background-color: white;
  }

  .slider-row { display: flex; align-items: center; gap: 4px; }
  .slider-label { width: 12px; font-size: 11px; font-weight: 600; text-align: center; color: var(--text); }
  .slider-track {
    flex: 1;
    height: 14px;
    border-radius: 7px;
    border: 1px solid var(--border);
    overflow: hidden;
    position: relative;
  }
  .slider-track input[type='range'] {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    opacity: 0;
    cursor: pointer;
    margin: 0;
  }
  .slider-val { width: 28px; font-size: 11px; text-align: right; color: var(--text); }

  .hex-row { display: flex; align-items: center; gap: 4px; }
  .hex-input {
    flex: 1;
    font-size: 11px;
    font-family: monospace;
    padding: 2px 4px;
    border: 1px solid var(--border);
    background: var(--bg);
    color: var(--text);
    border-radius: 3px;
  }
  .apply-btn {
    font-size: 11px;
    padding: 2px 6px;
    border: 1px solid var(--border);
    background: var(--panel-bg);
    color: var(--text);
    border-radius: 3px;
    cursor: pointer;
  }
  .apply-btn:hover { background: var(--accent); color: white; border-color: var(--accent); }

  .palette-grid { display: flex; flex-direction: column; gap: 1px; }
  .palette-row { display: flex; gap: 1px; }
  .palette-cell {
    width: 12px;
    height: 12px;
    border: none;
    padding: 0;
    cursor: pointer;
    flex-shrink: 0;
  }
  .palette-cell:hover { outline: 2px solid white; outline-offset: -1px; z-index: 1; position: relative; }

  .history-label { font-size: 10px; color: var(--text); opacity: 0.5; }
  .history-row { display: flex; flex-wrap: wrap; gap: 1px; }
</style>
