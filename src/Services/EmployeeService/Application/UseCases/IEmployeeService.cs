using Shared.Contracts.EmployeeService;

namespace EmployeeService.Application.UseCases;

/// <summary>
/// 従業員サービスインターフェース
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// 従業員を取得
    /// </summary>
    Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 全従業員を取得
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を作成
    /// </summary>
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を更新
    /// </summary>
    Task<EmployeeDto?> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を削除
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// ダッシュボード統計情報を取得
    /// </summary>
    Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 最近のアクティビティを取得
    /// </summary>
    Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10, CancellationToken cancellationToken = default);
}
