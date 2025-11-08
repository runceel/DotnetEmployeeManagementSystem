namespace Shared.Contracts.EmployeeService;

/// <summary>
/// 最近のアクティビティDTO
/// </summary>
public record RecentActivityDto
{
    /// <summary>
    /// アクティビティID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// アクティビティタイプ (Created, Updated)
    /// </summary>
    public string ActivityType { get; init; } = string.Empty;

    /// <summary>
    /// 従業員ID
    /// </summary>
    public Guid EmployeeId { get; init; }

    /// <summary>
    /// 従業員名
    /// </summary>
    public string EmployeeName { get; init; } = string.Empty;

    /// <summary>
    /// 説明
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// アクティビティのタイムスタンプ
    /// </summary>
    public DateTime Timestamp { get; init; }
}
