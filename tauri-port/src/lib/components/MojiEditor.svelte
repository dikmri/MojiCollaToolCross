<script lang="ts">
  import { appState } from '$lib/store.svelte';
  import type { MojiData } from '$lib/types';
  import ColorField from './ColorField.svelte';
  import FontPicker from './FontPicker.svelte';

  interface Props { moji: MojiData; }
  let { moji }: Props = $props();

  function upd<K extends keyof MojiData>(key: K, val: MojiData[K]) {
    appState.updateMoji({ ...moji, [key]: val });
  }

  function numVal(e: Event) {
    return Number((e.target as HTMLInputElement).value);
  }
  function checked(e: Event) {
    return (e.target as HTMLInputElement).checked;
  }
</script>

<div class="editor">
  <!-- テキスト -->
  <section>
    <div class="sec-title">テキスト</div>
    <textarea
      class="text-input"
      value={moji.fullText}
      oninput={e => upd('fullText', (e.target as HTMLTextAreaElement).value)}
      rows="4"
    ></textarea>
  </section>

  <!-- フォント -->
  <section>
    <div class="sec-title">フォント</div>
    <div class="row">
      <label class="lbl">フォント</label>
      <div class="flex1">
        <FontPicker value={moji.fontFamilyName} onChange={v => upd('fontFamilyName', v)} />
      </div>
    </div>
    <div class="row">
      <label class="lbl">サイズ</label>
      <input class="num-input" type="number" min="1" max="500"
        value={moji.fontSize} onchange={e => upd('fontSize', numVal(e))} />
      <span class="unit">px</span>
      <label class="chk-lbl"><input type="checkbox" checked={moji.isBold}
        onchange={e => upd('isBold', checked(e))} /> 太字</label>
      <label class="chk-lbl"><input type="checkbox" checked={moji.isItalic}
        onchange={e => upd('isItalic', checked(e))} /> 斜体</label>
    </div>
    <div class="row">
      <label class="lbl">向き</label>
      <label class="chk-lbl">
        <input type="radio" name="dir-{moji.id}" value="yokogaki"
          checked={moji.textDirection === 'yokogaki'}
          onchange={() => upd('textDirection', 'yokogaki')} /> 横書き
      </label>
      <label class="chk-lbl">
        <input type="radio" name="dir-{moji.id}" value="tategaki"
          checked={moji.textDirection === 'tategaki'}
          onchange={() => upd('textDirection', 'tategaki')} /> 縦書き
      </label>
    </div>
    <div class="row">
      <label class="lbl">文字間</label>
      <input class="num-input" type="number" min="-50" max="200"
        value={moji.characterMargin} onchange={e => upd('characterMargin', numVal(e))} />
      <label class="lbl" style="margin-left:8px">行間</label>
      <input class="num-input" type="number" min="-50" max="200"
        value={moji.lineMargin} onchange={e => upd('lineMargin', numVal(e))} />
    </div>
  </section>

  <!-- 文字色 -->
  <section>
    <div class="sec-title">文字色</div>
    <ColorField label="文字色" value={moji.foreColor}
      onChange={v => upd('foreColor', v)} />
  </section>

  <!-- 縁取り -->
  <section>
    <div class="sec-title">縁取り（一次）</div>
    <div class="row">
      <label class="lbl">太さ</label>
      <input class="num-input" type="number" min="0" max="100"
        value={moji.borderThickness} onchange={e => upd('borderThickness', numVal(e))} />
      <label class="lbl" style="margin-left:8px">ぼかし</label>
      <input class="num-input" type="number" min="0" max="100"
        value={moji.borderBlurRadius} onchange={e => upd('borderBlurRadius', numVal(e))} />
    </div>
    <ColorField label="縁取り色" value={moji.borderColor}
      onChange={v => upd('borderColor', v)} />
  </section>

  <!-- 二重縁取り -->
  <section>
    <div class="sec-title">縁取り（二次）</div>
    <div class="row">
      <label class="lbl">太さ</label>
      <input class="num-input" type="number" min="0" max="100"
        value={moji.secondBorderThickness} onchange={e => upd('secondBorderThickness', numVal(e))} />
      <label class="lbl" style="margin-left:8px">ぼかし</label>
      <input class="num-input" type="number" min="0" max="100"
        value={moji.secondBorderBlurRadius} onchange={e => upd('secondBorderBlurRadius', numVal(e))} />
    </div>
    <ColorField label="縁取り色" value={moji.secondBorderColor}
      onChange={v => upd('secondBorderColor', v)} />
  </section>

  <!-- 背景ボックス -->
  <section>
    <div class="sec-title">
      <label class="chk-lbl">
        <input type="checkbox" checked={moji.isBackgroundBoxExists}
          onchange={e => upd('isBackgroundBoxExists', checked(e))} />
        背景ボックス
      </label>
    </div>
    {#if moji.isBackgroundBoxExists}
      <ColorField label="背景色" value={moji.backgroundBoxColor}
        onChange={v => upd('backgroundBoxColor', v)} />
      <div class="row">
        <label class="lbl">padding</label>
        <input class="num-input" type="number" min="0" max="200"
          value={moji.backgroundBoxPadding} onchange={e => upd('backgroundBoxPadding', numVal(e))} />
        <label class="lbl" style="margin-left:8px">角丸</label>
        <input class="num-input" type="number" min="0" max="200"
          value={moji.backgroundBoxCornerRadius} onchange={e => upd('backgroundBoxCornerRadius', numVal(e))} />
      </div>
      <div class="row">
        <label class="lbl">枠太さ</label>
        <input class="num-input" type="number" min="0" max="50"
          value={moji.backgroundBoxBorderThickness} onchange={e => upd('backgroundBoxBorderThickness', numVal(e))} />
      </div>
      <ColorField label="枠色" value={moji.backgroundBoxBorderColor}
        onChange={v => upd('backgroundBoxBorderColor', v)} />
    {/if}
  </section>

  <!-- 回転 -->
  <section>
    <div class="sec-title">回転</div>
    <div class="row">
      <label class="lbl">角度</label>
      <input class="num-input" type="number" min="-360" max="360"
        value={moji.rotateAngle} onchange={e => upd('rotateAngle', numVal(e))} />
      <span class="unit">°</span>
      <button class="reset-btn" onclick={() => upd('rotateAngle', 0)}>リセット</button>
    </div>
  </section>
</div>

<style>
  .editor {
    padding: 4px 8px;
    display: flex;
    flex-direction: column;
    gap: 0;
    overflow-y: auto;
    height: 100%;
  }

  section {
    padding: 6px 0;
    border-bottom: 1px solid var(--border);
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .sec-title {
    font-size: 11px;
    font-weight: 600;
    color: var(--text);
    opacity: 0.6;
    margin-bottom: 2px;
  }

  .row {
    display: flex;
    align-items: center;
    gap: 4px;
    flex-wrap: wrap;
  }

  .lbl {
    font-size: 11px;
    color: var(--text);
    opacity: 0.7;
    white-space: nowrap;
    flex-shrink: 0;
  }

  .chk-lbl {
    font-size: 11px;
    color: var(--text);
    display: flex;
    align-items: center;
    gap: 3px;
    white-space: nowrap;
    cursor: pointer;
  }

  .text-input {
    width: 100%;
    resize: vertical;
    font-size: 13px;
    font-family: inherit;
    padding: 4px;
    border: 1px solid var(--border);
    background: var(--bg);
    color: var(--text);
    border-radius: 3px;
    min-height: 60px;
  }

  .flex1 { flex: 1; min-width: 0; }

  .num-input {
    width: 52px;
    font-size: 12px;
    padding: 2px 4px;
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

  .reset-btn {
    font-size: 11px;
    padding: 2px 6px;
    border: 1px solid var(--border);
    background: var(--panel-bg);
    color: var(--text);
    border-radius: 3px;
    cursor: pointer;
    margin-left: 4px;
  }

  .reset-btn:hover { background: var(--accent); color: white; border-color: var(--accent); }
</style>
