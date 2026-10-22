namespace BJCX.Domain.Entities;

/// <summary>
/// 用户/员工，同时承担申请人、审批人两种角色。
/// </summary>
public class User : BaseEntity
{
    /// <summary>登录名。</summary>
    public string UserName { get; set; } = null!;

    /// <summary>显示名（真实姓名）。</summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>邮箱（用于审批邮件通知）。</summary>
    public string Email { get; set; } = null!;

    /// <summary>工号。</summary>
    public string EmployeeNo { get; set; } = null!;

    /// <summary>部门。</summary>
    public string Department { get; set; } = null!;

    /// <summary>密码哈希（PBKDF2）。</summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>是否启用。</summary>
    public bool IsActive { get; set; } = true;
}
