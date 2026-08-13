namespace BoardGameCafe.Common;

/// <summary>
/// 分页参数基类
/// </summary>
public abstract class PaginationParams
{
    /// <summary>
    /// 默认页码
    /// </summary>
    private const int DefaultPageIndex = 1;

    /// <summary>
    /// 默认每页数据条数
    /// </summary>
    private const int DefaultPageSize = 10;

    /// <summary>
    /// 最大每页数据条数
    /// </summary>
    private const int MaxPageSize = 100;

    private int _pageIndex = DefaultPageIndex;

    /// <summary>
    /// 当前页码（从1开始，默认为1）
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value < 1 ? DefaultPageIndex : value;
    }

    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// 每页数据条数（默认为10，最大为100）
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value < 1)
            {
                _pageSize = DefaultPageSize;
            }
            else if (value > MaxPageSize)
            {
                _pageSize = MaxPageSize;
            }
            else
            {
                _pageSize = value;
            }
        }
    }

    /// <summary>
    /// 要跳过的记录数（用于数据库查询）
    /// </summary>
    public int Skip => (PageIndex - 1) * PageSize;

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否降序排序（true为降序，false为升序）
    /// </summary>
    public bool SortDescending { get; set; }
}
