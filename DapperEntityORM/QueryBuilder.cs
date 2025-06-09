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
using static Dapper.SqlMapper;

namespace DapperEntityORM
{
    public class QueryBuilder<T>
    {
        private readonly string _tableName;
        private string _whereClause = string.Empty;
        private string _orderByClause = string.Empty;
        private string[] _selectColumns = Array.Empty<string>();
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
            return ToEnumerable().ToList();
        }

        public IEnumerable<T> ToEnumerable()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var queryrest = Conexion.Query<T>(BuildSelectQuery(), _dynamicParameters);
                for (int x = 0; x < queryrest.Count(); x++)
                {
                    Type TypeElement = queryrest.ElementAt(x).GetType();
                    TypeElement.GetMethod("SetDataBase").Invoke(queryrest.ElementAt(x), new[] { _dataBase });
                    TypeElement.GetMethod("SetisNoNew").Invoke(queryrest.ElementAt(x), null);
                    LoadRelation(queryrest.ElementAt(x));
                }
                return queryrest;
            }
        }

        public Dictionary<object,T> ToDictionary()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var values = Conexion.Query<T>(BuildSelectQuery(), _dynamicParameters).ToList();
                for (int x = 0; x < values.Count; x++)
                {
                    values[x].GetType().GetMethod("SetDataBase").Invoke(values[x], new[] { _dataBase });
                    values[x].GetType().GetMethod("SetisNoNew").Invoke(values[x], null);
                    LoadRelation(values[x]);
                }

                Dictionary<object, T> ResDic = new Dictionary<object, T>();
                foreach ( T value in values)
                {
                    var key = getPropertyKey(typeof(T)).GetValue(value);
                    ResDic.Add(key, value);
                }
                return ResDic;
            }
        }

        public Dictionary<string, T> ToDictionaryKeyString()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var values = ToList();
               
                Dictionary<string, T> ResDic = new Dictionary<string, T>();
                foreach (T value in values)
                {
                    var key = (string)getPropertyKey(typeof(T)).GetValue(value);
                    ResDic.Add(key, value);
                }
                return ResDic;
            }
        }

        public Dictionary<Guid, T> ToDictionaryKeyGuid()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var values = ToList();
                
                Dictionary<Guid, T> ResDic = new Dictionary<Guid, T>();
                foreach (T value in values)
                {
                    var key = (Guid)getPropertyKey(typeof(T)).GetValue(value);
                    ResDic.Add(key, value);
                }
                return ResDic;
            }
        }

        public Dictionary<int, T> ToDictionaryKeyInt()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var values = ToList();
                
                Dictionary<int, T> ResDic = new Dictionary<int, T>();
                foreach (T value in values)
                {
                    var key =(int) getPropertyKey(typeof(T)).GetValue(value);
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
                {
                    var queryrest = Conexion.Query<T>(BuildSelectQuery(), _dynamicParameters).FirstOrDefault();
                    queryrest.GetType().GetMethod("SetDataBase").Invoke(queryrest, new[] { _dataBase });
                    queryrest.GetType().GetMethod("SetisNoNew").Invoke(queryrest, null);
                    queryrest = LoadRelation(queryrest);
                    return queryrest;
                }
            }

        }

        #region Async
        public async Task<T?> SingleAsync()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                if (_isCount)
                    return await Conexion.QueryFirstOrDefaultAsync<T>(BuildCountQuery(), _dynamicParameters);
                else
                {
                    var queryrest = await Conexion.QueryFirstOrDefaultAsync<T>(BuildSelectQuery(), _dynamicParameters);
                    queryrest.GetType().GetMethod("SetDataBase").Invoke(queryrest, new[] { _dataBase });
                    queryrest.GetType().GetMethod("SetisNoNew").Invoke(queryrest, null);
                    queryrest = await LoadRelationAsync(queryrest);
                    return queryrest;
                }
            }

        }
        
        public async Task<IEnumerable<T>> ToEnumerableAsync()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var queryrest = await Conexion.QueryAsync<T>(BuildSelectQuery(), _dynamicParameters);
                for (int x = 0; x < queryrest.Count(); x++)
                {
                    Type TypeElement = queryrest.ElementAt(x).GetType();
                    TypeElement.GetMethod("SetDataBase").Invoke(queryrest.ElementAt(x), new[] { _dataBase });
                    TypeElement.GetMethod("SetisNoNew").Invoke(queryrest.ElementAt(x), null);
                    await LoadRelationAsync(queryrest.ElementAt(x));
                }
                return queryrest;
            }
        }

        public async Task<List<T>> ToListAsync()
        {
            return (await ToEnumerableAsync()).ToList();
        }
        public async Task<Dictionary<object, T>> ToDictionaryAsync()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var values = await Conexion.QueryAsync<T>(BuildSelectQuery(), _dynamicParameters);
                for (int x = 0; x < values.Count(); x++)
                {
                    values.ElementAt(x).GetType().GetMethod("SetDataBase").Invoke(values.ElementAt(x), new[] { _dataBase });
                    values.ElementAt(x).GetType().GetMethod("SetisNoNew").Invoke(values.ElementAt(x), null);
                    await LoadRelationAsync(values.ElementAt(x));
                }

                Dictionary<object, T> ResDic = new Dictionary<object, T>();
                foreach (T value in values)
                {
                    var key = getPropertyKey(typeof(T)).GetValue(value);
                    ResDic.Add(key, value);
                }
                return ResDic;
            }
        }

        public async Task<Dictionary<string, T>> ToDictionaryKeyStringAsync()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var values = await ToListAsync();

                Dictionary<string, T> ResDic = new Dictionary<string, T>();
                foreach (T value in values)
                {
                    var key = (string)getPropertyKey(typeof(T)).GetValue(value);
                    ResDic.Add(key, value);
                }
                return ResDic;
            }
        }

        public async Task<Dictionary<Guid,T>> ToDictionaryKeyGuidAsync()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var values = await ToListAsync();

                Dictionary<Guid, T> ResDic = new Dictionary<Guid, T>();
                foreach (T value in values)
                {
                    var key = (Guid)getPropertyKey(typeof(T)).GetValue(value);
                    ResDic.Add(key, value);
                }
                return ResDic;
            }
        }

        public async Task<Dictionary<int, T>> ToDictionaryKeyIntAsync()
        {
            using (IDbConnection Conexion = _dataBase.Connection)
            {
                var values = await ToListAsync();

                Dictionary<int, T> ResDic = new Dictionary<int, T>();
                foreach (T value in values)
                {
                    var key = (int)getPropertyKey(typeof(T)).GetValue(value);
                    ResDic.Add(key, value);
                }
                return ResDic;
            }
        }

        private async Task<T?> LoadRelationAsync(T? queryrest)
        {
            foreach (PropertyInfo propertyrelation in GetPropertiesRelation(typeof(T)))
            {
                var AttributeRelation = propertyrelation.GetCustomAttribute<RelationAttribute>();
                string relationColumn = String.IsNullOrEmpty(AttributeRelation.ForeignKey) ? getColumnName(propertyrelation, out bool isMap) : AttributeRelation.ForeignKey;
                Type typeArgument = null;
                if (propertyrelation.PropertyType.GenericTypeArguments.Count() > 1)
                    typeArgument = propertyrelation.PropertyType.GenericTypeArguments[1];
                else if (propertyrelation.PropertyType.GenericTypeArguments.Count() == 1)
                    typeArgument = propertyrelation.PropertyType.GenericTypeArguments.Single();
                else
                    typeArgument = propertyrelation.PropertyType;
                var resSelect = typeArgument.BaseType.BaseType.GetMethod("Select").Invoke(propertyrelation, new object[] { _dataBase }) as dynamic;
                var resWhere = resSelect.Where($"{relationColumn} = @{relationColumn}", new List<object> { getPropertyKey(typeof(T)).GetValue(queryrest) });
                if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    propertyrelation.SetValue(queryrest, await resWhere.ToListAsync());
                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Type TypeKey = propertyrelation.PropertyType.GenericTypeArguments[0];

                    if (TypeKey == typeof(string))
                        propertyrelation.SetValue(queryrest,await resWhere.ToDictionaryKeyStringAsync());
                    else if (TypeKey == typeof(Guid))
                        propertyrelation.SetValue(queryrest,await resWhere.ToDictionaryKeyGuidAsync());
                    else if (TypeKey == typeof(int) || TypeKey == typeof(Int32) || TypeKey == typeof(Int16))
                        propertyrelation.SetValue(queryrest,await resWhere.ToDictionaryKeyIntAsync());
                    else
                        propertyrelation.SetValue(queryrest, await resWhere.ToDictionaryAsync());
                }
                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    propertyrelation.SetValue(queryrest, await resWhere.ToEnumerableAsync());
                else
                    propertyrelation.SetValue(queryrest, await resWhere.SingleAsync());
            }
            return queryrest;
        }

        #endregion
        private T? LoadRelation(T? queryrest)
        {
            foreach (PropertyInfo propertyrelation in GetPropertiesRelation(typeof(T)))
            {
                var AttributeRelation = propertyrelation.GetCustomAttribute<RelationAttribute>();
                string relationColumn = String.IsNullOrEmpty(AttributeRelation.ForeignKey) ? getColumnName(propertyrelation, out bool isMap) : AttributeRelation.ForeignKey;
                Type typeArgument = null;
                if (propertyrelation.PropertyType.GenericTypeArguments.Count() > 1)
                    typeArgument = propertyrelation.PropertyType.GenericTypeArguments[1];
                else if (propertyrelation.PropertyType.GenericTypeArguments.Count() == 1)
                    typeArgument = propertyrelation.PropertyType.GenericTypeArguments.Single();
                else 
                    typeArgument = propertyrelation.PropertyType;
                var resSelect = typeArgument.BaseType.BaseType.GetMethod("Select").Invoke(propertyrelation, new object[] { _dataBase }) as dynamic;
                var resWhere = resSelect.Where($"{relationColumn} = @{relationColumn}", new List<object> { getPropertyKey(typeof(T)).GetValue(queryrest) });
                if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    propertyrelation.SetValue(queryrest, resWhere.ToList());
                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Type TypeKey = propertyrelation.PropertyType.GenericTypeArguments[0];
                    
                    if (TypeKey == typeof(string))
                        propertyrelation.SetValue(queryrest, resWhere.ToDictionaryKeyString());
                    else if (TypeKey == typeof(Guid))
                        propertyrelation.SetValue(queryrest, resWhere.ToDictionaryKeyGuid());
                    else if (TypeKey == typeof(int) || TypeKey == typeof(Int32) || TypeKey == typeof(Int16))
                        propertyrelation.SetValue(queryrest, resWhere.ToDictionaryKeyInt());
                    else
                        propertyrelation.SetValue(queryrest, resWhere.ToDictionary());
                }
                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    propertyrelation.SetValue(queryrest, resWhere.ToEnumerable());
                else
                    propertyrelation.SetValue(queryrest, resWhere.Single());
            }
            return queryrest;
        }
        private string BuildSelectQuery()
        {

            if (_selectColumns == null || !_selectColumns.Any())
            {
                bool mapColum;
                IColumnNameResolver _columnNameResolver = new ColumnNameResolver();
                _selectColumns = new string[] { _columnNameResolver.ResolveKeyColumnName(getPropertyKey(typeof(T)), _dataBase.Encapsulation, out mapColum) };
                getColumnsNames(typeof(T)).ForEach(x => _selectColumns = _selectColumns.Append(x).ToArray());
            }
            
            string columns = string.Join(", ", _selectColumns);

            var query = $"SELECT {columns} FROM {_tableName} {_whereClause} {_orderByClause}";
            return query;
        }

        private string BuildCountQuery()
        {
            string ColumnCount = (_selectColumns != null && _selectColumns.Any()) ? _selectColumns[0] : "*";
            var query = $"SELECT COUNT({ColumnCount}) FROM {_tableName} {_whereClause}";
            return query;
        }


        private static PropertyInfo getPropertyKey(Type type)
        {
            var tp = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)).First();
            return tp !=null ? tp : type.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)).First();
        }
        private static List<PropertyInfo> GetPropertiesRelation(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(RelationAttribute).Name)).ToList();
           
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
