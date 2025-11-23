using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Repositories;
using ModelContextProtocol.Server;

namespace EmployeeService.API.Mcp.Tools;

/// <summary>
/// 従業員管理に関するMCPツール
/// </summary>
[McpServerToolType]
public class EmployeeTools
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<EmployeeTools> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmployeeTools(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        ILogger<EmployeeTools> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 指定された従業員IDの詳細情報を取得します。従業員の基本情報、連絡先、所属部署などが取得できます。
    /// </summary>
    [McpServerTool]
    public async Task<EmployeeDetailResponse> GetEmployeeAsync(
        string employeeId)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        
        if (!Guid.TryParse(employeeId, out var id))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
        }

        _logger.LogInformation("MCP Tool: GetEmployee - EmployeeId: {EmployeeId}", id);

        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
        {
            throw new InvalidOperationException($"従業員ID {employeeId} が見つかりませんでした。");
        }

        return new EmployeeDetailResponse(
            Id: employee.Id.ToString(),
            FirstName: employee.FirstName,
            LastName: employee.LastName,
            FullName: employee.GetFullName(),
            Email: employee.Email,
            DepartmentId: employee.DepartmentId.ToString(),
            DepartmentName: employee.Department?.Name ?? "不明",
            Position: employee.Position,
            HireDate: employee.HireDate,
            CreatedAt: employee.CreatedAt,
            UpdatedAt: employee.UpdatedAt
        );
    }

    /// <summary>
    /// 全従業員の一覧を取得します。各従業員の基本情報が含まれます。
    /// </summary>
    [McpServerTool]
    public async Task<EmployeeListResponse> ListEmployeesAsync()
    {
        _logger.LogInformation("MCP Tool: ListEmployees");

        var employees = await _employeeRepository.GetAllAsync();
        var employeeList = employees.Select(e => new EmployeeSummary(
            Id: e.Id.ToString(),
            FullName: e.GetFullName(),
            Email: e.Email,
            DepartmentName: e.Department?.Name ?? "不明",
            Position: e.Position,
            HireDate: e.HireDate
        )).ToList();

        return new EmployeeListResponse(
            Employees: employeeList,
            TotalCount: employeeList.Count
        );
    }

    /// <summary>
    /// メールアドレスで従業員を検索します。完全一致検索です。
    /// </summary>
    [McpServerTool]
    public async Task<EmployeeDetailResponse?> SearchEmployeeByEmailAsync(
        string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        _logger.LogInformation("MCP Tool: SearchEmployeeByEmail - Email: {Email}", email);

        var employee = await _employeeRepository.GetByEmailAsync(email);
        if (employee == null)
        {
            return null;
        }

        return new EmployeeDetailResponse(
            Id: employee.Id.ToString(),
            FirstName: employee.FirstName,
            LastName: employee.LastName,
            FullName: employee.GetFullName(),
            Email: employee.Email,
            DepartmentId: employee.DepartmentId.ToString(),
            DepartmentName: employee.Department?.Name ?? "不明",
            Position: employee.Position,
            HireDate: employee.HireDate,
            CreatedAt: employee.CreatedAt,
            UpdatedAt: employee.UpdatedAt
        );
    }

    /// <summary>
    /// 新しい従業員を登録します。必須項目は名前、苗字、メールアドレス、部署ID、役職、入社日です。
    /// </summary>
    [McpServerTool]
    public async Task<EmployeeDetailResponse> CreateEmployeeAsync(
        string firstName,
        string lastName,
        string email,
        string departmentId,
        string position,
        string hireDate)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(departmentId);
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(hireDate);

        _logger.LogInformation("MCP Tool: CreateEmployee - Name: {LastName} {FirstName}", lastName, firstName);

        if (!Guid.TryParse(departmentId, out var deptId))
        {
            throw new ArgumentException("Invalid department ID format", nameof(departmentId));
        }

        if (!DateTime.TryParse(hireDate, out var parsedHireDate))
        {
            throw new ArgumentException("Invalid hire date format. Use ISO 8601 format (e.g., 2024-01-01)", nameof(hireDate));
        }

        // 部署が存在するか確認
        var department = await _departmentRepository.GetByIdAsync(deptId);
        if (department == null)
        {
            throw new InvalidOperationException($"部署ID {departmentId} が見つかりませんでした。");
        }

        // ドメインエンティティ作成
        var employee = new Employee(
            firstName: firstName.Trim(),
            lastName: lastName.Trim(),
            email: email.Trim().ToLowerInvariant(),
            hireDate: parsedHireDate,
            departmentId: deptId,
            position: position.Trim()
        );

        await _employeeRepository.AddAsync(employee);

        _logger.LogInformation("MCP Tool: CreateEmployee - Successfully created employee {EmployeeId}", employee.Id);

        return new EmployeeDetailResponse(
            Id: employee.Id.ToString(),
            FirstName: employee.FirstName,
            LastName: employee.LastName,
            FullName: employee.GetFullName(),
            Email: employee.Email,
            DepartmentId: employee.DepartmentId.ToString(),
            DepartmentName: department.Name,
            Position: employee.Position,
            HireDate: employee.HireDate,
            CreatedAt: employee.CreatedAt,
            UpdatedAt: employee.UpdatedAt
        );
    }

    /// <summary>
    /// 既存の従業員情報を更新します。すべての項目を指定する必要があります。
    /// </summary>
    [McpServerTool]
    public async Task<EmployeeDetailResponse> UpdateEmployeeAsync(
        string employeeId,
        string firstName,
        string lastName,
        string email,
        string departmentId,
        string position,
        string hireDate)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(departmentId);
        ArgumentNullException.ThrowIfNull(position);
        ArgumentNullException.ThrowIfNull(hireDate);

        _logger.LogInformation("MCP Tool: UpdateEmployee - EmployeeId: {EmployeeId}", employeeId);

        if (!Guid.TryParse(employeeId, out var empId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
        }

        if (!Guid.TryParse(departmentId, out var deptId))
        {
            throw new ArgumentException("Invalid department ID format", nameof(departmentId));
        }

        if (!DateTime.TryParse(hireDate, out var parsedHireDate))
        {
            throw new ArgumentException("Invalid hire date format. Use ISO 8601 format (e.g., 2024-01-01)", nameof(hireDate));
        }

        var employee = await _employeeRepository.GetByIdAsync(empId);
        if (employee == null)
        {
            throw new InvalidOperationException($"従業員ID {employeeId} が見つかりませんでした。");
        }

        // 部署が存在するか確認
        var department = await _departmentRepository.GetByIdAsync(deptId);
        if (department == null)
        {
            throw new InvalidOperationException($"部署ID {departmentId} が見つかりませんでした。");
        }

        // 更新処理（ドメインロジック使用）
        employee.Update(
            firstName: firstName.Trim(),
            lastName: lastName.Trim(),
            email: email.Trim().ToLowerInvariant(),
            hireDate: parsedHireDate,
            departmentId: deptId,
            position: position.Trim()
        );

        await _employeeRepository.UpdateAsync(employee);

        _logger.LogInformation("MCP Tool: UpdateEmployee - Successfully updated employee {EmployeeId}", employeeId);

        return new EmployeeDetailResponse(
            Id: employee.Id.ToString(),
            FirstName: employee.FirstName,
            LastName: employee.LastName,
            FullName: employee.GetFullName(),
            Email: employee.Email,
            DepartmentId: employee.DepartmentId.ToString(),
            DepartmentName: department.Name,
            Position: employee.Position,
            HireDate: employee.HireDate,
            CreatedAt: employee.CreatedAt,
            UpdatedAt: employee.UpdatedAt
        );
    }

    /// <summary>
    /// 従業員を削除します。この操作は取り消せません。
    /// </summary>
    [McpServerTool]
    public async Task<DeleteEmployeeResponse> DeleteEmployeeAsync(
        string employeeId)
    {
        ArgumentNullException.ThrowIfNull(employeeId);

        if (!Guid.TryParse(employeeId, out var empId))
        {
            throw new ArgumentException("Invalid employee ID format", nameof(employeeId));
        }

        _logger.LogInformation("MCP Tool: DeleteEmployee - EmployeeId: {EmployeeId}", employeeId);

        var employee = await _employeeRepository.GetByIdAsync(empId);
        if (employee == null)
        {
            throw new InvalidOperationException($"従業員ID {employeeId} が見つかりませんでした。");
        }

        var employeeName = employee.GetFullName();
        await _employeeRepository.DeleteAsync(empId);

        _logger.LogInformation("MCP Tool: DeleteEmployee - Successfully deleted employee {EmployeeId}", employeeId);

        return new DeleteEmployeeResponse(
            Success: true,
            Message: $"従業員 {employeeName} (ID: {employeeId}) を削除しました。"
        );
    }
}

// レスポンスDTO定義

public record EmployeeDetailResponse(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string DepartmentId,
    string DepartmentName,
    string Position,
    DateTime HireDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record EmployeeSummary(
    string Id,
    string FullName,
    string Email,
    string DepartmentName,
    string Position,
    DateTime HireDate
);

public record EmployeeListResponse(
    IReadOnlyList<EmployeeSummary> Employees,
    int TotalCount
);

public record DeleteEmployeeResponse(
    bool Success,
    string Message
);
