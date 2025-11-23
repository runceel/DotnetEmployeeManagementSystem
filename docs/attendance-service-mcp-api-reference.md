# AttendanceService MCP API Reference

## Overview

AttendanceService implements Model Context Protocol (MCP) server functionality, exposing attendance and leave request management operations as MCP tools. These tools can be accessed by MCP clients, including AI-powered applications like GitHub Copilot.

**MCP Endpoint**: `/api/mcp`

**Transport**: HTTP/SSE (Server-Sent Events)

## MCP Tools

### Attendance Management Tools (AttendanceTools)

#### 1. GetAttendanceAsync

Gets detailed information about a specific attendance record.

**Method**: `GetAttendanceAsync`

**Parameters**:
- `attendanceId` (string): The attendance record's GUID

**Returns**: `AttendanceDetailResponse`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "workDate": "2024-01-15T00:00:00Z",
  "checkInTime": "2024-01-15T09:00:00Z",
  "checkOutTime": "2024-01-15T18:00:00Z",
  "type": "Normal",
  "notes": null,
  "workHours": 9.0,
  "createdAt": "2024-01-15T09:00:00Z",
  "updatedAt": "2024-01-15T18:00:00Z"
}
```

**Example Usage**:
```
"勤怠記録 3fa85f64-5717-4562-b3fc-2c963f66afa6 の詳細を表示して"
```

**Errors**:
- `ArgumentException`: Invalid attendance ID format
- `InvalidOperationException`: Attendance record not found

---

#### 2. ListAttendancesAsync

Gets a list of attendance records for a specific employee. Supports date range filtering.

**Method**: `ListAttendancesAsync`

**Parameters**:
- `employeeId` (string): The employee's GUID
- `startDate` (string, optional): Start date in ISO 8601 format (e.g., "2024-01-01")
- `endDate` (string, optional): End date in ISO 8601 format (e.g., "2024-01-31")

**Returns**: `AttendanceListResponse`
```json
{
  "attendances": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "workDate": "2024-01-15T00:00:00Z",
      "checkInTime": "2024-01-15T09:00:00Z",
      "checkOutTime": "2024-01-15T18:00:00Z",
      "type": "Normal",
      "workHours": 9.0
    }
  ],
  "totalCount": 1
}
```

**Example Usage**:
```
"従業員 3fa85f64-5717-4562-b3fc-2c963f66afa6 の2024年1月の勤怠記録を表示して"
"田中さんの先週の勤怠を確認して"
```

**Errors**:
- `ArgumentException`: Invalid employee ID, start date, or end date format

---

#### 3. CheckInAsync

Records an employee's check-in (arrival time).

**Method**: `CheckInAsync`

**Parameters**:
- `employeeId` (string): The employee's GUID
- `checkInTime` (string): Check-in time in ISO 8601 format (e.g., "2024-01-15T09:00:00Z")
- `attendanceType` (string, optional): Attendance type - valid values: "Normal", "Remote", "BusinessTrip", "HalfDay". Default: "Normal"
- `notes` (string, optional): Additional notes (max 500 characters)

**Returns**: `AttendanceDetailResponse`

**Example Usage**:
```
"従業員 3fa85f64-5717-4562-b3fc-2c963f66afa6 の出勤を今日の9時に記録して"
"田中さんのリモート勤務での出勤を8:30に記録"
```

**Business Rules**:
- Only one attendance record per employee per day
- Check-in time must be on the work date
- Attendance type defaults to "Normal" if not specified

**Errors**:
- `ArgumentException`: Invalid employee ID, check-in time, or attendance type
- `InvalidOperationException`: Attendance record already exists for the date

---

#### 4. CheckOutAsync

Records an employee's check-out (departure time).

**Method**: `CheckOutAsync`

**Parameters**:
- `employeeId` (string): The employee's GUID
- `checkOutTime` (string): Check-out time in ISO 8601 format (e.g., "2024-01-15T18:00:00Z")

**Returns**: `AttendanceDetailResponse`

**Example Usage**:
```
"従業員 3fa85f64-5717-4562-b3fc-2c963f66afa6 の退勤を18時に記録して"
"田中さんの退勤を記録"
```

**Business Rules**:
- Check-in must be recorded before check-out
- Check-out time must be after check-in time
- Only one check-out per attendance record

**Errors**:
- `ArgumentException`: Invalid employee ID or check-out time format
- `InvalidOperationException`: No check-in record found, or check-out already recorded

---

#### 5. GetMonthlySummaryAsync

Gets monthly attendance summary statistics for an employee.

**Method**: `GetMonthlySummaryAsync`

**Parameters**:
- `employeeId` (string): The employee's GUID
- `year` (int): Target year (2000-2100)
- `month` (int): Target month (1-12)

**Returns**: `MonthlySummaryResponse`
```json
{
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "year": 2024,
  "month": 1,
  "totalWorkDays": 20,
  "totalWorkHours": 160.0,
  "averageWorkHours": 8.0,
  "lateDays": 2
}
```

**Statistics Included**:
- `totalWorkDays`: Number of days with both check-in and check-out
- `totalWorkHours`: Sum of work hours for the month
- `averageWorkHours`: Average work hours per day
- `lateDays`: Number of days with check-in after 9:00 AM

**Example Usage**:
```
"従業員 3fa85f64-5717-4562-b3fc-2c963f66afa6 の2024年1月の勤怠集計を表示して"
"田中さんの今月の勤怠サマリーを見せて"
```

**Errors**:
- `ArgumentException`: Invalid employee ID, year out of range (2000-2100), or month out of range (1-12)

---

### Leave Request Management Tools (LeaveRequestTools)

#### 1. GetLeaveRequestAsync

Gets detailed information about a specific leave request.

**Method**: `GetLeaveRequestAsync`

**Parameters**:
- `leaveRequestId` (string): The leave request's GUID

**Returns**: `LeaveRequestDetailResponse`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "PaidLeave",
  "startDate": "2024-01-15T00:00:00Z",
  "endDate": "2024-01-17T00:00:00Z",
  "days": 3,
  "reason": "家族旅行",
  "status": "Pending",
  "approverId": null,
  "approvedAt": null,
  "approverComment": null,
  "createdAt": "2024-01-10T10:00:00Z",
  "updatedAt": "2024-01-10T10:00:00Z"
}
```

