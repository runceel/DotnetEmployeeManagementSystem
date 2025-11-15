# AttendanceService 本番デプロイメントガイド

## 概要

このドキュメントは、AttendanceService（勤怠管理サービス）を本番環境にデプロイするための手順と設定を説明します。

## 前提条件

### 必須要件
- .NET 10.0 SDK または Runtime
- SQLite または PostgreSQL / SQL Server（本番環境推奨）
- Redis サーバー（Pub/Sub メッセージング用）
- HTTPS 証明書（本番環境）

### 推奨環境
- **開発環境**: Docker Desktop + .NET Aspire
- **本番環境**: Azure Container Apps / Kubernetes / Azure App Service

## デプロイメント手順

### 1. データベースのセットアップ

#### SQLite（開発・テスト環境）

```bash
# マイグレーション適用
cd src/Services/AttendanceService/API
dotnet ef database update --project ../Infrastructure
```

#### PostgreSQL（本番環境推奨）

1. PostgreSQL インスタンスの作成
2. データベースの作成

```sql
CREATE DATABASE attendance_db;
CREATE USER attendance_user WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE attendance_db TO attendance_user;
```

3. 接続文字列の設定（環境変数または appsettings.json）

```json
{
  "ConnectionStrings": {
    "AttendanceDb": "Host=localhost;Database=attendance_db;Username=attendance_user;Password=secure_password"
  }
}
```

4. マイグレーション適用

```bash
cd src/Services/AttendanceService/Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../API
dotnet ef database update --startup-project ../API
```

### 2. Redis のセットアップ

#### ローカル Redis（開発環境）

```bash
docker run -d --name redis -p 6379:6379 redis:latest
```

#### Azure Cache for Redis（本番環境推奨）

1. Azure Portal で Redis インスタンスを作成
2. 接続文字列を取得
3. 環境変数に設定

```bash
export ConnectionStrings__redis="your-redis-connection-string"
```

### 3. アプリケーション設定

#### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "AttendanceDb": "YOUR_DATABASE_CONNECTION_STRING",
    "redis": "YOUR_REDIS_CONNECTION_STRING"
  },
  "Jwt": {
    "SecretKey": "YOUR_JWT_SECRET_KEY",
    "Issuer": "AttendanceService",
    "Audience": "AttendanceServiceClients",
    "ExpirationMinutes": 60
  }
}
```

#### 環境変数（推奨）

本番環境では、機密情報を環境変数で管理することを推奨します。

```bash
# データベース接続文字列
export ConnectionStrings__AttendanceDb="..."

# Redis 接続文字列
export ConnectionStrings__redis="..."

# JWT 設定
export Jwt__SecretKey="..."
export Jwt__Issuer="AttendanceService"
export Jwt__Audience="AttendanceServiceClients"

