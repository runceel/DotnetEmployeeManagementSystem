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

実行後、以下のURLにアクセスできます：
- **Aspireダッシュボード**: ブラウザが自動的に開きます
- **BlazorWeb**: ダッシュボードのリソースリストから確認
- **EmployeeService API**: ダッシュボードのリソースリストから確認
- **AuthService API**: ダッシュボードのリソースリストから確認

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

## Visual Studioでの実行

1. `DotnetEmployeeManagementSystem.sln` を開く
2. スタートアッププロジェクトを `AppHost` に設定
3. F5キーを押すか、デバッグ > デバッグの開始

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
- [Aspireダッシュボード](aspire-dashboard.md)の使い方を学ぶ
