namespace DapperEntityORM.Database.Interfaces
{
    using DapperEntityORM.Enums;
    public interface IDatabaseDialect
    {
        DataBaseTypes DataBaseType { get; }
        string Encapsulation { get; }
        string GetIdentitySql { get; }
        string GetPagedListSql { get; }
    }
}
