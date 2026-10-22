namespace BJCX.Application.DTOs;

/// <summary>
/// 分页结果通用封装。
/// </summary>
/// <typeparam name="T">列表元素类型。</typeparam>
public class PagedResult<T>
{
    /// <summary>当前页码（从 1 开始）。</summary>
    public int PageIndex { get; set; }

    /// <summary>每页条数。</summary>
    public int PageSize { get; set; }

    /// <summary>总记录数。</summary>
    public int TotalCount { get; set; }

    /// <summary>总页数。</summary>
    public int TotalPages { get; set; }

    /// <summary>当前页数据。</summary>
    public List<T> Items { get; set; } = new();
}
