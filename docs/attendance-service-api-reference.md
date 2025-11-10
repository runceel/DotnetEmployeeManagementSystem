# AttendanceService API リファレンス

## 概要

AttendanceService は、従業員の勤怠記録と休暇申請を管理するための RESTful API を提供します。

**ベースURL**: `https://your-domain.com`  
**バージョン**: v1  
**認証**: JWT Bearer Token

## 認証

全てのAPIエンドポイントは認証が必要です（health エンドポイントを除く）。

### Authorization ヘッダー

```
Authorization: Bearer <JWT_TOKEN>
```

### JWT トークンの取得

JWT トークンは AuthService から取得してください。

```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "user@example.com",
  "password": "password"
}
```

## エンドポイント一覧

### Attendances (勤怠記録)

| メソッド | エンドポイント | 説明 |
|---------|--------------|------|
| GET | `/api/attendances/` | 全勤怠記録を取得（実装予定） |
| GET | `/api/attendances/{id}` | IDで勤怠記録を取得（実装予定） |
| POST | `/api/attendances/` | 勤怠記録を作成（実装予定） |
| POST | `/api/attendances/checkin` | 出勤を記録 |
| POST | `/api/attendances/checkout` | 退勤を記録 |
| GET | `/api/attendances/employee/{employeeId}` | 従業員の勤怠履歴を取得 |
| GET | `/api/attendances/employee/{employeeId}/summary/{year}/{month}` | 月次勤怠集計を取得 |

### LeaveRequests (休暇申請)

| メソッド | エンドポイント | 説明 |
|---------|--------------|------|
| GET | `/api/leaverequests/` | 全休暇申請を取得 |
| GET | `/api/leaverequests/{id}` | IDで休暇申請を取得 |
| GET | `/api/leaverequests/employee/{employeeId}` | 従業員別の休暇申請を取得 |
| GET | `/api/leaverequests/status/{status}` | ステータス別の休暇申請を取得 |
| POST | `/api/leaverequests/` | 休暇申請を作成 |
| POST | `/api/leaverequests/{id}/approve` | 休暇申請を承認 |
| POST | `/api/leaverequests/{id}/reject` | 休暇申請を却下 |
| POST | `/api/leaverequests/{id}/cancel` | 休暇申請をキャンセル |

### Health (ヘルスチェック)

| メソッド | エンドポイント | 説明 |
|---------|--------------|------|
| GET | `/health` | ヘルスチェック |
| GET | `/health/live` | Liveness プローブ |
| GET | `/health/ready` | Readiness プローブ |

## 詳細仕様

### POST /api/attendances/checkin

従業員の出勤時刻を記録します。

**リクエスト:**

```http
POST /api/attendances/checkin HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "checkInTime": "2025-01-10T09:00:00Z"
}
```

**レスポンス（200 OK）:**

```json
{
  "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "workDate": "2025-01-10",
  "checkInTime": "2025-01-10T09:00:00Z",
  "checkOutTime": null,
  "type": "Normal",
  "notes": null,
  "workHours": null,
  "createdAt": "2025-01-10T09:00:00Z",
  "updatedAt": "2025-01-10T09:00:00Z"
}
```

**エラーレスポンス（400 Bad Request）:**

```json
{
  "error": "既にその日の出勤記録が存在します。",
  "traceId": "00-abc123..."
}
```

**バリデーション:**
- `employeeId`: 必須、有効なGUID
- `checkInTime`: 必須、未来の日時であってはならない
- 既にその日の出勤記録が存在する場合はエラー

---

### POST /api/attendances/checkout

従業員の退勤時刻を記録します。

**リクエスト:**

```http
POST /api/attendances/checkout HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "checkOutTime": "2025-01-10T18:00:00Z"
}
```

**レスポンス（200 OK）:**

