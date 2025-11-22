# MCP実装ガイド - 実践的な開発手順とサンプルコード

## ドキュメント概要

本ドキュメントは、[MCP統合設計書](mcp-integration-design.md)の内容を実際に実装する際の具体的な手順とサンプルコードを提供します。

**対象読者**: 開発者  
**前提条件**: MCP統合設計書の理解、.NET 10の基礎知識  
**作成日**: 2025-11-22

---

## 目次

1. [開発環境セットアップ](#1-開発環境セットアップ)
2. [EmployeeService MCP実装例](#2-employeeservice-mcp実装例)
3. [NotificationService MCP実装例](#3-notificationservice-mcp実装例)
4. [Blazor MCPクライアント実装例](#4-blazor-mcpクライアント実装例)
5. [テスト実装](#5-テスト実装)
6. [トラブルシューティング](#6-トラブルシューティング)

---

## 1. 開発環境セットアップ

### 1.1 NuGetパッケージの追加

#### サービス側（各マイクロサービスAPI）

```bash
# EmployeeService.APIプロジェクトに移動
cd src/Services/EmployeeService/API

# MCPパッケージ追加
dotnet add package ModelContextProtocol.AspNetCore --prerelease

# 同様に他のサービスにも追加
cd ../../../AuthService/API
dotnet add package ModelContextProtocol.AspNetCore --prerelease

cd ../../../NotificationService/API
dotnet add package ModelContextProtocol.AspNetCore --prerelease

cd ../../../AttendanceService/API
dotnet add package ModelContextProtocol.AspNetCore --prerelease
```

#### クライアント側（BlazorWeb）

```bash
cd src/WebApps/BlazorWeb

# MCPクライアントパッケージ追加
dotnet add package ModelContextProtocol --prerelease

# Azure OpenAI（推奨）
dotnet add package Azure.AI.OpenAI --prerelease

# または Semantic Kernel
dotnet add package Microsoft.SemanticKernel --prerelease

# Microsoft.Extensions.AI（MCPと連携）
dotnet add package Microsoft.Extensions.AI --prerelease
dotnet add package Microsoft.Extensions.AI.OpenAI --prerelease
```

### 1.2 設定ファイルの準備

#### appsettings.json（BlazorWeb）

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o",
    "ApiKey": "your-api-key-here"
  },
  "McpServers": {
    "EmployeeService": "https://localhost:5001/api/mcp",
    "AuthService": "https://localhost:5002/api/mcp",
    "NotificationService": "https://localhost:5003/api/mcp",
    "AttendanceService": "https://localhost:5004/api/mcp"
  },
  "Jwt": {
    "Issuer": "EmployeeManagementSystem",
    "Audience": "EmployeeManagementSystem",
    "Key": "your-secret-key-at-least-32-characters-long"
  }
}
```

---

## 2. EmployeeService MCP実装例

### 2.1 ディレクトリ構造作成

```bash
cd src/Services/EmployeeService/API

# MCPディレクトリ作成
mkdir -p Mcp/Tools
mkdir -p Mcp/Configuration
```

### 2.2 EmployeeTools.cs 実装

`src/Services/EmployeeService/API/Mcp/Tools/EmployeeTools.cs`:

```csharp
using EmployeeService.Application.DTOs;
using EmployeeService.Application.Interfaces;
using EmployeeService.Domain.Entities;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace EmployeeService.API.Mcp.Tools;

/// <summary>
/// 従業員管理に関するMCPツール
/// </summary>
[McpServerToolType]
public class EmployeeTools
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<EmployeeTools> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmployeeTools(
        IEmployeeRepository repository,
        ILogger<EmployeeTools> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 従業員情報を取得します
    /// </summary>
    [McpServerTool(Description = "指定された従業員IDの詳細情報を取得します。従業員の基本情報、連絡先、所属部署などが取得できます。")]
    public async Task<EmployeeDetailResponse> GetEmployeeAsync(
        [Description("取得したい従業員のID")] int employeeId)
    {
        _logger.LogInformation("MCP Tool: GetEmployee - EmployeeId: {EmployeeId}", employeeId);

        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"従業員ID {employeeId} が見つかりませんでした。");
        }

        return new EmployeeDetailResponse(
            Id: employee.Id,
            FirstName: employee.FirstName,
            LastName: employee.LastName,
            FullName: $"{employee.LastName} {employee.FirstName}",
            Email: employee.Email,
            Department: employee.Department,
            Position: employee.Position,
            HireDate: employee.HireDate,
            PhoneNumber: employee.PhoneNumber
        );
    }

    /// <summary>
    /// 全従業員の一覧を取得します
    /// </summary>
    [McpServerTool(Description = "全従業員の一覧を取得します。各従業員の基本情報が含まれます。")]
    public async Task<EmployeeListResponse> ListEmployeesAsync()
    {
        _logger.LogInformation("MCP Tool: ListEmployees");

        var employees = await _repository.GetAllAsync();
        var employeeList = employees.Select(e => new EmployeeSummary(
            Id: e.Id,
            FullName: $"{e.LastName} {e.FirstName}",
            Email: e.Email,
            Department: e.Department,
            Position: e.Position
        )).ToList();

        return new EmployeeListResponse(
            Employees: employeeList,
            TotalCount: employeeList.Count
        );
    }

    /// <summary>
    /// 従業員を検索します
    /// </summary>
    [McpServerTool(Description = "名前、メールアドレス、または部署で従業員を検索します。部分一致で検索できます。")]
    public async Task<EmployeeListResponse> SearchEmployeesAsync(
        [Description("検索キーワード（名前、メール、部署など）")] string query,
        [Description("検索対象の部署名（オプション）")] string? department = null)
    {
        _logger.LogInformation("MCP Tool: SearchEmployees - Query: {Query}, Department: {Department}", 
            query, department);

        if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(department))
        {
            throw new ArgumentException("検索キーワードまたは部署名を指定してください。");
        }

        var allEmployees = await _repository.GetAllAsync();
        var filteredEmployees = allEmployees.Where(e =>
        {
            var matchesQuery = string.IsNullOrWhiteSpace(query) ||
                e.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.LastName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Email.Contains(query, StringComparison.OrdinalIgnoreCase);

            var matchesDepartment = string.IsNullOrWhiteSpace(department) ||
                e.Department.Contains(department, StringComparison.OrdinalIgnoreCase);

            return matchesQuery && matchesDepartment;
        }).ToList();

        var employeeList = filteredEmployees.Select(e => new EmployeeSummary(
            Id: e.Id,
            FullName: $"{e.LastName} {e.FirstName}",
            Email: e.Email,
            Department: e.Department,
            Position: e.Position
        )).ToList();

        return new EmployeeListResponse(
            Employees: employeeList,
            TotalCount: employeeList.Count
        );
    }

    /// <summary>
    /// 新しい従業員を登録します
    /// </summary>
    [McpServerTool(Description = "新しい従業員を登録します。管理者権限が必要です。")]
    public async Task<EmployeeDetailResponse> CreateEmployeeAsync(
        [Description("名前（例: 太郎）")] string firstName,
        [Description("苗字（例: 田中）")] string lastName,
        [Description("メールアドレス")] string email,
        [Description("所属部署")] string department,
        [Description("役職")] string position,
        [Description("電話番号（オプション）")] string? phoneNumber = null)
    {
        _logger.LogInformation("MCP Tool: CreateEmployee - Name: {LastName} {FirstName}", 
            lastName, firstName);

        // 権限チェック
        EnsureAdminRole();

        // 入力検証
        ValidateEmployeeInput(firstName, lastName, email, department, position);

        // ドメインエンティティ作成
        var employee = Employee.Create(
            firstName: firstName.Trim(),
            lastName: lastName.Trim(),
            email: email.Trim().ToLowerInvariant(),
            department: department.Trim(),
            position: position.Trim(),
            phoneNumber: phoneNumber?.Trim()
        );

        await _repository.AddAsync(employee);

        _logger.LogInformation("MCP Tool: CreateEmployee - Successfully created employee {EmployeeId}", 
            employee.Id);

        return new EmployeeDetailResponse(
            Id: employee.Id,
            FirstName: employee.FirstName,
            LastName: employee.LastName,
            FullName: $"{employee.LastName} {employee.FirstName}",
            Email: employee.Email,
            Department: employee.Department,
            Position: employee.Position,
            HireDate: employee.HireDate,
            PhoneNumber: employee.PhoneNumber
        );
    }

    /// <summary>
    /// 従業員情報を更新します
    /// </summary>
    [McpServerTool(Description = "既存の従業員情報を更新します。更新したい項目のみ指定できます。管理者権限が必要です。")]
    public async Task<EmployeeDetailResponse> UpdateEmployeeAsync(
        [Description("更新対象の従業員ID")] int employeeId,
        [Description("新しい名前（オプション）")] string? firstName = null,
        [Description("新しい苗字（オプション）")] string? lastName = null,
        [Description("新しいメールアドレス（オプション）")] string? email = null,
        [Description("新しい部署（オプション）")] string? department = null,
        [Description("新しい役職（オプション）")] string? position = null,
        [Description("新しい電話番号（オプション）")] string? phoneNumber = null)
    {
        _logger.LogInformation("MCP Tool: UpdateEmployee - EmployeeId: {EmployeeId}", employeeId);

        // 権限チェック
        EnsureAdminRole();

        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"従業員ID {employeeId} が見つかりませんでした。");
        }

        // 更新処理（ドメインロジック使用）
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            ValidateNameField(firstName, nameof(firstName));
            employee.UpdateFirstName(firstName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            ValidateNameField(lastName, nameof(lastName));
            employee.UpdateLastName(lastName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            ValidateEmail(email);
            employee.UpdateEmail(email.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            employee.UpdateDepartment(department.Trim());
        }

        if (!string.IsNullOrWhiteSpace(position))
        {
            employee.UpdatePosition(position.Trim());
        }

        if (phoneNumber != null)
        {
            employee.UpdatePhoneNumber(phoneNumber.Trim());
        }

        await _repository.UpdateAsync(employee);

        _logger.LogInformation("MCP Tool: UpdateEmployee - Successfully updated employee {EmployeeId}", 
            employeeId);

        return new EmployeeDetailResponse(
            Id: employee.Id,
            FirstName: employee.FirstName,
            LastName: employee.LastName,
            FullName: $"{employee.LastName} {employee.FirstName}",
            Email: employee.Email,
            Department: employee.Department,
            Position: employee.Position,
            HireDate: employee.HireDate,
            PhoneNumber: employee.PhoneNumber
        );
    }

    /// <summary>
    /// 従業員を削除します
    /// </summary>
    [McpServerTool(Description = "従業員を削除します。この操作は取り消せません。管理者権限が必要です。")]
    public async Task<DeleteEmployeeResponse> DeleteEmployeeAsync(
        [Description("削除する従業員のID")] int employeeId)
    {
        _logger.LogInformation("MCP Tool: DeleteEmployee - EmployeeId: {EmployeeId}", employeeId);

        // 権限チェック
        EnsureAdminRole();

        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"従業員ID {employeeId} が見つかりませんでした。");
        }

        var employeeName = $"{employee.LastName} {employee.FirstName}";
        await _repository.DeleteAsync(employeeId);

        _logger.LogInformation("MCP Tool: DeleteEmployee - Successfully deleted employee {EmployeeId}", 
            employeeId);

        return new DeleteEmployeeResponse(
            Success: true,
            Message: $"従業員 {employeeName} (ID: {employeeId}) を削除しました。"
        );
    }

    /// <summary>
    /// 部署一覧を取得します
    /// </summary>
    [McpServerTool(Description = "システムに登録されている全部署の一覧を取得します。")]
    public async Task<DepartmentListResponse> GetDepartmentsAsync()
    {
        _logger.LogInformation("MCP Tool: GetDepartments");

        var employees = await _repository.GetAllAsync();
        var departments = employees
            .Select(e => e.Department)
            .Distinct()
            .OrderBy(d => d)
            .Select(d => new DepartmentInfo(
                Name: d,
                EmployeeCount: employees.Count(e => e.Department == d)
            ))
            .ToList();

        return new DepartmentListResponse(
            Departments: departments,
            TotalDepartments: departments.Count
        );
    }

    // プライベートヘルパーメソッド

    private void EnsureAdminRole()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.IsInRole("Admin") != true)
        {
            throw new UnauthorizedAccessException("この操作には管理者権限が必要です。");
        }
    }

    private static void ValidateEmployeeInput(string firstName, string lastName, string email, 
        string department, string position)
    {
        ValidateNameField(firstName, nameof(firstName));
        ValidateNameField(lastName, nameof(lastName));
        ValidateEmail(email);

        if (string.IsNullOrWhiteSpace(department) || department.Length > 100)
        {
            throw new ArgumentException("部署名は1～100文字で入力してください。", nameof(department));
        }

        if (string.IsNullOrWhiteSpace(position) || position.Length > 100)
        {
            throw new ArgumentException("役職は1～100文字で入力してください。", nameof(position));
        }
    }

    private static void ValidateNameField(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 50)
        {
            throw new ArgumentException($"{fieldName}は1～50文字で入力してください。", fieldName);
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("メールアドレスを入力してください。", nameof(email));
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
            {
                throw new ArgumentException("有効なメールアドレスを入力してください。", nameof(email));
            }
        }
        catch
        {
            throw new ArgumentException("有効なメールアドレスを入力してください。", nameof(email));
        }
    }
}

