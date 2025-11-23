# AuthService MCP API Reference

## Overview

AuthService implements Model Context Protocol (MCP) server functionality, exposing user and role management operations as MCP tools. These tools can be accessed by MCP clients, including AI-powered applications like GitHub Copilot.

**MCP Endpoint**: `/api/mcp`

**Transport**: HTTP/SSE (Server-Sent Events)

## Authentication & Authorization

All MCP tools require authentication via JWT token. Include the token in the `Authorization` header:

```
Authorization: Bearer <jwt_token>
```

### Role-Based Access Control

- **User Role**: Can access their own user information
- **Admin Role**: Required for:
  - Viewing other users' information
  - Listing all users
  - Managing roles (assign/remove)
  - Viewing role information

## MCP Tools

### User Management Tools

#### 1. GetCurrentUserAsync

Gets detailed information about the currently logged-in user.

**Method**: `GetCurrentUserAsync`

**Parameters**: None

**Returns**: `UserDetailResponse`
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "johndoe",
  "email": "john@example.com",
  "emailConfirmed": true,
  "roles": ["User", "Manager"],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

**Required Role**: Any authenticated user

**Example Usage**:
```
"現在ログイン中のユーザー情報を取得して"
"自分のロールを確認したい"
```

#### 2. GetUserAsync

Gets detailed information about a specific user (Admin only).

**Method**: `GetUserAsync`

**Parameters**:
- `userId` (string): The user's ID (GUID format)

**Returns**: `UserDetailResponse`

**Required Role**: Admin

**Example Usage**:
```
"ユーザーID 3fa85f64-5717-4562-b3fc-2c963f66afa6 の情報を取得して"
```

#### 3. SearchUserByEmailAsync

Searches for a user by their email address (Admin only).

**Method**: `SearchUserByEmailAsync`

**Parameters**:
- `email` (string): The user's email address (exact match)

**Returns**: `UserDetailResponse` or `null` if not found

**Required Role**: Admin

**Example Usage**:
```
"john@example.com のメールアドレスを持つユーザーを検索して"
```

#### 4. ListUsersAsync

Gets a list of all registered users (Admin only).

**Method**: `ListUsersAsync`

**Parameters**: None

**Returns**: `UserListResponse`
```json
{
  "users": [
    {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userName": "johndoe",
      "email": "john@example.com",
      "roles": ["User"],
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 1
}
```

**Required Role**: Admin

**Example Usage**:
```
"全ユーザーの一覧を表示して"
"登録されているユーザーを確認したい"
```

#### 5. GetUserRolesAsync

Gets the role list for a specific user.

**Method**: `GetUserRolesAsync`

**Parameters**:
- `userId` (string, optional): User ID (defaults to current user if omitted)

