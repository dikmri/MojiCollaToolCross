<script lang="ts">
  import { appState } from '$lib/store.svelte';
  import ColorField from './ColorField.svelte';

  interface Props {
    onClose: () => void;
  }
  let { onClose }: Props = $props();

  let width  = $state(appState.canvasData.canvasWidth);
  let height = $state(appState.canvasData.canvasHeight);
  let color  = $state(appState.canvasData.canvasColor);

  function clamp(v: number, lo: number, hi: number) {
    return Math.max(lo, Math.min(hi, Math.round(v)));
  }

  function apply() {
    appState.updateCanvasData({
      ...appState.canvasData,
      canvasWidth:  clamp(width,  1, 10000),
      canvasHeight: clamp(height, 1, 10000),
      canvasColor:  color,
    });
    onClose();
  }

  function handleOverlayClick(e: MouseEvent) {
    if (e.target === e.currentTarget) onClose();
  }

  function handleKeyDown(e: KeyboardEvent) {
    if (e.key === 'Escape') onClose();
    if (e.key === 'Enter')  apply();
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
      <div class="row">
        <span class="lbl">幅</span>
        <input
          class="num-input"
          type="number" min="1" max="10000"
          bind:value={width}
        />
        <span class="unit">px</span>
      </div>
      <div class="row">
        <span class="lbl">高さ</span>
        <input
          class="num-input"
          type="number" min="1" max="10000"
          bind:value={height}
        />
        <span class="unit">px</span>
      </div>
      <div class="row">
        <ColorField label="背景色" value={color} onChange={v => { color = v; }} />
      </div>
    </div>

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
    min-width: 280px;
    display: flex;
    flex-direction: column;
    gap: 14px;
    box-shadow: 0 4px 24px rgba(0, 0, 0, 0.3);
  }

  .dialog-title {
    font-size: 13px;
    font-weight: 600;
    color: var(--text);
    border-bottom: 1px solid var(--border);
    padding-bottom: 10px;
  }

  .dialog-body {
    display: flex;
    flex-direction: column;
    gap: 10px;
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
    width: 36px;
    flex-shrink: 0;
  }

  .num-input {
    width: 80px;
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

  .dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 8px;
    padding-top: 4px;
    border-top: 1px solid var(--border);
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