// レスポンスDTO定義

public record EmployeeDetailResponse(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Department,
    string Position,
    DateTime HireDate,
    string? PhoneNumber
);

public record EmployeeSummary(
    int Id,
    string FullName,
    string Email,
    string Department,
    string Position
);

public record EmployeeListResponse(
    IReadOnlyList<EmployeeSummary> Employees,
    int TotalCount
);

public record DeleteEmployeeResponse(
    bool Success,
    string Message
);

public record DepartmentInfo(
    string Name,
    int EmployeeCount
);

public record DepartmentListResponse(
    IReadOnlyList<DepartmentInfo> Departments,
    int TotalDepartments
);
```

### 2.3 Program.cs 統合

`src/Services/EmployeeService/API/Program.cs`:

```csharp
using EmployeeService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 既存サービス登録
builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext登録
builder.AddSqliteDbContext<ApplicationDbContext>("employeedb");

// Repositories登録
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// HttpContextAccessor登録（MCPツールで使用）
builder.Services.AddHttpContextAccessor();

// JWT認証
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeAccess", policy =>
        policy.RequireRole("Employee", "Manager", "Admin"));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// ★ MCP Server 登録
builder.Services.AddMcpServer()
    .WithHttpTransport()           // HTTP/SSE transport
    .WithToolsFromAssembly();      // 自動的に[McpServerToolType]を検出

