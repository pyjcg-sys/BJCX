namespace BJCX.Application.Abstractions;

/// <summary>
/// 工作单元抽象：为写操作（Command）提供事务边界。
/// 事务管道（TransactionBehavior）依赖它包裹 Handler 的数据库操作。
/// </summary>
public interface IUnitOfWork
{
    /// <summary>开启事务。</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>提交事务。</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>回滚事务。</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
