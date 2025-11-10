# Blazor レンダーモード設定ガイド

## 概要

このドキュメントでは、Blazor Web App における Interactive Server レンダーモードの設定方法について説明します。
本プロジェクトでは、**グローバルなインタラクティブサーバーレンダーモード**を採用しており、アプリケーション全体で統一されたレンダリング方式を使用しています。

## レンダーモードとは

Blazor Web App では、コンポーネントのレンダリング方法を制御する「レンダーモード」という概念があります。
レンダーモードは、コンポーネントがどこでレンダリングされ、インタラクティブ性を持つかを決定します。

### 主なレンダーモードの種類

| レンダーモード | 説明 | レンダリング場所 | インタラクティブ |
|---------------|------|-----------------|-----------------|
| **Static Server** | 静的サーバーサイドレンダリング (SSR) | サーバー | ❌ なし |
| **Interactive Server** | インタラクティブサーバーサイドレンダリング (SignalR経由) | サーバー | ✔️ あり |
| **Interactive WebAssembly** | クライアントサイドレンダリング (CSR) | クライアント | ✔️ あり |
| **Interactive Auto** | 初回は Server、その後 WebAssembly | サーバー → クライアント | ✔️ あり |

## 本プロジェクトの設定: グローバル Interactive Server

### 採用理由

本プロジェクトでは、以下の理由により**グローバルな Interactive Server** レンダーモードを採用しています：

1. **一貫性**: 全ページで同じレンダリング方式を使用することで、動作が予測しやすい
2. **シンプルさ**: 各ページに `@rendermode` ディレクティブを記述する必要がない
3. **SignalR の利点**: リアルタイム通知機能を活用しやすい
4. **デバッグの容易さ**: サーバーサイドでのデバッグが可能

### 設定方法

#### 1. Program.cs での設定

`Program.cs` で Interactive Server コンポーネントのサポートを有効化します：

```csharp
// Blazor コンポーネントサービスの追加
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// アプリケーション構成
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

#### 2. App.razor でのグローバル設定

`Components/App.razor` ファイルで、`Routes` と `HeadOutlet` コンポーネントにグローバルな Interactive Server レンダーモードを適用します：

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <!-- ... その他のヘッダー要素 ... -->
    <HeadOutlet @rendermode="InteractiveServer" />
</head>

<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

**重要なポイント:**
- `Routes` コンポーネントに `@rendermode="InteractiveServer"` を指定することで、ルーティングされる全ページが自動的に Interactive Server モードを継承します
- `HeadOutlet` コンポーネントにも同じレンダーモードを指定することで、動的な `<head>` 要素の更新が可能になります

#### 3. 個別のページ設定が不要

グローバル設定により、各ページやコンポーネントで `@rendermode` ディレクティブを指定する必要はありません：

```razor
@page "/employees"
@* @rendermode InteractiveServer ← 不要! *@

<PageTitle>従業員一覧</PageTitle>

@code {
    // コンポーネントは自動的に Interactive Server モードで動作します
}
```

### _Imports.razor の設定

`Components/_Imports.razor` に以下の using ディレクティブが含まれていることを確認してください：

```razor
@using static Microsoft.AspNetCore.Components.Web.RenderMode
```

これにより、`InteractiveServer` などのレンダーモードを短い構文で使用できます。

## レンダーモードの伝播

Blazor では、親コンポーネントから子コンポーネントへレンダーモードが伝播します：

```
App (Static SSR)
  └─ Routes (@rendermode="InteractiveServer")
      └─ Page Components (Interactive Server を継承)
          └─ Child Components (Interactive Server を継承)