// CORS設定（MCP用）
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("McpPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    }
    else
    {
        options.AddPolicy("McpPolicy", policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// CORS有効化
app.UseCors("McpPolicy");

// ★ MCP エンドポイントマッピング
app.MapMcp("/api/mcp")
    .RequireAuthorization("EmployeeAccess");  // 認証必須

// 既存REST API
app.MapControllers();

app.Run();
```

---

## 3. NotificationService MCP実装例

### 3.1 NotificationTools.cs 実装

`src/Services/NotificationService/API/Mcp/Tools/NotificationTools.cs`:

```csharp
using ModelContextProtocol.Server;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using System.ComponentModel;

namespace NotificationService.API.Mcp.Tools;

[McpServerToolType]
public class NotificationTools
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<NotificationTools> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotificationTools(
        INotificationRepository repository,
        ILogger<NotificationTools> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    [McpServerTool(Description = "ユーザーの通知一覧を取得します。未読/既読でフィルタリングできます。")]
    public async Task<NotificationListResponse> GetNotificationsAsync(
        [Description("ユーザーID（省略時は現在のユーザー）")] string? userId = null,
        [Description("未読のみ取得するか（true/false）")] bool? unreadOnly = null,
        [Description("取得件数上限")] int limit = 50)
    {
        userId ??= GetCurrentUserId();
        _logger.LogInformation("MCP Tool: GetNotifications - UserId: {UserId}, UnreadOnly: {UnreadOnly}", 
            userId, unreadOnly);

        var notifications = await _repository.GetByUserIdAsync(userId);

        if (unreadOnly == true)
        {
            notifications = notifications.Where(n => !n.IsRead);
        }

        var notificationList = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new NotificationDto(
                Id: n.Id,
                UserId: n.UserId,
                Title: n.Title,
                Message: n.Message,
                Type: n.Type,
                IsRead: n.IsRead,
                CreatedAt: n.CreatedAt
            ))
            .ToList();

        return new NotificationListResponse(
            Notifications: notificationList,
            TotalCount: notificationList.Count,
            UnreadCount: notifications.Count(n => !n.IsRead)
        );
    }

    [McpServerTool(Description = "未読通知の数を取得します。")]
    public async Task<UnreadCountResponse> GetUnreadCountAsync(
        [Description("ユーザーID（省略時は現在のユーザー）")] string? userId = null)
    {
        userId ??= GetCurrentUserId();
        _logger.LogInformation("MCP Tool: GetUnreadCount - UserId: {UserId}", userId);

        var notifications = await _repository.GetByUserIdAsync(userId);
        var unreadCount = notifications.Count(n => !n.IsRead);

        return new UnreadCountResponse(UnreadCount: unreadCount);
    }

    [McpServerTool(Description = "通知を既読にします。")]
    public async Task<MarkAsReadResponse> MarkAsReadAsync(
        [Description("既読にする通知のID")] int notificationId)
    {
        _logger.LogInformation("MCP Tool: MarkAsRead - NotificationId: {NotificationId}", notificationId);

        var notification = await _repository.GetByIdAsync(notificationId);
        if (notification == null)
        {
            throw new InvalidOperationException($"通知ID {notificationId} が見つかりませんでした。");
        }

        // 権限チェック：自分の通知のみ
        var currentUserId = GetCurrentUserId();
        if (notification.UserId != currentUserId && !IsAdmin())
        {
            throw new UnauthorizedAccessException("この通知にアクセスする権限がありません。");
        }

        notification.MarkAsRead();
        await _repository.UpdateAsync(notification);

        return new MarkAsReadResponse(Success: true, Message: "通知を既読にしました。");
    }

    [McpServerTool(Description = "通知を削除します。")]
    public async Task<DeleteNotificationResponse> DeleteNotificationAsync(
        [Description("削除する通知のID")] int notificationId)
    {
        _logger.LogInformation("MCP Tool: DeleteNotification - NotificationId: {NotificationId}", 
            notificationId);

        var notification = await _repository.GetByIdAsync(notificationId);
        if (notification == null)
        {
            throw new InvalidOperationException($"通知ID {notificationId} が見つかりませんでした。");
        }

        // 権限チェック
        var currentUserId = GetCurrentUserId();
        if (notification.UserId != currentUserId && !IsAdmin())
        {
            throw new UnauthorizedAccessException("この通知を削除する権限がありません。");
        }

        await _repository.DeleteAsync(notificationId);

        return new DeleteNotificationResponse(Success: true, Message: "通知を削除しました。");
    }

    private string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("認証が必要です。");
        }

        return userId;
    }

    private bool IsAdmin()
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") == true;
    }
}

