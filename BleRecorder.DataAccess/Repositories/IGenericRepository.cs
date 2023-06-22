namespace BleRecorder.DataAccess.Repositories
{
    public interface IGenericRepository<T>: IDisposable
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task SaveAsync();
        bool HasChanges();
        void Add(T model);
        void Remove(T model);
    }
}