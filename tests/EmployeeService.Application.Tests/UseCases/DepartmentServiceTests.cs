using EmployeeService.Application.UseCases;
using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Repositories;
using Moq;
using Shared.Contracts.DepartmentService;

namespace EmployeeService.Application.Tests.UseCases;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _mockRepository;
    private readonly IDepartmentService _service;

    public DepartmentServiceTests()
    {
        _mockRepository = new Mock<IDepartmentRepository>();
        _service = new DepartmentService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnDepartmentDto()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");
        
        _mockRepository.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        // Act
        var result = await _service.GetByIdAsync(departmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(department.Name, result.Name);
        Assert.Equal(department.Description, result.Description);
        _mockRepository.Verify(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _service.GetByIdAsync(departmentId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDepartments()
    {
        // Arrange
        var departments = new List<Department>
        {
            new Department("開発部", "ソフトウェア開発を担当する部署"),
            new Department("営業部", "営業活動を担当する部署")
        };

        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(departments);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateDepartment()
    {
        // Arrange
        var request = new CreateDepartmentRequest
        {
            Name = "開発部",
            Description = "ソフトウェア開発を担当する部署"
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department d, CancellationToken ct) => d);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdateDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");

        var request = new UpdateDepartmentRequest
        {
            Name = "営業部",
            Description = "営業活動を担当する部署"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department d, CancellationToken ct) => d);

        // Act
        var result = await _service.UpdateAsync(departmentId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        _mockRepository.Verify(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(department, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var request = new UpdateDepartmentRequest
        {
            Name = "営業部",
            Description = "営業活動を担当する部署"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _service.UpdateAsync(departmentId, request);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingIdAndNoEmployees_ShouldReturnTrue()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");

        _mockRepository.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _mockRepository.Setup(r => r.HasEmployeesAsync(department.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepository.Setup(r => r.DeleteAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(departmentId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.HasEmployeesAsync(department.Name, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _service.DeleteAsync(departmentId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.HasEmployeesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithDepartmentThatHasEmployees_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");

        _mockRepository.Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _mockRepository.Setup(r => r.HasEmployeesAsync(department.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.DeleteAsync(departmentId));

        Assert.Equal("従業員が所属している部署は削除できません。", exception.Message);
        _mockRepository.Verify(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.HasEmployeesAsync(department.Name, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
