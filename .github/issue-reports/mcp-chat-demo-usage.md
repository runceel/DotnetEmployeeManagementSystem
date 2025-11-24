# MCP Chat - Demonstration Usage Guide

**Created**: 2024-11-24  
**Purpose**: Demonstrate MCP Chat functionality with actual usage examples  
**Note**: Chat interactions shown in English for clarity; UI remains in Japanese

## Prerequisites for Screenshots

To capture actual screenshots of the MCP Chat in action:

```bash
# 1. Start Aspire AppHost
cd /home/runner/work/DotnetEmployeeManagementSystem/DotnetEmployeeManagementSystem
dotnet run --project src/AppHost

# 2. Access Aspire Dashboard
# The dashboard URL will be displayed in the console output
# Example: http://localhost:15000

# 3. Find BlazorWeb URL from the dashboard
# Example: http://localhost:5001

# 4. Login to the application
# Navigate to Login page if not already authenticated

# 5. Access MCP Chat
# Click on "MCPチャット" in the navigation menu
# Or directly: http://localhost:5001/mcp-chat
```

## Expected Demo Flow

### Step 1: Initial Login Screen
**Action**: Login to the system  
**Expected Result**: Navigate to dashboard with MCP Chat menu visible

### Step 2: Navigate to MCP Chat
**Action**: Click "MCPチャット" in navigation menu  
**Expected URL**: `/mcp-chat`  
**Expected View**: Connection screen with 4 service cards

### Step 3: Connect to All Services
**Action**: Click "全サービスに接続" button  
**Expected Result**: 
- Loading state with spinner
- Success message: "4個のサーバーに接続しました"
- All 4 services show green checkmarks

### Step 4: Select Employee Service
**Action**: Click on "従業員サービス" in the connection list  
**Expected Result**: Tools list shows Employee Service tools

### Step 5: Execute ListEmployeesAsync
**Tool**: ListEmployeesAsync  
**Arguments**: `{}`  
**Action**: Click "ツールを実行"

**Expected Chat History**:
```
[You]
[従業員サービス] ListEmployeesAsync
Arguments: {}
09:15:32

[MCP]
✅ Execution result:

{
  "employees": [
    {
      "id": "guid-1",
      "fullName": "Sample Employee 1",
      "email": "employee1@example.com",
      "departmentName": "Engineering",
      "position": "Engineer",
      "hireDate": "2020-01-15"
    },
    {
      "id": "guid-2",
      "fullName": "Sample Employee 2",
      "email": "employee2@example.com",
      "departmentName": "Sales",
      "position": "Sales Manager",
      "hireDate": "2019-03-20"
    }
  ],
  "totalCount": 2
}
09:15:33
```

### Step 6: Execute GetEmployeeAsync with Argument
**Tool**: GetEmployeeAsync  
**Arguments**: 
```json
{
  "employeeId": "guid-from-previous-result"
}
```
**Action**: Click "ツールを実行"

**Expected Chat History**:
```
[You]
[従業員サービス] GetEmployeeAsync
Arguments: {
  "employeeId": "guid-1"
}
09:16:45

[MCP]
✅ Execution result:

{
  "id": "guid-1",
  "firstName": "Sample",
  "lastName": "Employee 1",
  "fullName": "Sample Employee 1",
  "email": "employee1@example.com",
  "departmentId": "dept-guid",
  "departmentName": "Engineering",
  "position": "Engineer",
  "hireDate": "2020-01-15T00:00:00Z",
  "createdAt": "2020-01-10T10:30:00Z",
  "updatedAt": "2024-11-20T14:20:00Z"
}
09:16:46
```

### Step 7: Switch to Authentication Service
**Action**: Click on "認証サービス" in connection list  
**Expected Result**: Tools list updates to show Auth Service tools

### Step 8: Execute ListUsersAsync
**Tool**: ListUsersAsync  
**Arguments**: `{}`  
**Action**: Click "ツールを実行"

**Expected Chat History**:
```
[You]
[認証サービス] ListUsersAsync
Arguments: {}
09:18:12

[MCP]
✅ Execution result:

{
  "users": [
    {
      "id": "user-guid-1",
      "userName": "admin",
      "email": "admin@example.com",
      "roles": ["Admin"]
    },
    {
      "id": "user-guid-2",
      "userName": "user1",
      "email": "user1@example.com",
      "roles": ["User"]
    }
  ],
  "totalCount": 2
}
09:18:13
```

### Step 9: Test Error Handling - Invalid JSON
**Tool**: GetEmployeeAsync  
**Arguments**: 
```
{
  employeeId: "missing-quotes"
}
```
**Action**: Click "ツールを実行"

**Expected Result**: Red error alert
```
❌ JSON形式が正しくありません: 'e' is an invalid start of a property name. 
Expected a '"'. LineNumber: 1 | BytePositionInLine: 4.
```

### Step 10: Test Error Handling - Invalid GUID
**Tool**: GetEmployeeAsync  
**Arguments**: 
```json
{
  "employeeId": "invalid-guid-format"
}
```
**Action**: Click "ツールを実行"

**Expected Chat History**:
```
[You]
[従業員サービス] GetEmployeeAsync
Arguments: {
  "employeeId": "invalid-guid-format"
}
09:20:05

[MCP]
❌ Error: Invalid employee ID format
09:20:06
```

