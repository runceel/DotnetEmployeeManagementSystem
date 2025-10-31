# Blazor Web App

従業員管理システムのフロントエンドアプリケーションです。

## 技術スタック

- Blazor Web App (.NET 9)
- MudBlazor - Material Design UIコンポーネントライブラリ
- .NET Aspire Service Defaults

## MudBlazorについて

MudBlazorは、Material Designに基づいた包括的なBlazor UIコンポーネントライブラリです。以下の機能を提供します：

- リッチなUIコンポーネント（テーブル、フォーム、ダイアログなど）
- レスポンシブデザイン
- テーマカスタマイズ
- アクセシビリティ対応

## 開発

```bash
# Aspire経由で実行（推奨）
dotnet run --project ../../AppHost

# または直接実行
dotnet run
```

## 構成

- `/Components` - Blazorコンポーネント
- `/Pages` - ページコンポーネント
- `/Services` - クライアントサービス（API通信など）
