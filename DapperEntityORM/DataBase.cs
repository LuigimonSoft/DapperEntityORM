using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperEntityORM.Enums;

namespace DapperEntityORM
{
    public class DataBase
    {
        private DataBaseTypes _dialect;
        public string _encapsulation;
        private string _getIdentitySql;
        private string _getPagedListSql;
        private string _connectionString { get; }

        public DataBase(DataBaseTypes dialect, string connectionString)
        {
            _dialect = dialect;
            _connectionString = connectionString;
        }

        public DataBaseTypes TipoBaseDatos
        {
            set
            {
                switch (value)
                {
                    case DataBaseTypes.PostgreSQL:
                        _dialect = DataBaseTypes.PostgreSQL;
                        _encapsulation = "\"{0}\"";
                        _getIdentitySql = string.Format("SELECT LASTVAL() AS id");
                        _getPagedListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
                        break;
                    case DataBaseTypes.SQLite:
                        _dialect = DataBaseTypes.SQLite;
                        _encapsulation = "\"{0}\"";
                        _getIdentitySql = string.Format("SELECT LAST_INSERT_ROWID() AS id");
                        _getPagedListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
                        break;
                    case DataBaseTypes.MySQL:
                        _dialect = DataBaseTypes.MySQL;
                        _encapsulation = "`{0}`";
                        _getIdentitySql = string.Format("SELECT LAST_INSERT_ID() AS id");
                        _getPagedListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {Offset},{RowsPerPage}";
                        break;
                    default:
                        _dialect = DataBaseTypes.SQLServer;
                        _encapsulation = "[{0}]";
                        _getIdentitySql = string.Format("SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [id]");
                        _getPagedListSql = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {OrderBy}) AS PagedNumber, {SelectColumns} FROM {TableName} {WhereClause}) AS u WHERE PagedNUMBER BETWEEN (({PageNumber}-1) * {RowsPerPage} + 1) AND ({PageNumber} * {RowsPerPage})";
                        break;
                }
            }
            get { return _dialect; }
        }
    }
}