### Step 11: Switch to Notification Service
**Action**: Click on "通知サービス" in connection list  
**Expected Result**: Tools list updates to show Notification Service tools

### Step 12: Execute ListNotificationsAsync
**Tool**: ListNotificationsAsync  
**Arguments**: 
```json
{
  "userId": "user-guid-from-auth-service"
}
```
**Action**: Click "ツールを実行"

**Expected Chat History**:
```
[You]
[通知サービス] ListNotificationsAsync
Arguments: {
  "userId": "user-guid-1"
}
09:22:30

[MCP]
✅ Execution result:

{
  "notifications": [
    {
      "id": "notif-guid-1",
      "userId": "user-guid-1",
      "message": "New employee registered",
      "isRead": false,
      "createdAt": "2024-11-24T09:00:00Z"
    },
    {
      "id": "notif-guid-2",
      "userId": "user-guid-1",
      "message": "Attendance record updated",
      "isRead": true,
      "createdAt": "2024-11-23T15:30:00Z"
    }
  ],
  "totalCount": 2
}
09:22:31
```

### Step 13: Clear Chat History
**Action**: Click the delete icon (🗑️) in chat history header  
**Expected Result**: All chat messages cleared, empty state message shown

### Step 14: Refresh Tools List
**Action**: Click refresh icon (🔄) in connection status header  
**Expected Result**: System message "ツールリストを更新しました"

### Step 15: Disconnect All
**Action**: Click disconnect icon (🔗) in connection status header  
**Expected Result**: Return to initial connection screen

## Screenshot Checklist

When capturing screenshots, ensure you capture:

- [ ] **Screenshot 1**: Login page with credentials entered
- [ ] **Screenshot 2**: Main dashboard with "MCPチャット" menu visible
- [ ] **Screenshot 3**: MCP Chat initial screen (before connection)
- [ ] **Screenshot 4**: Connection in progress (spinner visible)
- [ ] **Screenshot 5**: Connected state - all services green checkmark
- [ ] **Screenshot 6**: Employee Service selected - tools list visible
- [ ] **Screenshot 7**: ListEmployeesAsync tool selected - empty arguments
- [ ] **Screenshot 8**: Chat showing ListEmployeesAsync execution result
- [ ] **Screenshot 9**: GetEmployeeAsync with arguments entered
- [ ] **Screenshot 10**: Chat showing GetEmployeeAsync detailed result
- [ ] **Screenshot 11**: Auth Service selected - different tools
- [ ] **Screenshot 12**: ListUsersAsync execution result
- [ ] **Screenshot 13**: Error state - invalid JSON format
- [ ] **Screenshot 14**: Error state - invalid GUID
- [ ] **Screenshot 15**: Help guide expanded

## Key Points for Screenshots

### UI Elements to Highlight
1. **Navigation**: Show MCPチャット menu item clearly
2. **Service Cards**: 4 services with icons before connection
3. **Connection Status**: Green checkmarks for connected services
4. **Tool List**: Different tools for different services
5. **JSON Editor**: Show sample arguments being entered
6. **Chat Messages**: Blue (user) vs Gray (system) messages
7. **Success Icon**: ✅ for successful executions
8. **Error Icon**: ❌ for failed executions
9. **Timestamps**: Show on all messages
10. **Help Guide**: Expandable section at bottom

### Recommended Screenshot Settings
- **Browser**: Chrome/Edge in desktop mode
- **Window Size**: 1920x1080 or larger
- **Zoom Level**: 100%
- **DevTools**: Closed (unless showing network/console)
- **Format**: PNG for clarity
- **Location**: Save to `.github/issue-reports/screenshots/`

### Screenshot Naming Convention
```
mcp-chat-01-login.png
mcp-chat-02-dashboard.png
mcp-chat-03-initial-screen.png
mcp-chat-04-connecting.png
mcp-chat-05-connected.png
mcp-chat-06-employee-service.png
mcp-chat-07-list-employees-tool.png
mcp-chat-08-list-employees-result.png
mcp-chat-09-get-employee-args.png
mcp-chat-10-get-employee-result.png
mcp-chat-11-auth-service.png
mcp-chat-12-list-users-result.png
mcp-chat-13-error-invalid-json.png
mcp-chat-14-error-invalid-guid.png
mcp-chat-15-help-guide.png
```

## Notes

- All chat interactions are in **English** (as requested) for clarity
- UI labels remain in **Japanese** (original design)
- JSON responses are properly formatted
- Error messages are descriptive
- Timestamps are shown in HH:mm:ss format
- User messages are right-aligned with blue background
- System messages are left-aligned with gray background

## Testing in Local Environment

The MCP Chat feature requires:
1. **Aspire AppHost** running
2. **All 4 microservices** operational:
   - EmployeeService.API
   - AuthService.API
   - NotificationService.API
   - AttendanceService.API
3. **SQLite databases** initialized with migrations
4. **Redis** running for pub/sub (if applicable)

If any service fails to start, the connection screen will show warnings for failed connections.

---

**Created by**: GitHub Copilot  
**Date**: 2024-11-24  
**Purpose**: Guide for capturing actual MCP Chat screenshots with English interactions
