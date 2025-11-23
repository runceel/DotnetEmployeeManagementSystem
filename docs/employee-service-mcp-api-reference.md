# EmployeeService MCP API Reference

## Overview

EmployeeService implements Model Context Protocol (MCP) server functionality, exposing employee and department management operations as MCP tools. These tools can be accessed by MCP clients, including AI-powered applications like GitHub Copilot.

**MCP Endpoint**: `/api/mcp`

**Transport**: HTTP/SSE (Server-Sent Events)

## MCP Tools

### Employee Management Tools

#### 1. GetEmployeeAsync

Gets detailed information about a specific employee.

**Method**: `GetEmployeeAsync`

**Parameters**:
- `employeeId` (string): The employee's GUID

**Returns**: `EmployeeDetailResponse`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "太郎",
  "lastName": "田中",
  "fullName": "田中 太郎",
  "email": "tanaka@example.com",
  "departmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "departmentName": "営業部",
  "position": "マネージャー",
  "hireDate": "2020-04-01T00:00:00Z",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

**Example Usage**:
```
"3fa85f64-5717-4562-b3fc-2c963f66afa6の従業員情報を取得して"
```

#### 2. ListEmployeesAsync

Gets a list of all employees.

**Method**: `ListEmployeesAsync`

**Parameters**: None

**Returns**: `EmployeeListResponse`
```json
{
  "employees": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fullName": "田中 太郎",
      "email": "tanaka@example.com",
      "departmentName": "営業部",
      "position": "マネージャー",
      "hireDate": "2020-04-01T00:00:00Z"
    }
  ],
  "totalCount": 1
}
```

**Example Usage**:
```
"全従業員の一覧を表示して"
```

#### 3. SearchEmployeeByEmailAsync

Searches for an employee by their email address.

**Method**: `SearchEmployeeByEmailAsync`

**Parameters**:
- `email` (string): The employee's email address (exact match)

**Returns**: `EmployeeDetailResponse` or `null` if not found

**Example Usage**:
```
"tanaka@example.comのメールアドレスを持つ従業員を検索して"
```

#### 4. CreateEmployeeAsync

Creates a new employee record.

**Method**: `CreateEmployeeAsync`

**Parameters**:
- `firstName` (string): Employee's first name
- `lastName` (string): Employee's last name
- `email` (string): Employee's email address
- `departmentId` (string): Department GUID
- `position` (string): Employee's position/role
- `hireDate` (string): Hire date in ISO 8601 format (e.g., "2024-01-01")

**Returns**: `EmployeeDetailResponse`

**Example Usage**:
```
"新しい従業員を登録して。名前は花子、苗字は佐藤、メールはsato@example.com、部署ID は3fa85f64-5717-4562-b3fc-2c963f66afa6、役職はエンジニア、入社日は2024-01-15"
```

#### 5. UpdateEmployeeAsync

Updates an existing employee's information.

**Method**: `UpdateEmployeeAsync`

**Parameters**:
- `employeeId` (string): Employee's GUID
- `firstName` (string): New first name
- `lastName` (string): New last name
- `email` (string): New email address
- `departmentId` (string): New department GUID
- `position` (string): New position
- `hireDate` (string): New hire date in ISO 8601 format

**Returns**: `EmployeeDetailResponse`

**Note**: All parameters are required when updating.

**Example Usage**:
```
"従業員3fa85f64-5717-4562-b3fc-2c963f66afa6の情報を更新して。メールをnewemail@example.comに変更"
```

#### 6. DeleteEmployeeAsync

Deletes an employee record.

**Method**: `DeleteEmployeeAsync`

**Parameters**:
- `employeeId` (string): Employee's GUID to delete

**Returns**: `DeleteEmployeeResponse`
```json
{
  "success": true,
  "message": "従業員 田中 太郎 (ID: 3fa85f64-5717-4562-b3fc-2c963f66afa6) を削除しました。"
}
```

**Warning**: This operation cannot be undone.

**Example Usage**:
```
"従業員3fa85f64-5717-4562-b3fc-2c963f66afa6を削除して"
```

### Department Management Tools

#### 7. GetDepartmentAsync

Gets detailed information about a specific department.

**Method**: `GetDepartmentAsync`

**Parameters**:
- `departmentId` (string): Department's GUID

**Returns**: `DepartmentDetailResponse`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "営業部",
  "description": "Sales department",
  "employeeCount": 5,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

**Example Usage**:
```
"3fa85f64-5717-4562-b3fc-2c963f66afa6の部署情報を取得して"
```

#### 8. ListDepartmentsAsync

Gets a list of all departments with employee counts.

**Method**: `ListDepartmentsAsync`

**Parameters**: None

**Returns**: `DepartmentListResponse`
```json
{
  "departments": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "営業部",
      "description": "Sales department",
      "employeeCount": 5
    }
  ],
  "totalCount": 1
}
```

**Example Usage**:
```
"全部署の一覧を表示して"
```

## Authentication & Authorization

The MCP endpoint currently does not require authentication in development mode. For production deployments, configure authentication as follows:

1. Update `Program.cs` to require authentication on the MCP endpoint:
```csharp
app.MapMcp("/api/mcp")
    .RequireAuthorization("EmployeeAccess");
```

2. Pass JWT tokens in MCP client requests using the `Authorization` header.

## Error Handling

MCP tools follow standard error handling patterns:

- **ArgumentNullException**: When required parameters are null or empty
- **ArgumentException**: When parameter formats are invalid (e.g., invalid GUID, invalid date format)
- **InvalidOperationException**: When requested entity is not found or business rules are violated

## Configuration

### CORS Settings

Development:
```csharp
policy.AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod();
```

Production (in `appsettings.json`):
```json
{
  "AllowedOrigins": [
    "https://yourdomain.com",
    "https://copilot.github.com"
  ]
}
```

## Testing

### Integration Tests

Run MCP endpoint integration tests:
```bash
dotnet test tests/EmployeeService.Integration.Tests/ --filter "FullyQualifiedName~McpEndpointTests"
```

### Manual Testing with MCP Client

See the sample MCP client in `samples/McpSample/McpSample.Client/` for examples of how to connect to and interact with the MCP server.

## Related Documentation

- [MCP Integration Design](mcp-integration-design.md) - Overall MCP architecture and design patterns
- [MCP Implementation Guide](mcp-implementation-guide.md) - Detailed implementation instructions
- [Development Guide](development-guide.md) - General development guidelines

## Future Enhancements

- Add support for search/filtering on ListEmployeesAsync
- Implement partial updates for UpdateEmployeeAsync
- Add batch operations for bulk employee creation/updates
- Integrate with NotificationService for employee lifecycle events
- Add MCP resources for retrieving employee documents
