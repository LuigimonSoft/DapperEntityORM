using System.Data.SqlClient;
using DapperEntityORM.Database.Interfaces;

namespace DapperEntityORM.Database.Connections
{
    public class DefaultConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public DefaultConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
