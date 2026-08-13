using System.Linq.Expressions;
using BoardGameCafe.Core.Interfaces;
using BoardGameCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGameCafe.Infrastructure.Repositories;

/// <summary>
/// 泛型仓储实现
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    protected readonly BoardGameCafeDbContext _context;

    /// <summary>
    /// 实体数据集
    /// </summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// 初始化泛型仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public Repository(BoardGameCafeDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// 根据Id获取实体
    /// </summary>
    /// <param name="id">实体主键Id</param>
    /// <returns>实体对象</returns>
    public T? GetById(Guid id)
    {
        return _dbSet.Find(id);
    }

    /// <summary>
    /// 获取所有实体
    /// </summary>
    /// <returns>实体集合</returns>
    public IEnumerable<T> GetAll()
    {
        return _dbSet.ToList();
    }

    /// <summary>
    /// 根据条件查找实体
    /// </summary>
    /// <param name="predicate">查询条件表达式</param>
    /// <returns>符合条件的实体集合</returns>
    public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate).ToList();
    }

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">要添加的实体</param>
    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">要更新的实体</param>
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// 删除实体（软删除）
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    public void Delete(T entity)
    {
        var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            isDeletedProperty.SetValue(entity, true);
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    /// <summary>
    /// 保存更改
    /// </summary>
    /// <returns>受影响的行数</returns>
    public int SaveChanges()
    {
        return _context.SaveChanges();
    }
}
