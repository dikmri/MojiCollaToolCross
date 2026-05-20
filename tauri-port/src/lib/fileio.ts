import { open, save } from '@tauri-apps/plugin-dialog';
import { invoke } from '@tauri-apps/api/core';
import type { ProjectData, MojiFormat } from './types';

const EXTENSION = 'mccp';
const FILTER = [{ name: 'MojiCollaToolCross Project', extensions: [EXTENSION] }];

export async function openProject(): Promise<{ data: ProjectData; path: string } | null> {
  const result = await open({ filters: FILTER, multiple: false });
  if (!result) return null;
  const path = Array.isArray(result) ? result[0] : result;
  const json = await invoke<string>('load_project', { path });
  return { data: JSON.parse(json) as ProjectData, path };
}

/** プロジェクトを保存し、実際に書き込んだパスを返す（連番付与済み）。 */
export async function saveProject(data: ProjectData, path: string): Promise<string | null> {
  try {
    return await invoke<string>('save_project', { path, data: JSON.stringify(data, null, 2) });
  } catch {
    return null;
  }
}

export async function saveProjectAs(data: ProjectData): Promise<string | null> {
  const path = await save({ filters: FILTER, defaultPath: `project.${EXTENSION}` });
  if (!path) return null;
  return saveProject(data, path);
}

export async function saveImage(path: string, dataUrl: string): Promise<boolean> {
  try {
    const base64 = dataUrl.split(',')[1];
    const bytes = Array.from(atob(base64), c => c.charCodeAt(0));
    await invoke<string>('save_image', { path, data: bytes });
    return true;
  } catch {
    return false;
  }
}

const FORMAT_EXTENSION = 'mcpf';
const FORMAT_FILTER = [{ name: 'MojiCollaToolCross Format', extensions: [FORMAT_EXTENSION] }];

export async function saveMojiFormat(format: MojiFormat): Promise<boolean> {
  const path = await save({ filters: FORMAT_FILTER, defaultPath: `format.${FORMAT_EXTENSION}` });
  if (!path) return false;
  try {
    await invoke<string>('save_project', { path, data: JSON.stringify(format, null, 2) });
    return true;
  } catch { return false; }
}

export async function loadMojiFormat(): Promise<MojiFormat | null> {
  const result = await open({ filters: FORMAT_FILTER, multiple: false });
  if (!result) return null;
  const path = Array.isArray(result) ? result[0] : result;
  try {
    const json = await invoke<string>('load_project', { path });
    return JSON.parse(json) as MojiFormat;
  } catch { return null; }
}

export async function openProjectFromPath(path: string): Promise<{ data: ProjectData; path: string } | null> {
  try {
    const json = await invoke<string>('load_project', { path });
    return { data: JSON.parse(json) as ProjectData, path };
  } catch { return null; }
}

export async function loadImageFromPath(path: string): Promise<{ dataUrl: string; width: number; height: number } | null> {
  try {
    const bytes = await invoke<number[]>('read_file_bytes', { path });
    const u8 = new Uint8Array(bytes);
    let binary = '';
    const chunkSize = 8192;
    for (let i = 0; i < u8.length; i += chunkSize) {
      binary += String.fromCharCode(...u8.subarray(i, i + chunkSize));
    }
    const ext = path.split('.').pop()?.toLowerCase() ?? 'png';
    const mime = ext === 'jpg' || ext === 'jpeg' ? 'image/jpeg'
               : ext === 'gif' ? 'image/gif'
               : ext === 'webp' ? 'image/webp'
               : ext === 'bmp' ? 'image/bmp'
               : 'image/png';
    const dataUrl = `data:${mime};base64,${btoa(binary)}`;
    const img = await new Promise<HTMLImageElement>((resolve, reject) => {
      const image = new Image();
      image.onload = () => resolve(image);
      image.onerror = reject;
      image.src = dataUrl;
    });
    return { dataUrl, width: img.naturalWidth, height: img.naturalHeight };
  } catch { return null; }
}

export async function savePngDialog(dataUrl: string): Promise<boolean> {
  const path = await save({
    filters: [{ name: 'PNG Image', extensions: ['png'] }],
    defaultPath: 'export.png',
  });
  if (!path) return false;
  return saveImage(path, dataUrl);
}
