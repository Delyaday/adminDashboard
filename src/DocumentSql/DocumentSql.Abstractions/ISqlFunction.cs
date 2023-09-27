namespace DocumentSql
{
    public interface ISqlFunction
    {
        string Render(string[] arguments);
    }
}
