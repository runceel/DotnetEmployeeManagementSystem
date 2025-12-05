using EmployeeService.Domain.Entities;

namespace EmployeeService.Domain.Tests.Entities;

public class EmployeeTests
{
    private static readonly Guid TestDepartmentId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEmployee()
    {
        // Arrange
        var firstName = "太郎";
        var lastName = "山田";
        var email = "yamada.taro@example.com";
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var departmentId = TestDepartmentId;
        var position = "エンジニア";

        // Act
        var employee = new Employee(firstName, lastName, email, hireDate, departmentId, position);

        // Assert
        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(email, employee.Email);
        Assert.Equal(hireDate, employee.HireDate);
        Assert.Equal(departmentId, employee.DepartmentId);
        Assert.Equal(position, employee.Position);
        Assert.True(employee.CreatedAt <= DateTime.UtcNow);
        Assert.True(employee.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullFirstName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Employee(null!, "山田", "test@example.com", hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("", "山田", "test@example.com", hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "山田", "invalid-email", hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithFutureHireDate_ShouldThrowArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "山田", "test@example.com", futureDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateEmployee()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");
        var originalUpdatedAt = employee.UpdatedAt;
        var newDepartmentId = Guid.NewGuid();

        // Act
        employee.Update("次郎", "田中", "tanaka.jiro@example.com", hireDate, newDepartmentId, "マネージャー");

        // Assert
        Assert.Equal("次郎", employee.FirstName);
        Assert.Equal("田中", employee.LastName);
        Assert.Equal("tanaka.jiro@example.com", employee.Email);
        Assert.Equal(newDepartmentId, employee.DepartmentId);
        Assert.Equal("マネージャー", employee.Position);
        Assert.True(employee.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Update_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            employee.Update("次郎", "田中", "invalid-email", hireDate, Guid.NewGuid(), "マネージャー"));
    }

    [Fact]
    public void GetFullName_ShouldReturnFullName()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act
        var fullName = employee.GetFullName();

        // Assert
        Assert.Equal("山田 太郎", fullName);
    }

    [Fact]
    public void Constructor_WithNullLastName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Employee("太郎", null!, "test@example.com", hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Employee("太郎", "山田", null!, hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithNullPosition_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Employee("太郎", "山田", "test@example.com", hireDate, TestDepartmentId, null!));
    }

    [Fact]
    public void Constructor_WithEmptyLastName_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "", "test@example.com", hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "山田", "", hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithEmptyPosition_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "山田", "test@example.com", hireDate, TestDepartmentId, ""));
    }

    [Fact]
    public void Constructor_WithEmptyDepartmentId_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "山田", "test@example.com", hireDate, Guid.Empty, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithWhitespaceFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("   ", "山田", "test@example.com", hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithWhitespaceLastName_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "   ", "test@example.com", hireDate, TestDepartmentId, "エンジニア"));
    }

    [Fact]
    public void Constructor_WithWhitespacePosition_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "山田", "test@example.com", hireDate, TestDepartmentId, "   "));
    }

    [Fact]
    public void Update_WithNullFirstName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            employee.Update(null!, "田中", "tanaka@example.com", hireDate, Guid.NewGuid(), "マネージャー"));
    }

    [Fact]
    public void Update_WithNullLastName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            employee.Update("次郎", null!, "tanaka@example.com", hireDate, Guid.NewGuid(), "マネージャー"));
    }

    [Fact]
    public void Update_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            employee.Update("次郎", "田中", null!, hireDate, Guid.NewGuid(), "マネージャー"));
    }

    [Fact]
    public void Update_WithNullPosition_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            employee.Update("次郎", "田中", "tanaka@example.com", hireDate, Guid.NewGuid(), null!));
    }

    [Fact]
    public void Update_WithEmptyDepartmentId_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            employee.Update("次郎", "田中", "tanaka@example.com", hireDate, Guid.Empty, "マネージャー"));
    }

    [Fact]
    public void Update_WithFutureHireDate_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            employee.Update("次郎", "田中", "tanaka@example.com", futureDate, Guid.NewGuid(), "マネージャー"));
    }

    [Fact]
    public void Update_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            employee.Update("", "田中", "tanaka@example.com", hireDate, Guid.NewGuid(), "マネージャー"));
    }

    [Fact]
    public void Update_WithEmptyLastName_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            employee.Update("次郎", "", "tanaka@example.com", hireDate, Guid.NewGuid(), "マネージャー"));
    }

    [Fact]
    public void Update_WithEmptyPosition_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, TestDepartmentId, "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            employee.Update("次郎", "田中", "tanaka@example.com", hireDate, Guid.NewGuid(), ""));
    }
}
