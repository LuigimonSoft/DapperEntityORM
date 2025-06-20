using System.Data.SqlClient;
using DapperEntityORM.Database.Connections;
using DapperEntityORM.Database.Dialects;
using DapperEntityORM.Database.Interfaces;
using DapperEntityORM.Enums;

namespace DapperEntityORM
{
    public class DataBase
    {
        private readonly IDatabaseDialect _dialect;
        private readonly IConnectionFactory _connectionFactory;

        public DataBase(DataBaseTypes dialect, string connectionString)
            : this(DialectFactory.Create(dialect), new DefaultConnectionFactory(connectionString))
        {
        }

        public DataBase(IDatabaseDialect dialect, IConnectionFactory connectionFactory)
        {
            _dialect = dialect;
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        ///   Gets the connection.
        /// </summary>
        public SqlConnection Connection => _connectionFactory.CreateConnection();
        public string Encapsulation => _dialect.Encapsulation;
        public string GetIdentitySql => _dialect.GetIdentitySql;
        public string GetPagedListSql => _dialect.GetPagedListSql;
        public DataBaseTypes DataBaseType => _dialect.DataBaseType;

        
    }
}
