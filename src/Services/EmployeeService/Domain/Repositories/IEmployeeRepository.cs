using EmployeeService.Domain.Entities;

namespace EmployeeService.Domain.Repositories;

/// <summary>
/// 従業員リポジトリインターフェース
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// 従業員を取得
    /// </summary>
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 全従業員を取得
    /// </summary>
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// メールアドレスで従業員を検索
    /// </summary>
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を追加
    /// </summary>
    Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を更新
    /// </summary>
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を削除
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
