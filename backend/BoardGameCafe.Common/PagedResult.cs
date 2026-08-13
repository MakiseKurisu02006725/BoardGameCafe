namespace BoardGameCafe.Common;

/// <summary>
/// 分页结果类
/// </summary>
/// <typeparam name="T">数据项类型</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// 当前页码（从1开始）
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页数据条数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// 分页数据项集合
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// 创建分页结果
    /// </summary>
    /// <param name="items">当前页数据项</param>
    /// <param name="pageIndex">当前页码</param>
    /// <param name="pageSize">每页数据条数</param>
    /// <param name="totalCount">总记录数</param>
    /// <returns>分页结果实例</returns>
    public static PagedResult<T> Create(IEnumerable<T> items, int pageIndex, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<T>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = items.ToList().AsReadOnly()
        };
    }

    /// <summary>
    /// 创建空分页结果
    /// </summary>
    /// <param name="pageIndex">当前页码</param>
    /// <param name="pageSize">每页数据条数</param>
    /// <returns>空分页结果实例</returns>
    public static PagedResult<T> Empty(int pageIndex = 1, int pageSize = 10)
    {
        return new PagedResult<T>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = 0,
            TotalPages = 0,
            Items = new List<T>().AsReadOnly()
        };
    }
}
