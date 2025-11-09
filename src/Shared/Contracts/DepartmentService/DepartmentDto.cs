namespace Shared.Contracts.DepartmentService;

/// <summary>
/// 部署DTO
/// </summary>
public record DepartmentDto
{
    /// <summary>
    /// 部署ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 部署名
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 部署説明
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
