namespace EmployeeService.Domain.Entities;

/// <summary>
/// 従業員エンティティ
/// </summary>
public class Employee
{
    /// <summary>
    /// 従業員ID
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 名
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// 姓
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// 入社日
    /// </summary>
    public DateTime HireDate { get; private set; }

    /// <summary>
    /// 部署ID（外部キー）
    /// </summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>
    /// 部署（ナビゲーションプロパティ）
    /// </summary>
    public Department? Department { get; private set; }

    /// <summary>
    /// 役職
    /// </summary>
    public string Position { get; private set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    private Employee()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Position = string.Empty;
    }

    public Employee(string firstName, string lastName, string email, DateTime hireDate, Guid departmentId, string position)
    {
        Id = Guid.NewGuid();
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        HireDate = hireDate;
        DepartmentId = departmentId;
        Position = position ?? throw new ArgumentNullException(nameof(position));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        ValidateEmployee();
    }

    public void Update(string firstName, string lastName, string email, DateTime hireDate, Guid departmentId, string position)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        HireDate = hireDate;
        DepartmentId = departmentId;
        Position = position ?? throw new ArgumentNullException(nameof(position));
        UpdatedAt = DateTime.UtcNow;

        ValidateEmployee();
    }

    private void ValidateEmployee()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            throw new ArgumentException("名を入力してください。", nameof(FirstName));

        if (string.IsNullOrWhiteSpace(LastName))
            throw new ArgumentException("姓を入力してください。", nameof(LastName));

        if (string.IsNullOrWhiteSpace(Email))
            throw new ArgumentException("メールアドレスを入力してください。", nameof(Email));

        if (!IsValidEmail(Email))
            throw new ArgumentException("有効なメールアドレスを入力してください。", nameof(Email));

        if (HireDate > DateTime.UtcNow)
            throw new ArgumentException("入社日は現在より前の日付を指定してください。", nameof(HireDate));

        if (DepartmentId == Guid.Empty)
            throw new ArgumentException("部署を指定してください。", nameof(DepartmentId));

        if (string.IsNullOrWhiteSpace(Position))
            throw new ArgumentException("役職を入力してください。", nameof(Position));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public string GetFullName() => $"{LastName} {FirstName}";
}
