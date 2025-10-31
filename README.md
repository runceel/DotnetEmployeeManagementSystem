# 従業員管理システム

.NET 9 + .NET Aspireを使用したマイクロサービスアーキテクチャの従業員管理システム

## 技術スタック
- .NET 9
- .NET Aspire
- Blazor Web App
- MudBlazor
- Entity Framework Core 9
- SQLite（開発環境）
- ASP.NET Core Identity

## プロジェクト構成
- `src/AppHost` - Aspireオーケストレーション
- `src/ServiceDefaults` - 共通設定
- `src/Services/EmployeeService` - 従業員管理サービス
- `src/Services/AuthService` - 認証サービス
- `src/WebApps/BlazorWeb` - フロントエンド
- `src/Shared/Contracts` - 共有DTO

## 実行方法
```bash
dotnet run --project src/AppHost
```

Aspireダッシュボードが起動し、すべてのサービスを管理できます。