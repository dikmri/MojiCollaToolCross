<script lang="ts">
  import { appState } from '$lib/store.svelte';
  import type { ImageData } from '$lib/types';
  import ColorField from './ColorField.svelte';

  interface Props {
    onClose: () => void;
  }
  let { onClose }: Props = $props();

  // ── キャンバス基本設定 ──────────────────────────────────────
  let width  = $state(appState.canvasData.canvasWidth);
  let height = $state(appState.canvasData.canvasHeight);
  let color  = $state(appState.canvasData.canvasColor);

  // ── 画像設定（ダイアログ内のローカルコピー） ────────────────
  let img1 = $state<ImageData>({ ...appState.canvasData.imageData1 });
  let img2 = $state<ImageData>({ ...appState.canvasData.imageData2 });
  let img2Pos  = $state(appState.canvasData.image2LocatePosition);
  let marginL  = $state(appState.canvasData.imageMarginLeft);
  let marginT  = $state(appState.canvasData.imageMarginTop);

  // ── 画像読み込み ─────────────────────────────────────────────
  let fileInput1: HTMLInputElement | undefined = $state(undefined);
  let fileInput2: HTMLInputElement | undefined = $state(undefined);

  function readImageFile(file: File): Promise<ImageData> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onerror = () => reject(new Error('読み込みに失敗しました'));
      reader.onload = () => {
        const dataUrl = reader.result as string;
        const img = new Image();
        img.onload = () => resolve({
          dataUrl,
          width:  img.naturalWidth,
          height: img.naturalHeight,
          modifiedWidth:  img.naturalWidth,
          modifiedHeight: img.naturalHeight,
        });
        img.onerror = () => reject(new Error('画像のデコードに失敗しました'));
        img.src = dataUrl;
      };
      reader.readAsDataURL(file);
    });
  }

  async function handleFileChange(
    e: Event,
    setter: (d: ImageData) => void,
  ) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;
    try {
      setter(await readImageFile(file));
    } catch (err) {
      console.error(err);
    }
    // reset so same file can be re-selected
    (e.target as HTMLInputElement).value = '';
  }

  async function handleImg1FileChange(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;
    try {
      const data = await readImageFile(file);
      img1   = data;
      // 画像1を読み込んだらキャンバスサイズを自動調整
      width  = data.modifiedWidth;
      height = data.modifiedHeight;
    } catch (err) {
      console.error(err);
    }
    (e.target as HTMLInputElement).value = '';
  }

  function clearImg(setter: (d: ImageData) => void) {
    setter({ dataUrl: '', width: 0, height: 0, modifiedWidth: 0, modifiedHeight: 0 });
  }

  // ── 適用 ─────────────────────────────────────────────────────
  function clamp(v: number, lo: number, hi: number) {
    return Math.max(lo, Math.min(hi, Math.round(v)));
  }

  function apply() {
    appState.updateCanvasData({
      ...appState.canvasData,
      canvasWidth:  clamp(width,  1, 10000),
      canvasHeight: clamp(height, 1, 10000),
      canvasColor:  color,
      imageData1:   img1,
      imageData2:   img2,
      image2LocatePosition: img2Pos,
      imageMarginLeft: marginL,
      imageMarginTop:  marginT,
    });
    onClose();
  }

  function handleOverlayClick(e: MouseEvent) {
    if (e.target === e.currentTarget) onClose();
  }

  function handleKeyDown(e: KeyboardEvent) {
    if (e.key === 'Escape') onClose();
  }
</script>

<!-- svelte-ignore a11y_no_noninteractive_element_interactions -->
<div
  class="overlay"
  role="dialog"
  aria-modal="true"
  tabindex="-1"
  onclick={handleOverlayClick}
  onkeydown={handleKeyDown}
