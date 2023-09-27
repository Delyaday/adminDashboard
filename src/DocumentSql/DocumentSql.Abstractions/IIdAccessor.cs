namespace DocumentSql
{
    public interface IIdAccessor<T>
    {
        T Get(object obj);

        void Set(object obj, T value);
    }
}
