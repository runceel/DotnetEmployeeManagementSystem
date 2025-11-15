# Issue #115 補足資料: オプション設定ファイルの例

このドキュメントは Issue #115 調査報告の補足資料として、将来的に必要になる可能性のあるオプション設定ファイルの例を提供します。

**重要**: これらのファイルは現時点では不要です。特殊なカスタマイズが必要になった場合の参考資料としてご利用ください。

---

## 例1: global.json（SDK バージョン固定）

**目的**: 特定の .NET SDK バージョンを強制する場合に使用  
**配置場所**: リポジトリルート（`/global.json`）  
**必要性**: 現時点では不要（環境の .NET 10.0.100 で正常動作中）

```json
{
  "$schema": "https://json.schemastore.org/global",
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMinor",
    "allowPrerelease": false
  },
  "msbuild-sdks": {
    "Aspire.AppHost.Sdk": "13.0.0"
  }
}
```

**設定の説明**:
- `version`: 要求する最小 SDK バージョン
- `rollForward`: バージョンロールフォワードポリシー
  - `latestMinor`: 指定されたメジャーバージョン内で最新のマイナーバージョンを使用
  - `major`: 最新のメジャーバージョンまでロールフォワード可能
  - `disable`: 厳密にバージョンを固定
- `allowPrerelease`: プレリリース版の使用を許可するかどうか

**使用ケース**:
- CI/CD パイプラインでのビルド再現性を保証したい場合
- チーム全体で同じ SDK バージョンを使用することを強制したい場合
- プレリリース版との互換性問題を避けたい場合

---

## 例2: .github/copilot-instructions.md（Agent向けガイダンス）

**目的**: GitHub Copilot Coding Agent に特化した指示を提供  
**配置場所**: `.github/copilot-instructions.md`  
**必要性**: 現時点では不要（AGENTS.md と .github/agents/csharp-expert.md で十分）

```markdown
# GitHub Copilot Instructions for DotnetEmployeeManagementSystem

## Project Overview

This is a microservices-based employee management system built with .NET 10 and .NET Aspire.

## Environment Requirements

### .NET SDK

- **Minimum Version**: 10.0.100
- **Target Framework**: net10.0
- **C# Language Version**: 14 (default for .NET 10)

### Key Technologies

- **.NET Aspire**: 13.0.0 (as NuGet package, not workload)
- **Entity Framework Core**: 10.0.0
- **ASP.NET Core**: 10.0.0
- **Blazor**: .NET 10 with MudBlazor 8.14.0

## Build & Test Commands

Always use these commands:

\`\`\`bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the application (via Aspire AppHost)
dotnet run --project src/AppHost

# Create EF Core migrations (example)
cd src/Services/EmployeeService/API
dotnet ef migrations add MigrationName --project ../Infrastructure
dotnet ef database update --project ../Infrastructure
\`\`\`

## Architecture Pattern

This project follows Clean Architecture with the following layers:

- **Domain**: Core business entities and repository interfaces
- **Application**: Business logic and use cases
- **Infrastructure**: Data access implementation (EF Core)
- **API**: Presentation layer (Web API endpoints)

## Important Notes

1. **Aspire Usage**: Aspire is used as NuGet packages (Aspire.Hosting.AppHost, etc.), not as a workload installation.

2. **Database**: SQLite is used for development. Each service has its own database file in the `data/` directory.

3. **Service Communication**:
   - Synchronous: HTTP/REST via Aspire Service Discovery
   - Asynchronous: Redis Pub/Sub for events

4. **Testing**:
   - Unit tests use xUnit
   - Integration tests use WebApplicationFactory
   - Follow the Arrange-Act-Assert pattern

5. **Code Style**:
   - PascalCase for classes and methods
   - camelCase for variables and parameters
   - Nullable reference types are enabled
   - Async methods must have the "Async" suffix

## Documentation

For comprehensive guidelines, always refer to:

- **AGENTS.md**: Complete project overview and guidelines
- **.github/agents/csharp-expert.md**: .NET 10 / C# 14 specific guidance
- **docs/**: Architecture, development guide, and service specifications

## Common Tasks

### Adding a new entity

1. Create the entity in the Domain layer
2. Add repository interface in Domain
3. Implement repository in Infrastructure
4. Create use cases in Application layer
5. Add API endpoints in the API layer
6. Write unit and integration tests

### Creating a database migration

\`\`\`bash
cd src/Services/{ServiceName}/API
dotnet ef migrations add {MigrationName} --project ../Infrastructure
dotnet ef database update --project ../Infrastructure
\`\`\`

### Adding a new microservice

Follow the pattern of existing services:
- Create Domain, Application, Infrastructure, API projects
- Register with Aspire AppHost
- Add service discovery configuration
- Implement health checks

## Troubleshooting

- **Build errors**: Ensure .NET 10 SDK is being used (`dotnet --version`)
- **Aspire issues**: Verify NuGet packages are restored
- **Database errors**: Check that migrations are applied
- **Port conflicts**: Aspire dynamically assigns ports; check dashboard

For more troubleshooting, see [docs/getting-started.md](../docs/getting-started.md).
```

