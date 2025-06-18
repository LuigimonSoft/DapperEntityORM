using DapperEntityORM.Database.Interfaces;
using DapperEntityORM.Enums;

namespace DapperEntityORM.Database.Dialects
{
    public class MySqlDialect : IDatabaseDialect
    {
        public DataBaseTypes DataBaseType => DataBaseTypes.MySQL;
        public string Encapsulation => "`{0}`";
        public string GetIdentitySql => "SELECT LAST_INSERT_ID() AS id";
        public string GetPagedListSql => "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {Offset},{RowsPerPage}";
    }
}
