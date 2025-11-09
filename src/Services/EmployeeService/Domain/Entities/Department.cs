namespace EmployeeService.Domain.Entities;

/// <summary>
/// 部署エンティティ
/// </summary>
public class Department
{
    /// <summary>
    /// 部署ID
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 部署名
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 部署説明
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    private Department()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public Department(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        ValidateDepartment();
    }

    public void Update(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        UpdatedAt = DateTime.UtcNow;

        ValidateDepartment();
    }

    private void ValidateDepartment()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("部署名を入力してください。", nameof(Name));

        if (string.IsNullOrWhiteSpace(Description))
            throw new ArgumentException("部署説明を入力してください。", nameof(Description));
    }
}