**Returns**: `UserRolesResponse`
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "johndoe",
  "roles": ["User", "Manager"]
}
```

**Required Role**: 
- Any user (for their own roles)
- Admin (for other users' roles)

**Example Usage**:
```
"自分のロールを確認して"
"ユーザー3fa85f64-5717-4562-b3fc-2c963f66afa6のロールを教えて"
```

### Role Management Tools

#### 6. ListRolesAsync

Gets a list of all available roles in the system (Admin only).

**Method**: `ListRolesAsync`

**Parameters**: None

**Returns**: `RoleListResponse`
```json
{
  "roles": [
    {
      "roleId": "role-guid",
      "roleName": "Admin",
      "userCount": 2
    },
    {
      "roleId": "role-guid",
      "roleName": "User",
      "userCount": 15
    }
  ],
  "totalCount": 2
}
```

**Required Role**: Admin

**Example Usage**:
```
"利用可能なロールの一覧を表示して"
"システムにどんなロールがあるか教えて"
```

#### 7. GetUsersInRoleAsync

Gets a list of users who have a specific role (Admin only).

**Method**: `GetUsersInRoleAsync`

**Parameters**:
- `roleName` (string): Role name (e.g., "Admin", "User", "Manager")

**Returns**: `UsersInRoleResponse`
```json
{
  "roleName": "Admin",
  "users": [
    {
      "userId": "user-guid",
      "userName": "admin1",
      "email": "admin1@example.com"
    }
  ],
  "userCount": 1
}
```

**Required Role**: Admin

**Example Usage**:
```
"Adminロールを持つユーザーを全て表示して"
"Managerロールが割り当てられているのは誰？"
```

#### 8. AssignRoleAsync

Assigns a role to a user (Admin only).

**Method**: `AssignRoleAsync`

**Parameters**:
- `userId` (string): User's ID to assign the role to
- `roleName` (string): Role name to assign (e.g., "Admin", "Manager")

**Returns**: `AssignRoleResponse`
```json
{
  "success": true,
  "message": "ユーザー johndoe にロール 'Manager' を割り当てました。",
  "userId": "user-guid",
  "userName": "johndoe",
  "roleName": "Manager"
}
```

**Required Role**: Admin

**Example Usage**:
```
"ユーザー3fa85f64-5717-4562-b3fc-2c963f66afa6にManagerロールを割り当てて"
"john@example.comのユーザーをAdminにして"
```

**Note**: If the user already has the role, the operation succeeds with an appropriate message.

#### 9. RemoveRoleAsync

Removes a role from a user (Admin only).

**Method**: `RemoveRoleAsync`

**Parameters**:
- `userId` (string): User's ID to remove the role from
- `roleName` (string): Role name to remove (e.g., "Manager")

**Returns**: `RemoveRoleResponse`
```json
{
  "success": true,
  "message": "ユーザー johndoe からロール 'Manager' を削除しました。",
  "userId": "user-guid",
  "userName": "johndoe",
  "roleName": "Manager"
}
```

**Required Role**: Admin

**Example Usage**:
```
"ユーザー3fa85f64-5717-4562-b3fc-2c963f66afa6からManagerロールを削除して"
```

**Note**: If the user doesn't have the role, the operation succeeds with an appropriate message.

## Error Handling

MCP tools follow standard error handling patterns:

### Common Errors

- **UnauthorizedAccessException**: 
  - When user is not authenticated
  - When user lacks required role (e.g., Admin role for admin-only operations)
  
- **InvalidOperationException**: 
  - When requested user is not found
  - When requested role is not found
  - When role assignment/removal fails

- **ArgumentNullException**: 
  - When required parameters are null or empty

## Security Considerations

### Input Validation

All user inputs are validated:
- User IDs must be valid GUIDs
- Email addresses are validated for format
- Role names are checked for existence before operations

### Authorization Checks

- Each MCP tool enforces role-based access control
- Users can only view their own information unless they have Admin role
- All role management operations require Admin role
- Authorization is checked before any database operations

### Logging

All MCP tool calls are logged with:
- Tool name
- Requesting user ID (from JWT claims)
- Parameters
- Operation result (success/failure)

Example log entry:
```
MCP Tool: GetUser - UserId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
MCP Tool: AssignRole - UserId: target-user-id, RoleName: Manager
Successfully assigned role Manager to user target-user-id
```

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

### JWT Configuration

Configure JWT settings in `appsettings.json`:
```json
{
  "Jwt": {
    "Issuer": "EmployeeManagementSystem",
    "Audience": "EmployeeManagementSystem",
    "Key": "your-secret-key-at-least-32-characters-long",
    "ExpiryMinutes": 60
  }
}
```

## Integration with Other Services

### Employee Service Integration

AuthService MCP tools can be used in conjunction with EmployeeService MCP tools:

**Example workflow**:
1. Use `SearchUserByEmailAsync` to find user by email
2. Use `GetUserRolesAsync` to check user's roles
3. Use EmployeeService's `SearchEmployeeByEmailAsync` to find associated employee record
4. Use `AssignRoleAsync` to grant Manager role if employee is promoted

### Notification Service Integration

Role changes can trigger notifications:
- When a user is assigned Admin role
- When a user's Manager role is removed
- When a new user registers (automatic User role assignment)

## Testing

### Unit Tests

Run MCP tools unit tests:
```bash
dotnet test tests/AuthService.Tests/ --filter "FullyQualifiedName~Mcp"
```

### Integration Tests

Manual testing with MCP client:

1. **Authenticate and get JWT token**:
```bash
curl -X POST https://localhost:5002/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmail":"admin","password":"Admin@123"}'
```

2. **Connect MCP client to AuthService**:
```csharp
var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    BaseUrl = new Uri("https://localhost:5002/api/mcp"),
    HttpClient = new HttpClient
    {
        DefaultRequestHeaders = 
        {
            Authorization = new AuthenticationHeaderValue("Bearer", jwtToken)
        }
    }
});
var client = await McpClient.CreateAsync(transport);
```

3. **List available tools**:
```csharp
var tools = await client.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Tool: {tool.Name}");
}
```

## Common Use Cases

### 1. User Self-Service

Users can check their own information and roles:
```
User: "自分の情報を確認したい"
→ Calls: GetCurrentUserAsync()

User: "自分にどんなロールが割り当てられているか教えて"
→ Calls: GetUserRolesAsync()
```

### 2. Admin User Management

Admins can manage all users:
```
Admin: "john@example.com のユーザーにManagerロールを追加して"
→ Calls: SearchUserByEmailAsync(email) → AssignRoleAsync(userId, "Manager")

Admin: "Adminロールを持つユーザーを全て表示して"
→ Calls: GetUsersInRoleAsync("Admin")
```

### 3. Role Auditing

Check who has specific permissions:
```
Admin: "システムにどんなロールがあって、それぞれ何人のユーザーがいるか教えて"
→ Calls: ListRolesAsync()

Admin: "Managerロールを持つユーザーのロール一覧を表示して"
→ Calls: GetUsersInRoleAsync("Manager") → GetUserRolesAsync(userId) for each user
```

## Related Documentation

- [MCP Integration Design](mcp-integration-design.md) - Overall MCP architecture and design patterns
- [MCP Implementation Guide](mcp-implementation-guide.md) - Detailed implementation instructions
- [Authorization Implementation](authorization-implementation.md) - Authentication and authorization details
- [Employee Service MCP API](employee-service-mcp-api-reference.md) - Employee service MCP tools

## Future Enhancements

- Add password change operation (with additional security checks)
- Implement email verification status management
- Add user account lockout/unlock operations
- Implement audit log for security-sensitive operations
- Add batch operations for bulk role assignments
- Integrate with Notification Service for role change alerts

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-23  
**Status**: Production Ready