// レスポンスDTO

public record NotificationDto(
    int Id,
    string UserId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);

public record NotificationListResponse(
    IReadOnlyList<NotificationDto> Notifications,
    int TotalCount,
    int UnreadCount
);

public record UnreadCountResponse(int UnreadCount);

public record MarkAsReadResponse(bool Success, string Message);

public record DeleteNotificationResponse(bool Success, string Message);
```

---

## 4. Blazor MCPクライアント実装例

### 4.1 ディレクトリ構造作成

```bash
cd src/WebApps/BlazorWeb

mkdir -p Components/Chat/Services
mkdir -p Components/Chat/Models
```

### 4.2 MCPクライアントサービス実装

`src/WebApps/BlazorWeb/Components/Chat/Services/McpChatService.cs`:

```csharp
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text.Json;

namespace BlazorWeb.Components.Chat.Services;

public interface IMcpChatService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<string> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetAvailableToolsAsync(CancellationToken cancellationToken = default);
}

public class McpChatService : IMcpChatService, IAsyncDisposable
{
    private readonly IChatClient _chatClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<McpChatService> _logger;
    private readonly Dictionary<string, McpClient> _mcpClients = new();
    private readonly List<ChatMessage> _conversationHistory = new();
    private bool _initialized = false;

    public McpChatService(
        IChatClient chatClient,
        IConfiguration configuration,
        ILogger<McpChatService> logger)
    {
        _chatClient = chatClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        _logger.LogInformation("Initializing MCP Chat Service...");

        try
        {
            // システムプロンプト設定
            _conversationHistory.Add(new ChatMessage(
                ChatRole.System,
                """
                あなたは従業員管理システムのAIアシスタントです。
                ユーザーの要求に応じて、利用可能なツールを適切に呼び出してください。
                
                主な機能:
                - 従業員情報の検索・取得
                - 従業員情報の登録・更新・削除（管理者のみ）
                - 通知の確認・管理
                - 部署情報の取得
                
                常に丁寧で分かりやすい日本語で応答してください。
                エラーが発生した場合は、ユーザーに分かりやすく説明してください。
                """
            ));

            // 各MCPサーバーに接続
            var mcpServers = _configuration.GetSection("McpServers");
            
            await ConnectToMcpServerAsync("employee", mcpServers["EmployeeService"]!, cancellationToken);
            await ConnectToMcpServerAsync("notification", mcpServers["NotificationService"]!, cancellationToken);
            await ConnectToMcpServerAsync("auth", mcpServers["AuthService"]!, cancellationToken);

            _initialized = true;
            _logger.LogInformation("MCP Chat Service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MCP Chat Service");
            throw;
        }
    }