```json
{
  "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "workDate": "2025-01-10",
  "checkInTime": "2025-01-10T09:00:00Z",
  "checkOutTime": "2025-01-10T18:00:00Z",
  "type": "Normal",
  "notes": null,
  "workHours": 9.0,
  "createdAt": "2025-01-10T09:00:00Z",
  "updatedAt": "2025-01-10T18:00:00Z"
}
```

**エラーレスポンス（400 Bad Request）:**

```json
{
  "error": "その日の出勤記録が見つかりません。",
  "traceId": "00-def456..."
}
```

**バリデーション:**
- `employeeId`: 必須、有効なGUID
- `checkOutTime`: 必須、出勤時刻より後である必要がある
- その日の出勤記録が存在しない場合はエラー

---

### GET /api/attendances/employee/{employeeId}

従業員の勤怠履歴を取得します。

**リクエスト:**

```http
GET /api/attendances/employee/3fa85f64-5717-4562-b3fc-2c963f66afa6?startDate=2025-01-01&endDate=2025-01-31 HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
```

**クエリパラメータ:**
- `startDate` (オプション): 開始日（ISO 8601形式）
- `endDate` (オプション): 終了日（ISO 8601形式）

**レスポンス（200 OK）:**

```json
[
  {
    "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "workDate": "2025-01-10",
    "checkInTime": "2025-01-10T09:00:00Z",
    "checkOutTime": "2025-01-10T18:00:00Z",
    "type": "Normal",
    "notes": null,
    "workHours": 9.0,
    "createdAt": "2025-01-10T09:00:00Z",
    "updatedAt": "2025-01-10T18:00:00Z"
  }
]
```

**エラーレスポンス（400 Bad Request）:**

```json
{
  "error": "開始日は終了日より前である必要があります。",
  "traceId": "00-ghi789..."
}
```

---

### GET /api/attendances/employee/{employeeId}/summary/{year}/{month}

従業員の月次勤怠集計を取得します。

**リクエスト:**

```http
GET /api/attendances/employee/3fa85f64-5717-4562-b3fc-2c963f66afa6/summary/2025/1 HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
```

**パスパラメータ:**
- `employeeId`: 従業員ID（GUID）
- `year`: 年（2000-2100）
- `month`: 月（1-12）

**レスポンス（200 OK）:**

```json
{
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "year": 2025,
  "month": 1,
  "totalWorkDays": 20,
  "totalWorkHours": 180.0,
  "averageWorkHours": 9.0,
  "lateDays": 2,
  "absentDays": 0,
  "paidLeaveDays": 0,
  "attendances": [
    {
      "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "workDate": "2025-01-10",
      "checkInTime": "2025-01-10T09:00:00Z",
      "checkOutTime": "2025-01-10T18:00:00Z",
      "type": "Normal",
      "notes": null,
      "workHours": 9.0,
      "createdAt": "2025-01-10T09:00:00Z",
      "updatedAt": "2025-01-10T18:00:00Z"
    }
  ]
}
```

**集計項目:**
- `totalWorkDays`: 総出勤日数（出勤・退勤両方記録がある日数）
- `totalWorkHours`: 総勤務時間
- `averageWorkHours`: 平均勤務時間
- `lateDays`: 遅刻回数（9:00以降の出勤）
- `absentDays`: 欠勤日数（実装予定）
- `paidLeaveDays`: 有給休暇日数（実装予定）

**エラーレスポンス（400 Bad Request）:**

```json
{
  "error": "月は1から12の範囲で指定してください。"
}
```

---

### POST /api/leaverequests/

新しい休暇申請を作成します。

**リクエスト:**

```http
POST /api/leaverequests/ HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "PaidLeave",
  "startDate": "2025-01-20",
  "endDate": "2025-01-21",
  "reason": "私用のため"
}
```

**休暇種別（Type）:**
- `PaidLeave`: 有給休暇
- `SickLeave`: 病気休暇
- `SpecialLeave`: 特別休暇
- `Unpaid`: 無給休暇

