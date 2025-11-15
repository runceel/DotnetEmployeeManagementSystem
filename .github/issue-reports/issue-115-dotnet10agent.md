# Issue #115: GitHub Copilot Coding Agent での .NET 10 SDK 利用に関する調査報告

## 📋 調査概要

本リポジトリの GitHub Copilot Coding Agent 環境における .NET 10 SDK の利用状況を調査し、必要な設定ファイルやカスタマイズ手順を明確化しました。

**調査日**: 2025-11-15  
**調査対象**: runceel/DotnetEmployeeManagementSystem リポジトリ  
**GitHub Copilot Agent Version**: 最新版（.NET 10.0.100 SDK プリインストール済み）

---

## 🎯 主要な発見事項

### 1. .NET 10 SDK は既に利用可能

GitHub Copilot Coding Agent 環境には、**既に .NET 10 SDK（バージョン 10.0.100）がプリインストールされています**。

```bash
$ dotnet --version
10.0.100

$ dotnet --list-sdks
8.0.122 [/usr/share/dotnet/sdk]
8.0.206 [/usr/share/dotnet/sdk]
8.0.319 [/usr/share/dotnet/sdk]
8.0.416 [/usr/share/dotnet/sdk]
9.0.112 [/usr/share/dotnet/sdk]
9.0.205 [/usr/share/dotnet/sdk]
9.0.307 [/usr/share/dotnet/sdk]
10.0.100 [/usr/share/dotnet/sdk]  ← デフォルト
```

**結論**: 追加のインストール作業は不要です。

### 2. プロジェクトは既に .NET 10 に移行済み

本リポジトリは既に .NET 10 へのアップグレードが完了しており、以下が確認できました：

- ✅ すべての `.csproj` ファイルで `<TargetFramework>net10.0</TargetFramework>` を使用
- ✅ `Directory.Packages.props` で .NET 10 対応パッケージ（バージョン 10.0.0）を使用
- ✅ Aspire 13.0.0 を使用（.NET 10 対応版）
- ✅ ビルド成功（警告のみ、エラーなし）
- ✅ すべてのテストが成功（136 テスト中 136 テストがパス）

```
Passed!  - Failed: 0, Passed: 136, Skipped: 0, Total: 136
```

### 3. エージェント設定ファイルも .NET 10 対応済み

`.github/agents/csharp-expert.md` には既に .NET 10 / C# 14 の情報が含まれています：

```markdown
# version: 2025-11-12a (Updated for .NET 10 / C# 14)
```

- C# 14 の新機能（Extension Members, Field Keyword, Implicit Span Conversions など）の説明
- .NET 10 Blazor および ASP.NET Core 10 の新機能ガイド
- .NET 10 ランタイムパフォーマンス改善の解説

---

## 📚 GitHub Copilot Agent 環境のカスタマイズ方法

調査の結果、GitHub Copilot Coding Agent の環境をカスタマイズする方法は以下の通りです。

### 方法 1: `global.json` ファイル（推奨）

特定の .NET SDK バージョンを固定したい場合、リポジトリルートに `global.json` を配置します。

**現状**: このリポジトリには `global.json` は存在しません。これにより、エージェント環境で最新の .NET SDK（10.0.100）が自動的に使用されます。

**必要に応じた設定例**:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMinor",
    "allowPrerelease": false
  }
}
```

**推奨事項**: 現状、プロジェクトは .NET 10.0.100 で正常に動作しているため、`global.json` の追加は不要です。ただし、将来的に特定のバージョンに固定したい場合は、上記のような設定を追加できます。

### 方法 2: `.github/copilot-instructions.md`（エージェント向けガイダンス）

GitHub Copilot Agent に特定の指示を与えるために、`.github/copilot-instructions.md` ファイルを作成できます。

**現状**: このファイルは存在しませんが、`.github/agents/csharp-expert.md` が同様の役割を果たしています。

**必要に応じた追加設定例**:

```markdown
# GitHub Copilot Instructions for DotnetEmployeeManagementSystem

## Environment

This project targets .NET 10.0 and uses the following:

- .NET SDK: 10.0.100+
- C#: 14 (default for .NET 10)
- Aspire: 13.0.0
- Target Framework: net10.0

## Build & Test

Always use .NET 10 SDK commands:

\`\`\`bash
dotnet build
dotnet test
dotnet run --project src/AppHost
\`\`\`

## Key Dependencies

- Aspire 13.0.0 (not installed as workload, installed as NuGet package)
- Entity Framework Core 10.0.0
- ASP.NET Core 10.0.0

Refer to `AGENTS.md` for comprehensive project guidelines.
```

**推奨事項**: 現状の `AGENTS.md` と `.github/agents/csharp-expert.md` で十分なガイダンスが提供されているため、追加ファイルは不要です。

### 方法 3: カスタム GitHub Actions ワークフロー（高度なカスタマイズ）

GitHub Copilot Agent は GitHub Actions ベースの環境で動作するため、特殊な依存関係やツールをインストールする必要がある場合、`.github/workflows/copilot-setup-steps.yml` を作成できます。

