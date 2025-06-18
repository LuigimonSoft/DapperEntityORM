using DapperEntityORM.Database.Interfaces;
using DapperEntityORM.Enums;

namespace DapperEntityORM.Database.Dialects
{
    public class SqlServerDialect : IDatabaseDialect
    {
        public DataBaseTypes DataBaseType => DataBaseTypes.SQLServer;
        public string Encapsulation => "[{0}]";
        public string GetIdentitySql => "SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [id]";
        public string GetPagedListSql => "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {OrderBy}) AS PagedNumber, {SelectColumns} FROM {TableName} {WhereClause}) AS u WHERE PagedNUMBER BETWEEN (({PageNumber}-1) * {RowsPerPage} + 1) AND ({PageNumber} * {RowsPerPage})";
    }
}
