# SasaraUtil

<p align="center" style="background-color:lightblue;">
	<img src="./SasaraUtil.UI/Assets/icon.ico" alt="logo" width="256" />
</p>

**[SasaraUtil](https://github.com/InuInu2022/SasaraUtil)**（ささらユーティル）はCeVIOエディタのあれこれを使いやすくする補助ツールです。

---

[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE) [![C Sharp 10](https://img.shields.io/badge/C%20Sharp-10-4FC08D.svg?logo=csharp&style=flat)](https://learn.microsoft.com/ja-jp/dotnet/csharp/) ![.NET 7.0](https://img.shields.io/badge/%20.NET-7.0-blue.svg?logo=dotnet&style=flat)
![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/inuinu2022/sasarautil?include_prereleases&label=%F0%9F%9A%80release) ![GitHub all releases](https://img.shields.io/github/downloads/InuInu2022/SasaraUtil/total?color=green&label=%E2%AC%87%20downloads) ![GitHub Repo stars](https://img.shields.io/github/stars/InuInu2022/SasaraUtil?label=%E2%98%85&logo=github)
[![CeVIO CS](https://img.shields.io/badge/CeVIO_Creative_Studio-7.0-d08cbb.svg?logo=&style=flat)](https://cevio.jp/) [![CeVIO AI](https://img.shields.io/badge/CeVIO_AI-8.4-lightgray.svg?logo=&style=flat)](https://cevio.jp/)

## 機能 / Functions

> ![screenshots](./documents/screenshots/sasarautil.png)
> ver.0.1

- 共通向け
  - [オーディオトラック変換](/###オーディオトラック変換)
- CeVIOトーク向け
  - _coming soon_
- CeVIOソング向け
  - _coming soon_

### オーディオトラック変換

CeVIOのオーディオ取込は16bit/48kHzのwav形式の制限があります。自動で対応形式に変換します。

#### ファイル形式変換

SasaraUtilの「**オーディオトラック変換**」にファイルをドラッグ＆ドロップすると、対応している音声ファイルを自動で**16bit/48kHzのwav形式**に変換します。

動画ファイルも音声ファイルに変換できます。

- [対応形式](https://learn.microsoft.com/ja-jp/windows/win32/medfound/supported-media-formats-in-media-foundation?redirectedfrom=MSDN)
  - 音声ファイル (wav, mp3, aiff, etc...)
  - 動画ファイル（mp4, etc...）

複数ファイルの同時変換に対応しています。

「**Save**」ボタンを押すと、保存先を選ぶダイアログが開き、
変換されたファイルは、
`【元のファイル名】.16bit48khz.wav`
という名前で保存されます。

#### CeVIO取込機能

「**Send**」ボタンを押すことでCeVIOエディタにオーディオトラックとして変換済みの音声ファイルを自動で取り込みます。

- ※ 変換済みの音声ファイルは元のファイルの隣に作られます。
- ※ `.ccst` ファイルがCeVIOのエディタに関連付けられている必要があります

オーディオの開始秒数を事前に設定できます。

## 動作環境 / Requirements

- Windows (10,11)
  - ※Windows以外で利用したい場合ソースコードからビルドすることで使える可能性があります！
- CeVIO連携機能を利用するには、CeVIO （CS/AI）がインストールされている必要があります

## Libraries

- [Avalonia UI](https://avaloniaui.net/)
- [Epoxy](https://github.com/kekyo/Epoxy)

## Projects

* `SasaraUtil.Core`: Independent common component project includes MVVM `Model` code.
* `SasaraUtil.UI`: UI (independent platform) project includes MVVM `View` and `ViewModel` code.
* `SasaraUtil`: The application project code.

## Licenses

### SasasraUtil

>MIT License
>
>Copyright (c) 2023 InuInu

- [LICENSE](LICENSE)

### Libraries licenses

- [licenses](./licenses/)

## 🐶Developed by InuInu

- InuInu（いぬいぬ）
  - YouTube [YouTube](https://bit.ly/InuInuMusic)
  - Twitter [@InuInuGames](https://twitter.com/InuInuGames)
  - Blog [note.com](https://note.com/inuinu_)
  - niconico [niconico](https://nico.ms/user/98013232)