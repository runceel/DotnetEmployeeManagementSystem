using EmployeeService.Domain.Entities;
using EmployeeService.Infrastructure.Data;
using EmployeeService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Integration.Tests.Repositories;

public class EmployeeRepositoryIntegrationTests : IDisposable
{
    private readonly EmployeeDbContext _context;
    private readonly EmployeeRepository _repository;
    private readonly Guid _testDepartmentId;

    public EmployeeRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EmployeeDbContext(options);
        _repository = new EmployeeRepository(_context);
        
        // Add a test department
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");
        _context.Departments.Add(department);
        _context.SaveChanges();
        _testDepartmentId = department.Id;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEmployeeToDatabase()
    {
        // Arrange
        var employee = new Employee(
            "太郎",
            "山田",
            "yamada.taro@example.com",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _testDepartmentId,
            "エンジニア"
        );

        // Act
        var result = await _repository.AddAsync(employee);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
        Assert.Equal(1, await _context.Employees.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEmployee_WhenExists()
    {
        // Arrange
        var employee = new Employee(
            "太郎",
            "山田",
            "yamada.taro@example.com",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _testDepartmentId,
            "エンジニア"
        );
        await _repository.AddAsync(employee);

        // Act
        var result = await _repository.GetByIdAsync(employee.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
        Assert.Equal(employee.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEmployees()
    {
        // Arrange
        var employee1 = new Employee(
            "太郎",
            "山田",
            "yamada.taro@example.com",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _testDepartmentId,
            "エンジニア"
        );
        var employee2 = new Employee(
            "花子",
            "佐藤",
            "sato.hanako@example.com",
            new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            _testDepartmentId,
            "マネージャー"
        );

        await _repository.AddAsync(employee1);
        await _repository.AddAsync(employee2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnEmployee_WhenExists()
    {
        // Arrange
        var employee = new Employee(
            "太郎",
            "山田",
            "yamada.taro@example.com",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _testDepartmentId,
            "エンジニア"
        );
        await _repository.AddAsync(employee);

        // Act
        var result = await _repository.GetByEmailAsync("yamada.taro@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
        Assert.Equal("yamada.taro@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEmployee()
    {
        // Arrange
        var employee = new Employee(
            "太郎",
            "山田",
            "yamada.taro@example.com",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _testDepartmentId,
            "エンジニア"
        );
        await _repository.AddAsync(employee);

        // Act
        employee.Update(
            "次郎",
            "田中",
            "tanaka.jiro@example.com",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _testDepartmentId,
            "マネージャー"
        );
        await _repository.UpdateAsync(employee);

        // Assert
        var updated = await _repository.GetByIdAsync(employee.Id);
        Assert.NotNull(updated);
        Assert.Equal("次郎", updated.FirstName);
        Assert.Equal("田中", updated.LastName);
        Assert.Equal("tanaka.jiro@example.com", updated.Email);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEmployee()
    {
        // Arrange
        var employee = new Employee(
            "太郎",
            "山田",
            "yamada.taro@example.com",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _testDepartmentId,
            "エンジニア"
        );
        await _repository.AddAsync(employee);

        // Act
        await _repository.DeleteAsync(employee.Id);

        // Assert
        var deleted = await _repository.GetByIdAsync(employee.Id);
        Assert.Null(deleted);
        Assert.Equal(0, await _context.Employees.CountAsync());
    }
}