**使用ケース**:
- Copilot Agent に対して、AGENTS.md よりも簡潔で直接的な指示を与えたい場合
- プロジェクト固有のコマンドやベストプラクティスを強調したい場合
- ドキュメントへのナビゲーションを簡略化したい場合

**注意**: この内容は AGENTS.md と重複する部分が多いため、現時点では追加しないことを推奨します。

---

## 例3: .github/workflows/copilot-setup-steps.yml（環境セットアップワークフロー）

**目的**: Copilot Agent 環境に追加のツールや依存関係をインストール  
**配置場所**: `.github/workflows/copilot-setup-steps.yml`  
**必要性**: 現時点では不要（標準環境で全て動作中）

```yaml
name: "Copilot Agent Setup Steps"

# このワークフローは GitHub Copilot Coding Agent 環境で自動実行されます
# 特殊な依存関係やツールのインストールが必要な場合に使用します

on:
  workflow_dispatch:
  push:
    paths:
      - '.github/workflows/copilot-setup-steps.yml'

jobs:
  setup:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Verify .NET SDK version
        run: |
          echo "=== Installed .NET SDKs ==="
          dotnet --list-sdks
          
          echo ""
          echo "=== Active .NET SDK ==="
          dotnet --version
          
          # .NET 10.0.100 以上が必要
          DOTNET_VERSION=$(dotnet --version)
          if [[ ! "$DOTNET_VERSION" =~ ^10\. ]]; then
            echo "Error: .NET 10 SDK is required but found $DOTNET_VERSION"
            exit 1
          fi
          
          echo "✅ .NET 10 SDK is available"

      - name: Verify Aspire packages (optional check)
        run: |
          echo "=== Checking Aspire availability ==="
          # Aspire は NuGet パッケージとして利用されるため、workload は不要
          dotnet workload list || true
          
          echo ""
          echo "Aspire is used as NuGet packages (Aspire.Hosting.AppHost, etc.)"
          echo "No workload installation is required."

      - name: Restore dependencies
        run: |
          echo "=== Restoring NuGet packages ==="
          dotnet restore
          
      - name: Build solution
        run: |
          echo "=== Building solution ==="
          dotnet build --no-restore

      - name: Run tests
        run: |
          echo "=== Running tests ==="
          dotnet test --no-build --verbosity normal

      - name: Environment verification complete
        run: |
          echo ""
          echo "=========================================="
          echo "✅ Environment verification complete"
          echo "=========================================="
          echo ""
          echo "The GitHub Copilot Coding Agent environment is ready for:"
          echo "  - .NET 10 development"
          echo "  - Aspire-based microservices"
          echo "  - Entity Framework Core migrations"
          echo "  - xUnit testing"
          echo ""
```

**使用ケース**:
- プロジェクトに特殊な CLI ツールが必要な場合（例: Azure CLI, AWS CLI など）
- カスタムビルドステップが必要な場合
- 環境の健全性を自動検証したい場合

**注意**: 現在の環境では標準の .NET 10 SDK で全て動作するため、このワークフローは不要です。

---

## 例4: .github/dependabot.yml（自動依存関係更新）

**目的**: NuGet パッケージと GitHub Actions の自動更新  
**配置場所**: `.github/dependabot.yml`  
**必要性**: 推奨（セキュリティアップデートの自動検出に有用）

```yaml
version: 2

updates:
  # .NET NuGet packages
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
      day: "monday"
      time: "09:00"
      timezone: "Asia/Tokyo"
    open-pull-requests-limit: 10
    labels:
      - "dependencies"
      - "dotnet"
    commit-message:
      prefix: "chore(deps)"
      include: "scope"
    # .NET 10 に関連するパッケージを優先的に更新
    allow:
      - dependency-type: "production"
      - dependency-type: "development"
    # 特定のパッケージの更新を無視する場合（例）
    # ignore:
    #   - dependency-name: "MudBlazor"
    #     versions: ["9.x"]

  # GitHub Actions workflows
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "monthly"
    labels:
      - "dependencies"
      - "github-actions"
    commit-message:
      prefix: "chore(ci)"
```

