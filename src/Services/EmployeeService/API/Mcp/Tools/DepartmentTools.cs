using EmployeeService.Domain.Repositories;
using ModelContextProtocol.Server;

namespace EmployeeService.API.Mcp.Tools;

/// <summary>
/// 部署管理に関するMCPツール
/// </summary>
[McpServerToolType]
public class DepartmentTools
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<DepartmentTools> _logger;

    public DepartmentTools(
        IDepartmentRepository departmentRepository,
        IEmployeeRepository employeeRepository,
        ILogger<DepartmentTools> logger)
    {
        _departmentRepository = departmentRepository;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    /// <summary>
    /// 指定された部署IDの詳細情報を取得します。
    /// </summary>
    [McpServerTool]
    public async Task<DepartmentDetailResponse> GetDepartmentAsync(
        string departmentId)
    {
        ArgumentNullException.ThrowIfNull(departmentId);

        if (!Guid.TryParse(departmentId, out var id))
        {
            throw new ArgumentException("Invalid department ID format", nameof(departmentId));
        }

        _logger.LogInformation("MCP Tool: GetDepartment - DepartmentId: {DepartmentId}", id);

        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            throw new InvalidOperationException($"部署ID {departmentId} が見つかりませんでした。");
        }

        // この部署に所属する従業員数を取得
        var employees = await _employeeRepository.GetAllAsync();
        var employeeCount = employees.Count(e => e.DepartmentId == id);

        return new DepartmentDetailResponse(
            Id: department.Id.ToString(),
            Name: department.Name,
            Description: department.Description,
            EmployeeCount: employeeCount,
            CreatedAt: department.CreatedAt,
            UpdatedAt: department.UpdatedAt
        );
    }

    /// <summary>
    /// 全部署の一覧を取得します。各部署の基本情報と所属従業員数が含まれます。
    /// </summary>
    [McpServerTool]
    public async Task<DepartmentListResponse> ListDepartmentsAsync()
    {
        _logger.LogInformation("MCP Tool: ListDepartments");

        var departments = await _departmentRepository.GetAllAsync();
        var employees = await _employeeRepository.GetAllAsync();

        var departmentList = departments.Select(d => new DepartmentSummary(
            Id: d.Id.ToString(),
            Name: d.Name,
            Description: d.Description,
            EmployeeCount: employees.Count(e => e.DepartmentId == d.Id)
        )).ToList();

        return new DepartmentListResponse(
            Departments: departmentList,
            TotalCount: departmentList.Count
        );
    }


}

// レスポンスDTO定義

public record DepartmentDetailResponse(
    string Id,
    string Name,
    string Description,
    int EmployeeCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record DepartmentSummary(
    string Id,
    string Name,
    string Description,
    int EmployeeCount
);

public record DepartmentListResponse(
    IReadOnlyList<DepartmentSummary> Departments,
    int TotalCount
);
