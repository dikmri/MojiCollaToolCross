<script lang="ts">
  import { invoke } from '@tauri-apps/api/core';

  interface Props {
    value: string;
    onChange: (v: string) => void;
  }
  let { value, onChange }: Props = $props();

  let isOpen = $state(false);
  let search = $state('');
  let fonts = $state<string[]>([]);
  let fontsLoaded = $state(false);
  let triggerEl: HTMLButtonElement | undefined = $state(undefined);
  let searchEl: HTMLInputElement | undefined = $state(undefined);
  let dropPos = $state({ top: 0, left: 0, width: 220 });

  async function loadFonts() {
    if (fontsLoaded) return;
    try {
      if ('queryLocalFonts' in window) {
        const raw = await (window as any).queryLocalFonts();
        fonts = [...new Set<string>(raw.map((x: any) => x.family as string))].sort();
        fontsLoaded = true;
        return;
      }
    } catch { /* API unavailable or permission denied */ }
    try {
      fonts = await invoke<string[]>('get_system_fonts');
      fontsLoaded = true;
    } catch { /* fallback failed */ }
  }

  const filtered = $derived(
    search.trim() === ''
      ? fonts
      : fonts.filter(f => f.toLowerCase().includes(search.toLowerCase()))
  );

  async function open() {
    await loadFonts();
    if (triggerEl) {
      const r = triggerEl.getBoundingClientRect();
      dropPos = { top: r.bottom + 2, left: r.left, width: Math.max(r.width, 240) };
    }
    isOpen = true;
    search = '';
    requestAnimationFrame(() => searchEl?.focus());
  }

  function select(f: string) {
    onChange(f);
    isOpen = false;
  }

  function handleDropdownKey(e: KeyboardEvent) {
    if (e.key === 'Escape') isOpen = false;
  }
</script>

<!-- Full-screen overlay: catches outside clicks to close -->
{#if isOpen}
  <div
    class="fp-overlay"
    role="presentation"
    onclick={() => { isOpen = false; }}
  ></div>
{/if}

<!-- Trigger button: shows current font rendered in that font -->
<button
  bind:this={triggerEl}
  class="fp-trigger"
  style="font-family: {value};"
  onclick={open}
  type="button"
>
  {value || 'フォントを選択'}
</button>

<!-- Fixed-position dropdown (z-index above overlay) -->
{#if isOpen}
  <div
    class="fp-dropdown"
    style="top:{dropPos.top}px; left:{dropPos.left}px; width:{dropPos.width}px;"
    role="listbox"
    tabindex="-1"
    onkeydown={handleDropdownKey}
  >
    <input
      bind:this={searchEl}
      class="fp-search"
      type="text"
      placeholder="フォントを検索..."
      bind:value={search}
    />
    <div class="fp-list">
      {#if !fontsLoaded}
        <div class="fp-empty">読み込み中...</div>
      {:else if filtered.length === 0}
        <div class="fp-empty">見つかりません</div>
      {:else}
        {#each filtered as f (f)}
          <button
            class="fp-item"
            class:fp-selected={f === value}
            style="font-family: {f};"
            onclick={() => select(f)}
            type="button"
            role="option"
            aria-selected={f === value}
          >{f}</button>
        {/each}
      {/if}
    </div>
  </div>
{/if}

<style>
  .fp-overlay {
    position: fixed;
    inset: 0;
    z-index: 999;
  }

  .fp-trigger {
    width: 100%;
    text-align: left;
    font-size: 13px;
    padding: 3px 6px;
    border: 1px solid var(--border);
    background: var(--bg);
    color: var(--text);
    border-radius: 3px;
    cursor: pointer;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  .fp-trigger:hover { border-color: var(--accent); }

  .fp-dropdown {
    position: fixed;
    z-index: 1000;
    background: var(--panel-bg, #fff);
    border: 1px solid var(--border, #ccc);
    border-radius: 4px;
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
    display: flex;
    flex-direction: column;
    max-height: 320px;
    overflow: hidden;
  }

  .fp-search {
    padding: 5px 8px;
    border: none;
    border-bottom: 1px solid var(--border, #ccc);
    background: var(--bg, #f0f0f0);
    color: var(--text, #000);
    font-size: 12px;
    outline: none;
    flex-shrink: 0;
  }

  .fp-list {
    overflow-y: auto;
    flex: 1;
  }

  .fp-item {
    display: block;
    width: 100%;
    text-align: left;
    padding: 5px 10px;
    border: none;
    background: none;
    color: var(--text, #000);
    font-size: 14px;
    cursor: pointer;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
  .fp-item:hover { background: color-mix(in srgb, var(--accent, #0078d4) 15%, transparent); }
  .fp-selected { background: color-mix(in srgb, var(--accent, #0078d4) 25%, transparent); }

  .fp-empty {
    padding: 10px;
    font-size: 11px;
    color: var(--text, #000);
    opacity: 0.5;
    text-align: center;
  }
</style>
