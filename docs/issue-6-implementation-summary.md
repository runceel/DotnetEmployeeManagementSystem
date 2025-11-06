# Issue #6 実装サマリー: BlazorWeb EmployeeService API連携

## 概要
BlazorWebからEmployeeServiceのAPIを呼び出すためのクライアント実装、エラーハンドリング、リトライ設計、およびサンプル画面の実装を完了しました。

## 実装内容

### 1. EmployeeService APIクライアント

#### インターフェース (`Services/IEmployeeApiClient.cs`)
```csharp
public interface IEmployeeApiClient
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default);
}
```

#### 実装クラス (`Services/EmployeeApiClient.cs`)
- **包括的なエラーハンドリング**
  - HTTP 404: nullを返却（リソースが見つからない場合）
  - HTTP 400/5xx: 適切な例外メッセージ
  - ユーザーフレンドリーな日本語エラーメッセージ
  
- **構造化ロギング**
  - 全API呼び出しのログ記録
  - エラー詳細の記録
  - パフォーマンス監視用のログ

### 2. リトライ・レジリエンス設計

#### ServiceDefaultsによる統一設定
`AddStandardResilienceHandler()`により以下が自動適用：

- **Retry Pattern**
  - 最大3回のリトライ
  - 指数バックオフ (2秒、4秒、8秒)
  - HTTP 5xx、408、429エラー時に実行

- **Circuit Breaker**
  - 連続した失敗を検出
  - サービス過負荷防止
  - 自動復旧試行

- **Timeout Management**
  - リクエスト全体のタイムアウト設定
  - デッドロック防止

- **Hedge Strategy**
  - 並列リクエストでレイテンシ削減

#### 登録方法 (`Program.cs`)
```csharp
builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>("employeeservice-api", client =>
{
    client.BaseAddress = new Uri("http://employeeservice-api");
});
```

ServiceDefaultsの`ConfigureHttpClientDefaults`により、全HttpClientに自動適用：
```csharp
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
    http.AddServiceDiscovery();
});
```

### 3. サンプル画面

#### 従業員一覧ページ (`Components/Pages/Employees.razor`)
- **機能**
  - 全従業員の一覧表示
  - ローディング状態の表示
  - エラーハンドリングとリトライボタン
  - 詳細ページへのナビゲーション
  
- **UIコンポーネント**
  - MudTable による一覧表示
  - MudProgressCircular によるローディング表示
  - MudAlert によるエラー表示
  - MudSnackbar による通知

#### 従業員詳細ページ (`Components/Pages/EmployeeDetail.razor`)
- **機能**
  - 特定従業員の詳細情報表示
  - パンくずナビゲーション
  - 存在しない従業員の適切な処理
  
- **UIコンポーネント**
  - MudCard によるレイアウト
  - MudBreadcrumbs によるナビゲーション
  - MudField によるデータ表示
  - MudGrid によるレスポンシブレイアウト

### 4. ナビゲーション統合

#### メインメニュー (`Components/Layout/NavMenu.razor`)
```razor
<MudNavLink Href="employees" Icon="@Icons.Material.Filled.People">従業員管理</MudNavLink>
```

#### ホームページ (`Components/Pages/Home.razor`)
- 従業員管理カードの有効化
- ステータス表示の更新（実装予定 → 実装済み）

### 5. API契約（Shared.Contracts）の利用

既存の型を利用：
- `EmployeeDto` - 従業員データ転送オブジェクト
- `CreateEmployeeRequest` - 従業員作成リクエスト
- `UpdateEmployeeRequest` - 従業員更新リクエスト

これらは`Shared.Contracts`プロジェクトで定義され、BlazorWebとEmployeeService.APIの両方で参照されています。

## 技術的な特徴

### エラーハンドリングの階層化

1. **HTTPクライアント層**
   - HTTP例外のキャッチ
   - ステータスコードの適切な処理
   - 詳細ログの記録

2. **UIコンポーネント層**
   - try-catch によるエラーキャッチ
   - ユーザーへのフィードバック（Snackbar）
   - エラー状態の視覚的表示

3. **リトライ層（自動）**
   - Microsoft.Extensions.Http.Resilience による自動リトライ
   - ポリシーベースのエラー処理
   - サーキットブレーカーによる保護

### 可観測性

OpenTelemetryによる完全なトレーシング：
- HTTPクライアントの自動計装
- メトリクス収集（リクエスト数、エラー率、レイテンシ）
- 構造化ログによる詳細記録
- Aspireダッシュボードでのリアルタイム監視

## テスト結果

- ✅ 全ての既存テスト (40件) が合格
- ✅ リグレッションなし
- ✅ ビルド成功

## カスタマイズ例

### リトライポリシーのカスタマイズ

```csharp
builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>("employeeservice-api", client =>
{
    client.BaseAddress = new Uri("http://employeeservice-api");
})
.AddStandardResilienceHandler(options =>
{
    // リトライ回数を増やす
    options.Retry.MaxRetryAttempts = 5;
    
    // リトライ間隔を短くする
    options.Retry.Delay = TimeSpan.FromSeconds(1);
    
    // サーキットブレーカーの閾値を調整
    options.CircuitBreaker.FailureRatio = 0.5;
    
    // タイムアウトを延長
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
});
```

### カスタムエラーハンドリング

```csharp
public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
{
    try
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<EmployeeDto>>(ApiBasePath, cancellationToken) 
            ?? Enumerable.Empty<EmployeeDto>();
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP request failed");
        // カスタムフォールバック処理
        return GetCachedData();
    }
}
```

## 次のステップ（オプション）

1. **キャッシング戦略の実装**
   - メモリキャッシュまたは分散キャッシュ
   - キャッシュ無効化戦略

2. **オフライン対応**
   - Service Workerによるオフライン機能
   - ローカルストレージの活用

3. **パフォーマンス最適化**
   - ページング実装
   - 遅延読み込み
   - 仮想スクロール

4. **追加機能**
   - 検索・フィルタリング
   - 並べ替え
   - 従業員の作成・編集・削除UI

## まとめ

Issue #6の要件を全て満たす実装を完了しました：

✅ BlazorWebからEmployeeServiceのAPI呼び出し実装  
✅ 包括的なエラーハンドリング  
✅ リトライ設計（Microsoft.Extensions.Http.Resilience使用）  
✅ API契約（Shared/Contracts）の参照  
✅ サンプル画面での一覧・詳細表示  
✅ 詳細なドキュメント作成  

全ての実装は、.NET Aspireのベストプラクティスに従い、ServiceDefaultsを活用して統一された可観測性とレジリエンスを実現しています。
