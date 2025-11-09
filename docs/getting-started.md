# Getting Started

このドキュメントでは、従業員管理システムの開発環境のセットアップと実行方法について説明します。

## 前提条件

### 必須
- **.NET 9 SDK** (9.0.305以降)
  - [ダウンロード](https://dotnet.microsoft.com/download/dotnet/9.0)
  - インストール確認: `dotnet --version`

- **.NET Aspire Workload**
  ```bash
  dotnet workload install aspire
  ```

### 推奨
- **Visual Studio 2022 (v17.12以降)** または **Visual Studio Code**
  - VS Code拡張機能:
    - C# Dev Kit
    - .NET Aspire
- **Git**

## セットアップ手順

### 1. リポジトリのクローン

```bash
git clone https://github.com/runceel/DotnetEmployeeManagementSystem.git
cd DotnetEmployeeManagementSystem
```

### 2. 依存関係の復元

```bash
dotnet restore
```

### 3. ビルド

```bash
dotnet build
```

### 4. テストの実行

```bash
dotnet test
```

## アプリケーションの実行

### Aspire AppHostを使用した実行（推奨）

Aspire AppHostを使用すると、すべてのサービスとBlazor Webアプリが自動的に起動し、Aspireダッシュボードで監視できます。

```bash
dotnet run --project src/AppHost
```

**起動時に実行されること:**
- ✅ SQLiteデータベース（employeedb, authdb, notificationdb）の自動作成
- ✅ Redis サーバーの起動（Pub/Subメッセージング用）
- ✅ EmployeeService API の起動
- ✅ AuthService API の起動
- ✅ NotificationService API の起動
- ✅ BlazorWeb UI の起動
- ✅ Aspireダッシュボードの起動とブラウザで自動オープン
- ✅ OpenTelemetryによる可観測性の有効化
- ✅ Health checksの有効化
- ✅ サービスディスカバリーの構成

**起動後のアクセス:**
- **Aspireダッシュボード**: `https://localhost:15001` または `http://localhost:15000`
  - コンソールに表示されるURLを確認してください
- **BlazorWeb**: ダッシュボードの「Resources」タブから確認（通常 `https://localhost:7000`）
- **EmployeeService API**: ダッシュボードの「Resources」タブから確認（通常 `https://localhost:7001`）
- **AuthService API**: ダッシュボードの「Resources」タブから確認（通常 `https://localhost:7002`）
- **NotificationService API**: ダッシュボードの「Resources」タブから確認（通常 `https://localhost:7003`）
- **Redis**: ダッシュボードの「Resources」タブで接続情報を確認

**Aspireダッシュボードの機能:**
- 📊 **リソースビュー**: すべてのサービスの状態とエンドポイントを確認
- 📝 **コンソールログ**: リアルタイムログの表示とフィルタリング
- 🔍 **構造化ログ**: 詳細なログ検索と分析
- 🔗 **トレース**: サービス間のリクエストフローを可視化
- 📈 **メトリクス**: パフォーマンス指標のモニタリング
- 🌐 **環境変数**: 各サービスの設定を確認

詳細は[Aspireダッシュボードガイド](aspire-dashboard.md)を参照してください。

### アプリケーションの停止

```bash
# コンソールでCtrl+Cを押す
# または
pkill -f AppHost
```

### 個別サービスの実行

開発中に特定のサービスのみを実行したい場合：

```bash
# EmployeeService API
dotnet run --project src/Services/EmployeeService/API

# AuthService API
dotnet run --project src/Services/AuthService/API

# BlazorWeb
dotnet run --project src/WebApps/BlazorWeb
```

**注意**: 個別起動時は、以下の機能が利用できません：
- Aspireダッシュボード
- 統合的なトレーシング
- 自動サービスディスカバリー
- SQLiteデータベースの自動管理

## Visual Studioでの実行

1. `DotnetEmployeeManagementSystem.sln` を開く
2. スタートアッププロジェクトを `AppHost` に設定
3. F5キーを押すか、デバッグ > デバッグの開始
4. Aspireダッシュボードが自動的に開きます

## Visual Studio Codeでの実行

1. リポジトリをVS Codeで開く
2. `Run and Debug` パネルを開く (Ctrl+Shift+D / Cmd+Shift+D)
3. `Aspire AppHost` 構成を選択
4. F5キーを押す

## トラブルシューティング

### ポートが既に使用されている

Aspireは動的にポートを割り当てますが、問題が発生した場合は：
```bash
# すべてのdotnetプロセスを停止
pkill -f dotnet
```

### Aspire Workloadのインストールエラー

```bash
# Aspire Workloadを再インストール
dotnet workload update
dotnet workload install aspire
```

### ビルドエラー

```bash
# クリーンビルド
dotnet clean
dotnet build
```

## 次のステップ

- [アーキテクチャ概要](architecture.md)を読む
- [開発ガイド](development-guide.md)を確認する
- [通知サービス実装ガイド](notification-service.md)で通知機能について学ぶ
- [Aspireダッシュボード](aspire-dashboard.md)の使い方を学ぶ
