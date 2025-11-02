using EmployeeService.Domain.Entities;

namespace EmployeeService.Domain.Tests.Entities;

public class EmployeeTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEmployee()
    {
        // Arrange
        var firstName = "太郎";
        var lastName = "山田";
        var email = "yamada.taro@example.com";
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var department = "開発部";
        var position = "エンジニア";

        // Act
        var employee = new Employee(firstName, lastName, email, hireDate, department, position);

        // Assert
        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(email, employee.Email);
        Assert.Equal(hireDate, employee.HireDate);
        Assert.Equal(department, employee.Department);
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
            new Employee(null!, "山田", "test@example.com", hireDate, "開発部", "エンジニア"));
    }

    [Fact]
    public void Constructor_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("", "山田", "test@example.com", hireDate, "開発部", "エンジニア"));
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "山田", "invalid-email", hireDate, "開発部", "エンジニア"));
    }

    [Fact]
    public void Constructor_WithFutureHireDate_ShouldThrowArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Employee("太郎", "山田", "test@example.com", futureDate, "開発部", "エンジニア"));
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateEmployee()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, "開発部", "エンジニア");
        var originalUpdatedAt = employee.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10); // Ensure time difference
        employee.Update("次郎", "田中", "tanaka.jiro@example.com", hireDate, "営業部", "マネージャー");

        // Assert
        Assert.Equal("次郎", employee.FirstName);
        Assert.Equal("田中", employee.LastName);
        Assert.Equal("tanaka.jiro@example.com", employee.Email);
        Assert.Equal("営業部", employee.Department);
        Assert.Equal("マネージャー", employee.Position);
        Assert.True(employee.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void Update_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, "開発部", "エンジニア");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            employee.Update("次郎", "田中", "invalid-email", hireDate, "営業部", "マネージャー"));
    }

    [Fact]
    public void GetFullName_ShouldReturnFullName()
    {
        // Arrange
        var hireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var employee = new Employee("太郎", "山田", "yamada.taro@example.com", hireDate, "開発部", "エンジニア");

        // Act
        var fullName = employee.GetFullName();

        // Assert
        Assert.Equal("山田 太郎", fullName);
    }
}
