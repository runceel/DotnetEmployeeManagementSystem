using EmployeeService.Application.UseCases;
using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Repositories;
using Moq;
using Shared.Contracts.EmployeeService;

namespace EmployeeService.Application.Tests.UseCases;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockRepository;
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly IEmployeeService _service;
    private static readonly Guid TestDepartmentId = Guid.NewGuid();
    private static readonly Department TestDepartment = new Department("開発部", "ソフトウェア開発を担当する部署");
    private static readonly Department TestDepartment2 = new Department("営業部", "営業活動を担当する部署");

    public EmployeeServiceTests()
    {
        _mockRepository = new Mock<IEmployeeRepository>();
        _mockDepartmentRepository = new Mock<IDepartmentRepository>();
        
        // Setup default department lookup - include multiple departments
        _mockDepartmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestDepartment, TestDepartment2 });
            
        _service = new EmployeeService.Application.UseCases.EmployeeService(
            _mockRepository.Object, 
            _mockDepartmentRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnEmployeeDto()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", 
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "エンジニア");
        
        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _service.GetByIdAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.FirstName, result.FirstName);
        Assert.Equal(employee.LastName, result.LastName);
        Assert.Equal(employee.Email, result.Email);
        _mockRepository.Verify(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _service.GetByIdAsync(employeeId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEmployees()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new Employee("太郎", "山田", "yamada.taro@example.com", 
                new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "エンジニア"),
            new Employee("花子", "佐藤", "sato.hanako@example.com", 
                new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "マネージャー")
        };

        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateEmployee()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee e, CancellationToken ct) => e);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.LastName, result.LastName);
        Assert.Equal(request.Email, result.Email);
        _mockRepository.Verify(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingEmployee = new Employee("次郎", "田中", "duplicate@example.com", 
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "エンジニア");
        
        var request = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "duplicate@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "エンジニア"
        };

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEmployee);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        _mockRepository.Verify(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdateEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", 
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "エンジニア");

        var request = new UpdateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "tanaka.jiro@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "営業部",
            Position = "マネージャー"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _service.UpdateAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.LastName, result.LastName);
        Assert.Equal(request.Email, result.Email);
        _mockRepository.Verify(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(employee, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new UpdateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "tanaka.jiro@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "営業部",
            Position = "マネージャー"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _service.UpdateAsync(employeeId, request);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", 
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "エンジニア");

        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _service.DeleteAsync(employeeId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _service.DeleteAsync(employeeId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingDepartment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "太郎",
            LastName = "山田",
            Email = "yamada.taro@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "存在しない部署",
            Position = "エンジニア"
        };

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        _mockDepartmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestDepartment, TestDepartment2 });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        Assert.Contains("存在しない部署", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingDepartment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", 
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "エンジニア");

        var request = new UpdateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "tanaka.jiro@example.com",
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "存在しない部署",
            Position = "マネージャー"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        _mockDepartmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestDepartment, TestDepartment2 });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(employeeId, request));
        Assert.Contains("存在しない部署", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateEmailForOtherEmployee_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", 
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "エンジニア");

        var otherEmployee = new Employee("花子", "佐藤", "sato.hanako@example.com", 
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "デザイナー");

        var request = new UpdateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "sato.hanako@example.com", // 他の従業員のメールアドレス
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "マネージャー"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherEmployee);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(employeeId, request));
        Assert.Contains("sato.hanako@example.com", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithSameEmailAsCurrentEmployee_ShouldUpdateEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", 
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), TestDepartmentId, "エンジニア");

        // Use reflection to set the Id (since it's private set)
        typeof(Employee).GetProperty("Id")!.SetValue(employee, employeeId);

        var request = new UpdateEmployeeRequest
        {
            FirstName = "次郎",
            LastName = "田中",
            Email = "yamada.taro@example.com", // 同じメールアドレス
            HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Department = "開発部",
            Position = "マネージャー"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee); // 同じ従業員

        // Act
        var result = await _service.UpdateAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.FirstName, result.FirstName);
        _mockRepository.Verify(r => r.UpdateAsync(employee, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyRepository_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
