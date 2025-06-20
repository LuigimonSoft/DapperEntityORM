using DapperEntityORM.Database.Interfaces;
using DapperEntityORM.Enums;

namespace DapperEntityORM.Database.Dialects
{
    public class PostgreSqlDialect : IDatabaseDialect
    {
        public DataBaseTypes DataBaseType => DataBaseTypes.PostgreSQL;
        public string Encapsulation => "\"{0}\"";
        public string GetIdentitySql => "SELECT LASTVAL() AS id";
        public string GetPagedListSql => "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
    }
}
