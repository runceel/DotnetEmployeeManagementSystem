using EmployeeService.Domain.Entities;

namespace EmployeeService.Domain.Tests.Entities;

public class DepartmentTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateDepartment()
    {
        // Arrange
        var name = "開発部";
        var description = "ソフトウェア開発を担当する部署";

        // Act
        var department = new Department(name, description);

        // Assert
        Assert.NotEqual(Guid.Empty, department.Id);
        Assert.Equal(name, department.Name);
        Assert.Equal(description, department.Description);
        Assert.True(department.CreatedAt <= DateTime.UtcNow);
        Assert.True(department.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Department(null!, "説明"));
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Department("", "説明"));
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Department("開発部", null!));
    }

    [Fact]
    public void Constructor_WithEmptyDescription_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Department("開発部", ""));
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateDepartment()
    {
        // Arrange
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");
        var originalUpdatedAt = department.UpdatedAt;

        // Act
        department.Update("営業部", "営業活動を担当する部署");

        // Assert
        Assert.Equal("営業部", department.Name);
        Assert.Equal("営業活動を担当する部署", department.Description);
        Assert.True(department.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Update_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            department.Update(null!, "説明"));
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            department.Update("", "説明"));
    }

    [Fact]
    public void Update_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Arrange
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            department.Update("営業部", null!));
    }

    [Fact]
    public void Update_WithEmptyDescription_ShouldThrowArgumentException()
    {
        // Arrange
        var department = new Department("開発部", "ソフトウェア開発を担当する部署");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            department.Update("営業部", ""));
    }
}
