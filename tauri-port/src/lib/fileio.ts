import { open, save } from '@tauri-apps/plugin-dialog';
import { invoke } from '@tauri-apps/api/core';
import type { ProjectData } from './types';

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

export async function savePngDialog(dataUrl: string): Promise<boolean> {
  const path = await save({
    filters: [{ name: 'PNG Image', extensions: ['png'] }],
    defaultPath: 'export.png',
  });
  if (!path) return false;
  return saveImage(path, dataUrl);
}