>
  <div class="dialog" onclick={e => e.stopPropagation()} role="presentation">
    <div class="dialog-title">キャンバス設定</div>

    <div class="dialog-body">

      <!-- ── キャンバスサイズ ── -->
      <div class="section-title">キャンバス</div>
      <div class="row">
        <span class="lbl">幅</span>
        <input class="num-input" type="number" min="1" max="10000" bind:value={width} />
        <span class="unit">px</span>
      </div>
      <div class="row">
        <span class="lbl">高さ</span>
        <input class="num-input" type="number" min="1" max="10000" bind:value={height} />
        <span class="unit">px</span>
      </div>
      <div class="row">
        <ColorField label="背景色" value={color} onChange={v => { color = v; }} />
      </div>

      <!-- ── 画像1 ── -->
      <div class="section-title">画像1（主）</div>
      <div class="img-row">
        {#if img1.dataUrl}
          <img class="thumb" src={img1.dataUrl} alt="画像1プレビュー" />
          <span class="img-info">{img1.width}×{img1.height}</span>
        {:else}
          <div class="thumb-empty">なし</div>
        {/if}
        <button class="img-btn" onclick={() => fileInput1?.click()}>選択</button>
        {#if img1.dataUrl}
          <button class="img-btn clear-btn" onclick={() => clearImg(d => { img1 = d; })}>クリア</button>
        {/if}
      </div>
      <!-- 非表示ファイル入力 -->
      <input
        bind:this={fileInput1}
        type="file"
        accept="image/png,image/jpeg,image/gif,image/webp,image/bmp"
        style="display:none"
        onchange={handleImg1FileChange}
      />
      {#if img1.dataUrl}
        <div class="row">
          <span class="lbl">幅</span>
          <input class="num-input" type="number" min="1" max="10000"
            bind:value={img1.modifiedWidth} />
          <span class="lbl" style="margin-left:6px">高さ</span>
          <input class="num-input" type="number" min="1" max="10000"
            bind:value={img1.modifiedHeight} />
          <span class="unit">px</span>
        </div>
        <div class="row">
          <span class="lbl">位置 X</span>
          <input class="num-input" type="number" min="-9999" max="10000" bind:value={marginL} />
          <span class="lbl" style="margin-left:6px">Y</span>
          <input class="num-input" type="number" min="-9999" max="10000" bind:value={marginT} />
          <span class="unit">px</span>
        </div>
      {/if}

      <!-- ── 画像2 ── -->
      <div class="section-title">画像2（副）</div>
      <div class="img-row">
        {#if img2.dataUrl}
          <img class="thumb" src={img2.dataUrl} alt="画像2プレビュー" />
          <span class="img-info">{img2.width}×{img2.height}</span>
        {:else}
          <div class="thumb-empty">なし</div>
        {/if}
        <button class="img-btn" onclick={() => fileInput2?.click()}>選択</button>
        {#if img2.dataUrl}
          <button class="img-btn clear-btn" onclick={() => clearImg(d => { img2 = d; })}>クリア</button>
        {/if}
      </div>
      <input
        bind:this={fileInput2}
        type="file"
        accept="image/png,image/jpeg,image/gif,image/webp,image/bmp"
        style="display:none"
        onchange={e => handleFileChange(e, d => { img2 = d; })}
      />
      {#if img2.dataUrl}
        <div class="row">
          <span class="lbl">幅</span>
          <input class="num-input" type="number" min="1" max="10000"
            bind:value={img2.modifiedWidth} />
          <span class="lbl" style="margin-left:6px">高さ</span>
          <input class="num-input" type="number" min="1" max="10000"
            bind:value={img2.modifiedHeight} />
          <span class="unit">px</span>
        </div>
        <div class="row">
          <span class="lbl">配置</span>
          {#each (['left','right','top','bottom'] as const) as pos}
            <label class="radio-lbl">
              <input type="radio" bind:group={img2Pos} value={pos} />
              {pos === 'left' ? '左' : pos === 'right' ? '右' : pos === 'top' ? '上' : '下'}
            </label>
          {/each}
        </div>
      {/if}

    </div><!-- /dialog-body -->

    <div class="dialog-footer">
      <button class="btn-apply" onclick={apply}>適用</button>
      <button class="btn-cancel" onclick={onClose}>キャンセル</button>
    </div>
  </div>
</div>

<style>
  .overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.45);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 200;
  }

  .dialog {
    background: var(--panel-bg);
    border: 1px solid var(--border);
    border-radius: 6px;
    padding: 16px;
    width: 340px;
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    gap: 0;
    box-shadow: 0 4px 24px rgba(0, 0, 0, 0.3);
  }

  .dialog-title {
    font-size: 13px;
    font-weight: 600;
    color: var(--text);
    border-bottom: 1px solid var(--border);
    padding-bottom: 10px;
    margin-bottom: 12px;
    flex-shrink: 0;
  }

  .dialog-body {
    display: flex;
    flex-direction: column;
    gap: 8px;
    overflow-y: auto;
    flex: 1;
    padding-right: 2px;
  }

  .section-title {
    font-size: 11px;
    font-weight: 600;
    color: var(--text);
    opacity: 0.55;
    border-bottom: 1px solid var(--border);
    padding-bottom: 4px;
    margin-top: 6px;
  }

  .row {
    display: flex;
    align-items: center;
    gap: 6px;
  }

  .lbl {
    font-size: 11px;
    color: var(--text);
    opacity: 0.7;
    white-space: nowrap;
    flex-shrink: 0;
  }

  .num-input {
    width: 68px;
    font-size: 12px;
    padding: 3px 6px;
    border: 1px solid var(--border);
    background: var(--bg);
    color: var(--text);
    border-radius: 3px;
    text-align: right;
  }

  .unit {
    font-size: 11px;
    color: var(--text);
    opacity: 0.5;
  }

  .img-row {
    display: flex;
    align-items: center;
    gap: 6px;
  }

  .thumb {
    width: 48px;
    height: 48px;
    object-fit: contain;
    border: 1px solid var(--border);
    border-radius: 3px;
    background: #888;
    flex-shrink: 0;
  }

  .thumb-empty {
    width: 48px;
    height: 48px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px dashed var(--border);
    border-radius: 3px;
    font-size: 10px;
    color: var(--text);
    opacity: 0.4;
    flex-shrink: 0;
  }

  .img-info {
    font-size: 10px;
    color: var(--text);
    opacity: 0.5;
    flex: 1;
  }

  .img-btn {
    font-size: 11px;
    padding: 3px 10px;
    border: 1px solid var(--border);
    background: var(--panel-bg);
    color: var(--text);
    border-radius: 3px;
    cursor: pointer;
    white-space: nowrap;
  }
  .img-btn:hover { background: var(--accent); color: white; border-color: var(--accent); }

  .clear-btn:hover { background: #e53935; border-color: #e53935; color: white; }

  .radio-lbl {
    font-size: 11px;
    color: var(--text);
    display: flex;
    align-items: center;
    gap: 2px;
    cursor: pointer;
  }

  .dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 8px;
    padding-top: 12px;
    margin-top: 4px;
    border-top: 1px solid var(--border);
    flex-shrink: 0;
  }

  .btn-apply, .btn-cancel {
    font-size: 12px;
    padding: 5px 16px;
    border-radius: 3px;
    cursor: pointer;
    border: 1px solid var(--border);
  }

  .btn-apply {
    background: var(--accent);
    color: white;
    border-color: var(--accent);
  }
  .btn-apply:hover { opacity: 0.85; }

  .btn-cancel {
    background: var(--panel-bg);
    color: var(--text);
  }
  .btn-cancel:hover { background: var(--bg); }
</style>