**Example Usage**:
```
"休暇申請 3fa85f64-5717-4562-b3fc-2c963f66afa6 の詳細を表示して"
```

**Errors**:
- `ArgumentException`: Invalid leave request ID format
- `InvalidOperationException`: Leave request not found

---

#### 2. ListLeaveRequestsAsync

Gets a list of leave requests. Supports filtering by employee ID or status.

**Method**: `ListLeaveRequestsAsync`

**Parameters**:
- `employeeId` (string, optional): Filter by employee GUID
- `status` (string, optional): Filter by status - valid values: "Pending", "Approved", "Rejected", "Cancelled"

**Returns**: `LeaveRequestListResponse`
```json
{
  "leaveRequests": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "type": "PaidLeave",
      "startDate": "2024-01-15T00:00:00Z",
      "endDate": "2024-01-17T00:00:00Z",
      "days": 3,
      "status": "Pending",
      "createdAt": "2024-01-10T10:00:00Z"
    }
  ],
  "totalCount": 1
}
```

**Example Usage**:
```
"申請中の休暇申請を全て表示して"
"従業員 3fa85f64-5717-4562-b3fc-2c963f66afa6 の承認済み休暇申請を表示"
"田中さんの休暇申請一覧を見せて"
```

**Errors**:
- `ArgumentException`: Invalid employee ID or status format

---

#### 3. CreateLeaveRequestAsync

Creates a new leave request.

**Method**: `CreateLeaveRequestAsync`

