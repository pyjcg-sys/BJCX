using BJCX.Application.Abstractions;
using MediatR;

namespace BJCX.Application.Behaviors;

/// <summary>
/// 事务管道：对写操作（以 Command 结尾的请求）包裹数据库事务，
/// 保证审批流转等复合写操作的原子性与一致性。
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 仅对写操作（Command）开启事务，读操作（Query）直接放行
        if (!typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal))
        {
            return await next();
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next();
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
