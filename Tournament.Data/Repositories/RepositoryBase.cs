// -----------------------------------------------------------------------------
// File: RepositoryBase.cs
// Summary: Provides a generic base class implementation for data repository
//          operations using Entity Framework Core.
// <author> [Clive Leddy] </author>
// <created> [2025-07-04] </created>
// Notes: Implements the base repository pattern for common CRUD operations,
//        supporting both tracked and untracked queries.
// -----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;

/// <summary>
/// Provides a generic implementation of the <see cref="IRepositoryBase{T}"/> interface
/// using Entity Framework Core to interact with the database. Supports CRUD operations and
/// query tracking control.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public class RepositoryBase<T>(TournamentApiContext context) : IRepositoryBase<T> where T : class
{
    protected TournamentApiContext Context => context;
    protected DbSet<T> DbSet => Context.Set<T>();

    /// <inheritdoc/>
    public void Create(T entity)
    {
        DbSet.Add(entity);
    }

    /// <inheritdoc/>
    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    /// <inheritdoc/>
    public IQueryable<T> FindAll(bool trackChanges = false) =>
        trackChanges ?
        DbSet :
        DbSet.AsNoTracking();

    /// <inheritdoc/>
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
        trackChanges ?
        DbSet.Where(expression) :
        DbSet.AsNoTracking().Where(expression);

    /// <inheritdoc/>
    public void Update(T entity)
    {
        DbSet.Update(entity);
    }
}
