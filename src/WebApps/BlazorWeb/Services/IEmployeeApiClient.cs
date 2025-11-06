using Shared.Contracts.EmployeeService;

namespace BlazorWeb.Services;

/// <summary>
/// EmployeeService APIクライアントのインターフェース
/// </summary>
public interface IEmployeeApiClient
{
    /// <summary>
    /// 全従業員を取得
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// IDで従業員を取得
    /// </summary>
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を作成
    /// </summary>
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を更新
    /// </summary>
    Task<EmployeeDto?> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 従業員を削除
    /// </summary>
    Task<bool> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default);
}
