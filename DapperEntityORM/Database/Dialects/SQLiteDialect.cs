using DapperEntityORM.Database.Interfaces;
using DapperEntityORM.Enums;

namespace DapperEntityORM.Database.Dialects
{
    public class SQLiteDialect : IDatabaseDialect
    {
        public DataBaseTypes DataBaseType => DataBaseTypes.SQLite;
        public string Encapsulation => "\"{0}\"";
        public string GetIdentitySql => "SELECT LAST_INSERT_ROWID() AS id";
        public string GetPagedListSql => "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
    }
}
