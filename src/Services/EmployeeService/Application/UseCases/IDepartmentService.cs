using Shared.Contracts.DepartmentService;

namespace EmployeeService.Application.UseCases;

/// <summary>
/// 部署サービスインターフェース
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// 部署を取得
    /// </summary>
    Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 全部署を取得
    /// </summary>
    Task<IEnumerable<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を作成
    /// </summary>
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を更新
    /// </summary>
    Task<DepartmentDto?> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を削除
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
