export type TextDirection = 'yokogaki' | 'tategaki';
export type TextAlign = 'left' | 'center' | 'right';
export type LocatePosition = 'left' | 'right' | 'top' | 'bottom';

export interface MojiData {
  id: number;
  fullText: string;
  x: number;
  y: number;
  fontSize: number;
  fontFamilyName: string;
  textDirection: TextDirection;
  isBold: boolean;
  isItalic: boolean;
  characterMargin: number;
  lineMargin: number;
  foreColor: string;           // "#AARRGGBB"
  borderColor: string;
  borderThickness: number;
  borderBlurRadius: number;
  secondBorderColor: string;
  secondBorderThickness: number;
  secondBorderBlurRadius: number;
  isBackgroundBoxExists: boolean;
  backgroundBoxColor: string;
  backgroundBoxPadding: number;
  backgroundBoxBorderThickness: number;
  backgroundBoxBorderColor: string;
  backgroundBoxCornerRadius: number;
  rotateAngle: number;
  textAlign: TextAlign;
}

// 装飾テンプレート: 位置/テキスト/ID を除いた装飾情報
export type MojiFormat = Omit<MojiData, 'id' | 'x' | 'y' | 'fullText'>;

export interface ImageData {
  dataUrl: string;        // base64 data URL。'' = 画像なし
  width: number;          // 元画像の自然サイズ
  height: number;
  modifiedWidth: number;  // 表示サイズ（0 = 自然サイズを使用）
  modifiedHeight: number;
}

export interface CanvasData {
  canvasWidth: number;
  canvasHeight: number;
  canvasColor: string;
  imageData1: ImageData;
  imageData2: ImageData;
  image2LocatePosition: LocatePosition;
  imageMarginTop: number;
  imageMarginLeft: number;
  imageMarginBottom: number;
  imageMarginRight: number;
}

export interface ProjectData {
  version: string;
  canvasData: CanvasData;
  mojiList: MojiData[];
}

export function createDefaultMojiData(id: number): MojiData {
  return {
    id,
    fullText: `サンプル${id}`,
    x: 0,
    y: 0,
    fontSize: 50,
    fontFamilyName: 'serif',
    textDirection: 'yokogaki',
    isBold: false,
    isItalic: false,
    characterMargin: 0,
    lineMargin: 0,
    foreColor: '#FF000000',
    borderColor: '#FFFFFFFF',
    borderThickness: 0,
    borderBlurRadius: 0,
    secondBorderColor: '#FF000000',
    secondBorderThickness: 0,
    secondBorderBlurRadius: 0,
    isBackgroundBoxExists: false,
    backgroundBoxColor: '#FFFFFFFF',
    backgroundBoxPadding: 0,
    backgroundBoxBorderThickness: 0,
    backgroundBoxBorderColor: '#FF000000',
    backgroundBoxCornerRadius: 0,
    rotateAngle: 0,
    textAlign: 'left',
  };
}

export function createDefaultImageData(): ImageData {
  return { dataUrl: '', width: 0, height: 0, modifiedWidth: 0, modifiedHeight: 0 };
}

export function createDefaultCanvasData(): CanvasData {
  return {
    canvasWidth: 800,
    canvasHeight: 600,
    canvasColor: '#FFFFFFFF',
    imageData1: createDefaultImageData(),
    imageData2: createDefaultImageData(),
    image2LocatePosition: 'left',
    imageMarginTop: 0,
    imageMarginLeft: 0,
    imageMarginBottom: 0,
    imageMarginRight: 0,
  };
}
