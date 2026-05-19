<script lang="ts">
  import { appState } from '$lib/store.svelte';
  import { argbToRgba } from '$lib/color';
  import { openProject, saveProject, saveProjectAs, savePngDialog } from '$lib/fileio';
  import { renderToPngDataUrl } from '$lib/pngExport';
  import MojiPanel from '$lib/components/MojiPanel.svelte';
  import MojiEditor from '$lib/components/MojiEditor.svelte';
  import CanvasSettingsDialog from '$lib/components/CanvasSettingsDialog.svelte';

  const zoom = $derived(appState.zoomPercent / 100);
  const cw = $derived(appState.canvasData.canvasWidth);
  const ch = $derived(appState.canvasData.canvasHeight);
  const canvasBg = $derived(argbToRgba(appState.canvasData.canvasColor));

  // File operations
  async function handleOpen() {
    const result = await openProject();
    if (result) appState.loadProject(result.data, result.path);
  }

  async function handleSave() {
    const path = appState.projectFilePath;
    if (path) {
      await saveProject(appState.toProjectData(), path);
    } else {
      const newPath = await saveProjectAs(appState.toProjectData());
      if (newPath) appState.setProjectFilePath(newPath);
    }
  }

  async function handleSaveAs() {
    const newPath = await saveProjectAs(appState.toProjectData());
    if (newPath) appState.setProjectFilePath(newPath);
  }

  // ファイル名表示（パスの末尾だけ）
  const fileName = $derived(
    appState.projectFilePath
      ? appState.projectFilePath.replace(/.*[\\/]/, '')
      : '（未保存）'
  );

  // Keyboard shortcuts
  async function handleKeyDown(e: KeyboardEvent) {
    // テキスト入力中は横取りしない
    const tag = (e.target as HTMLElement).tagName;
    const isEditing = tag === 'INPUT' || tag === 'TEXTAREA';

    if (e.key === 'Delete' && !isEditing && appState.selectedMojiId !== null) {
      appState.removeMoji(appState.selectedMojiId);
      return;
    }
    if (e.key === 'Escape') { appState.selectMoji(null); return; }

    if (e.ctrlKey || e.metaKey) {
      if (e.key === 's' && e.shiftKey) { e.preventDefault(); await handleSaveAs(); return; }
      if (e.key === 's')               { e.preventDefault(); await handleSave();   return; }
      if (e.key === 'o')               { e.preventDefault(); await handleOpen();   return; }
    }
  }

  // Deselect on canvas background click
  function handleCanvasClick(e: MouseEvent) {
    if (e.target === e.currentTarget) appState.selectMoji(null);
  }

  function onZoomChange(e: Event) {
    appState.setZoom(Number((e.target as HTMLInputElement).value));
  }

  // CanvasSettingsDialog
  let isCanvasSettingsOpen = $state(false);

  // PNG export
  let canvasSvgEl = $state<SVGSVGElement | undefined>(undefined);
  let isExporting  = $state(false);

  async function handleExportPng() {
    if (!canvasSvgEl || isExporting) return;
    isExporting = true;
    try {
      const dataUrl = await renderToPngDataUrl(
        canvasSvgEl,
        appState.canvasData.canvasWidth,
        appState.canvasData.canvasHeight,
        appState.canvasData.canvasColor,
      );
      await savePngDialog(dataUrl);
    } finally {
      isExporting = false;
    }
  }
</script>

<!-- svelte-ignore a11y_no_noninteractive_element_to_interactive_role -->
<!-- svelte-ignore a11y_no_noninteractive_tabindex -->
<div
  class="app"
  data-theme={appState.theme}
  tabindex="0"
  role="application"
  onkeydown={handleKeyDown}