**レスポンス（201 Created）:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "PaidLeave",
  "startDate": "2025-01-20",
  "endDate": "2025-01-21",
  "reason": "私用のため",
  "status": "Pending",
  "approverId": null,
  "approvedAt": null,
  "approverComment": null,
  "days": 2,
  "createdAt": "2025-01-10T09:00:00Z",
  "updatedAt": "2025-01-10T09:00:00Z"
}
```

**バリデーション:**
- `employeeId`: 必須、有効なGUID
- `type`: 必須、有効な休暇種別
- `startDate`: 必須、過去の日付は不可
- `endDate`: 必須、startDate より後である必要がある
- `reason`: 必須、最大500文字

---

### GET /api/leaverequests/

全休暇申請を取得します。

**リクエスト:**

```http
GET /api/leaverequests/ HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
```

**レスポンス（200 OK）:**

```json
[
  {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "type": "PaidLeave",
    "startDate": "2025-01-20",
    "endDate": "2025-01-21",
    "reason": "私用のため",
    "status": "Pending",
    "approverId": null,
    "approvedAt": null,
    "approverComment": null,
    "days": 2,
    "createdAt": "2025-01-10T09:00:00Z",
    "updatedAt": "2025-01-10T09:00:00Z"
  }
]
```

---

### GET /api/leaverequests/status/{status}

ステータス別の休暇申請を取得します。

**リクエスト:**

```http
GET /api/leaverequests/status/Pending HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
```

**ステータス（Status）:**
- `Pending`: 承認待ち
- `Approved`: 承認済み
- `Rejected`: 却下
- `Cancelled`: キャンセル

**レスポンス（200 OK）:**

```json
[
  {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "type": "PaidLeave",
    "startDate": "2025-01-20",
    "endDate": "2025-01-21",
    "reason": "私用のため",
    "status": "Pending",
    "approverId": null,
    "approvedAt": null,
    "approverComment": null,
    "days": 2,
    "createdAt": "2025-01-10T09:00:00Z",
    "updatedAt": "2025-01-10T09:00:00Z"
  }
]
```

---

### POST /api/leaverequests/{id}/approve

休暇申請を承認します。

**リクエスト:**

```http
POST /api/leaverequests/7c9e6679-7425-40de-944b-e07fc1f90ae7/approve HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "approverId": "8d7f6543-1234-5678-9abc-def012345678",
  "comment": "承認します。"
}
```

**レスポンス（200 OK）:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "PaidLeave",
  "startDate": "2025-01-20",
  "endDate": "2025-01-21",
  "reason": "私用のため",
  "status": "Approved",
  "approverId": "8d7f6543-1234-5678-9abc-def012345678",
  "approvedAt": "2025-01-10T10:00:00Z",
  "approverComment": "承認します。",
  "days": 2,
  "createdAt": "2025-01-10T09:00:00Z",
  "updatedAt": "2025-01-10T10:00:00Z"
}
```

---

### POST /api/leaverequests/{id}/reject

休暇申請を却下します。

**リクエスト:**

```http
POST /api/leaverequests/7c9e6679-7425-40de-944b-e07fc1f90ae7/reject HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "approverId": "8d7f6543-1234-5678-9abc-def012345678",
  "comment": "業務上の理由により却下します。"
}
```

**レスポンス（200 OK）:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "PaidLeave",
  "startDate": "2025-01-20",
  "endDate": "2025-01-21",
  "reason": "私用のため",
  "status": "Rejected",
  "approverId": "8d7f6543-1234-5678-9abc-def012345678",
  "approvedAt": "2025-01-10T10:00:00Z",
  "approverComment": "業務上の理由により却下します。",
  "days": 2,
  "createdAt": "2025-01-10T09:00:00Z",
  "updatedAt": "2025-01-10T10:00:00Z"
}
```

---

### POST /api/leaverequests/{id}/cancel

休暇申請をキャンセルします。

**リクエスト:**

```http
POST /api/leaverequests/7c9e6679-7425-40de-944b-e07fc1f90ae7/cancel HTTP/1.1
Host: your-domain.com
Authorization: Bearer <JWT_TOKEN>
```

**レスポンス（200 OK）:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "PaidLeave",
  "startDate": "2025-01-20",
  "endDate": "2025-01-21",
  "reason": "私用のため",
  "status": "Cancelled",
  "approverId": null,
  "approvedAt": null,
  "approverComment": null,
  "days": 2,
  "createdAt": "2025-01-10T09:00:00Z",
  "updatedAt": "2025-01-10T10:00:00Z"
}
```

