namespace BJCX.Domain.Entities;

/// <summary>
/// 实体基类：统一主键 Id。
/// </summary>
public abstract class BaseEntity
{
    /// <summary>主键（自增）。</summary>
    public long Id { get; set; }
}