**現状**: このファイルは存在せず、現時点では不要です。

**必要に応じた設定例（参考）**:

```yaml
name: "Copilot Agent Setup Steps"

on:
  workflow_dispatch:

jobs:
  setup:
    runs-on: ubuntu-latest
    steps:
      - name: Verify .NET 10 SDK
        run: |
          dotnet --version
          dotnet --list-sdks
          
      - name: Install Aspire workload (if needed)
        run: |
          # 注: Aspire 13.0.0 は NuGet パッケージとして利用可能
          # Workload インストールは不要
          echo "Aspire is used as NuGet package, no workload installation needed"
          
      - name: Verify build
        run: |
          dotnet build
          dotnet test
```

**推奨事項**: 現状の環境で問題なくビルド・テストが実行できているため、このワークフローは不要です。

---

## 🔧 .NET Aspire 対応状況

### Aspire Workload について

調査の結果、以下のことが判明しました：

```bash
$ dotnet workload list
Workload version: 10.0.100-manifests.4c0ca8ba

Installed Workload Id      Manifest Version      Installation Source
--------------------------------------------------------------------

Use `dotnet workload search` to find additional workloads to install.
```

**現状**: Aspire workload はインストールされていませんが、これは正常です。

**理由**:
- Aspire 13.0.0（.NET 10 対応版）は NuGet パッケージとして提供されています
- `Directory.Packages.props` で以下のパッケージを使用：
  - `Aspire.Hosting.AppHost` Version="13.0.0"
  - `Aspire.Hosting.Redis` Version="13.0.0"
  - `Aspire.StackExchange.Redis` Version="13.0.0"
- プロジェクトファイルに `<Sdk Name="Aspire.AppHost.Sdk" Version="9.5.2" />` が含まれている

**注意事項**: `AppHost.csproj` の Aspire SDK バージョン（9.5.2）が古い可能性がありますが、NuGet パッケージ経由で Aspire 13.0.0 が使用されているため、実質的には .NET 10 対応済みです。

**将来の対応案**:
```xml
<!-- src/AppHost/AppHost.csproj -->
<Sdk Name="Aspire.AppHost.Sdk" Version="13.0.0" />
```

このように更新することで、SDK バージョンも統一できます。

---

## 📝 ドキュメント整合性の確認

### AGENTS.md

現在の記載:
```markdown
**.NET 9** - 最新のフレームワーク
**.NET Aspire 9.5.2** - マイクロサービスオーケストレーション
```

**更新推奨**:
```markdown
**.NET 10** - 最新のフレームワーク
**.NET Aspire 13.0.0** - マイクロサービスオーケストレーション
```

### docs/ 配下のドキュメント

同様に、以下のドキュメントも .NET 9 から .NET 10 への更新が必要な可能性があります：

- `docs/getting-started.md`
- `docs/architecture-detailed.md`
- `docs/development-guide.md`

**推奨事項**: 別途 Issue を作成し、ドキュメント全体の .NET 10 対応更新を実施することをお勧めします。

---

## 🚀 実装推奨事項

### 必須対応

なし（既に .NET 10 SDK が利用可能で、プロジェクトも対応済み）

### 推奨対応（優先度順）

#### 優先度 高

1. **AGENTS.md の更新**
   - .NET 9 → .NET 10 の記載修正
   - Aspire 9.5.2 → 13.0.0 の記載修正

2. **AppHost.csproj の Aspire SDK バージョン更新**
   ```xml
   <Sdk Name="Aspire.AppHost.Sdk" Version="13.0.0" />
   ```

#### 優先度 中

3. **docs/ ドキュメントの一括更新**
   - getting-started.md
   - architecture-detailed.md
   - development-guide.md
   - その他関連ドキュメント

4. **global.json の追加（オプション）**
   - SDK バージョンを明示的に固定したい場合のみ

#### 優先度 低

5. **.github/copilot-instructions.md の追加（オプション）**
   - 現状の AGENTS.md で十分だが、Copilot Agent 専用の簡潔な指示を追加したい場合

---

## 📖 参考情報

### 公式ドキュメント

1. **GitHub Copilot Coding Agent 環境のカスタマイズ**
   - https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/customize-the-agent-environment

2. **global.json による SDK バージョン管理**
   - https://learn.microsoft.com/ja-jp/dotnet/core/tools/global-json

3. **.NET 10 の新機能**
   - https://learn.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-10/overview

4. **C# 14 の新機能**
   - https://learn.microsoft.com/ja-jp/dotnet/csharp/whats-new/csharp-14

5. **.NET Aspire 13.0.0**
   - https://learn.microsoft.com/ja-jp/dotnet/aspire/whats-new/aspire-13

### コミュニティリソース

1. **GitHub Copilot Agent のデフォルト環境に関するディスカッション**
   - https://github.com/orgs/community/discussions/161042

2. **.NET MAUI の Copilot 指示ファイル例**
   - https://github.com/dotnet/maui/blob/main/.github/copilot-instructions.md

---

## 🏁 結論

### 現状の評価

✅ **GitHub Copilot Coding Agent 環境は既に .NET 10 SDK をサポートしています**