    private async Task ConnectToMcpServerAsync(string serviceName, string serverUrl, 
        CancellationToken cancellationToken)
    {
        try
        {
            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                BaseUrl = new Uri(serverUrl)
            });

            var client = await McpClient.CreateAsync(transport, cancellationToken);
            _mcpClients[serviceName] = client;

            _logger.LogInformation("Connected to MCP server: {ServiceName} at {ServerUrl}", 
                serviceName, serverUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MCP server: {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<string> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            await InitializeAsync(cancellationToken);
        }

        _logger.LogInformation("Processing user message: {Message}", userMessage);

        try
        {
            // ユーザーメッセージを履歴に追加
            _conversationHistory.Add(new ChatMessage(ChatRole.User, userMessage));

            // 利用可能なツールを取得
            var availableTools = await GetAllMcpToolsAsync(cancellationToken);

            // LLMにリクエスト
            var options = new ChatOptions
            {
                Tools = availableTools.ToList()
            };

            var response = await _chatClient.CompleteAsync(
                _conversationHistory,
                options,
                cancellationToken);

            // ツール呼び出しの処理
            while (response.Choices[0].FinishReason == ChatFinishReason.ToolCalls)
            {
                _conversationHistory.Add(response.Message);

                foreach (var toolCall in response.Message.Contents.OfType<FunctionCallContent>())
                {
                    _logger.LogInformation("Executing tool: {ToolName}", toolCall.Name);

                    var toolResult = await ExecuteMcpToolAsync(toolCall, cancellationToken);
                    
                    _conversationHistory.Add(new ChatMessage(
                        ChatRole.Tool,
                        [new FunctionResultContent(toolCall.CallId, toolCall.Name, toolResult)]
                    ));
                }

                // ツール実行結果を含めて再度LLMに問い合わせ
                response = await _chatClient.CompleteAsync(
                    _conversationHistory,
                    options,
                    cancellationToken);
            }

            // アシスタントの応答を履歴に追加
            _conversationHistory.Add(response.Message);

            return response.Message.Text ?? "応答がありませんでした。";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return $"エラーが発生しました: {ex.Message}";
        }
    }