```

### 伝播のルール

1. **デフォルトは Static**: 何も指定しない場合は静的サーバーサイドレンダリング
2. **親から継承**: 子コンポーネントは親のレンダーモードを自動的に継承
3. **異なるモードへの切り替えは不可**: Interactive Server の子に Interactive WebAssembly を適用することはできません
4. **パラメータのシリアライズ**: Static 親から Interactive 子へパラメータを渡す場合は JSON シリアライズ可能である必要があります

## プリレンダリング

Interactive Server レンダーモードは、デフォルトでプリレンダリングが有効になっています。
これにより、初期表示が高速化されます：

1. **初回リクエスト**: ページが静的にサーバーからレンダリングされる
2. **SignalR 接続確立**: ブラウザと SignalR 接続が確立される
3. **インタラクティブ化**: コンポーネントがインタラクティブになり、イベントハンドリングが有効になる

### プリレンダリングを無効化する場合

特定のコンポーネントでプリレンダリングを無効化したい場合：

```razor
@rendermode @(new InteractiveServerRenderMode(prerender: false))
```

ただし、通常はプリレンダリングを有効にしたままにすることを推奨します。

## ベストプラクティス

### 1. グローバル設定を維持する

- 特別な理由がない限り、グローバル設定を使用する
- 個別のページで `@rendermode` を指定しない

### 2. ステートフルなコンポーネント設計

Interactive Server モードでは、SignalR 接続が維持されている間、コンポーネントの状態が保持されます：

```csharp
@code {
    private int _counter = 0;  // 接続中は状態が保持される
    
    private void IncrementCounter()
    {
        _counter++;  // UI が自動的に更新される
    }
}
```

### 3. 非同期パターンの活用

```csharp
protected override async Task OnInitializedAsync()
{
    _employees = await EmployeeApiClient.GetAllEmployeesAsync();
}

private async Task HandleSubmitAsync()
{
    await EmployeeApiClient.CreateEmployeeAsync(model);
    // 完了後に UI を更新
    StateHasChanged();
}
```

### 4. IDisposable の実装

イベントハンドラーやリソースのクリーンアップが必要な場合は `IDisposable` を実装します：

```csharp
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        AuthStateService.OnAuthStateChanged += StateHasChanged;
    }

    public void Dispose()
    {
        AuthStateService.OnAuthStateChanged -= StateHasChanged;
    }
}
```

## トラブルシューティング

### Q: ボタンをクリックしても反応しない

**A:** 以下を確認してください：
1. `Program.cs` で `AddInteractiveServerComponents()` と `AddInteractiveServerRenderMode()` が呼ばれているか
2. `App.razor` で `Routes` に `@rendermode="InteractiveServer"` が設定されているか
3. ブラウザの開発者ツールで SignalR 接続エラーが出ていないか

### Q: ページ遷移時に状態がリセットされる

**A:** これは期待される動作です。Interactive Server モードでは、ページ遷移時にコンポーネントが再初期化されます。
状態を保持したい場合は、以下のいずれかを検討してください：
- Blazor の状態管理サービス（例: `AuthStateService`）を使用
- ブラウザのローカルストレージを使用
- URL クエリパラメータで状態を渡す

### Q: プリレンダリング中にエラーが発生する

**A:** プリレンダリング中は JavaScript が実行できません。`OnAfterRenderAsync` を使用してください：

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // JavaScript 呼び出しはここで実行
        await JSRuntime.InvokeVoidAsync("myFunction");
    }
}
```

## 参考リンク

- [ASP.NET Core Blazor render modes - Microsoft Learn](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/render-modes)
- [Tooling for ASP.NET Core Blazor - Microsoft Learn](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/tooling)
- [Prerender ASP.NET Core Razor components - Microsoft Learn](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/prerender)

## 変更履歴

| 日付 | バージョン | 変更内容 |
|------|-----------|---------|
| 2025-11-10 | 1.0 | 初版作成 - グローバル Interactive Server レンダーモード設定の文書化 |

---

**関連ドキュメント:**
- [アーキテクチャ概要](architecture.md)
- [詳細アーキテクチャ設計書](architecture-detailed.md)
- [開発ガイド](development-guide.md)
