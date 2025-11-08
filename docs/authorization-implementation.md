# 従業員登録・編集機能の認可実装

## 概要

従業員情報の新規登録および編集は管理者（Adminロール）ユーザーのみが実行可能です。この文書では、認可機能の実装詳細と使用方法を説明します。

## 実装アーキテクチャ

### 1. ロール管理（AuthService）

#### ロールの種類
- **Admin**: 管理者ロール - 従業員の作成、更新、削除が可能
- **User**: 一般ユーザーロール - 従業員情報の閲覧のみ可能

#### デフォルトユーザー
システム起動時に以下のテストユーザーが自動作成されます：

| ユーザー名 | パスワード | ロール | 説明 |
|----------|----------|--------|-----|
| admin | Admin123! | Admin | 管理者アカウント |
| testuser | Password123! | User | 一般ユーザーアカウント |

### 2. API認可（EmployeeService）

#### 保護されたエンドポイント

以下のエンドポイントはAdminロールが必要です：

- `POST /api/employees` - 従業員の新規作成
- `PUT /api/employees/{id}` - 従業員情報の更新

#### 認証メカニズム

カスタム認証ハンドラー（CustomAuthenticationHandler）を使用して、HTTPヘッダーから認証情報を取得します：

- `X-User-Id`: ユーザーID
- `X-User-Name`: ユーザー名
- `X-User-Roles`: ロール（カンマ区切り）

#### レスポンスコード

| 状況 | HTTPステータスコード | 説明 |
|-----|-------------------|------|
| 認証情報なし | 401 Unauthorized | 認証ヘッダーが送信されていない |
| 権限不足 | 403 Forbidden | 認証済みだが必要なロールを持っていない |
| 成功 | 201 Created / 200 OK | 操作が正常に完了 |

### 3. UI認可（BlazorWeb）

#### 認可による表示制御

管理者（Admin）のみに以下のUI要素が表示されます：

1. **従業員一覧ページ**
   - 「従業員を追加」ボタン
   - 各従業員の「編集」ボタン

2. **従業員登録・編集ダイアログ**
   - EmployeeFormDialog コンポーネント
   - バリデーション付きフォーム

#### AuthStateService

認証状態とロール情報を管理するサービス：

```csharp
public class AuthStateService
{
    // 現在のユーザー情報
    public AuthResponse? CurrentUser { get; }
    
    // ログイン状態
    public bool IsAuthenticated { get; }
    
    // 管理者かどうか
    public bool IsAdmin { get; }
}
```

#### 使用例

```razor
@if (AuthStateService.IsAdmin)
{
    <MudButton OnClick="OpenCreateDialog">
        従業員を追加
    </MudButton>
}
```

## セキュリティ考慮事項

### 多層防御

認可チェックは以下の複数レイヤーで実装されています：

1. **UIレイヤー**: 管理者以外のユーザーには操作UIを表示しない
2. **APIレイヤー**: バックエンドで厳密な認可チェックを実施
3. **認証ヘッダー**: すべてのリクエストで認証情報を送信・検証

### ベストプラクティス

- ✅ クライアント側とサーバー側の両方で認可を実装
- ✅ 最小権限の原則に従う（デフォルトはUserロール）
- ✅ 明示的な権限チェック（`[Authorize(Roles = "Admin")]`）
- ✅ エラーメッセージは詳細すぎないようにする

## ロール割り当て手順

### 新規ユーザーへのロール割り当て

1. **ユーザー登録時**
   ```csharp
   // デフォルトでUserロールが自動割り当て
   await _authService.RegisterAsync(new RegisterRequest { ... });
   ```

2. **既存ユーザーへのAdmin権限付与**
   ```csharp
   var user = await _userManager.FindByNameAsync("username");
   await _userManager.AddToRoleAsync(user, "Admin");
   ```

3. **データベース直接操作（開発/テスト用）**
   ```sql
   -- ユーザーIDとロールIDを取得して関連付け
   INSERT INTO AspNetUserRoles (UserId, RoleId)
   SELECT Users.Id, Roles.Id
   FROM AspNetUsers Users, AspNetRoles Roles
   WHERE Users.UserName = 'targetuser' AND Roles.Name = 'Admin';
   ```

## テスト

### 統合テスト

認可機能の統合テストは `EmployeeAuthorizationTests` クラスで実装されています：

```bash
# 認可テストのみ実行
dotnet test --filter "FullyQualifiedName~EmployeeAuthorizationTests"

# すべてのテスト実行
dotnet test
```

### テストシナリオ

1. ✅ 管理者による従業員作成（成功）
2. ✅ 一般ユーザーによる従業員作成（403エラー）
3. ✅ 管理者による従業員更新（成功）
4. ✅ 一般ユーザーによる従業員更新（403エラー）
5. ✅ 全ユーザーによる従業員閲覧（成功）
6. ✅ 未認証ユーザーによる従業員作成（401エラー）

## 動作確認手順

### 1. システム起動

```bash
# プロジェクトルートで実行
dotnet run --project src/AppHost/AppHost.csproj
```

### 2. 管理者でログイン

1. ブラウザで `http://localhost:5000/login` にアクセス
2. 以下の認証情報でログイン：
   - ユーザー名: `admin`
   - パスワード: `Admin123!`

### 3. 機能確認

管理者としてログイン後、以下を確認：
- ✅ 従業員一覧ページに「従業員を追加」ボタンが表示される
- ✅ 各従業員行に「編集」ボタンが表示される
- ✅ 従業員の新規作成が可能
- ✅ 従業員情報の更新が可能

### 4. 一般ユーザーで確認

1. ログアウト
2. 以下の認証情報でログイン：
   - ユーザー名: `testuser`
   - パスワード: `Password123!`
3. 確認事項：
   - ❌ 「従業員を追加」ボタンが表示されない
   - ❌ 「編集」ボタンが表示されない
   - ✅ 従業員一覧の閲覧は可能
   - ✅ 従業員詳細の閲覧は可能

## トラブルシューティング

### 問題: 管理者でログインしても編集ボタンが表示されない

**原因**: ロール情報が正しく設定されていない可能性があります。

**解決策**:
1. データベースを削除して再起動
   ```bash
   rm src/Services/AuthService/API/auth.db
   dotnet run --project src/AppHost/AppHost.csproj
   ```

2. ユーザーのロールを確認
   ```sql
   SELECT u.UserName, r.Name as RoleName
   FROM AspNetUsers u
   JOIN AspNetUserRoles ur ON u.Id = ur.UserId
   JOIN AspNetRoles r ON ur.RoleId = r.Id;
   ```

### 問題: APIが403エラーを返す

**原因**: 認証ヘッダーが正しく送信されていない可能性があります。

**解決策**:
1. ブラウザの開発者ツールでネットワークタブを確認
2. リクエストヘッダーに以下が含まれているか確認：
   - `X-User-Id`
   - `X-User-Name`
   - `X-User-Roles`

### 問題: テストユーザーでロールエラーが発生

**原因**: テスト環境でロールが作成されていない可能性があります。

**解決策**:
```csharp
// テストのセットアップで必ずロールを作成
var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
await roleManager.CreateAsync(new IdentityRole("Admin"));
await roleManager.CreateAsync(new IdentityRole("User"));
```

## まとめ

この実装により、従業員情報の登録・編集は管理者のみが実行可能となり、不正な操作を防止できます。UI・API両方での認可チェックにより、セキュアなシステムを実現しています。

## 関連ドキュメント

- [アーキテクチャ設計](./architecture.md)
- [開発ガイド](./development-guide.md)
- [データベース設計](./database.md)
