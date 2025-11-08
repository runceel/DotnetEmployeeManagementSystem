namespace Shared.Contracts.EmployeeService;

/// <summary>
/// ダッシュボード統計情報DTO
/// </summary>
public record DashboardStatisticsDto
{
    /// <summary>
    /// 総従業員数
    /// </summary>
    public int TotalEmployees { get; init; }

    /// <summary>
    /// 部署数
    /// </summary>
    public int DepartmentCount { get; init; }

    /// <summary>
    /// 今月の新規登録数
    /// </summary>
    public int NewEmployeesThisMonth { get; init; }
}
