namespace BlazorWeb.Models;

/// <summary>
/// 従業員フォーム用モデル
/// </summary>
public class EmployeeFormModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Today;
}
