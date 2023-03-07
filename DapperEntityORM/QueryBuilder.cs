using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DapperEntityORM.Attributes;
using DapperEntityORM.Resolvers.Interfaces;
using DapperEntityORM.Resolvers;
using System.Text.RegularExpressions;

namespace DapperEntityORM
{
    public class QueryBuilder<T>
    {
        private readonly string _tableName;
        private string _whereClause = string.Empty;
        private string _orderByClause = string.Empty;
        private string[] _selectColumns;
        private DynamicParameters _dynamicParameters;
        private readonly DataBase _dataBase;
        private bool _isCount = false;

        public QueryBuilder(DataBase dataBase)
        {
            ITableNameResolver _tableNameResolver = new TableNameResolver();
            _tableName = _tableNameResolver.ResolveTableName(typeof(T), dataBase.Encapsulation);
            _dynamicParameters = new DynamicParameters();
            _dataBase = dataBase;
        }

        public QueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            var visitor = new WhereVisitor(_dynamicParameters);
            visitor.Visit(predicate);

            _whereClause = visitor.WhereClause;

            return this;
        }

        public QueryBuilder<T> Where(string whereclause, List<object>? parameters=null)
        { 
            _whereClause =$" WHERE {whereclause}";

            Regex regex = new Regex(@"@\w+");
            MatchCollection matches = regex.Matches(_whereClause);

            if (parameters != null) 
            {
                for (int i = 0; i < matches.Count; i++)
                    _dynamicParameters.Add(matches[i].Value, parameters[i]);
                
            }   
            return this;
        }

        public QueryBuilder<T> OrderByASC(string orderByClause)
        {
            _orderByClause = $"ORDER BY {orderByClause} ASC ";
            return this;
        }

        public QueryBuilder<T> OrderByDESC(string orderByClause)
        {
            _orderByClause = $"ORDER BY {orderByClause} DESC ";
            return this;
        }

        public QueryBuilder<T> Select(string[]? selectColumns=null)
        {
            if (selectColumns == null)
            {
                bool mapColum;
                 IColumnNameResolver _columnNameResolver = new ColumnNameResolver();
                _selectColumns = new string[] { _columnNameResolver.ResolveKeyColumnName(getPropertyKey(typeof(T)), _dataBase.Encapsulation, out mapColum) };
                getColumnsNames(typeof(T)).ForEach(x => _selectColumns = _selectColumns.Append(x).ToArray());
            }
            else
                _selectColumns = selectColumns;
            return this;
        }

        public QueryBuilder<T> Count()
        {
            _isCount = true;
            return this;
        }

        public List<T> ToList()
        {
            using(IDbConnection Conexion = _dataBase.Connection)
            {
                return Conexion.Query<T>(BuildSelectQuery(), _dynamicParameters).ToList();
            }
        }

        public IEnumerable<T> ToEnumerable()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                return Conexion.Query<T>(BuildSelectQuery(), _dynamicParameters);
            }
        }

        public Dictionary<object, T> ToDictionary()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                Dictionary<object, T> ResDic = new Dictionary<object, T>();
                var values = Conexion.Query<T>(BuildSelectQuery(), _dynamicParameters).ToList();
                foreach( T value in values)
                {
                    var key = getPropertyKey(typeof(T)).GetValue(value);
                    ResDic.Add(key, value);
                }
                return ResDic;
            }
        }

        public T? Single()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                if (_isCount)
                    return Conexion.Query<T>(BuildCountQuery(), _dynamicParameters).FirstOrDefault();
                else
                    return Conexion.Query<T>(BuildSelectQuery(), _dynamicParameters).FirstOrDefault();
            }

        }
        private string BuildSelectQuery()
        {

            if (!_selectColumns.Any())
            {
                bool mapColum;
                IColumnNameResolver _columnNameResolver = new ColumnNameResolver();
                _selectColumns = new string[] { _columnNameResolver.ResolveKeyColumnName(getPropertyKey(typeof(T)), _dataBase.Encapsulation, out mapColum) };
                getColumnsNames(typeof(T)).ForEach(x => _selectColumns.Append(x));
            }
            
            string columns = string.Join(", ", _selectColumns);

            var query = $"SELECT {columns} FROM {_tableName} {_whereClause} {_orderByClause}";
            return query;
        }

        private string BuildCountQuery()
        {
            string ColumnCount = _selectColumns.Any() ? _selectColumns[0] : "*";
            var query = $"SELECT COUNT({ColumnCount}) FROM {_tableName} {_whereClause}";
            return query;
        }


        private static PropertyInfo getPropertyKey(Type type)
        {
            var tp = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)).First();
            return tp !=null ? tp : type.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)).First();
        }

        private string getColumnName(PropertyInfo propertyInfo, out bool isMapColumn)
        {
            isMapColumn = false;
            IColumnNameResolver _columnNameResolver = new ColumnNameResolver();
            //string columnName, key = string.Format("{0}.{1}", propertyInfo.DeclaringType, propertyInfo.Name);
            return _columnNameResolver.ResolveColumnName(propertyInfo, _dataBase.Encapsulation, out isMapColumn);
        }

        private List<string> getColumnsNames(Type type)
        {
            List<PropertyInfo> properties = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(ColumnAttribute).Name)).ToList();
            List<string> columnNames = new List<string>();
            foreach (PropertyInfo propertyInfo in properties)
            {
                string columnName = getColumnName(propertyInfo, out bool isMapColumn);
                //if (!isMapColumn)
                    columnNames.Add(columnName);
            }
            return columnNames;
        }
    }
}
