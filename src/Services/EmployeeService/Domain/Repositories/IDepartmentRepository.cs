using EmployeeService.Domain.Entities;

namespace EmployeeService.Domain.Repositories;

/// <summary>
/// 部署リポジトリインターフェース
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>
    /// 部署を取得
    /// </summary>
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 全部署を取得
    /// </summary>
    Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を追加
    /// </summary>
    Task<Department> AddAsync(Department department, CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を更新
    /// </summary>
    Task<Department> UpdateAsync(Department department, CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署を削除
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 部署に所属する従業員が存在するか確認
    /// </summary>
    Task<bool> HasEmployeesAsync(string departmentName, CancellationToken cancellationToken = default);
}
