
namespace SimpleDB
{
    /// <summary>
    /// Defines basic operations for storing and retrieving items in a database repository.
    /// </summary>
    /// <typeparam name="T">The type of objects stored in the repository.</typeparam>
    public interface IDatabaseRepository<T>
    {
        /// <summary>
        /// Adds a new item to the repository.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void Add(T item);
        
        /// <summary>
        /// Retrieves all items stored in the repository.
        /// </summary>
        /// <returns>An enumerable collection of all items.</returns>
        IEnumerable<T> GetAll();
    }
}