**注意:** キャンセルは申請者のみが実行できます。

---

### GET /health

ヘルスチェックエンドポイント。

**リクエスト:**

```http
GET /health HTTP/1.1
Host: your-domain.com
```

**レスポンス（200 OK）:**

```json
{
  "status": "healthy"
}
```

**注意:** このエンドポイントは認証不要です。

## エラーレスポンス

全てのエラーレスポンスは以下の形式で返されます。

### 400 Bad Request

```json
{
  "error": "エラーメッセージ",
  "traceId": "00-abc123..."
}
```

### 401 Unauthorized

```json
{
  "error": "認証が必要です。"
}
```

### 404 Not Found

```json
{
  "error": "リソースが見つかりません。"
}
```

### 500 Internal Server Error

```json
{
  "error": "内部サーバーエラーが発生しました。",
  "message": "詳細なエラーメッセージ（開発環境のみ）",
  "traceId": "00-def456..."
}
```

**注意:** `traceId` を使用してログとトレースを検索できます。

## レート制限

現在、レート制限は実装されていません。将来的に追加される可能性があります。

## バージョニング

現在は v1 のみサポートしています。将来的に新しいバージョンが追加される場合、URL に `/v2/` などを含める形式を検討しています。

## インタラクティブAPIドキュメント

開発環境では、Scalar UI を使用してインタラクティブにAPIを試すことができます。

```
https://your-domain.com/scalar
```

または、OpenAPI JSON ドキュメントは以下で取得できます。

```
https://your-domain.com/openapi/v1.json
```

## サンプルコード

### C# (.NET)

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;

var client = new HttpClient();
client.BaseAddress = new Uri("https://your-domain.com");
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", jwtToken);

// 出勤を記録
var checkInRequest = new
{
    employeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    checkInTime = DateTime.UtcNow
};

var response = await client.PostAsJsonAsync("/api/attendances/checkin", checkInRequest);
var attendance = await response.Content.ReadFromJsonAsync<AttendanceDto>();
```

### JavaScript (Fetch API)

```javascript
const baseUrl = 'https://your-domain.com';
const token = 'YOUR_JWT_TOKEN';

// 出勤を記録
const checkIn = async (employeeId) => {
  const response = await fetch(`${baseUrl}/api/attendances/checkin`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      employeeId: employeeId,
      checkInTime: new Date().toISOString()
    })
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error);
  }
  
  return await response.json();
};
```

### Python (requests)

```python
import requests
from datetime import datetime

base_url = 'https://your-domain.com'
token = 'YOUR_JWT_TOKEN'

headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

# 出勤を記録
def check_in(employee_id):
    response = requests.post(
        f'{base_url}/api/attendances/checkin',
        headers=headers,
        json={
            'employeeId': employee_id,
            'checkInTime': datetime.utcnow().isoformat() + 'Z'
        }
    )
    
    if response.status_code == 200:
        return response.json()
    else:
        raise Exception(response.json()['error'])
```

## 関連ドキュメント

- [AttendanceService 概要](./attendance-service.md)
- [デプロイメントガイド](./attendance-service-production-deployment.md)
- [トラブルシューティング](./attendance-service-troubleshooting.md)

## 更新履歴

| 日付 | バージョン | 変更内容 |
|------|-----------|---------|
| 2025-01-10 | 1.0 | 初版作成 |
