using EmployeeService.Domain.Entities;
using Shared.Contracts.DepartmentService;

namespace EmployeeService.Application.Mappings;

/// <summary>
/// 部署マッパー
/// </summary>
public static class DepartmentMapper
{
    /// <summary>
    /// DepartmentエンティティをDepartmentDtoに変換
    /// </summary>
    public static DepartmentDto ToDto(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);

        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt
        };
    }

    /// <summary>
    /// CreateDepartmentRequestからDepartmentエンティティを作成
    /// </summary>
    public static Department ToEntity(CreateDepartmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new Department(
            request.Name,
            request.Description
        );
    }
}
