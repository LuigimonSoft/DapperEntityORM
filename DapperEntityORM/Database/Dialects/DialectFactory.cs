using DapperEntityORM.Database.Interfaces;
using DapperEntityORM.Enums;

namespace DapperEntityORM.Database.Dialects
{
    internal static class DialectFactory
    {
        public static IDatabaseDialect Create(DataBaseTypes type)
        {
            return type switch
            {
                DataBaseTypes.PostgreSQL => new PostgreSqlDialect(),
                DataBaseTypes.SQLite => new SQLiteDialect(),
                DataBaseTypes.MySQL => new MySqlDialect(),
                _ => new SqlServerDialect(),
            };
        }
    }
}
