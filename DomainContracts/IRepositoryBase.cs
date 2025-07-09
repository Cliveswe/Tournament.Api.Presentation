// -----------------------------------------------------------------------------
// File: IRepositoryBase.cs
// Summary: Defines a generic repository contract for basic CRUD operations,
//          supporting query customization and change tracking control.
// <author> [Clive Leddy] </author>
// <created> [2025-07-04] </created>
// Notes: Enables abstraction of data access logic for entity types,
//        improving testability, maintainability, and separation of concerns.
// -----------------------------------------------------------------------------

using System.Linq.Expressions;

/// <summary>
/// Represents the generic base repository interface that defines standard 
/// Create, Read, Update, and Delete (CRUD) operations for a given entity type.
/// </summary>
/// <typeparam name="T">The entity type for which the repository provides data access operations.</typeparam>
/// <remarks>
/// This interface supports expression-based querying and allows enabling or disabling
/// EF Core change tracking through a parameter, providing flexibility for read-only vs. tracked queries.
/// </remarks>
public interface IRepositoryBase<T>
{
    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="trackChanges">
    /// Indicates whether to track changes for the returned entities.
    /// Set to <c>false</c> for read-only operations to improve performance.
    /// </param>
    /// <returns>An <see cref="IQueryable{T}"/> of all entities.</returns>
    IQueryable<T> FindAll(bool trackChanges = false);

    /// <summary>
    /// Retrieves entities of type <typeparamref name="T"/> that satisfy the specified condition.
    /// </summary>
    /// <param name="expression">The filter expression.</param>
    /// <param name="trackChanges">
    /// Indicates whether to track changes for the returned entities.
    /// </param>
    /// <returns>An <see cref="IQueryable{T}"/> of filtered entities.</returns>
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);

    /// <summary>
    /// Adds a new entity to the data context.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    void Create(T entity);

    /// <summary>
    /// Updates an existing entity in the data context.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(T entity);

    /// <summary>
    /// Removes an entity from the data context.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Delete(T entity);
}
