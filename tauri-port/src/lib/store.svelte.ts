import type { MojiData, CanvasData, ProjectData } from './types';
import { createDefaultCanvasData, createDefaultMojiData } from './types';

export type Theme = 'light' | 'dark';

function createAppState() {
  let mojiList = $state<MojiData[]>([]);
  let canvasData = $state<CanvasData>(createDefaultCanvasData());
  let selectedMojiId = $state<number | null>(null);
  let zoomPercent = $state(100);
  let theme = $state<Theme>('light');
  let nextId = $state(1);
  let projectFilePath = $state<string | null>(null);
  let colorHistory = $state<string[]>([]);

  return {
    get mojiList() { return mojiList; },
    get canvasData() { return canvasData; },
    get selectedMojiId() { return selectedMojiId; },
    get selectedMoji(): MojiData | null {
      return selectedMojiId !== null
        ? (mojiList.find(m => m.id === selectedMojiId) ?? null)
        : null;
    },
    get zoomPercent() { return zoomPercent; },
    get theme() { return theme; },
    get projectFilePath() { return projectFilePath; },
    get colorHistory() { return colorHistory; },

    addMoji() {
      const moji = createDefaultMojiData(nextId++);
      mojiList.push(moji);
      selectedMojiId = moji.id;
    },

    removeMoji(id: number) {
      const idx = mojiList.findIndex(m => m.id === id);
      if (idx !== -1) mojiList.splice(idx, 1);
      if (selectedMojiId === id) selectedMojiId = null;
    },

    selectMoji(id: number | null) {
      selectedMojiId = id;
    },

    updateMoji(updated: MojiData) {
      const idx = mojiList.findIndex(m => m.id === updated.id);
      if (idx !== -1) mojiList[idx] = updated;
    },

    updateCanvasData(data: CanvasData) {
      canvasData = data;
    },

    setZoom(pct: number) {
      zoomPercent = Math.max(10, Math.min(500, pct));
    },

    toggleTheme() {
      theme = theme === 'light' ? 'dark' : 'light';
    },

    addToColorHistory(color: string) {
      colorHistory = [color, ...colorHistory.filter(c => c !== color)].slice(0, 20);
    },

    loadProject(data: ProjectData, filePath: string) {
      canvasData = data.canvasData;
      mojiList = data.mojiList;
      nextId = mojiList.reduce((max, m) => Math.max(max, m.id), 0) + 1;
      selectedMojiId = null;
      projectFilePath = filePath;
    },

    toProjectData(): ProjectData {
      return { version: '2.0.0', canvasData, mojiList };
    },

    setProjectFilePath(path: string | null) {
      projectFilePath = path;
    },
  };
}

export const appState = createAppState();
