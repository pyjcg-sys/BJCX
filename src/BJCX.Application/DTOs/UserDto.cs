namespace BJCX.Application.DTOs;

/// <summary>
/// 用户信息 DTO（审批流程配置中选择审批人时展示）。
/// </summary>
public class UserDto
{
    /// <summary>用户 Id。</summary>
    public long Id { get; set; }

    /// <summary>登录名。</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>显示名（真实姓名）。</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>邮箱。</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>工号。</summary>
    public string EmployeeNo { get; set; } = string.Empty;

    /// <summary>部门。</summary>
    public string Department { get; set; } = string.Empty;
}
