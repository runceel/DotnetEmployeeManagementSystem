using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.DepartmentService;

/// <summary>
/// 部署作成リクエスト
/// </summary>
public record CreateDepartmentRequest
{
    /// <summary>
    /// 部署名
    /// </summary>
    [Required(ErrorMessage = "部署名を入力してください。")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 部署説明
    /// </summary>
    [Required(ErrorMessage = "部署説明を入力してください。")]
    public string Description { get; init; } = string.Empty;
}
