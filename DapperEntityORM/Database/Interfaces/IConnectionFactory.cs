namespace DapperEntityORM.Database.Interfaces
{
    using System.Data.SqlClient;
    public interface IConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
