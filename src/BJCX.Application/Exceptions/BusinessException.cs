namespace BJCX.Application.Exceptions;

/// <summary>
/// 业务异常：表示请求在业务规则层面不合法（权限不足、状态不符、数据不存在等）。
/// 由全局异常处理中间件转换为 4xx 响应。
/// </summary>
public class BusinessException : Exception
{
    /// <summary>业务异常构造。</summary>
    /// <param name="message">提示信息。</param>
    public BusinessException(string message) : base(message)
    {
    }
}
