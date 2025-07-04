
using Microsoft.EntityFrameworkCore;
using Service.Contracts;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class RepositoryBase<T>(TournamentApiContext context) : IRepositoryBase<T> where T : class
{
    protected TournamentApiContext Context => context;
    protected DbSet<T> DbSet => Context.Set<T>();


    public IQueryable<T> FindAll(bool trackChanges = false) => trackChanges ? DbSet : DbSet.AsNoTracking();
}