**使用ケース**:
- NuGet パッケージのセキュリティ更新を自動検出したい場合
- 依存関係の最新バージョンへの更新を自動提案してほしい場合
- GitHub Actions のバージョン管理を自動化したい場合

**注意**: このファイルは現時点で作成してもよいですが、Central Package Management（Directory.Packages.props）との併用に注意が必要です。

---

## 例5: .editorconfig（コーディングスタイル統一）

**目的**: C# コーディングスタイルをエディタ間で統一  
**配置場所**: リポジトリルート（`/.editorconfig`）  
**必要性**: 推奨（チーム開発でのコード品質向上）

```editorconfig
# EditorConfig is awesome: https://EditorConfig.org

# Top-most EditorConfig file
root = true

# All files
[*]
charset = utf-8
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space

# Code files
[*.{cs,csx,vb,vbx}]
indent_size = 4

# XML project files
[*.{csproj,vbproj,vcxproj,vcxproj.filters,proj,projitems,shproj}]
indent_size = 2

# XML config files
[*.{props,targets,ruleset,config,nuspec,resx,vsixmanifest,vsct}]
indent_size = 2

# JSON files
[*.json]
indent_size = 2

# YAML files
[*.{yml,yaml}]
indent_size = 2

# Markdown files
[*.md]
trim_trailing_whitespace = false

# Shell scripts
[*.sh]
end_of_line = lf

# C# files
[*.cs]

# New line preferences
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true

# Indentation preferences
csharp_indent_case_contents = true
csharp_indent_switch_labels = true

# Space preferences
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true

# Style preferences
csharp_prefer_braces = true:suggestion
csharp_prefer_simple_using_statement = true:suggestion
csharp_style_namespace_declarations = file_scoped:suggestion

# Naming conventions
dotnet_naming_rule.interface_should_be_begins_with_i.severity = suggestion
dotnet_naming_rule.interface_should_be_begins_with_i.symbols = interface
dotnet_naming_rule.interface_should_be_begins_with_i.style = begins_with_i

dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_symbols.interface.applicable_accessibilities = public, internal, private, protected, protected_internal, private_protected

dotnet_naming_style.begins_with_i.required_prefix = I
dotnet_naming_style.begins_with_i.capitalization = pascal_case

# Code style rules
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_event = false:suggestion

# Modern C# features (C# 14 / .NET 10)
csharp_prefer_implicit_object_creation = true:suggestion
csharp_prefer_inferred_tuple_names = true:suggestion
csharp_prefer_inferred_anonymous_type_member_names = true:suggestion
csharp_style_pattern_matching_over_as_with_null_check = true:suggestion
csharp_style_pattern_matching_over_is_with_cast_check = true:suggestion

# Nullable reference types
dotnet_diagnostic.CS8618.severity = warning
```

**使用ケース**:
- チーム全体で一貫したコーディングスタイルを保ちたい場合
- Visual Studio、VS Code、Rider など異なるエディタで同じフォーマットを使いたい場合
- コードレビューでのスタイル議論を減らしたい場合

**注意**: このファイルは導入を検討する価値がありますが、既存コードへの適用には大規模な変更が伴う可能性があります。

---

## まとめ

### 現時点で必要なファイル

なし（既存の設定で十分動作中）

### 将来的に検討すべきファイル（優先度順）

1. **高優先度**: なし
2. **中優先度**: 
   - `.github/dependabot.yml` - セキュリティ更新の自動検出
   - `.editorconfig` - コーディングスタイル統一
3. **低優先度**: 
   - `global.json` - SDK バージョン固定が必要になった場合
   - `.github/copilot-instructions.md` - Copilot Agent 向けの簡潔な指示が必要な場合
   - `.github/workflows/copilot-setup-steps.yml` - 特殊な環境構築が必要な場合

### 推奨アクション

現時点では、これらのオプション設定ファイルを追加する必要はありません。プロジェクトの成長や要件の変化に応じて、必要になったタイミングで追加を検討してください。

---

**関連資料**:
- [Issue #115 調査報告](issue-115-dotnet10agent.md) - メイン調査報告書
- [AGENTS.md](../../AGENTS.md) - プロジェクト全体のガイドライン
- [.github/agents/csharp-expert.md](../agents/csharp-expert.md) - .NET 10 / C# 14 ガイダンス

**作成日**: 2025-11-15  
**最終更新**: 2025-11-15
