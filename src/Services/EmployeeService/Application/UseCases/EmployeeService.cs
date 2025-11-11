using EmployeeService.Application.Mappings;
using EmployeeService.Application.Services;
using EmployeeService.Domain.Repositories;
using Shared.Contracts.EmployeeService;
using Shared.Contracts.Events;

namespace EmployeeService.Application.UseCases;

/// <summary>
/// 従業員サービス
/// </summary>
public class EmployeeService(IEmployeeRepository repository, IDepartmentRepository departmentRepository, IEventPublisher? eventPublisher = null) : IEmployeeService
{
    private readonly IEmployeeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IDepartmentRepository _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly IEventPublisher? _eventPublisher = eventPublisher;

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

        // 部署名から部署IDを検索
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);
        var department = departments.FirstOrDefault(d => d.Name == request.Department);
        if (department == null)
        {
            throw new InvalidOperationException($"部署 '{request.Department}' が見つかりません。");
        }

        var employee = request.ToEntity(department.Id);
        var created = await _repository.AddAsync(employee, cancellationToken);

        // イベントを発行
        if (_eventPublisher != null)
        {
            var eventData = new EmployeeCreatedEvent
            {
                EmployeeId = created.Id,
                FirstName = created.FirstName,
                LastName = created.LastName,
                Email = created.Email,
                Department = department.Name,
                Position = created.Position,
                CreatedAt = created.CreatedAt
            };
            await _eventPublisher.PublishAsync("employee.created", eventData, cancellationToken);
        }

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

        // 部署名から部署IDを検索
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);
        var department = departments.FirstOrDefault(d => d.Name == request.Department);
        if (department == null)
        {
            throw new InvalidOperationException($"部署 '{request.Department}' が見つかりません。");
        }

        employee.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.HireDate,
            department.Id,
            request.Position
        );

        await _repository.UpdateAsync(employee, cancellationToken);

        // イベントを発行
        if (_eventPublisher != null)
        {
            var eventData = new EmployeeUpdatedEvent
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Department = department.Name,
                Position = employee.Position,
                UpdatedAt = employee.UpdatedAt
            };
            await _eventPublisher.PublishAsync("employee.updated", eventData, cancellationToken);
        }

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

        // イベントを発行
        if (_eventPublisher != null)
        {
            var eventData = new EmployeeDeletedEvent
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                DeletedAt = DateTime.UtcNow
            };
            await _eventPublisher.PublishAsync("employee.deleted", eventData, cancellationToken);
        }

        return true;
    }

    public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _repository.GetAllAsync(cancellationToken);
        var employeeList = employees.ToList();

        // 今月の新規登録数を計算
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var newEmployeesThisMonth = employeeList.Count(e => e.CreatedAt >= startOfMonth);

        // 部署のユニーク数を計算
        var departmentCount = employeeList
            .Select(e => e.DepartmentId)
            .Where(d => d != Guid.Empty)
            .Distinct()
            .Count();

        return new DashboardStatisticsDto
        {
            TotalEmployees = employeeList.Count,
            DepartmentCount = departmentCount,
            NewEmployeesThisMonth = newEmployeesThisMonth
        };
    }

    public async Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var employees = await _repository.GetAllAsync(cancellationToken);
        var employeeList = employees.ToList();

        // 従業員の作成・更新イベントをアクティビティとして扱う
        var activities = new List<RecentActivityDto>();

        foreach (var employee in employeeList)
        {
            var fullName = employee.GetFullName();

            // 作成イベント
            activities.Add(new RecentActivityDto
            {
                Id = Guid.NewGuid(),
                ActivityType = "Created",
                EmployeeId = employee.Id,
                EmployeeName = fullName,
                Description = $"{fullName}さんが登録されました",
                Timestamp = employee.CreatedAt
            });

            // 更新イベント (作成時刻と更新時刻が異なる場合のみ)
            if (employee.UpdatedAt > employee.CreatedAt.AddSeconds(1))
            {
                activities.Add(new RecentActivityDto
                {
                    Id = Guid.NewGuid(),
                    ActivityType = "Updated",
                    EmployeeId = employee.Id,
                    EmployeeName = fullName,
                    Description = $"{fullName}さんの情報が更新されました",
                    Timestamp = employee.UpdatedAt
                });
            }
        }

        // タイムスタンプでソートして最新のものを返す
        return activities
            .OrderByDescending(a => a.Timestamp)
            .Take(count);
    }
}
