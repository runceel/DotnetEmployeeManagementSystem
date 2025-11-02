using EmployeeService.Domain.Entities;
using Shared.Contracts.EmployeeService;

namespace EmployeeService.Application.Mappings;

/// <summary>
/// 従業員マッピング
/// </summary>
public static class EmployeeMapper
{
    /// <summary>
    /// エンティティをDTOに変換
    /// </summary>
    public static EmployeeDto ToDto(this Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            HireDate = employee.HireDate,
            Department = employee.Department,
            Position = employee.Position,
            FullName = employee.GetFullName(),
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }

    /// <summary>
    /// 作成リクエストからエンティティを生成
    /// </summary>
    public static Employee ToEntity(this CreateEmployeeRequest request)
    {
        return new Employee(
            request.FirstName,
            request.LastName,
            request.Email,
            request.HireDate,
            request.Department,
            request.Position
        );
    }
}