    private async Task<IEnumerable<AITool>> GetAllMcpToolsAsync(CancellationToken cancellationToken)
    {
        var tools = new List<AITool>();

        foreach (var (serviceName, client) in _mcpClients)
        {
            try
            {
                var mcpTools = await client.ListToolsAsync(cancellationToken);

                foreach (var mcpTool in mcpTools)
                {
                    tools.Add(AIFunctionFactory.Create(
                        method: (Dictionary<string, object?> arguments) => 
                            ExecuteMcpToolDirectlyAsync(serviceName, mcpTool.Name, arguments, cancellationToken),
                        name: $"{serviceName}_{mcpTool.Name}",
                        description: mcpTool.Description ?? "No description"
                    ));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tools from {ServiceName}", serviceName);
            }
        }

        return tools;
    }

    private async Task<string> ExecuteMcpToolAsync(FunctionCallContent toolCall, 
        CancellationToken cancellationToken)
    {
        var parts = toolCall.Name.Split('_', 2);
        if (parts.Length != 2 || !_mcpClients.TryGetValue(parts[0], out var client))
        {
            return JsonSerializer.Serialize(new { error = $"Unknown tool: {toolCall.Name}" });
        }

        var serviceName = parts[0];
        var actualToolName = parts[1];

        try
        {
            var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                toolCall.Arguments ?? "{}") ?? new Dictionary<string, object?>();

            var result = await client.CallToolAsync(actualToolName, arguments, cancellationToken);

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute tool: {Service}.{Tool}", serviceName, actualToolName);
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<object> ExecuteMcpToolDirectlyAsync(string serviceName, string toolName, 
        Dictionary<string, object?> arguments, CancellationToken cancellationToken)
    {
        if (!_mcpClients.TryGetValue(serviceName, out var client))
        {
            throw new InvalidOperationException($"MCP client not found: {serviceName}");
        }

        return await client.CallToolAsync(toolName, arguments, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        var toolNames = new List<string>();

        foreach (var (serviceName, client) in _mcpClients)
        {
            try
            {
                var tools = await client.ListToolsAsync(cancellationToken);
                toolNames.AddRange(tools.Select(t => $"{serviceName}.{t.Name}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list tools from {ServiceName}", serviceName);
            }
        }

        return toolNames;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in _mcpClients.Values)
        {
            if (client is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync();
            }
        }
        _mcpClients.Clear();
    }
}
```

### 4.3 Program.cs登録

`src/WebApps/BlazorWeb/Program.cs`:

```csharp
// Azure OpenAI Chat Completion
builder.Services.AddChatClient(services =>
{
    var config = services.GetRequiredService<IConfiguration>();
    return new AzureOpenAIClient(
        new Uri(config["AzureOpenAI:Endpoint"]!),
        new AzureKeyCredential(config["AzureOpenAI:ApiKey"]!)
    ).AsChatClient(config["AzureOpenAI:DeploymentName"]!);
});

// MCP Chat Service
builder.Services.AddScoped<IMcpChatService, McpChatService>();
```

---

## 5. テスト実装

### 5.1 EmployeeTools ユニットテスト

`tests/EmployeeService.API.Tests/Mcp/EmployeeToolsTests.cs`:

```csharp
using EmployeeService.API.Mcp.Tools;
using EmployeeService.Application.Interfaces;
using EmployeeService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EmployeeService.API.Tests.Mcp;

public class EmployeeToolsTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly Mock<ILogger<EmployeeTools>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly EmployeeTools _employeeTools;

    public EmployeeToolsTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<EmployeeTools>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _employeeTools = new EmployeeTools(
            _repositoryMock.Object,
            _loggerMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task GetEmployeeAsync_WhenEmployeeExists_ReturnsEmployeeDetails()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "太郎",
            LastName = "田中",
            Email = "tanaka@example.com",
            Department = "営業部",
            Position = "マネージャー",
            HireDate = new DateTime(2020, 4, 1),
            PhoneNumber = "03-1234-5678"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(employee);

        // Act
        var result = await _employeeTools.GetEmployeeAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("太郎", result.FirstName);
        Assert.Equal("田中", result.LastName);
        Assert.Equal("田中 太郎", result.FullName);
        Assert.Equal("tanaka@example.com", result.Email);
    }

    [Fact]
    public async Task GetEmployeeAsync_WhenEmployeeNotFound_ThrowsException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _employeeTools.GetEmployeeAsync(999));
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithAdminRole_CreatesEmployee()
    {
        // Arrange
        SetupAdminUser();

        var newEmployee = new Employee
        {
            Id = 1,
            FirstName = "花子",
            LastName = "佐藤",
            Email = "sato@example.com",
            Department = "開発部",
            Position = "エンジニア"
        };

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .ReturnsAsync(newEmployee);

        // Act
        var result = await _employeeTools.CreateEmployeeAsync(
            "花子", "佐藤", "sato@example.com", "開発部", "エンジニア");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("花子", result.FirstName);
        Assert.Equal("佐藤", result.LastName);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithoutAdminRole_ThrowsUnauthorizedException()
    {
        // Arrange
        SetupNonAdminUser();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _employeeTools.CreateEmployeeAsync(
                "花子", "佐藤", "sato@example.com", "開発部", "エンジニア"));
    }

    private void SetupAdminUser()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
    }

