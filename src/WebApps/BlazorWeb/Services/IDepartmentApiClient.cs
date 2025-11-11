using Shared.Contracts.DepartmentService;

namespace BlazorWeb.Services;

/// <summary>
/// DepartmentService APIクライアントのインターフェース
/// </summary>
public interface IDepartmentApiClient
{
    /// <summary>
    /// 全部署を取得
    /// </summary>
    Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// IDで部署を取得
    /// </summary>
    Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を作成
    /// </summary>
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を更新
    /// </summary>
    Task<DepartmentDto?> UpdateDepartmentAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を削除
    /// </summary>
    Task<bool> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default);
}
