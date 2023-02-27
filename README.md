# SasaraUtil

<p align="center" style="background-color:lightblue;">
	<img src="./SasaraUtil.UI/Assets/icon.ico" alt="logo" width="256" />
</p>

**[SasaraUtil](https://github.com/InuInu2022/SasaraUtil)**（ささらユーティル）はCeVIOエディタのあれこれを使いやすくする補助ツールです。

---

[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE) [![C Sharp 10](https://img.shields.io/badge/C%20Sharp-10-4FC08D.svg?logo=csharp&style=flat)](https://learn.microsoft.com/ja-jp/dotnet/csharp/) ![.NET 7.0](https://img.shields.io/badge/%20.NET-7.0-blue.svg?logo=dotnet&style=flat)
![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/inuinu2022/sasarautil?include_prereleases&label=%F0%9F%9A%80release) ![GitHub all releases](https://img.shields.io/github/downloads/InuInu2022/SasaraUtil/total?color=green&label=%E2%AC%87%20downloads) ![GitHub Repo stars](https://img.shields.io/github/stars/InuInu2022/SasaraUtil?label=%E2%98%85&logo=github)
[![CeVIO CS](https://img.shields.io/badge/CeVIO_Creative_Studio-7.0-d08cbb.svg?logo=&style=flat)](https://cevio.jp/) [![CeVIO AI](https://img.shields.io/badge/CeVIO_AI-8.4-lightgray.svg?logo=&style=flat)](https://cevio.jp/) [![VoiSona](https://img.shields.io/badge/VoiSona-1.3-53abdb.svg?logo=&style=flat)](https://voisona.com/)

## 機能 / Functions

> ![screenshots](./documents/screenshots/sasarautil.png)
> ver.0.1

- 共通向け
  - [オーディオトラック変換](#オーディオトラック変換)
- CeVIOトーク向け
  - [キャストを別トラック振り分け](#キャストを別トラック振り分け)
  - _coming soon_
- CeVIOソング向け
  - [ブレス削除・抑制](#ブレス削除抑制)
  - _coming soon_

---

### 共通機能

#### オーディオトラック変換

[![CeVIO CS](https://img.shields.io/badge/CeVIO_Creative_Studio-7.0-d08cbb.svg?logo=&style=flat)](https://cevio.jp/) [![CeVIO AI](https://img.shields.io/badge/CeVIO_AI-8.4-lightgray.svg?logo=&style=flat)](https://cevio.jp/)

![screenshots](./documents/screenshots/Common_AudioConverter.png)

CeVIOのオーディオ取込は16bit/48kHzのwav形式の制限があります。これに対応した形式に自動で変換します。

<details>
<summary>詳細</summary>

##### ファイル形式変換

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

##### CeVIO取込機能

「**Send**」ボタンを押すことでCeVIOエディタにオーディオトラックとして変換済みの音声ファイルを自動で取り込みます。

- ※ 変換済みの音声ファイルは元のファイルの隣に作られます。
- ※ `.ccst` ファイルがCeVIOのエディタに関連付けられている必要があります
- ※この機能は1ファイルのみ対応です。

なお、オーディオの開始秒数は事前に設定できます。

</details>

---

### トーク向け機能

#### キャストを別トラック振り分け

[![CeVIO CS](https://img.shields.io/badge/CeVIO_Creative_Studio-7.0-d08cbb.svg?logo=&style=flat)](https://cevio.jp/) [![CeVIO AI](https://img.shields.io/badge/CeVIO_AI-8.4-lightgray.svg?logo=&style=flat)](https://cevio.jp/)

![screenshots](./documents/screenshots/Talk_CastSplitter.PNG)

<details>

通常、ひとつのトークトラックには複数のキャストが記録されます。

これをキャストごとに別々のトラックに振り分けようとすると手作業が大変ですが、この「**キャストを別トラック振り分け**」機能を使うと自動で振り分けられます。

|振り分け前|振り分け後|
|---------|---------|
|![screenshots](./documents/screenshots/Talk_CastSplitter_Before.png)|![after](./documents/screenshots/Talk_CastSplitter_After.png)|

※CeVIOのトラックは32トラックが最大のため、合計で32トラック以上になる場合は何が起きるかわかりません。。。

※現在ベータ版ではキャスト名は内部のIDで表示されます。

</details>

---

### ソング向け機能

#### ブレス削除・抑制

[![CeVIO CS](https://img.shields.io/badge/CeVIO_Creative_Studio-7.0-d08cbb.svg?logo=&style=flat)](https://cevio.jp/) [![CeVIO AI](https://img.shields.io/badge/CeVIO_AI-8.4-lightgray.svg?logo=&style=flat)](https://cevio.jp/) [![VoiSona](https://img.shields.io/badge/VoiSona-1.3-53abdb.svg?logo=&style=flat)](https://voisona.com/)

![screenshots](./documents/screenshots/Song_BreathSuppressor.png)

<details>

#### ブレス消去

タイミング情報をもとに自動でブレス部分の`VOL`をけずります。すでに調整済みのデータでも対応しています（ブレス部分の`VOL`だけが削られます）。

|ブレス消去前|ブレス消去後|
|-----------|-----------|
|![screenshots](./documents/screenshots/Song_BreathSuppressor_Before.png)|![after](./documents/screenshots/Song_BreathSuppressor_After.png)|

CeVIOトラックファイル (`.ccst`) とタイミング情報ファイル (`.lab`) を一緒にドラッグ＆ドロップしてください。トラックファイルだけでも同じ名前のタイミング情報があれば自動で読み取ります。

※ブレスを復活させたい場合は、`VOL`の線を消しゴムで消せば戻ります。最初に一括で消して、ブレスを入れたいところで復活させる…といった使い方を想定しています。

#### ブレス抑制

※音量を抑える抑制機能は将来的に実装予定

</details>

---

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