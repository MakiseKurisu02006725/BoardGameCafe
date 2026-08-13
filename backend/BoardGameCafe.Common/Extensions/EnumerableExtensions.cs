namespace BoardGameCafe.Common.Extensions;

/// <summary>
/// 集合扩展方法类
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// 对集合进行分页处理（内存分页）
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="pageIndex">页码（从1开始）</param>
    /// <param name="pageSize">每页数据条数</param>
    /// <returns>分页结果</returns>
    /// <exception cref="ArgumentNullException">当source为null时抛出</exception>
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "源集合不能为空");
        }

        if (pageIndex < 1)
        {
            pageIndex = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        var totalCount = source.Count();
        var skip = (pageIndex - 1) * pageSize;
        var items = source.Skip(skip).Take(pageSize).ToList();

        return PagedResult<T>.Create(items, pageIndex, pageSize, totalCount);
    }

    /// <summary>
    /// 使用分页参数对集合进行分页处理（内存分页）
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="paginationParams">分页参数</param>
    /// <returns>分页结果</returns>
    /// <exception cref="ArgumentNullException">当source或paginationParams为null时抛出</exception>
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, PaginationParams paginationParams)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "源集合不能为空");
        }

        if (paginationParams == null)
        {
            throw new ArgumentNullException(nameof(paginationParams), "分页参数不能为空");
        }

        return source.ToPagedResult(paginationParams.PageIndex, paginationParams.PageSize);
    }

    /// <summary>
    /// 对查询进行分页处理（数据库分页）
    /// </summary>
    /// <typeparam name="T">查询元素类型</typeparam>
    /// <param name="source">源查询</param>
    /// <param name="pageIndex">页码（从1开始）</param>
    /// <param name="pageSize">每页数据条数</param>
    /// <param name="totalCount">总记录数（用于已统计总数的场景）</param>
    /// <returns>分页结果</returns>
    /// <exception cref="ArgumentNullException">当source为null时抛出</exception>
    public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> source, int pageIndex, int pageSize, int? totalCount = null)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "源查询不能为空");
        }

        if (pageIndex < 1)
        {
            pageIndex = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        var count = totalCount ?? source.Count();
        var skip = (pageIndex - 1) * pageSize;
        var items = source.Skip(skip).Take(pageSize).ToList();

        return PagedResult<T>.Create(items, pageIndex, pageSize, count);
    }

    /// <summary>
    /// 使用分页参数对查询进行分页处理（数据库分页）
    /// </summary>
    /// <typeparam name="T">查询元素类型</typeparam>
    /// <param name="source">源查询</param>
    /// <param name="paginationParams">分页参数</param>
    /// <param name="totalCount">总记录数（用于已统计总数的场景）</param>
    /// <returns>分页结果</returns>
    /// <exception cref="ArgumentNullException">当source或paginationParams为null时抛出</exception>
    public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> source, PaginationParams paginationParams, int? totalCount = null)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "源查询不能为空");
        }

        if (paginationParams == null)
        {
            throw new ArgumentNullException(nameof(paginationParams), "分页参数不能为空");
        }

        return source.ToPagedResult(paginationParams.PageIndex, paginationParams.PageSize, totalCount);
    }

    /// <summary>
    /// 根据分页参数跳过指定数量的记录
    /// </summary>
    /// <typeparam name="T">查询元素类型</typeparam>
    /// <param name="source">源查询</param>
    /// <param name="paginationParams">分页参数</param>
    /// <returns>跳过记录后的查询</returns>
    /// <exception cref="ArgumentNullException">当source或paginationParams为null时抛出</exception>
    public static IQueryable<T> SkipWith<T>(this IQueryable<T> source, PaginationParams paginationParams)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "源查询不能为空");
        }

        if (paginationParams == null)
        {
            throw new ArgumentNullException(nameof(paginationParams), "分页参数不能为空");
        }

        return source.Skip(paginationParams.Skip);
    }

    /// <summary>
    /// 根据分页参数获取指定数量的记录
    /// </summary>
    /// <typeparam name="T">查询元素类型</typeparam>
    /// <param name="source">源查询</param>
    /// <param name="paginationParams">分页参数</param>
    /// <returns>获取指定数量记录后的查询</returns>
    /// <exception cref="ArgumentNullException">当source或paginationParams为null时抛出</exception>
    public static IQueryable<T> TakeWith<T>(this IQueryable<T> source, PaginationParams paginationParams)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "源查询不能为空");
        }

        if (paginationParams == null)
        {
            throw new ArgumentNullException(nameof(paginationParams), "分页参数不能为空");
        }

        return source.Take(paginationParams.PageSize);
    }

    /// <summary>
    /// 根据分页参数进行分页（Skip + Take）
    /// </summary>
    /// <typeparam name="T">查询元素类型</typeparam>
    /// <param name="source">源查询</param>
    /// <param name="paginationParams">分页参数</param>
    /// <returns>分页后的查询</returns>
    /// <exception cref="ArgumentNullException">当source或paginationParams为null时抛出</exception>
    public static IQueryable<T> PaginateWith<T>(this IQueryable<T> source, PaginationParams paginationParams)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "源查询不能为空");
        }

        if (paginationParams == null)
        {
            throw new ArgumentNullException(nameof(paginationParams), "分页参数不能为空");
        }

        return source.Skip(paginationParams.Skip).Take(paginationParams.PageSize);
    }

    /// <summary>
    /// 判断集合是否为null或空
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <returns>如果为null或空返回true，否则返回false</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// 如果集合为null则返回空集合
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <returns>非null的集合</returns>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// 对集合的每个元素执行指定操作
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">源集合</param>
    /// <param name="action">要执行的操作</param>
    /// <exception cref="ArgumentNullException">当source或action为null时抛出</exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "源集合不能为空");
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action), "操作不能为空");
        }

        foreach (var item in source)
        {
            action(item);
        }
    }
}
