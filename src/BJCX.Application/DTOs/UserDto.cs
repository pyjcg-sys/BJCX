namespace BJCX.Application.DTOs;

/// <summary>
/// 用户信息 DTO（审批流程配置中选择审批人时展示）。
/// </summary>
public class UserDto
{
    public long Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string EmployeeNo { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;
}
