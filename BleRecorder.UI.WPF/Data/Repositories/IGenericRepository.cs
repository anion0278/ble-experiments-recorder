using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BleRecorder.UI.WPF.Data.Repositories
{
    public interface IGenericRepository<T>
    {
        event EventHandler ChangesOccurred;
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task SaveAsync();
        bool HasChanges();
        void Add(T model);
        void Remove(T model);
    }
}