public interface IDatabaseRepository<T>
{
    void Add(T item);
    IEnumerable<T> GetAll();
    T? FindById(int id);
    bool Remove(int id);
}