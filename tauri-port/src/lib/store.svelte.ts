import type { MojiData, CanvasData, ProjectData, LocatePosition } from './types';
import { createDefaultCanvasData, createDefaultMojiData } from './types';

export type Theme = 'light' | 'dark';

// image dataUrl は大容量のため履歴から除外する
type HistoryState = {
  mojiList: MojiData[];
  canvasWidth: number;
  canvasHeight: number;
  canvasColor: string;
  imageMarginTop: number;
  imageMarginLeft: number;
  imageMarginBottom: number;
  imageMarginRight: number;
  image2LocatePosition: LocatePosition;
  imageData1: { width: number; height: number; modifiedWidth: number; modifiedHeight: number };
  imageData2: { width: number; height: number; modifiedWidth: number; modifiedHeight: number };
  selectedMojiId: number | null;
};

const MAX_HISTORY = 50;

function createAppState() {
  let mojiList = $state<MojiData[]>([]);
  let canvasData = $state<CanvasData>(createDefaultCanvasData());
  let selectedMojiId = $state<number | null>(null);
  let zoomPercent = $state(100);
  let theme = $state<Theme>('light');
  let nextId = $state(1);
  let projectFilePath = $state<string | null>(null);
  let colorHistory = $state<string[]>([]);

  // Undo/Redo
  let undoStack = $state<HistoryState[]>([]);
  let redoStack = $state<HistoryState[]>([]);
  // 連続するupdateMojiをまとめるための遅延キャプチャ
  let pendingCapture: { state: HistoryState; timer: ReturnType<typeof setTimeout> } | null = null;

  function captureState(): HistoryState {
    return {
      mojiList: JSON.parse(JSON.stringify(mojiList)) as MojiData[],
      canvasWidth: canvasData.canvasWidth,
      canvasHeight: canvasData.canvasHeight,
      canvasColor: canvasData.canvasColor,
      imageMarginTop: canvasData.imageMarginTop,
      imageMarginLeft: canvasData.imageMarginLeft,
      imageMarginBottom: canvasData.imageMarginBottom,
      imageMarginRight: canvasData.imageMarginRight,
      image2LocatePosition: canvasData.image2LocatePosition,
      imageData1: { ...canvasData.imageData1 },
      imageData2: { ...canvasData.imageData2 },
      selectedMojiId,
    };
  }

  function pushHistory(state: HistoryState) {
    undoStack = [...undoStack.slice(-(MAX_HISTORY - 1)), state];
    redoStack = [];
  }

  // 遅延コミット中の履歴を即時確定する
  function flushPending() {
    if (pendingCapture) {
      clearTimeout(pendingCapture.timer);
      pushHistory(pendingCapture.state);
      pendingCapture = null;
    }
  }

  function restoreState(state: HistoryState) {
    mojiList = state.mojiList;
    // image dataUrl は変更しない（履歴から除外しているため）
    canvasData = {
      ...canvasData,
      canvasWidth: state.canvasWidth,
      canvasHeight: state.canvasHeight,
      canvasColor: state.canvasColor,
      imageMarginTop: state.imageMarginTop,
      imageMarginLeft: state.imageMarginLeft,
      imageMarginBottom: state.imageMarginBottom,
      imageMarginRight: state.imageMarginRight,
      image2LocatePosition: state.image2LocatePosition,
      imageData1: { ...canvasData.imageData1, ...state.imageData1 },
      imageData2: { ...canvasData.imageData2, ...state.imageData2 },
    };
    selectedMojiId = state.selectedMojiId;
  }

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
    get canUndo() { return undoStack.length > 0 || pendingCapture !== null; },
    get canRedo() { return redoStack.length > 0; },

    addMoji() {
      flushPending();
      pushHistory(captureState());
      const moji = createDefaultMojiData(nextId++);
      mojiList.push(moji);
      selectedMojiId = moji.id;
    },

    removeMoji(id: number) {
      flushPending();
      pushHistory(captureState());
      const idx = mojiList.findIndex(m => m.id === id);
      if (idx !== -1) mojiList.splice(idx, 1);
      if (selectedMojiId === id) selectedMojiId = null;
    },

    duplicateMoji(id: number) {
      const src = mojiList.find(m => m.id === id);
      if (!src) return;
      flushPending();
      pushHistory(captureState());
      const clone = JSON.parse(JSON.stringify(src)) as MojiData;
      clone.id = nextId++;
      clone.x = src.x + 20;
      clone.y = src.y + 20;
      mojiList.push(clone);
      selectedMojiId = clone.id;
    },

    // SVGで後ろにある要素（配列の末尾）が前面に描画される
    // 前面へ = 配列の後ろへ移動
    moveMojiForward(id: number) {
      const idx = mojiList.findIndex(m => m.id === id);
      if (idx < 0 || idx >= mojiList.length - 1) return;
      flushPending();
      pushHistory(captureState());
      [mojiList[idx], mojiList[idx + 1]] = [mojiList[idx + 1], mojiList[idx]];
    },

    // 背面へ = 配列の前へ移動
    moveMojiBackward(id: number) {
      const idx = mojiList.findIndex(m => m.id === id);
      if (idx <= 0) return;
      flushPending();
      pushHistory(captureState());
      [mojiList[idx], mojiList[idx - 1]] = [mojiList[idx - 1], mojiList[idx]];
    },

    selectMoji(id: number | null) {
      selectedMojiId = id;
    },

    updateMoji(updated: MojiData) {
      // ドラッグや連続入力をグループ化：最初の変更前の状態をキャプチャし、
      // 600ms 無操作後にコミット
      if (pendingCapture) {
        clearTimeout(pendingCapture.timer);
        pendingCapture.timer = setTimeout(() => {
          pushHistory(pendingCapture!.state);
          pendingCapture = null;
        }, 600);
      } else {
        const snapshot = captureState();
        pendingCapture = {
          state: snapshot,
          timer: setTimeout(() => {
            pushHistory(pendingCapture!.state);
            pendingCapture = null;
          }, 600),
        };
      }
      const idx = mojiList.findIndex(m => m.id === updated.id);
      if (idx !== -1) mojiList[idx] = updated;
    },

    updateCanvasData(data: CanvasData) {
      flushPending();
      pushHistory(captureState());
      canvasData = data;
    },

    undo() {
      flushPending();
      if (undoStack.length === 0) return;
      redoStack = [...redoStack, captureState()];
      const prev = undoStack[undoStack.length - 1];
      undoStack = undoStack.slice(0, -1);
      restoreState(prev);
    },

    redo() {
      if (redoStack.length === 0) return;
      undoStack = [...undoStack, captureState()];
      const next = redoStack[redoStack.length - 1];
      redoStack = redoStack.slice(0, -1);
      restoreState(next);
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
      if (pendingCapture) { clearTimeout(pendingCapture.timer); pendingCapture = null; }
      undoStack = [];
      redoStack = [];
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