**Parameters**:
- `employeeId` (string): The employee's GUID
- `leaveType` (string): Leave type - valid values: "PaidLeave", "SickLeave", "SpecialLeave", "UnpaidLeave"
- `startDate` (string): Start date in ISO 8601 format (e.g., "2024-01-15")
- `endDate` (string): End date in ISO 8601 format (e.g., "2024-01-17")
- `reason` (string): Reason for leave request (max 1000 characters)

**Returns**: `LeaveRequestDetailResponse`

**Example Usage**:
```
"従業員 3fa85f64-5717-4562-b3fc-2c963f66afa6 の有給休暇申請を1月15日から1月17日で作成して、理由は家族旅行"
"田中さんの病気休暇を明日から3日間申請して、理由は体調不良"
```

**Business Rules**:
- Start date must be before or equal to end date
- End date must be today or in the future
- System checks for overlapping approved leave requests
- Automatically calculates number of days

**Leave Types**:
- `PaidLeave`: 有給休暇
- `SickLeave`: 病気休暇
- `SpecialLeave`: 特別休暇
- `UnpaidLeave`: 無給休暇

**Errors**:
- `ArgumentException`: Invalid parameters or date format
- `InvalidOperationException`: Overlapping approved leave request exists

---

#### 4. ApproveLeaveRequestAsync

Approves a leave request.

**Method**: `ApproveLeaveRequestAsync`

**Parameters**:
- `leaveRequestId` (string): The leave request's GUID
- `approverId` (string): The approver's GUID
- `comment` (string, optional): Approver's comment

**Returns**: `LeaveRequestDetailResponse`

**Example Usage**:
```
"休暇申請 3fa85f64-5717-4562-b3fc-2c963f66afa6 を承認者 xxx で承認して"
"この休暇申請を承認して、コメントは問題ありません"
```

**Business Rules**:
- Only pending requests can be approved
- Approval is permanent (cannot be undone)

**Errors**:
- `ArgumentException`: Invalid leave request ID or approver ID
- `InvalidOperationException`: Leave request not found, or status is not pending

---

#### 5. RejectLeaveRequestAsync

Rejects a leave request.

**Method**: `RejectLeaveRequestAsync`

**Parameters**:
- `leaveRequestId` (string): The leave request's GUID
- `approverId` (string): The approver's GUID
- `comment` (string, optional): Rejection reason

**Returns**: `LeaveRequestDetailResponse`

**Example Usage**:
```
"休暇申請 3fa85f64-5717-4562-b3fc-2c963f66afa6 を却下して、理由は業務繁忙期のため"
```

**Business Rules**:
- Only pending requests can be rejected
- Rejection is permanent (cannot be undone)

**Errors**:
- `ArgumentException`: Invalid leave request ID or approver ID
- `InvalidOperationException`: Leave request not found, or status is not pending

---

#### 6. CancelLeaveRequestAsync

Cancels a leave request (by the employee).

**Method**: `CancelLeaveRequestAsync`

**Parameters**:
- `leaveRequestId` (string): The leave request's GUID

**Returns**: `LeaveRequestDetailResponse`

**Example Usage**:
```
"休暇申請 3fa85f64-5717-4562-b3fc-2c963f66afa6 をキャンセルして"
```

**Business Rules**:
- Pending or approved requests can be cancelled
- Rejected requests cannot be cancelled

**Errors**:
- `ArgumentException`: Invalid leave request ID
- `InvalidOperationException`: Leave request not found, already cancelled, or rejected

---

## Data Types Reference

### Attendance Types
- `Normal`: 通常勤務
- `Remote`: リモートワーク
- `BusinessTrip`: 出張
- `HalfDay`: 半日勤務

### Leave Types
- `PaidLeave`: 有給休暇
- `SickLeave`: 病気休暇
- `SpecialLeave`: 特別休暇
- `UnpaidLeave`: 無給休暇

### Leave Request Status
- `Pending`: 申請中
- `Approved`: 承認済み
- `Rejected`: 却下
- `Cancelled`: キャンセル

---

## Error Handling