# ASPNETCORE_ENVIRONMENT
export ASPNETCORE_ENVIRONMENT="Production"
```

### 4. ビルドとパッケージング

#### Docker コンテナイメージの作成

```dockerfile
# Dockerfile (例)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/AttendanceService/API/AttendanceService.API.csproj", "AttendanceService.API/"]
COPY ["src/Services/AttendanceService/Application/AttendanceService.Application.csproj", "AttendanceService.Application/"]
COPY ["src/Services/AttendanceService/Domain/AttendanceService.Domain.csproj", "AttendanceService.Domain/"]
COPY ["src/Services/AttendanceService/Infrastructure/AttendanceService.Infrastructure.csproj", "AttendanceService.Infrastructure/"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["src/Shared/Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]

RUN dotnet restore "AttendanceService.API/AttendanceService.API.csproj"
COPY . .
WORKDIR "/src/AttendanceService.API"
RUN dotnet build "AttendanceService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AttendanceService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AttendanceService.API.dll"]
```

#### ビルドコマンド

```bash
# アプリケーションのビルド
dotnet publish src/Services/AttendanceService/API/AttendanceService.API.csproj \
  -c Release \
  -o ./publish

# Docker イメージのビルド
docker build -t attendance-service:latest -f Dockerfile .

# Docker イメージのタグ付けとプッシュ
docker tag attendance-service:latest your-registry.azurecr.io/attendance-service:v1.0.0
docker push your-registry.azurecr.io/attendance-service:v1.0.0
```

### 5. デプロイ

#### Azure Container Apps へのデプロイ

```bash
# Azure Container Apps にデプロイ
az containerapp create \
  --name attendance-service \
  --resource-group your-resource-group \
  --environment your-environment \
  --image your-registry.azurecr.io/attendance-service:v1.0.0 \
  --target-port 8080 \
  --ingress external \
  --env-vars \
    ConnectionStrings__AttendanceDb="..." \
    ConnectionStrings__redis="..." \
    Jwt__SecretKey="..." \
    ASPNETCORE_ENVIRONMENT="Production"
```

#### Kubernetes へのデプロイ

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: attendance-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: attendance-service
  template:
    metadata:
      labels:
        app: attendance-service
    spec:
      containers:
      - name: attendance-service
        image: your-registry.azurecr.io/attendance-service:v1.0.0
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__AttendanceDb
          valueFrom:
            secretKeyRef:
              name: attendance-secrets
              key: db-connection-string
        - name: ConnectionStrings__redis
          valueFrom:
            secretKeyRef:
              name: attendance-secrets
              key: redis-connection-string
---
apiVersion: v1
kind: Service
metadata:
  name: attendance-service
spec:
  selector:
    app: attendance-service
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: LoadBalancer
```

```bash
kubectl apply -f deployment.yaml
```

## セキュリティ設定

### HTTPS の有効化

本番環境では HTTPS を必須とします。

```csharp
// Program.cs で HTTPS リダイレクトが有効化されています
app.UseHttpsRedirection();
```

### 認証・認可

AttendanceService は JWT Bearer 認証をサポートしています。

#### JWT トークンの生成（AuthService で実施）

```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(secretKey);
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role)
    }),
    Expires = DateTime.UtcNow.AddMinutes(60),
    Issuer = issuer,
    Audience = audience,
    SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature)
};
var token = tokenHandler.CreateToken(tokenDescriptor);
var tokenString = tokenHandler.WriteToken(token);
```

#### API リクエストの認証

```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  https://your-domain.com/api/attendances/employee/{employeeId}
```

### CORS 設定（必要に応じて）

```csharp
// Program.cs に追加
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder => builder
            .WithOrigins("https://your-frontend-domain.com")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ...

app.UseCors("AllowSpecificOrigins");
```

## モニタリングと可観測性

### ヘルスチェック

AttendanceService は標準的なヘルスチェックエンドポイントを提供しています。

```bash
# ヘルスチェック
curl https://your-domain.com/health

# Liveness プローブ
curl https://your-domain.com/health/live

# Readiness プローブ
curl https://your-domain.com/health/ready
```

### OpenTelemetry による分散トレーシング

.NET Aspire ServiceDefaults により、OpenTelemetry が自動的に構成されています。

- **トレース**: HTTP リクエスト、データベースクエリ
- **メトリクス**: リクエスト数、レスポンスタイム、エラー率
- **ログ**: 構造化ログ（JSON形式）

#### Application Insights への送信（Azure環境）

```bash
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=...;IngestionEndpoint=..."
```

### ログの確認

```bash
# Docker コンテナのログ
docker logs attendance-service

# Kubernetes のログ
kubectl logs -f deployment/attendance-service

# Azure Container Apps のログ
az containerapp logs show \
  --name attendance-service \
  --resource-group your-resource-group \
  --tail 100
```

## バックアップとリカバリ

### データベースバックアップ

#### PostgreSQL

```bash
# バックアップ
pg_dump -h localhost -U attendance_user -d attendance_db > backup_$(date +%Y%m%d_%H%M%S).sql

# リストア
psql -h localhost -U attendance_user -d attendance_db < backup_20250110_120000.sql
```

#### 自動バックアップ（Azure Database for PostgreSQL）

Azure Portal で自動バックアップを有効化し、保持期間を設定します（推奨: 7日以上）。

### Redis データの永続化

本番環境では Redis の RDB または AOF による永続化を有効化することを推奨します。

## パフォーマンス最適化

### データベース接続プール

```json
{
  "ConnectionStrings": {
    "AttendanceDb": "Host=localhost;Database=attendance_db;Username=attendance_user;Password=secure_password;Pooling=true;MinPoolSize=5;MaxPoolSize=100"
  }
}
```

### Redis 接続プール

Aspire の Redis クライアントはデフォルトで接続プーリングを使用します。

### キャッシング戦略

頻繁にアクセスされるデータ（従業員情報など）は Redis でキャッシュすることを検討してください。

## トラブルシューティング

詳細なトラブルシューティングガイドは [attendance-service-troubleshooting.md](./attendance-service-troubleshooting.md) を参照してください。

### よくある問題

#### 問題: データベース接続エラー

```
Failed to connect to database
```

**解決策:**
1. 接続文字列が正しいか確認
2. データベースサーバーが起動しているか確認
3. ファイアウォール設定を確認
4. ネットワーク接続を確認

#### 問題: Redis 接続エラー

```
Unable to connect to Redis
```

**解決策:**
1. Redis サーバーが起動しているか確認
2. 接続文字列が正しいか確認
3. ネットワーク設定を確認

#### 問題: JWT 認証エラー

```
401 Unauthorized
```

**解決策:**
1. トークンが有効期限内か確認
2. Issuer と Audience が一致しているか確認
3. SecretKey が正しいか確認

## スケーリング

### 水平スケーリング

AttendanceService はステートレスなため、複数のインスタンスを並行実行可能です。

```bash
# Kubernetes でのスケーリング
kubectl scale deployment attendance-service --replicas=5

# Azure Container Apps でのスケーリング
az containerapp update \
  --name attendance-service \
  --resource-group your-resource-group \
  --min-replicas 2 \
  --max-replicas 10
```

### 垂直スケーリング

高負荷時は CPU とメモリを増やすことを検討してください。

## セキュリティベストプラクティス

1. **機密情報の管理**: Azure Key Vault などのシークレット管理サービスを使用
2. **HTTPS の強制**: 本番環境では常に HTTPS を使用
3. **最小権限の原則**: データベースユーザーには必要最小限の権限のみ付与
4. **定期的な更新**: .NET SDK とパッケージを最新に保つ
5. **セキュリティスキャン**: 定期的に脆弱性スキャンを実施

## ロールバック手順

### Docker / Kubernetes

```bash
# 以前のバージョンにロールバック
kubectl rollout undo deployment/attendance-service

# 特定のリビジョンにロールバック
kubectl rollout undo deployment/attendance-service --to-revision=2
```

### Azure Container Apps

```bash
az containerapp revision list \
  --name attendance-service \
  --resource-group your-resource-group

az containerapp revision activate \
  --name attendance-service \
  --resource-group your-resource-group \
  --revision attendance-service--previous-revision
```

## サポートとメンテナンス

### 定期メンテナンス

- **毎週**: ログとメトリクスの確認
- **毎月**: セキュリティパッチの適用
- **四半期ごと**: データベースパフォーマンスの最適化
- **年次**: 災害復旧訓練の実施

### サポート連絡先

- **開発チーム**: dev@example.com
- **運用チーム**: ops@example.com
- **緊急連絡先**: +81-XX-XXXX-XXXX

## まとめ

このガイドに従うことで、AttendanceService を安全かつ効率的に本番環境にデプロイできます。質問や問題がある場合は、開発チームまでお問い合わせください。

## 関連ドキュメント

- [AttendanceService アーキテクチャ](./attendance-service.md)
- [トラブルシューティングガイド](./attendance-service-troubleshooting.md)
- [API リファレンス](./attendance-service-api-reference.md)
- [開発ガイド](./development-guide.md)
