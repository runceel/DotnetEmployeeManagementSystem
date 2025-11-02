using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.EmployeeService;

/// <summary>
/// 従業員更新リクエスト
/// </summary>
public record UpdateEmployeeRequest
{
    /// <summary>
    /// 名
    /// </summary>
    [Required(ErrorMessage = "名を入力してください。")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// 姓
    /// </summary>
    [Required(ErrorMessage = "姓を入力してください。")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスを入力してください。")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください。")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// 入社日
    /// </summary>
    [Required(ErrorMessage = "入社日を入力してください。")]
    public DateTime HireDate { get; init; }

    /// <summary>
    /// 部署
    /// </summary>
    [Required(ErrorMessage = "部署を入力してください。")]
    public string Department { get; init; } = string.Empty;

    /// <summary>
    /// 役職
    /// </summary>
    [Required(ErrorMessage = "役職を入力してください。")]
    public string Position { get; init; } = string.Empty;
}
