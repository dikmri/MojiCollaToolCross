# MojiCollaToolCross

## なにこれ
文字コラ用ツール「MojiCollaTool」をフォークし、Windows・macOS・Linux に対応させたものです。  
偉大なる先人のツール、moji build、MojiCollaToolに敬意を表します。  
[moji build](https://archive.md/2015.02.04-215832/http://www.geocities.jp/moji564/)  
[MojiCollaTool](https://github.com/kuramiya/MojiCollaTool)

## ダウンロード
[最新版](https://github.com/dikmri/MojiCollaToolCross/releases/latest)からどうぞ。

## インストール方法

### Windows
1. Releases から `.msi` をダウンロードして実行する
2. インストーラーの指示に従ってインストール完了

> インストーラーなしで使いたい場合は `.exe` をダウンロードしてそのまま起動できます。

### macOS
1. Releases から `.dmg` をダウンロードして開く
   - Apple Silicon (M1以降): `_aarch64.dmg`
   - Intel Mac: `_x64.dmg`
2. アプリをアプリケーションフォルダへドラッグ＆ドロップ

> macOS Gatekeeper の警告が出る場合は、システム設定 → プライバシーとセキュリティ から「このまま開く」を選択してください。

### Linux
1. Releases から `.AppImage` または `.deb` をダウンロードする
2. AppImage の場合:
```bash
chmod +x MojiCollaToolCross_*.AppImage
./MojiCollaToolCross_*.AppImage
```
3. .deb の場合:
```bash
sudo dpkg -i MojiCollaToolCross_*.deb
```

### ソースからビルドする場合（全OS共通）
[Rust](https://rustup.rs/) と [Node.js (LTS)](https://nodejs.org/) をインストールしたうえで以下を実行してください。
```bash
cd tauri-port
npm install
npm run tauri build
```
