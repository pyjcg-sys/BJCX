using BJCX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Infrastructure.Persistence;

/// <summary>
/// 通用仓储实现（BaseRepository）：封装 EF Core 的 DbSet 操作。
/// </summary>
/// <typeparam name="TEntity">实体类型。</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly AppDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(AppDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => DbSet;

    public IQueryable<TEntity> QueryAsNoTracking() => DbSet.AsNoTracking();

    public async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync(new object[] { id }, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Remove(TEntity entity) => DbSet.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => DbContext.SaveChangesAsync(cancellationToken);
}
