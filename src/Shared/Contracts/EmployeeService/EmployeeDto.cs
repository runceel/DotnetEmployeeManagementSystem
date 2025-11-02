namespace Shared.Contracts.EmployeeService;

/// <summary>
/// 従業員DTO
/// </summary>
public record EmployeeDto
{
    /// <summary>
    /// 従業員ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 名
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// 姓
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// 入社日
    /// </summary>
    public DateTime HireDate { get; init; }

    /// <summary>
    /// 部署
    /// </summary>
    public string Department { get; init; } = string.Empty;

    /// <summary>
    /// 役職
    /// </summary>
    public string Position { get; init; } = string.Empty;

    /// <summary>
    /// フルネーム
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
