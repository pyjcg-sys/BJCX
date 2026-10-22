using BJCX.Application.Abstractions;

namespace BJCX.Infrastructure.Persistence;

/// <summary>
/// 工作单元实现：基于 EF Core 的数据库事务。
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _dbContext.Database.BeginTransactionAsync(cancellationToken);

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => _dbContext.Database.CommitTransactionAsync(cancellationToken);

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        => _dbContext.Database.RollbackTransactionAsync(cancellationToken);
}
