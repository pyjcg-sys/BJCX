using BJCX.Domain.Entities;

namespace BJCX.Application.Abstractions;

/// <summary>
/// 通用仓储接口：统一增删改查入口，避免业务代码直接操作 DbContext。
/// 定义在应用层，基础设施层提供 EF Core 实现。
/// </summary>
/// <typeparam name="TEntity">实体类型（继承 BaseEntity）。</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>返回可追踪查询（用于写操作前的实体加载）。</summary>
    IQueryable<TEntity> Query();

    /// <summary>返回只读查询（AsNoTracking），用于读操作。</summary>
    IQueryable<TEntity> QueryAsNoTracking();

    /// <summary>按主键查询，不存在返回 null。</summary>
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>新增实体。</summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>更新实体。</summary>
    void Update(TEntity entity);

    /// <summary>删除实体。</summary>
    void Remove(TEntity entity);

    /// <summary>保存变更并返回受影响行数。</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
