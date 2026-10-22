namespace BJCX.Application.Exceptions;

/// <summary>
/// 资源不存在异常：查询的实体不存在，转换为 404 响应。
/// </summary>
public class NotFoundException : BusinessException
{
    /// <summary>资源不存在异常构造。</summary>
    /// <param name="message">提示信息。</param>
    public NotFoundException(string message) : base(message)
    {
    }
}
