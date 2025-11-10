# 勤怠管理サービス (Attendance Service)

## 概要

AttendanceServiceは、従業員の勤怠記録と集計を管理するマイクロサービスです。出退勤時刻の記録、勤怠履歴の照会、月次集計などの機能を提供します。

## 主要機能

### 1. 勤怠記録管理

- **出勤記録 (Check-in)**: 従業員の出勤時刻を記録
- **退勤記録 (Check-out)**: 従業員の退勤時刻を記録
- **勤務時間計算**: 出勤・退勤時刻から自動的に勤務時間を計算

### 2. 勤怠履歴照会

- **従業員別履歴取得**: 特定の従業員の勤怠履歴を取得
- **期間指定フィルタ**: 開始日・終了日を指定して履歴をフィルタリング

### 3. 月次集計

- **総出勤日数**: 月内の実際に勤務した日数
- **総勤務時間**: 月内の総勤務時間（時間単位）
- **平均勤務時間**: 1日あたりの平均勤務時間
- **遅刻回数**: 9:00以降に出勤した日数をカウント

### 4. 休暇申請管理

- **休暇申請作成**: 有給休暇、病気休暇などの申請
- **承認/却下/キャンセル**: 休暇申請の承認フロー

## API エンドポイント

### 勤怠記録API

#### 出勤記録
```
POST /api/attendances/checkin
```

**リクエストボディ:**
```json
{
  "employeeId": "guid",
  "checkInTime": "2024-01-15T09:00:00Z"
}
```

#### 退勤記録
```
POST /api/attendances/checkout
```

**リクエストボディ:**
```json
{
  "employeeId": "guid",
  "checkOutTime": "2024-01-15T18:00:00Z"
}
```

### 勤怠履歴API

#### 従業員の勤怠履歴を取得
```
GET /api/attendances/employee/{employeeId}?startDate=2024-01-01&endDate=2024-01-31
```

**クエリパラメータ:**
- `startDate` (optional): 開始日 (YYYY-MM-DD)
- `endDate` (optional): 終了日 (YYYY-MM-DD)

**レスポンス:**
```json
[
  {
    "id": "guid",
    "employeeId": "guid",
    "workDate": "2024-01-15",
    "checkInTime": "2024-01-15T09:00:00Z",
    "checkOutTime": "2024-01-15T18:00:00Z",
    "type": "Normal",
    "notes": null,
    "workHours": 9.0,
    "createdAt": "2024-01-15T09:00:00Z",
    "updatedAt": "2024-01-15T18:00:00Z"
  }
]
```

### 月次集計API

#### 従業員の月次勤怠集計を取得
```
GET /api/attendances/employee/{employeeId}/summary/{year}/{month}
```

**パスパラメータ:**
- `employeeId`: 従業員ID (GUID)
- `year`: 対象年 (例: 2024)
- `month`: 対象月 (1-12)

**レスポンス:**
```json
{
  "employeeId": "guid",
  "year": 2024,
  "month": 1,
  "totalWorkDays": 20,
  "totalWorkHours": 160.0,
  "averageWorkHours": 8.0,
  "lateDays": 2,
  "absentDays": 0,
  "paidLeaveDays": 0,
  "attendances": [...]
}
```

### 休暇申請API

詳細は既存のLeaveRequest APIドキュメントを参照してください。

## BlazorWeb UI

### 勤怠管理画面

**アクセスURL:** `/attendances`

#### 主要機能

1. **従業員選択フィルタ**
   - 全従業員または特定の従業員を選択可能

2. **期間フィルタ**
   - 開始日・終了日を指定して勤怠履歴を絞り込み

3. **月次集計表示**
   - 従業員と年月を指定して月次集計を表示
   - 出勤日数、総勤務時間、平均勤務時間、遅刻回数を表示

4. **勤怠記録一覧**
   - 日付、従業員名、出勤時刻、退勤時刻、勤務時間、種別、備考を一覧表示
   - ページネーション対応

## データモデル

### Attendance (勤怠記録)

| フィールド | 型 | 説明 |
|-----------|-----|------|
| Id | Guid | 勤怠記録ID |
| EmployeeId | Guid | 従業員ID |
| WorkDate | DateTime | 勤務日（日付部分のみ） |
| CheckInTime | DateTime? | 出勤時刻 |
| CheckOutTime | DateTime? | 退勤時刻 |
| Type | AttendanceType | 勤怠種別 |
| Notes | string? | 備考 |
| CreatedAt | DateTime | 作成日時 |
| UpdatedAt | DateTime | 更新日時 |

### AttendanceType (勤怠種別)

| 値 | 説明 |
|----|------|
| Normal | 通常勤務 |
| Remote | リモートワーク |
| BusinessTrip | 出張 |
| HalfDay | 半日勤務 |

## テストデータの投入

### 開発環境での自動シード

開発環境で`AttendanceService.API`を起動すると、`DbInitializer`が自動的に実行され、過去3ヶ月分のサンプル勤怠データが投入されます。

**注意:** 
- サンプルデータは固定のGUID（`00000000-0000-0000-0000-000000000001`など）を使用しています
- 実際のEmployeeServiceの従業員IDとは異なるため、Blazor UIでは正しく従業員名が表示されない可能性があります