>
  <!-- ツールバー -->
  <header class="toolbar">
    <div class="toolbar-group">
      <button onclick={() => appState.addMoji()}>文字を追加</button>
    </div>
    <div class="toolbar-group">
      <button onclick={handleOpen}>開く</button>
      <button onclick={handleSave}>保存</button>
      <button onclick={handleSaveAs}>別名保存</button>
      <button onclick={handleExportPng} disabled={isExporting}>
        {isExporting ? '出力中…' : 'PNG出力'}
      </button>
    </div>
    <div class="toolbar-group file-name-group">
      <span class="file-name">{fileName}</span>
    </div>
    <div class="toolbar-group">
      <button onclick={() => { isCanvasSettingsOpen = true; }}>キャンバス設定</button>
    </div>
    <div class="toolbar-group zoom-group">
      <label for="zoom-input">ズーム</label>
      <input
        id="zoom-input"
        type="number"
        min="10" max="500" step="10"
        value={appState.zoomPercent}
        onchange={onZoomChange}
        style="width:60px"
      />
      <span>%</span>
      <button onclick={() => appState.setZoom(100)}>100%</button>
    </div>
    <div class="toolbar-group" style="margin-left:auto; border-right:none;">
      <button onclick={() => appState.toggleTheme()}>
        {appState.theme === 'light' ? '🌙 Dark' : '☀️ Light'}
      </button>
    </div>
  </header>

  <div class="main-area">
    <!-- 左パネル: 文字リスト -->
    <aside class="moji-list-panel">
      <div class="panel-title">文字リスト</div>
      {#each appState.mojiList as moji (moji.id)}
        <div
          class="moji-item"
          class:selected={appState.selectedMojiId === moji.id}
          onclick={() => appState.selectMoji(moji.id)}
          role="button"
          tabindex="0"
          onkeydown={e => e.key === 'Enter' && appState.selectMoji(moji.id)}
        >
          <span class="moji-preview">{moji.fullText.slice(0, 10).replace(/\n/g, '↵')}</span>
          <button
            class="moji-delete"
            onclick={e => { e.stopPropagation(); appState.removeMoji(moji.id); }}
            aria-label="削除"
          >×</button>
        </div>
      {/each}
      {#if appState.mojiList.length === 0}
        <div class="empty-hint">「文字を追加」ボタンで<br/>文字を追加できます<br/><br/>Deleteキーで選択中の<br/>文字を削除できます</div>
      {/if}
    </aside>

    <!-- 中央: キャンバスエリア -->
    <main class="canvas-area" onclick={handleCanvasClick} role="presentation">
      <!--
        zoom対応スクロール:
        helper divが実際のスクロール範囲を決める（スケール後のサイズ + padding）
        canvas-stageはCSSスケールで拡縮し、スクロール可能にする
      -->
      <div
        class="canvas-scroll-helper"
        style="width: {cw * zoom + 40}px; height: {ch * zoom + 40}px;"
      >
        <div
          class="canvas-stage"
          style="
            width: {cw}px;
            height: {ch}px;
            background: {canvasBg};
            transform: scale({zoom});
            transform-origin: top left;
          "
        >
          <!-- 全MojiPanelを1つのSVGにまとめる（OverflowをvisibleにしてはみだしOK） -->
          <svg
            bind:this={canvasSvgEl}
            width={cw}
            height={ch}
            style="position:absolute;top:0;left:0;overflow:visible;"
          >
            {#each appState.mojiList as moji (moji.id)}
              <MojiPanel
                {moji}
                selected={appState.selectedMojiId === moji.id}
                zoomScale={zoom}
                onSelect={() => appState.selectMoji(moji.id)}
                onMove={(x, y) => appState.updateMoji({ ...moji, x, y })}
              />
            {/each}
          </svg>
        </div>
      </div>
    </main>

    <!-- 右パネル: 文字プロパティ（選択中の文字があるとき表示） -->
    {#if appState.selectedMoji}
      <aside class="moji-editor-panel">
        <div class="panel-title">
          文字プロパティ
          <button class="close-btn" onclick={() => appState.selectMoji(null)}>×</button>
        </div>
        {#key appState.selectedMojiId}
          <MojiEditor moji={appState.selectedMoji} />
        {/key}
      </aside>
    {/if}
  </div>

  {#if isCanvasSettingsOpen}
    <CanvasSettingsDialog onClose={() => { isCanvasSettingsOpen = false; }} />
  {/if}
</div>

<style>
  :global(*, *::before, *::after) {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
  }

  :global(body) {
    overflow: hidden;
    font-family: 'Segoe UI', system-ui, sans-serif;
    font-size: 13px;
  }

  .app {
    display: flex;
    flex-direction: column;
    height: 100vh;
    outline: none; /* tabindex用のフォーカスリングを非表示 */
    --bg: #f0f0f0;
    --panel-bg: #ffffff;
    --border: #cccccc;
    --text: #1a1a1a;
    --accent: #0078d4;
    --toolbar-bg: #e8e8e8;
    background: var(--bg);
    color: var(--text);
  }

  .app[data-theme='dark'] {
    --bg: #1e1e1e;
    --panel-bg: #252526;
    --border: #3e3e42;
    --text: #d4d4d4;
    --accent: #4fc3f7;
    --toolbar-bg: #2d2d2d;
  }

  /* ツールバー */
  .toolbar {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 4px 8px;
    background: var(--toolbar-bg);
    border-bottom: 1px solid var(--border);
    flex-shrink: 0;
    flex-wrap: wrap;
  }

  .toolbar-group {
    display: flex;
    align-items: center;
    gap: 4px;
    padding-right: 8px;
    border-right: 1px solid var(--border);
  }

  .zoom-group label {
    font-size: 12px;
  }

  .file-name-group {
    border-right: none;
  }
  .file-name {
    font-size: 11px;
    color: var(--text);
    opacity: 0.55;
    white-space: nowrap;
    max-width: 260px;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .toolbar button {
    padding: 3px 10px;
    border: 1px solid var(--border);
    background: var(--panel-bg);
    color: var(--text);
    border-radius: 3px;
    cursor: pointer;
    font-size: 12px;
    white-space: nowrap;
  }

  .toolbar button:hover {
    background: var(--accent);
    color: white;
    border-color: var(--accent);
  }

  .toolbar input[type='number'] {
    border: 1px solid var(--border);
    background: var(--panel-bg);
    color: var(--text);
    border-radius: 3px;
    padding: 2px 4px;
    font-size: 12px;
  }

  /* メインエリア */
  .main-area {
    display: flex;
    flex: 1;
    overflow: hidden;
  }

  /* 左パネル */
  .moji-list-panel {
    width: 180px;
    flex-shrink: 0;
    background: var(--panel-bg);
    border-right: 1px solid var(--border);
    display: flex;
    flex-direction: column;
    overflow-y: auto;
  }

  .panel-title {
    padding: 6px 8px;
    font-size: 11px;
    font-weight: 600;
    color: var(--text);
    opacity: 0.6;
    border-bottom: 1px solid var(--border);
    flex-shrink: 0;
  }

  .moji-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 5px 8px;
    cursor: pointer;
    border-bottom: 1px solid var(--border);
    font-size: 12px;
  }

  .moji-item:hover { background: color-mix(in srgb, var(--accent) 15%, transparent); }
  .moji-item.selected { background: color-mix(in srgb, var(--accent) 25%, transparent); }

  .moji-preview {
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
    flex: 1;
  }

  .moji-delete {
    background: none;
    border: none;
    color: var(--text);
    opacity: 0.4;
    cursor: pointer;
    font-size: 14px;
    padding: 0 2px;
    line-height: 1;
  }

  .moji-delete:hover { opacity: 1; color: #e53935; }

  .empty-hint {
    padding: 16px 8px;
    font-size: 11px;
    color: var(--text);
    opacity: 0.5;
    text-align: center;
    line-height: 1.6;
  }

  /* キャンバスエリア */
  .canvas-area {
    flex: 1;
    overflow: auto;
    background: #888;
  }

  /* スクロール範囲確保用ヘルパー（zoomに応じて拡縮） */
  .canvas-scroll-helper {
    position: relative;
    min-width: 100%;
    min-height: 100%;
  }

  .canvas-stage {
    position: absolute;
    top: 20px;
    left: 20px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.5);
  }

  /* 右パネル: 文字プロパティエディタ */
  .moji-editor-panel {
    width: 270px;
    flex-shrink: 0;
    background: var(--panel-bg);
    border-left: 1px solid var(--border);
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  .panel-title {
    padding: 6px 8px;
    font-size: 11px;
    font-weight: 600;
    color: var(--text);
    opacity: 0.6;
    border-bottom: 1px solid var(--border);
    flex-shrink: 0;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .close-btn {
    background: none;
    border: none;
    color: var(--text);
    opacity: 0.5;
    cursor: pointer;
    font-size: 14px;
    line-height: 1;
    padding: 0 2px;
  }
  .close-btn:hover { opacity: 1; }
</style>
