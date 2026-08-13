using System.Linq.Expressions;

namespace BoardGameCafe.Core.Interfaces;

/// <summary>
/// 泛型仓储接口
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// 根据Id获取实体
    /// </summary>
    /// <param name="id">实体主键Id</param>
    /// <returns>实体对象</returns>
    T? GetById(Guid id);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    /// <returns>实体集合</returns>
    IEnumerable<T> GetAll();

    /// <summary>
    /// 根据条件查找实体
    /// </summary>
    /// <param name="predicate">查询条件表达式</param>
    /// <returns>符合条件的实体集合</returns>
    IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">要添加的实体</param>
    void Add(T entity);

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">要更新的实体</param>
    void Update(T entity);

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    void Delete(T entity);

    /// <summary>
    /// 保存更改
    /// </summary>
    /// <returns>受影响的行数</returns>
    int SaveChanges();
}