### 実在する従業員IDでのデータ投入

開発環境では、実在する従業員IDを使用してテストデータを投入する専用のAPIエンドポイントを利用できます。

**エンドポイント:**
```
POST /api/dev/seed-attendances
```

**リクエストボディ:**
```json
[
  "actual-employee-guid-1",
  "actual-employee-guid-2",
  "actual-employee-guid-3"
]
```

**使用例（curl）:**
```bash
# まずEmployeeServiceから従業員IDを取得
curl http://localhost:5001/api/employees

# 取得したIDを使って勤怠データを投入
curl -X POST http://localhost:5003/api/dev/seed-attendances \
  -H "Content-Type: application/json" \
  -d '["guid1", "guid2", "guid3"]'
```

このエンドポイントは開発環境でのみ利用可能で、本番環境では無効化されます。

## データベース

- **データベース種別**: SQLite（開発環境）
- **マイグレーション**: Entity Framework Core Migrations
- **接続文字列**: `Data Source=attendance.db`

### マイグレーションの作成

```bash
cd src/Services/AttendanceService/API
dotnet ef migrations add MigrationName --project ../Infrastructure
```

### マイグレーションの適用

```bash
dotnet ef database update --project ../Infrastructure
```

アプリケーション起動時に自動的にマイグレーションが適用されます。

## イベント駆動

AttendanceServiceは以下のイベントをRedis Pub/Subを通じて発行します：

- **attendance:checkin**: 出勤記録時に発行
- **attendance:checkout**: 退勤記録時に発行
- **leaverequest:created**: 休暇申請作成時に発行
- **leaverequest:approved**: 休暇申請承認時に発行
- **leaverequest:rejected**: 休暇申請却下時に発行

これらのイベントはNotificationServiceが購読し、関連する通知を生成します。

## テスト

### ユニットテスト

```bash
dotnet test tests/AttendanceService.Domain.Tests
dotnet test tests/AttendanceService.Application.Tests
```

### 統合テスト

```bash
dotnet test tests/AttendanceService.Integration.Tests
```

統合テストは以下をカバーしています：
- Check-in/Check-out API
- 勤怠履歴取得API
- 月次集計API
- 休暇申請API

## セキュリティ

- **認証**: 他サービスと同様、将来的にはEntra ID統合予定
- **認可**: ユーザーは自分の勤怠記録のみアクセス可能（管理者は全従業員の記録にアクセス可能）

## 既知の制限事項

1. **従業員IDの整合性**: 現在のDbInitializerは固定GUIDを使用しているため、実際のEmployeeServiceの従業員とリンクしていません
2. **欠勤日数・有給休暇日数**: 月次集計の欠勤日数と有給休暇日数は現在0を返します（LeaveRequestとの統合が必要）
3. **遅刻判定**: 遅刻判定は9:00を基準にハードコードされています（将来的には設定可能にする予定）

## 今後の改善予定

- [ ] EmployeeServiceとの連携強化（実際の従業員IDを使用）
- [ ] LeaveRequestとの統合による正確な欠勤・有給日数の集計
- [ ] 勤務時間のルール設定機能（標準勤務時間、遅刻判定時刻など）
- [ ] カレンダーUIの実装
- [ ] 勤怠データのエクスポート機能（CSV、Excel）
- [ ] 勤怠の編集・削除機能
- [ ] 承認フロー機能（勤怠記録の承認が必要な場合）

## 本番運用ドキュメント

### OpenAPI / Swagger ドキュメント

開発環境では、Scalar UI を使用してインタラクティブな API ドキュメントにアクセスできます。

```
http://localhost:{port}/scalar
```

または、OpenAPI JSON 仕様を取得できます。

```
http://localhost:{port}/openapi/v1.json
```

**本番環境でのアクセス**: セキュリティ上の理由から、本番環境では OpenAPI/Scalar エンドポイントは自動的に無効化されます。

### セキュリティ機能

1. **JWT 認証**: 全APIエンドポイントで JWT Bearer 認証をサポート
2. **グローバル例外ハンドラー**: 予期しないエラーを適切に処理し、詳細情報の漏洩を防止
3. **入力バリデーション**: ArgumentNullException.ThrowIfNull による厳格な null チェック
4. **構造化ログ**: エラーとトレース情報を構造化形式でログ記録

### 可観測性

- **ヘルスチェック**: `/health`, `/health/live`, `/health/ready` エンドポイント
- **OpenTelemetry**: 分散トレーシングとメトリクス収集
- **構造化ログ**: JSON 形式でログ出力、traceId による追跡

## 関連ドキュメント

### 開発
- [アーキテクチャ概要](./architecture.md)
- [開発ガイド](./development-guide.md)
- [データベース管理](./database.md)
- [通知サービス](./notification-service.md)

### 本番運用
- **[本番デプロイメントガイド](./attendance-service-production-deployment.md)** - デプロイ手順と環境設定
- **[API リファレンス](./attendance-service-api-reference.md)** - 完全なAPIドキュメント
- **[トラブルシューティングガイド](./attendance-service-troubleshooting.md)** - 問題解決方法
