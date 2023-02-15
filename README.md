# SasaraUtil

*[SasaraUtil]()*（ささらユーティル）はCeVIOエディタのあれこれを使いやすくする補助ツールです。

## 機能 / Functions

- 共通向け
  - [オーディオトラック変換](/###オーディオトラック変換)
- CeVIOトーク向け
  - _coming soon_
- CeVIOソング向け
  - _coming soon_

### オーディオトラック変換

CeVIOのオーディオ取込は16bit/48kHzのwav形式の制限があります。自動で対応形式に変換します。

#### ファイル形式変換

SasaraUtilの「**オーディオトラック変換**」にファイルをドラッグ＆ドロップすると、対応している音声ファイルを自動で16bit/48kHzのwav形式に変換します。

動画ファイルも音声ファイルに変換できます。

- 対応形式
  - 音声ファイル (wav, mp3, aiff, etc...)
  - 動画ファイル（mp4, etc...）

変換されたファイルは、元のファイルと同じ場所に `【元のファイル名】.16bit48kHz.wav` という名前で保存されます。

#### CeVIO取込機能

「送る」ボタンを押すことでCeVIOエディタにオーディオトラックとして取込ます。

（ccstファイルがCeVIOのエディタに関連付けられている必要があります）

## Libraries

* [Avalonia](https://avaloniaui.net/)
* [Epoxy](https://github.com/kekyo/Epoxy)

## Projects

* `SasaraUtil.Core`: Independent common component project includes MVVM `Model` code.
* `SasaraUtil.UI`: UI (independent platform) project includes MVVM `View` and `ViewModel` code.
* `SasaraUtil`: The application project code.