本リポジトリは以下の点で既に .NET 10 に対応しています：

1. ✅ Agent 環境に .NET 10.0.100 SDK がプリインストール済み
2. ✅ すべてのプロジェクトが net10.0 をターゲットに設定済み
3. ✅ NuGet パッケージが .NET 10 対応版に更新済み
4. ✅ ビルドとテストが正常に実行可能
5. ✅ エージェント設定ファイル（csharp-expert.md）が .NET 10 / C# 14 に対応済み

### 追加作業の必要性

**必須の追加設定やカスタマイズファイルは不要です。**

ただし、ドキュメントの整合性を保つために、以下の更新を推奨します：

1. AGENTS.md の .NET バージョン表記更新（.NET 9 → .NET 10）
2. Aspire SDK バージョンの統一（9.5.2 → 13.0.0）
3. docs/ 配下のドキュメント更新

これらは機能的な問題ではなく、ドキュメントの正確性向上のための推奨事項です。

### オプション設定の位置づけ

`global.json` や `.github/copilot-instructions.md` は、特殊なカスタマイズが必要になった場合の拡張オプションとして理解してください。現時点では、既存の設定で十分です。

---

## 📎 添付資料

### A. 環境確認コマンド結果

```bash
# SDK バージョン確認
$ dotnet --version
10.0.100

# インストール済み SDK 一覧
$ dotnet --list-sdks
8.0.122 [/usr/share/dotnet/sdk]
8.0.206 [/usr/share/dotnet/sdk]
8.0.319 [/usr/share/dotnet/sdk]
8.0.416 [/usr/share/dotnet/sdk]
9.0.112 [/usr/share/dotnet/sdk]
9.0.205 [/usr/share/dotnet/sdk]
9.0.307 [/usr/share/dotnet/sdk]
10.0.100 [/usr/share/dotnet/sdk]

# Workload 確認（Aspire は NuGet パッケージとして利用）
$ dotnet workload list
Workload version: 10.0.100-manifests.4c0ca8ba

Installed Workload Id      Manifest Version      Installation Source
--------------------------------------------------------------------
(Empty - Aspire is used as NuGet package)
```

### B. ビルド結果

```bash
$ dotnet build --no-incremental

# 結果: 成功（警告あり、エラーなし）
# 警告内容: WithOpenApi の廃止予定（ASPDEPR002）
# 影響: 軽微（OpenAPI エンドポイント定義の新しい方法への移行推奨）
```

### C. テスト結果

```bash
$ dotnet test

# 結果: 全テストパス
Passed!  - Failed: 0, Passed: 46, Skipped: 0, Total: 46
           AttendanceService.Domain.Tests

Passed!  - Failed: 0, Passed: 18, Skipped: 0, Total: 18
           EmployeeService.Application.Tests

Passed!  - Failed: 0, Passed: 18, Skipped: 0, Total: 18
           EmployeeService.Domain.Tests

Passed!  - Failed: 0, Passed: 9, Skipped: 0, Total: 9
           AuthService.Tests

Passed!  - Failed: 0, Passed: 45, Skipped: 0, Total: 45
           EmployeeService.Integration.Tests

Total: 136 tests, 136 passed
```

---

## 📦 実施した作業

このIssueの完了にあたり、以下の作業を実施しました：

### ドキュメント作成
1. ✅ **調査報告書** (issue-115-dotnet10agent.md)
   - 環境調査結果
   - .NET 10 SDK の利用可能性確認
   - カスタマイズ方法の調査
   - 推奨事項のまとめ

2. ✅ **オプション設定例集** (issue-115-optional-config-examples.md)
   - global.json の例
   - .github/copilot-instructions.md の例
   - GitHub Actions ワークフローの例
   - .editorconfig の例
   - dependabot.yml の例

### ドキュメント更新
3. ✅ **AGENTS.md**
   - .NET 9 → .NET 10
   - Aspire 9.5.2 → 13.0.0
   - Entity Framework Core 9 → 10
   - Aspire workload インストール手順の修正

4. ✅ **docs/getting-started.md**
   - 前提条件を .NET 10 SDK に更新
   - Aspire の取得方法を明確化

5. ✅ **docs/architecture.md**
   - Blazor 技術スタックを .NET 10 に更新

6. ✅ **docs/architecture-detailed.md**
   - .NET 9 → .NET 10 の参照更新
   - ドキュメントバージョンと対象システムバージョンの更新

7. ✅ **docs/manual/01-intro.md**
   - システム紹介の .NET バージョン更新

8. ✅ **docs/manual/04-dashboard.md**
   - 対象バージョンの更新

### 検証
9. ✅ **ビルドとテストの実行**
   - ビルド成功確認（警告のみ、エラーなし）
   - 全テスト成功確認（136/136 テストパス）

---

## 👤 作成者情報

**調査実施者**: GitHub Copilot Coding Agent  
**レビュー担当**: @runceel  
**Issue 番号**: #115  
**作成日**: 2025-11-15

---

## 📮 フィードバック

本調査報告に関する質問や追加の要望がある場合は、Issue #115 にコメントしてください。
