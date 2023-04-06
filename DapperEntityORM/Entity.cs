using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DapperEntityORM.Resolvers;
using DapperEntityORM.Resolvers.Interfaces;
using DapperEntityORM.Attributes;
using System.ComponentModel;
using PropertyChanged;
using System.Diagnostics;
using System.Data;
using Dapper;

namespace DapperEntityORM
{
    [AddINotifyPropertyChangedInterface]
    public class Entity<T> : INotifyPropertyChanged
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DataBase? _database;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ITableNameResolver _tableNameResolver = new TableNameResolver();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IColumnNameResolver _columnNameResolver = new ColumnNameResolver();

        private bool isNew { set; get; } = true; 
        private string _tableName;
        private string _primaryKeyColumn;
        private List<string> _columns;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<PropertyInfo> _columnsModified;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null)
            {
                PropertyInfo? property = typeof(T).GetProperty(e.PropertyName);
                if (property != null && property.CustomAttributes.Where(x => x.AttributeType == typeof(ColumnAttribute)).Count()>0)
                {
                    if (!_columnsModified.Contains(property))
                        _columnsModified.Add(property);
                }
                if (property != null && property.CustomAttributes.Where(x => x.AttributeType == typeof(KeyAttribute)).Count() > 0)
                    isNew = false;
            }

            PropertyChanged?.Invoke(this, e);
        }

        public Entity()
        {
            _columnsModified = new List<PropertyInfo>();
        }
        public Entity(DataBase dataBase)
        {
            _database = dataBase;
            init();
            _columnsModified = new List<PropertyInfo>();
            isNew = true;
        }

        private void init()
        {
            _tableName = getTableName(this);
            _primaryKeyColumn = getKeyColumnName(this);
            _columns = getColumnsNames();
            
        }

        public void SetDataBase(DataBase dataBase)
        {
            _database = dataBase;
            init();
        }

        public void SetisNoNew()
        {             isNew = false;
        }

        #region Get Attributes
        private static IEnumerable<PropertyInfo> getPropertyKey(Type type)
        {
            var tp = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)).ToList();
            return tp.Any() ? tp : type.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<PropertyInfo> getPropertyKey(object entity)
        {
            var type = entity.GetType();
            return getPropertyKey(type);
        }
        private static string getKeyColumnName(object entity)
        {
            var type = entity.GetType();
            return getPropertyKey(type).First().Name;
        }

        private string getTableName(Type type)
        {
            return _tableNameResolver.ResolveTableName(type, _database.Encapsulation);
        }

        private string getTableName(object entity)
        {
            var type = entity.GetType();
            return getTableName(type);
        }

        private string getColumnName(PropertyInfo propertyInfo, out bool isMapColumn)
        {
            isMapColumn = false;
            //string columnName, key = string.Format("{0}.{1}", propertyInfo.DeclaringType, propertyInfo.Name);
            return _columnNameResolver.ResolveColumnName(propertyInfo, _database.Encapsulation, out isMapColumn);
        }

        private List<string> getColumnsNames()
        {
            var type = this.GetType();
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
        private List<PropertyInfo> GetPropertiesRelation(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(RelationAttribute).Name)).ToList();
        }
        private PropertyInfo? GetKeyProperty(Type type)
        {
            var tp = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)).ToList();
            return tp.Any() ? tp.FirstOrDefault() : type.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }


        #endregion

        #region CRUD
        public bool Load()
        {
            bool mapColum;
            string idName = _columnNameResolver.ResolveKeyColumnName(GetKeyProperty(this.GetType()), _database.Encapsulation, out mapColum);
            object? idValue = GetKeyProperty(GetType()).GetValue(this);
            var EntityLoaded = Select(_database).Where($"{idName}=@{idName.Replace("[", "").Replace("]", "")}", new List<object>() { idValue }).Single();
            if (EntityLoaded != null)
            {
                foreach (var property in EntityLoaded.GetType().GetProperties())
                {
                    var value = property.GetValue(EntityLoaded);
                    property.SetValue(this, value);
                }
                _columnsModified.Clear();
                isNew = false;
                return true;
            }

            return false;
        }

        public bool Load(object primaryKeyValue)
        {
            GetKeyProperty(GetType()).SetValue(this, primaryKeyValue);
            if (_database == null)
                throw new Exception("The entity must have a database");
            if (string.IsNullOrEmpty(_tableName))
                init();
            return Load();
        }

        public bool Load(object primaryKeyValue, DataBase database)
        {
            this._database = database;
            if(string.IsNullOrEmpty(_tableName))
                init();
            return Load(primaryKeyValue);
        }

        public bool Update()
        {
            PropertyInfo? propertyKey = (PropertyInfo?)GetKeyProperty(GetType());
            if (propertyKey == null)
                throw new Exception("The entity must have a key property");
            if(_database==null)
                throw new Exception("The entity must have a database");

            object? idValue = propertyKey.GetValue(this);
            if (_columnsModified.Count > 0 && !isNew && idValue!=null)
            {
                int resId = -1;
                string sql = $"UPDATE {_tableName} SET ";
                string setSQL = string.Empty;
                foreach (var property in _columnsModified)
                {
                    string columnName = getColumnName(property, out bool isMapColumn);
                    if (setSQL != string.Empty)
                        setSQL += ",";
                    setSQL += $" {columnName}=@{property.Name}";
                }
                
                string idName = _columnNameResolver.ResolveKeyColumnName(propertyKey, _database.Encapsulation, out bool mapColum);
                string whereSQL = $" WHERE { idName}=@{ idName.Replace("[", "").Replace("]", "")}";
                using (IDbConnection Conexion = _database.Connection)
                {
                    resId = Conexion.Execute(sql + setSQL + whereSQL, this);
                    _columnsModified.Clear();
                    isNew = false;
                    if(resId > 0)
                    {
                        foreach (PropertyInfo propertyrelation in GetPropertiesRelation(this.GetType()))
                        {
                            var propertyValue = propertyrelation.GetValue(this, null) as dynamic;
                            if (propertyValue != null)
                            {
                                if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    for (int x = 0; x < propertyValue.Count; x++)
                                    {
                                        propertyValue[x].GetType().GetMethod("SetDataBase").Invoke(propertyValue[x], new object[] { _database });
                                        propertyValue[x].GetType().GetMethod("Update").Invoke(propertyValue[x], null);
                                    }
                                    
                                }
                                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                {
                                    Dictionary<int, string> pv = new Dictionary<int, string>();
                                    foreach (var pkey in propertyValue.Keys)
                                    {
                                        propertyValue[pkey].GetType().GetMethod("SetDataBase").Invoke(propertyValue[pkey], new object[] { _database });
                                        propertyValue[pkey].GetType().GetMethod("Update").Invoke(propertyValue[pkey], null);
                                    }
                                }
                                else
                                {
                                    propertyValue.GetType().GetMethod("SetDataBase").Invoke(propertyValue, new object[] { _database });
                                    propertyValue.GetType().GetMethod("Update").Invoke(propertyValue, null);
                                }

                               
                            }
                        }
                        return true;
                    }
                    else 
                        return false;
                }
                
            }
            else if(isNew)
                return Insert();
            
            
            return false;
        }

        public bool Delete()
        {
            PropertyInfo? propertyKey = (PropertyInfo?)GetKeyProperty(GetType());
            if (propertyKey == null)
                throw new Exception("The entity must have a key property");
            if (_database == null)
                throw new Exception("The entity must have a database");

            object? idValue = propertyKey.GetValue(this);
            if (idValue == null)
                throw new Exception("The entity must have a key value");
            if (idValue == DBNull.Value)
                throw new Exception("The entity must have a key value");
            if (idValue is int)
            {
                if((int)idValue  == 0) 
                    throw new Exception("The entity must have a key value");
            }
            if (!isNew && idValue != null)
            {
                int resId = -1;
                string sql = $"DELETE FROM {_tableName} ";
                string idName = _columnNameResolver.ResolveKeyColumnName(propertyKey, _database.Encapsulation, out bool mapColum);
                string whereSQL = $" WHERE { idName}=@{ idName.Replace("[", "").Replace("]", "")}";
                using (IDbConnection Conexion = _database.Connection)
                {
                    resId = Conexion.Execute(sql + whereSQL, this);
                    _columnsModified.Clear();
                    isNew = false;
                }

            }
            
            _columnsModified.Clear();
            return false;
        }

        public bool Insert(DataBase database = null)
        {
            if (database != null)
            {
                _database = database;
                _tableName = getTableName(this);
                _primaryKeyColumn = getKeyColumnName(this);
                _columns = getColumnsNames();
            }
            if (_database == null)
                throw new Exception("The entity must have a database");

            string InserColumnsSQL = string.Empty;
            string InsertValuesSQL = string.Empty;
            string sqlScopeIdentity = string.Empty;
            bool getIdGen = false;
            int resId = -1;

            PropertyInfo? propertyKey = (PropertyInfo?)GetKeyProperty(GetType());

            if (_columnsModified.Count > 0 && isNew)
            {
                foreach (var property in _columnsModified)
                {
                    string columnName = getColumnName(property, out bool isMapColumn);
                    if (InserColumnsSQL != string.Empty)
                    {
                        InserColumnsSQL += ",";
                        InsertValuesSQL += ",";
                    }
                    InserColumnsSQL += $" {columnName}";
                    InsertValuesSQL += $" @{property.Name}";
                }
                if(propertyKey!=null)
                {
                    KeyAttribute keyattribute = (KeyAttribute)propertyKey.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(KeyAttribute)).Single();
                    object? idValue = propertyKey.GetValue(this);
                    
                    if (idValue == null)
                    {
                        if (keyattribute.IsIdentity && (propertyKey.PropertyType == typeof(int?) || propertyKey.PropertyType == typeof(Int32?) || propertyKey.PropertyType == typeof(Int16?)))
                            getIdGen = true;  
                    }
                    else
                    {
                        if (keyattribute.IsIdentity && (propertyKey.PropertyType == typeof(int) || propertyKey.PropertyType == typeof(Int32) || propertyKey.PropertyType == typeof(Int16)) && (int)idValue == 0)
                            getIdGen = true;
                    }

                    if(getIdGen)
                    {
                        sqlScopeIdentity = "; SELECT SCOPE_IDENTITY() as Id";
                    }
                    else
                    {
                        string columnName = getColumnName(propertyKey, out bool isMapColumn);
                        InserColumnsSQL += $", {columnName}";
                        InsertValuesSQL += $", @{propertyKey.Name}";
                    }
                    
                }
                string sql = $"INSERT INTO {_tableName} ({InserColumnsSQL}) VALUES ({InsertValuesSQL}){sqlScopeIdentity}";

                using (IDbConnection Conexion = _database.Connection)
                {
                    if (getIdGen)
                    {
                        resId = Conexion.Query<int>(sql, this).Single();
                        propertyKey.SetValue(this, resId);

                        foreach (PropertyInfo propertyrelation in GetPropertiesRelation(this.GetType()))
                        {
                            var propertyValue = propertyrelation.GetValue(this, null) as dynamic;
                            RelationAttribute attributerelation = propertyrelation.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(RelationAttribute)).Single() as RelationAttribute;
                            string foreignKeyName = attributerelation.ForeignKey;
                            if (propertyValue != null)
                            {
                                if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    for (int x = 0; x < propertyValue.Count; x++)
                                    {
                                        propertyValue[x].GetType().GetProperty(foreignKeyName).SetValue(propertyValue[x], resId);
                                        {
                                            propertyValue[x].GetType().GetMethod("SetDataBase").Invoke(propertyValue[x], new object[] { _database });
                                            propertyValue[x].GetType().GetMethod("Update").Invoke(propertyValue[x], null);
                                        }
                                    }
                                }
                                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                {
                                    Dictionary<int, string> pv = new Dictionary<int, string>();
                                    foreach (var pkey in propertyValue.Keys)
                                    {
                                        propertyValue[pkey].GetType().GetMethod("SetDataBase").Invoke(propertyValue[pkey], new object[] { _database });
                                        propertyValue[pkey].GetType().GetMethod("Update").Invoke(propertyValue[pkey], null);
                                    }
                                }
                                else
                                {
                                    propertyValue.GetType().GetMethod("SetDataBase").Invoke(propertyValue, new object[] { _database });
                                    propertyValue.GetType().GetMethod("Update").Invoke(propertyValue, null);
                                }
                            }
                        }
                    }
                    else
                        resId = Conexion.Execute(sql, this);
                    _columnsModified.Clear();
                    return resId > 0;
                }
            }
            else
                return false;
        }

        #endregion

        #region Static
        public static QueryBuilder<T> Select(DataBase dataBase)
        {
            return new QueryBuilder<T>(dataBase).Select();
        }


        #endregion
    }
}
