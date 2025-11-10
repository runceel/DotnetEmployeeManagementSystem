# AttendanceService トラブルシューティングガイド

## 概要

このドキュメントは、AttendanceService（勤怠管理サービス）の運用中に発生する可能性のある問題と、その解決方法を説明します。

## 目次

1. [データベース関連の問題](#データベース関連の問題)
2. [Redis 関連の問題](#redis-関連の問題)
3. [認証・認可の問題](#認証認可の問題)
4. [API エラー](#api-エラー)
5. [パフォーマンス問題](#パフォーマンス問題)
6. [デプロイメント問題](#デプロイメント問題)
7. [ログとトレースの確認方法](#ログとトレースの確認方法)

## データベース関連の問題

### 問題: データベース接続エラー

**症状:**
```
Microsoft.Data.SqlClient.SqlException: Cannot open database
```

または

```
Npgsql.NpgsqlException: Connection refused
```

**原因:**
- データベースサーバーが停止している
- 接続文字列が間違っている
- ファイアウォールがポートをブロックしている
- ネットワーク問題

**解決手順:**

1. **データベースサーバーの状態確認**

```bash
# PostgreSQL の場合
sudo systemctl status postgresql

# SQL Server の場合
sudo systemctl status mssql-server

# Docker コンテナの場合
docker ps | grep postgres
```

2. **接続文字列の確認**

```bash
# 環境変数の確認
echo $ConnectionStrings__AttendanceDb

# または appsettings.json を確認
cat appsettings.Production.json | grep ConnectionStrings
```

3. **ネットワーク接続テスト**

```bash
# PostgreSQL（デフォルトポート: 5432）
telnet database-host 5432

# SQL Server（デフォルトポート: 1433）
telnet database-host 1433

# または nc コマンド
nc -zv database-host 5432
```

4. **ファイアウォール設定の確認**

```bash
# Azure の場合、ファイアウォールルールを確認
az postgres server firewall-rule list \
  --resource-group your-resource-group \
  --server-name your-server-name
```

### 問題: マイグレーションエラー

**症状:**
```
An error occurred while applying migrations
```

**原因:**
- マイグレーションが未適用
- データベーススキーマの不整合
- 権限不足

**解決手順:**

1. **マイグレーション状態の確認**

```bash
cd src/Services/AttendanceService/API
dotnet ef migrations list --project ../Infrastructure
```

2. **マイグレーションの適用**

```bash
dotnet ef database update --project ../Infrastructure
```

3. **特定のマイグレーションへのロールバック**

```bash
dotnet ef database update PreviousMigrationName --project ../Infrastructure
```

4. **マイグレーション履歴テーブルの確認**

```sql
-- PostgreSQL / SQL Server
SELECT * FROM "__EFMigrationsHistory";
```

### 問題: データベース接続プールの枯渇

**症状:**
```
System.InvalidOperationException: Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool.
```

**原因:**
- 接続リークが発生している
- 接続プールサイズが小さすぎる
- 長時間実行されるクエリが多い

**解決手順:**

1. **接続プールサイズの増加**

```json
{
  "ConnectionStrings": {
    "AttendanceDb": "Host=localhost;Database=attendance_db;Username=user;Password=pass;Pooling=true;MinPoolSize=10;MaxPoolSize=200"
  }
}
```

2. **長時間実行クエリの特定**

```sql
-- PostgreSQL
SELECT pid, now() - pg_stat_activity.query_start AS duration, query
FROM pg_stat_activity
WHERE state = 'active'
ORDER BY duration DESC;

-- SQL Server
SELECT r.session_id, r.start_time, r.status, r.command, t.text
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE r.session_id <> @@SPID
ORDER BY r.start_time;
```

3. **DbContext の適切な Dispose 確認**

```csharp
// 正しい使用方法
await using var context = serviceProvider.GetRequiredService<AttendanceDbContext>();
// 処理
```

## Redis 関連の問題

### 問題: Redis 接続エラー

**症状:**
```
StackExchange.Redis.RedisConnectionException: It was not possible to connect to the redis server(s)
```

**原因:**
- Redis サーバーが停止している
- 接続文字列が間違っている
- ネットワーク問題

**解決手順:**

1. **Redis サーバーの状態確認**

```bash
# Redis CLI で接続テスト
redis-cli ping
# 応答: PONG

# Docker コンテナの場合
docker ps | grep redis
```

2. **接続文字列の確認**

```bash
echo $ConnectionStrings__redis
```

3. **ネットワーク接続テスト**

```bash
telnet redis-host 6379
```

### 問題: Redis メモリ不足

**症状:**
```
OOM command not allowed when used memory > 'maxmemory'
```

**原因:**
- Redis のメモリ使用量が上限に達した
- メモリポリシーの設定が適切でない

**解決手順:**

1. **メモリ使用量の確認**

```bash
redis-cli INFO memory
```

2. **不要なキーの削除**

```bash
# パターンにマッチするキーを削除
redis-cli KEYS "temp:*" | xargs redis-cli DEL
```

3. **メモリポリシーの設定**

```bash
# redis.conf に追加
maxmemory 2gb
maxmemory-policy allkeys-lru
```

## 認証・認可の問題

### 問題: 401 Unauthorized エラー

**症状:**
```
HTTP/1.1 401 Unauthorized
```

**原因:**
- JWT トークンが無効または期限切れ
- Authorization ヘッダーが欠落
- JWT 設定の不一致

**解決手順:**

1. **トークンの確認**

```bash
# JWT トークンをデコード（jwt.io を使用）
# または jq コマンドで確認
echo "YOUR_TOKEN" | cut -d. -f2 | base64 -d | jq
```

2. **トークンの有効期限確認**

```json
{
  "exp": 1234567890,  // UNIX timestamp
  "iat": 1234567800,
  "nbf": 1234567800
}
```

3. **JWT 設定の確認**

```bash
# Issuer と Audience が一致しているか確認
echo $Jwt__Issuer
echo $Jwt__Audience
```

4. **正しい Authorization ヘッダーの設定**

```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  https://api-domain.com/api/attendances/employee/{employeeId}
```

### 問題: 403 Forbidden エラー

**症状:**
```
HTTP/1.1 403 Forbidden
```

**原因:**
- 権限不足
- ロールが正しく設定されていない

**解決手順:**

1. **ユーザーのロール確認**

```bash
# JWT トークンの claims を確認
echo "YOUR_TOKEN" | cut -d. -f2 | base64 -d | jq '.role'
```

2. **必要な権限の確認**

エンドポイントに必要なロールまたは権限を確認してください。

## API エラー

### 問題: 400 Bad Request エラー

**症状:**
```json
{
  "error": "月は1から12の範囲で指定してください。",
  "traceId": "00-abc123..."
}
```

**原因:**
- 入力パラメータが不正
- リクエストボディのフォーマットが間違っている

**解決手順:**

1. **リクエストパラメータの確認**

API ドキュメント（Scalar UI）で正しいパラメータ形式を確認してください。

```bash
# Scalar UI にアクセス
https://your-domain.com/scalar
```

2. **バリデーションエラーの詳細確認**

エラーレスポンスの `traceId` を使用してログを検索してください。

```bash
# ログから traceId で検索
kubectl logs -f deployment/attendance-service | grep "00-abc123"
```

### 問題: 500 Internal Server Error

**症状:**
```json
{
  "error": "内部サーバーエラーが発生しました。",
  "message": "...",
  "traceId": "00-def456..."
}
```

**原因:**
- アプリケーション内部のエラー
- データベースエラー
- 予期しない例外

**解決手順:**

1. **ログの確認**

```bash
# Docker
docker logs attendance-service --tail 100

# Kubernetes
kubectl logs -f deployment/attendance-service

# Azure Container Apps
az containerapp logs show \
  --name attendance-service \
  --resource-group your-resource-group \
  --tail 100
```

2. **トレースの確認（Application Insights）**

Azure Portal で Application Insights を開き、`traceId` で検索してください。

3. **スタックトレースの分析**

エラーログからスタックトレースを確認し、問題の原因を特定してください。

## パフォーマンス問題

### 問題: レスポンスタイムが遅い

**症状:**
- API レスポンスが 1 秒以上かかる

**原因:**
- N+1 クエリ問題
- データベースインデックスの欠如
- ネットワークレイテンシ

**解決手順:**

1. **データベースクエリの最適化**

```bash
# EF Core のログレベルを Information に設定して SQL を確認
export Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command="Information"
```

2. **インデックスの追加**

```sql
-- PostgreSQL
CREATE INDEX idx_attendance_employee_date ON "Attendances" ("EmployeeId", "WorkDate");
CREATE INDEX idx_leaverequest_employee_status ON "LeaveRequests" ("EmployeeId", "Status");
```

3. **クエリプランの分析**

```sql
-- PostgreSQL
EXPLAIN ANALYZE SELECT * FROM "Attendances" WHERE "EmployeeId" = 'guid' AND "WorkDate" >= '2025-01-01';

-- SQL Server
SET STATISTICS TIME ON;
SET STATISTICS IO ON;
SELECT * FROM Attendances WHERE EmployeeId = 'guid' AND WorkDate >= '2025-01-01';
```

4. **Application Insights でボトルネック特定**

Azure Portal で Application Insights を開き、Performance ブレードで遅いリクエストを特定してください。

### 問題: メモリ使用量が高い

**症状:**
- アプリケーションのメモリ使用量が継続的に増加

**原因:**
- メモリリーク
- 大量のデータをメモリにロード

**解決手順:**

1. **メモリダンプの取得**

```bash
# .NET Core の診断ツール
dotnet-dump collect --process-id <pid>

# 分析
dotnet-dump analyze dump_*.dmp
```

2. **大量データのストリーム処理**

```csharp
// 悪い例: 全データをメモリにロード
var allAttendances = await context.Attendances.ToListAsync();

// 良い例: ページングを使用
var attendances = await context.Attendances
    .Skip(pageNumber * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

## デプロイメント問題

### 問題: コンテナが起動しない

**症状:**
```
CrashLoopBackOff
```

**原因:**
- 設定エラー
- 依存サービスへの接続失敗
- ヘルスチェックの失敗

**解決手順:**

1. **コンテナログの確認**

```bash
# Kubernetes
kubectl logs attendance-service-pod-name

# Docker
docker logs attendance-service
```

2. **ヘルスチェックエンドポイントの確認**

```bash
curl http://localhost:8080/health
```

3. **環境変数の確認**

```bash
# Kubernetes
kubectl describe pod attendance-service-pod-name

# 環境変数セクションを確認
```

### 問題: マイグレーションが適用されない

**症状:**
- アプリケーション起動時にテーブルが見つからないエラー

**原因:**
- `DbInitializer.InitializeAsync` が実行されていない
- マイグレーションの自動適用が無効

**解決手順:**

1. **手動でマイグレーションを適用**

```bash
# 本番環境のデータベースに直接適用
dotnet ef database update --project src/Services/AttendanceService/Infrastructure \
  --connection "YOUR_PRODUCTION_CONNECTION_STRING"
```

2. **初期化ログの確認**

```bash
# 起動時のログを確認
kubectl logs attendance-service-pod-name | grep "Database migration"
```

## ログとトレースの確認方法

### アプリケーションログ

```bash
# 構造化ログの確認
kubectl logs -f deployment/attendance-service | jq '.'

# 特定のログレベルでフィルタ
kubectl logs -f deployment/attendance-service | grep "Error"

# 特定の時間範囲でフィルタ
kubectl logs --since=1h deployment/attendance-service
```

### 分散トレーシング（Application Insights）

1. Azure Portal で Application Insights を開く
2. **Transaction search** で検索
3. **TraceId** でエンドツーエンドのトレースを確認
4. **Dependencies** でデータベース・Redis呼び出しを確認

### メトリクスの確認

```bash
# Prometheus メトリクスエンドポイント（.NET Aspire）
curl http://localhost:8080/metrics
```

## 緊急時の対応

### サービスの再起動

```bash
# Kubernetes
kubectl rollout restart deployment/attendance-service

# Azure Container Apps
az containerapp revision restart \
  --name attendance-service \
  --resource-group your-resource-group
```

### ロールバック

```bash
# Kubernetes
kubectl rollout undo deployment/attendance-service

# Azure Container Apps
az containerapp revision activate \
  --name attendance-service \
  --resource-group your-resource-group \
  --revision attendance-service--previous-revision
```

### スケールダウン（負荷軽減）

```bash
# Kubernetes
kubectl scale deployment attendance-service --replicas=1

# Azure Container Apps
az containerapp update \
  --name attendance-service \
  --resource-group your-resource-group \
  --min-replicas 1 \
  --max-replicas 1
```

## よくある質問（FAQ）

### Q1: データベースマイグレーションを本番環境で実行するタイミングは？

**A:** デプロイ前に手動で実行することを推奨します。自動マイグレーションは開発環境のみで使用してください。

### Q2: Redis のデータが消えた場合、どうすればいいですか？

**A:** Redis は主にキャッシュとメッセージングに使用されており、永続化データではありません。アプリケーションは Redis データの消失に対処できるように設計されています。

### Q3: トークンの有効期限はどれくらいですか？

**A:** デフォルトでは 60 分です。`Jwt:ExpirationMinutes` 設定で変更できます。

### Q4: 本番環境でOpenAPI/Scalarは無効にすべきですか？

**A:** はい。`Program.cs` では `IsDevelopment()` チェックにより、本番環境では自動的に無効化されます。

## サポート連絡先

- **緊急対応**: ops@example.com / +81-XX-XXXX-XXXX
- **開発チーム**: dev@example.com
- **ドキュメント**: https://docs.yourcompany.com

## まとめ

このガイドに記載されていない問題が発生した場合は、以下の手順で対応してください：

1. ログとトレースを確認
2. 関連ドキュメントを参照
3. 開発チームに問い合わせ
4. 必要に応じてロールバック

## 関連ドキュメント

- [デプロイメントガイド](./attendance-service-production-deployment.md)
- [API リファレンス](./attendance-service-api-reference.md)
- [アーキテクチャ概要](./attendance-service.md)