    private void SetupNonAdminUser()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "Employee") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
    }
}
```

---

## 6. トラブルシューティング

### 6.1 よくあるエラーと対処法

#### エラー1: "MCP server connection failed"

**原因**: MCPサーバーが起動していない、またはURLが間違っている

**対処法**:
```bash
# サービスが起動しているか確認
dotnet run --project src/AppHost

# Aspireダッシュボードでサービスのポート番号確認
# appsettings.jsonのMcpServersセクションを正しいURLに更新
```

#### エラー2: "Tool not found"

**原因**: ツール名の形式が間違っている、またはツールが登録されていない

**対処法**:
```csharp
// 利用可能なツール一覧を確認
var tools = await chatService.GetAvailableToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine(tool);
}
```

#### エラー3: "Unauthorized access"

**原因**: 認証トークンが設定されていない、または期限切れ

**対処法**:
```csharp
// HttpClientTransportにJWTトークンを設定
var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    BaseUrl = new Uri("https://localhost:5001/api/mcp"),
    HttpClient = new HttpClient
    {
        DefaultRequestHeaders = 
        {
            Authorization = new AuthenticationHeaderValue("Bearer", jwtToken)
        }
    }
});
```

### 6.2 デバッグ方法

#### ログレベルの設定

`appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ModelContextProtocol": "Debug",
      "EmployeeService.API.Mcp": "Debug",
      "BlazorWeb.Components.Chat": "Debug"
    }
  }
}
```

#### MCP通信のトレース

```csharp
// OpenTelemetryでMCP通信をトレース
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("ModelContextProtocol");
        tracing.AddConsoleExporter();  // 開発環境で確認
    });
```

---

## まとめ

本ガイドでは、MCPサーバーとクライアントの実装方法を具体的なコード例とともに示しました。

**次のステップ**:
1. EmployeeServiceでPoC実装
2. 動作確認とデバッグ
3. 他のサービスへの展開
4. チャット画面のUI改善
5. 本番環境対応

**参考資料**:
- [MCP統合設計書](mcp-integration-design.md)
- [公式MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Microsoft Learn - MCP](https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp)
