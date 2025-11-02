using EmployeeService.Application.Mappings;
using EmployeeService.Domain.Repositories;
using Shared.Contracts.EmployeeService;

namespace EmployeeService.Application.UseCases;

/// <summary>
/// 従業員サービス
/// </summary>
public class EmployeeService(IEmployeeRepository repository) : IEmployeeService
{
    private readonly IEmployeeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        return employee?.ToDto();
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _repository.GetAllAsync(cancellationToken);
        return employees.Select(e => e.ToDto());
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        // メールアドレスの重複チェック
        // Note: データベース層でのユニーク制約とトランザクション処理により、完全な整合性を保証する必要があります
        var existingEmployee = await _repository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmployee != null)
        {
            throw new InvalidOperationException($"メールアドレス '{request.Email}' は既に使用されています。");
        }

        var employee = request.ToEntity();
        var created = await _repository.AddAsync(employee, cancellationToken);
        return created.ToDto();
    }

    public async Task<EmployeeDto?> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return null;
        }

        // メールアドレスの重複チェック（自分以外）
        // Note: データベース層でのユニーク制約とトランザクション処理により、完全な整合性を保証する必要があります
        var existingEmployee = await _repository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmployee != null && existingEmployee.Id != id)
        {
            throw new InvalidOperationException($"メールアドレス '{request.Email}' は既に使用されています。");
        }

        employee.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.HireDate,
            request.Department,
            request.Position
        );

        await _repository.UpdateAsync(employee, cancellationToken);
        return employee.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return false;
        }

        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
