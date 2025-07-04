namespace Service.Contracts;
public interface IRepositoryBase<T>
{
    IQueryable<T> FindAll(bool trackChanges = false);
}
