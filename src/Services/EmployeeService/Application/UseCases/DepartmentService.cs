using EmployeeService.Application.Mappings;
using EmployeeService.Domain.Repositories;
using Shared.Contracts.DepartmentService;

namespace EmployeeService.Application.UseCases;

/// <summary>
/// 部署サービス実装
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentService(IDepartmentRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(id, cancellationToken);
        return department is not null ? DepartmentMapper.ToDto(department) : null;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _repository.GetAllAsync(cancellationToken);
        return departments.Select(DepartmentMapper.ToDto);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var department = DepartmentMapper.ToEntity(request);
        var created = await _repository.AddAsync(department, cancellationToken);
        return DepartmentMapper.ToDto(created);
    }

    public async Task<DepartmentDto?> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var department = await _repository.GetByIdAsync(id, cancellationToken);
        if (department is null)
            return null;

        department.Update(request.Name, request.Description);
        var updated = await _repository.UpdateAsync(department, cancellationToken);
        return DepartmentMapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }
}
