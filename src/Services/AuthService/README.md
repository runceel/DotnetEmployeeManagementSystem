# 認証サービス (AuthService)

ユーザー認証とアクセス制御を担当するマイクロサービスです。

## 機能

- ✅ ユーザー登録（POST /api/auth/register）
- ✅ ログイン（POST /api/auth/login）
- ✅ トークンベース認証（ダミー実装）
- ⏳ ロールベースアクセス制御（将来対応）
- ⏳ ログアウト（将来対応）

## 技術スタック

- ASP.NET Core Identity 9.0
- Entity Framework Core 9.0
- SQLite（開発環境）
- Clean Architecture（Domain, Application, Infrastructure, API層）

## アーキテクチャ

```
AuthService/
├── Domain/              # エンティティとドメインロジック
│   └── Entities/
│       └── ApplicationUser.cs
├── Application/         # DTOとサービスインターフェース
│   ├── DTOs/
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   └── AuthResponse.cs
│   └── Services/
│       └── IAuthService.cs
├── Infrastructure/      # データアクセスとサービス実装
│   ├── Data/
│   │   ├── AuthDbContext.cs
│   │   └── DbInitializer.cs
│   ├── Services/
│   │   └── AuthService.cs
│   └── Migrations/
└── API/                 # Webエンドポイント
    └── Program.cs
```

## APIエンドポイント

### ユーザー登録
```bash
POST /api/auth/register
Content-Type: application/json

{
  "userName": "newuser",
  "email": "newuser@example.com",
  "password": "Password123!"
}

# レスポンス (201 Created)
{
  "userId": "guid",
  "userName": "newuser",
  "email": "newuser@example.com",
  "token": "base64-encoded-dummy-token"
}
```

### ログイン
```bash
POST /api/auth/login
Content-Type: application/json

{
  "userNameOrEmail": "testuser",  # ユーザー名またはメールアドレス
  "password": "Password123!"
}

# レスポンス (200 OK)
{
  "userId": "guid",
  "userName": "testuser",
  "email": "testuser@example.com",
  "token": "base64-encoded-dummy-token"
}
```

## ダミーユーザー

アプリケーション起動時に以下のダミーユーザーが自動的に作成されます：

| ユーザー名 | メールアドレス | パスワード |
|----------|--------------|----------|
| testuser | testuser@example.com | Password123! |
| admin | admin@example.com | Admin123! |

## 実行方法

```bash
# AuthService APIを実行
cd src/Services/AuthService/API
dotnet run

# ブラウザで Swagger UI にアクセス
# http://localhost:5130/openapi/v1.json
```

## テスト

```bash
# ユニットテストを実行
cd tests/AuthService.Tests
dotnet test

# すべてのテストを実行
dotnet test
```

テストは7つのシナリオをカバーしています：
- 有効な認証情報でのログイン
- 無効なユーザー名でのログイン失敗
- 無効なパスワードでのログイン失敗
- 有効なリクエストでのユーザー登録
- 既存のユーザー名での登録失敗
- 既存のメールアドレスでの登録失敗
- メールアドレスを使用したログイン

## データベース

開発環境ではSQLiteを使用してユーザー情報を保存しています。

- データベースファイル: `auth.db`（自動生成）
- マイグレーション: `Infrastructure/Migrations/`

### マイグレーション管理

```bash
# 新しいマイグレーションを作成
cd src/Services/AuthService/Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../API

# データベースを更新（アプリケーション起動時に自動実行）
# マニュアルで実行する場合：
dotnet ef database update --startup-project ../API
```

## セキュリティについて

### 現在の実装（ダミー認証）

- トークン生成は Base64 エンコードされた文字列（ユーザーID:ユーザー名:タイムスタンプ）
- 署名なし、暗号化なし
- **本番環境では使用しないでください**

### 将来的な対応

本番環境では以下の実装を予定しています：

1. **JWT（JSON Web Token）** - 署名付きトークン
2. **Microsoft Entra ID（旧Azure AD）統合** - エンタープライズ認証
3. **OAuth 2.0 / OpenID Connect** - 標準プロトコル対応
4. **リフレッシュトークン** - トークンの更新機能
5. **多要素認証（MFA）** - 追加のセキュリティ層

## 開発時の注意事項

- パスワードは ASP.NET Core Identity のデフォルトルールに従います：
  - 最小8文字
  - 大文字、小文字、数字、特殊文字をそれぞれ1文字以上含む
- ユーザー名とメールアドレスは一意である必要があります
- `auth.db` ファイルは `.gitignore` に含まれています
