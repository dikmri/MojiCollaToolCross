# MojiCollaToolCross

## なにこれ
文字コラ用ツール「MojiCollaTool」をフォークし、Windows・macOS・Linux に対応させたものです。  
偉大なる先人のツール、moji build、MojiCollaToolに敬意を表します。  
[moji build](https://archive.md/2015.02.04-215832/http://www.geocities.jp/moji564/)  
[MojiCollaTool](https://github.com/kuramiya/MojiCollaTool)

## ダウンロード
Releasesからどうぞ。  
OS別の実行バイナリを含むzip形式で配布しています。

## インストール方法

### Windows
1. Releases から `win-x64.zip` をダウンロードして解凍する
2. `MojiCollaToolCross.exe` をダブルクリックで起動

### macOS
1. Releases から `osx-x64.zip` をダウンロードして解凍する
2. ターミナルで実行権限を付与して起動する
```bash
chmod +x MojiCollaToolCross
./MojiCollaToolCross
```
> macOS Gatekeeper の警告が出る場合は、システム設定 → プライバシーとセキュリティ から許可してください。

### Linux
1. Releases から `linux-x64.zip` をダウンロードして解凍する
2. ターミナルで実行権限を付与して起動する
```bash
chmod +x MojiCollaToolCross
./MojiCollaToolCross
```

### ソースからビルドする場合（全OS共通）
[.NET 8 SDK](https://dotnet.microsoft.com/download) をインストールしたうえで以下を実行してください。
```bash
cd MojiCollaTool/MojiCollaTool
dotnet publish -r win-x64   --self-contained -o publish/win
dotnet publish -r osx-x64   --self-contained -o publish/mac
dotnet publish -r linux-x64 --self-contained -o publish/linux
```
