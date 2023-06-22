using Microsoft.EntityFrameworkCore;

namespace BleRecorder.DataAccess.Repositories
{
    public class GenericRepository<TEntity, TContext> : IGenericRepository<TEntity>
      where TEntity : class
      where TContext : DbContext
    {
        protected readonly TContext Context;
        protected readonly DbSet<TEntity> Table;

        protected GenericRepository(TContext context)
        {
            this.Context = context;
            Table = Context.Set<TEntity>();
        }

        public void Add(TEntity model)
        {
            Table.Add(model);
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await Table.FindAsync(id);
        }

        public async Task<IReadOnlyList<TEntity>> GetAllAsync()
        {
            return await Table.ToListAsync();
        }

        public void Remove(TEntity model)
        {
            Table.Remove(model);
        }

        public bool HasChanges()
        {
            return Context.ChangeTracker.HasChanges();
        }

        public async Task SaveAsync()
        {
            await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
