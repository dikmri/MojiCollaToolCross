<script lang="ts">
  import { appState } from '$lib/store.svelte';
  import { argbToRgba } from '$lib/color';
  import ColorPicker from './ColorPicker.svelte';

  interface Props {
    label: string;
    value: string;   // #AARRGGBB
    onChange: (v: string) => void;
  }

  let { label, value, onChange }: Props = $props();

  let isOpen = $state(false);
  let beforeColor = $state('');

  function toggle() {
    if (!isOpen) beforeColor = value;
    isOpen = !isOpen;
  }
</script>

<div class="color-field">
  <span class="cf-label">{label}</span>
  <button class="cf-swatch" onclick={toggle} title={value}>
    <span class="cf-swatch-inner" style="background:{argbToRgba(value)}"></span>
  </button>
  <span class="cf-hex" onclick={toggle} role="button" tabindex="0" onkeydown={e=>e.key==='Enter'&&toggle()}>
    {value.slice(1)}
  </span>
</div>

{#if isOpen}
  <div class="cf-picker-wrap">
    <ColorPicker
      {value}
      {beforeColor}
      history={appState.colorHistory}
      onChange={onChange}
      onPickColor={(v) => { onChange(v); appState.addToColorHistory(v); }}
    />
  </div>
{/if}

<style>
  .color-field {
    display: flex;
    align-items: center;
    gap: 5px;
  }

  .cf-label {
    font-size: 11px;
    color: var(--text);
    opacity: 0.7;
    width: 52px;
    flex-shrink: 0;
  }

  .cf-swatch {
    width: 20px;
    height: 20px;
    border: 1px solid var(--border);
    border-radius: 3px;
    padding: 0;
    cursor: pointer;
    flex-shrink: 0;
    background-image: linear-gradient(45deg, #ccc 25%, transparent 25%),
                      linear-gradient(-45deg, #ccc 25%, transparent 25%),
                      linear-gradient(45deg, transparent 75%, #ccc 75%),
                      linear-gradient(-45deg, transparent 75%, #ccc 75%);
    background-size: 6px 6px;
    background-position: 0 0, 0 3px, 3px -3px, -3px 0;
    background-color: white;
    overflow: hidden;
  }

  .cf-swatch-inner {
    display: block;
    width: 100%;
    height: 100%;
  }

  .cf-hex {
    font-size: 10px;
    font-family: monospace;
    color: var(--text);
    opacity: 0.6;
    cursor: pointer;
    overflow: hidden;
    white-space: nowrap;
  }

  .cf-picker-wrap {
    margin-top: 4px;
    margin-left: 0;
  }
</style>