All MCP tools follow consistent error handling patterns:

### Common Error Types

1. **ArgumentException**
   - Invalid GUID format
   - Invalid date format (must be ISO 8601)
   - Invalid enum values
   - Out of range values

2. **InvalidOperationException**
   - Resource not found
   - Business rule violations
   - State transition errors

### Error Response Format

Errors are returned as structured exceptions with descriptive messages in Japanese.

**Example**:
```
InvalidOperationException: 従業員ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 の 2024-01-15 の勤怠記録は既に存在します。
```

---

## Security

### CORS Configuration

#### Development Environment
- All origins allowed for ease of development
- Configured in `Program.cs` with `AllowAnyOrigin()`

#### Production Environment
- Only configured origins allowed
- Set in `appsettings.json` under `AllowedOrigins` section
- Credentials allowed for authenticated requests

**Example Configuration**:
```json
{
  "AllowedOrigins": [
    "https://your-blazor-app.azurewebsites.net",
    "https://your-copilot-client.com"
  ]
}
```

### Authentication

Current status: **Not implemented**

Future plan: JWT Bearer token authentication integrated with AuthService

---

## OpenTelemetry Integration

All MCP tool invocations are automatically traced and logged:

### Structured Logging

Every tool call includes:
- Tool name
- Input parameters
- Employee/Resource IDs
- Timestamps

**Example Log Entries**:
```
MCP Tool: CheckIn - EmployeeId: 3fa85f64-5717-4562-b3fc-2c963f66afa6, CheckInTime: 2024-01-15T09:00:00Z
MCP Tool: ListAttendances - EmployeeId: 3fa85f64-5717-4562-b3fc-2c963f66afa6, StartDate: 2024-01-01, EndDate: 2024-01-31
MCP Tool: CreateLeaveRequest - EmployeeId: 3fa85f64-5717-4562-b3fc-2c963f66afa6, Type: PaidLeave, StartDate: 2024-01-15, EndDate: 2024-01-17
```

### Distributed Tracing

- All operations include trace IDs for end-to-end tracking
- Integrated with Aspire dashboard for visualization
- Correlates with database operations and event publishing

---

## Testing

### Integration Tests

Location: `tests/AttendanceService.Integration.Tests/Mcp/`

**Test Coverage**:
- MCP endpoint accessibility
- Invalid request handling
- CORS configuration validation

**Running Tests**:
```bash
dotnet test tests/AttendanceService.Integration.Tests/
```

### Manual Testing

Use tools like:
- **Postman** or **curl** for HTTP requests
- **MCP Inspector** for protocol-level testing
- **Blazor MCP Client** for end-to-end scenarios

---

## Troubleshooting

### Issue: MCP endpoint returns 404

**Solution**: Verify that:
- `MapMcp("/api/mcp")` is called in `Program.cs`
- Service is running in the correct environment
- Route is not conflicting with other endpoints

### Issue: CORS errors in browser

**Solution**:
- For development: Ensure `McpPolicy` allows any origin
- For production: Add your client origin to `AllowedOrigins` in `appsettings.json`
- Verify CORS middleware is enabled with `app.UseCors("McpPolicy")`

### Issue: Tool returns "Invalid ID format"

**Solution**: 
- Ensure GUIDs are in standard format with hyphens: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- Use lowercase or uppercase consistently

### Issue: Date parsing errors

**Solution**:
- Use ISO 8601 format: `YYYY-MM-DD` or `YYYY-MM-DDTHH:mm:ssZ`
- Include timezone information when specifying time
- Example: `2024-01-15T09:00:00Z`

---

## Related Documentation

- [AttendanceService Overview](./attendance-service.md)
- [MCP Integration Design](./mcp-integration-design.md)
- [MCP Implementation Guide](./mcp-implementation-guide.md)
- [AttendanceService API Reference](./attendance-service-api-reference.md)
- [EmployeeService MCP API Reference](./employee-service-mcp-api-reference.md)

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-23  
**Status**: Complete
