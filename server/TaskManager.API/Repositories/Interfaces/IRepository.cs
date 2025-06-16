using System.Linq.Expressions;

namespace TaskManager.API.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Basic CRUD operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);

        // Advanced querying
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        // Save changes
        Task SaveChangesAsync();
    }
}

// Generic <T>, works with any entity type (TaskItem, Project or Team)
// where T: class constraint ensuring T is a reference type
// Expression <Func<T, bool>> Allows complex query expressions such as t => t.IsComplete == true
