﻿using System;
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
using DapperEntityORM.Statics;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DapperEntityORM
{
    public class Entity<T> : EntityStaticMethods<T>, INotifyPropertyChanged
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

        public bool IsValid(out List<string> Errors,bool throwErrors=false)
        {
            bool result = true;
            string ErrorMessage=string.Empty;
            Errors = new List<string>();
            List<string> ErrorRelations = new List<string>();

            foreach (var property in this.GetType().GetProperties())
            {
                if (property.CustomAttributes.Where(x => x.AttributeType == typeof(ColumnAttribute)).Count() > 0)
                {
                    var column = property.GetCustomAttribute<ColumnAttribute>();
                    if (column != null)
                    {
                        if (property.GetValue(this) != null)
                        {
                            if (column.MaxLength > 0 && property.GetValue(this).ToString().Trim().Length > column.MaxLength)
                            {
                                result = false;
                                if (column.ErrorMaximunMessage != null)
                                    ErrorMessage = column.ErrorMaximunMessage;
                                else
                                    ErrorMessage = string.Format("The value of the column {0} is greater than the maximum length of {1}", property.Name, column.MaxLength);

                                setError(Errors, ErrorMessage, throwErrors);
                            }
                            if (column.MinLength > 0 && property.GetValue(this).ToString().Trim().Length < column.MinLength)
                            {
                                result = false;
                                if (column.ErrorMinimumMessage != null)
                                    ErrorMessage = column.ErrorMinimumMessage;
                                else
                                    ErrorMessage = string.Format("The value of the column {0} is less than the minimum length of {1}", property.Name, column.MinLength);

                                setError(Errors, ErrorMessage, throwErrors);
                            }
                            if (!column.AllowEmpty && property.GetValue(this).ToString().Trim().Length == 0)
                            {
                                result = false;
                                if (column.ErrorEmptyMessage != null)
                                    ErrorMessage = column.ErrorEmptyMessage;
                                else
                                    ErrorMessage = string.Format("The value of the column {0} is empty", property.Name);

                                setError(Errors, ErrorMessage, throwErrors);
                            }
                            if(column.Required && property.GetValue(this).ToString().Trim().Length == 0)
                            {
                                result = false;
                                if (column.ErrorEmptyMessage != null)
                                    ErrorMessage = column.ErrorEmptyMessage;
                                else
                                    ErrorMessage = string.Format("The value of the column {0} is required", property.Name);

                                setError(Errors, ErrorMessage, throwErrors);
                            }
                            if (column.RegExPattern != null && !System.Text.RegularExpressions.Regex.IsMatch(property.GetValue(this).ToString(), column.RegExPattern))
                            {
                                result = false;
                                if (column.ErrorRegExMessage != null)
                                    ErrorMessage = column.ErrorRegExMessage;
                                else
                                    ErrorMessage = string.Format("The value of the column {0} does not match the regular expression pattern {1}", property.Name, column.RegExPattern);

                                setError(Errors, ErrorMessage, throwErrors);
                            }
                        }
                        else
                        {
                            if (column.AllowNull == false)
                            {
                                result = false;
                                if (column.ErrorNullMessage != null)
                                    ErrorMessage = column.ErrorNullMessage;
                                else
                                    ErrorMessage = string.Format("The value of the column {0} is null", property.Name);

                                setError(Errors, ErrorMessage, throwErrors);
                            }
                        }
                    }
                }
                
                if (property.CustomAttributes.Where(x => x.AttributeType == typeof(RelationAttribute)).Count() > 0)
                {
                    var relation = property.GetCustomAttribute<RelationAttribute>();
                    if (!relation.IgnoreInInsert || !relation.IgnoreInUpdate)
                    {
                        if (property.GetValue(this) != null)
                        {
                            var propertyValue = property.GetValue(this) as dynamic;
                            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                            {
                                for (int x = 0; x < propertyValue.Count; x++)
                                {
                                    object[] parameters = new object[] { new List<string>(), throwErrors };
                                    ((MethodInfo[])propertyValue[x].GetType().GetMethods()).Where(m => m.Name == "IsValid" && m.ToString() == "Boolean IsValid(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue[x], parameters);
                                    addError(ErrorRelations, (List<string>)parameters[0]);
                                }

                            }
                            else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                            {
                                Dictionary<int, string> pv = new Dictionary<int, string>();
                                foreach (var pkey in propertyValue.Keys)
                                {
                                    object[] parameters = new object[] { new List<string>(), throwErrors };
                                    ((MethodInfo[])propertyValue[pkey].GetType().GetMethods()).Where(m => m.Name == "Save" && m.ToString() == "Boolean Save(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue[pkey], parameters);
                                    addError(ErrorRelations, (List<string>)parameters[0]);
                                }
                            }
                            else
                            {
                                object[] parameters = new object[] { new List<string>(), throwErrors };
                                ((MethodInfo[])propertyValue.GetType().GetMethods()).Where(m => m.Name == "Save" && m.ToString() == "Boolean Save(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue, parameters);
                                addError(ErrorRelations, (List<string>)parameters[0]);
                            }
                        }
                    }
                }
            }
            addError(Errors, ErrorRelations);

            return result && ErrorRelations.Count==0;
        }

        private void setError(List<string> Errors, string ErrorMessage ,bool throwErrors = false)
        {
            if (throwErrors)
                throw new Exception(ErrorMessage);
            else
                Errors.Add(ErrorMessage);
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
            return type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType() == typeof(RelationAttribute))).ToList();
        }

        private List<PropertyInfo> GetPropertiesRelationUpdatable(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType() == typeof(RelationAttribute) && !((RelationAttribute)attr).IgnoreInUpdate)).ToList();
        }

        private List<PropertyInfo> GetPropertiesRelationInsertable(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType() == typeof(RelationAttribute) && !((RelationAttribute)attr).IgnoreInInsert)).ToList();
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

        public async Task<bool> LoadAsync()
        {
            bool mapColum;
            string idName = _columnNameResolver.ResolveKeyColumnName(GetKeyProperty(this.GetType()), _database.Encapsulation, out mapColum);
            object? idValue = GetKeyProperty(GetType()).GetValue(this);
            var EntityLoaded = await Select(_database).Where($"{idName}=@{idName.Replace("[", "").Replace("]", "")}", new List<object>() { idValue }).SingleAsync();
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
        public async Task<bool> LoadAsync(object primaryKeyValue)
        {
            GetKeyProperty(GetType()).SetValue(this, primaryKeyValue);
            if (_database == null)
                throw new Exception("The entity must have a database");
            if (string.IsNullOrEmpty(_tableName))
                init();
            return await LoadAsync();
        }
        public async Task<bool> LoadAsync(object primaryKeyValue, DataBase database)
        {
            this._database = database;
            if (string.IsNullOrEmpty(_tableName))
                init();
            return await LoadAsync(primaryKeyValue);
        }

        public bool Save()
        {
            List<string> Errors = new List<string>();
            return Save(out Errors, true);
        }

        public bool Save(out List<string> Errors, bool throwErrors = false)
        {
            Errors = new List<string>();
            PropertyInfo? propertyKey = (PropertyInfo?)GetKeyProperty(GetType());
            if (propertyKey == null)
            {
                setError(Errors, "The entity must have a key property", throwErrors);
                return false;
            }

            object? idValue = propertyKey.GetValue(this);
            if (!isNew && idValue != null)
            {
                if (idValue.GetType() == typeof(int))
                {
                    if ((int)idValue == 0)
                        return Insert(out Errors, throwErrors);
                }
                else if (idValue.GetType() == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)idValue))
                        return Insert(out Errors, throwErrors);
                }
                else if (idValue.GetType() == typeof(Guid))
                {
                    if ((Guid)idValue == Guid.Empty)
                        return Insert(out Errors, throwErrors);
                }
                return Update(out Errors, throwErrors);
            }
            else
                return Insert(out Errors, throwErrors);
        }
        public async Task<bool> SaveAsync()
        {
            List<string> Errors = new List<string>();
            return Save(out Errors, true);
        }
        public async Task<(bool success, List<string> errors)> SaveAsync(bool throwErrors = false)
        {
            List<string> Errors = new List<string>();
            PropertyInfo? propertyKey = (PropertyInfo?)GetKeyProperty(GetType());
            if (propertyKey == null)
            {
                setError(Errors, "The entity must have a key property", throwErrors);
                return (false, Errors);
            }

            object? idValue = propertyKey.GetValue(this);
            if (!isNew && idValue != null)
            {
                if (idValue.GetType() == typeof(int))
                {
                    if ((int)idValue == 0)
                    {
                        (bool success, List<string> errors) res = await InsertAsync(throwErrors);
                        return (res.success, res.errors);
                    }
                }
                else if (idValue.GetType() == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)idValue))
                    {
                        (bool success, List<string> errors) res = await InsertAsync(throwErrors);
                        return (res.success, res.errors);
                    }
                }
                else if (idValue.GetType() == typeof(Guid))
                {
                    if ((Guid)idValue == Guid.Empty)
                    {
                        (bool success, List<string> errors) res = await InsertAsync(throwErrors);
                        return (res.success, res.errors);
                    }
                }
                (bool success, List<string> errors) res2 = await UpdateAsync(throwErrors);
                return (res2.success, res2.errors);
            }
            else
            {
                (bool success, List<string> errors) res = await InsertAsync(throwErrors);
                return (res.success, res.errors);
            }
        }

        public bool Update()
        {
            List<string> Errors = new List<string>();
            return Update(out Errors, true);
        }
        public bool Update(out List<string> Errors, bool throwErrors=false)
        {
            Errors = new List<string>();

            PropertyInfo? propertyKey = (PropertyInfo?)GetKeyProperty(GetType());
            if (propertyKey == null)
                setError(Errors, "The entity must have a key property", throwErrors);
            if(_database==null)
                setError(Errors, "The entity must have a database", throwErrors);

            IsValid(out Errors, throwErrors);
              
            object? idValue = propertyKey.GetValue(this);
            if (idValue == null)
                setError(Errors, "The key value is null", throwErrors);
            else if(idValue.GetType()==typeof(int))
                if((int)idValue==0)
                    setError(Errors, "The key value is 0", throwErrors);
            else if (idValue.GetType() == typeof(string))
                if (string.IsNullOrEmpty((string)idValue))
                    setError(Errors, "The key value is empty", throwErrors);
            else if (idValue.GetType() == typeof(Guid))
                if ((Guid)idValue == Guid.Empty)
                    setError(Errors, "The key value is empty", throwErrors);

            if (Errors.Count > 0)
                return false;

            if (_columnsModified.Count > 0)
            {
                int resId = -1;
                string sql = $"UPDATE {_tableName} SET ";
                string setSQL = string.Empty;
                foreach (var property in _columnsModified)
                {
                    var columnattr = property.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType() == typeof(ColumnAttribute)) as ColumnAttribute;
                    string columnName = getColumnName(property, out bool isMapColumn);
                    if (isMapColumn && !columnattr.IgnoreInUpdate)
                    {
                        if (setSQL != string.Empty)
                            setSQL += ",";
                        setSQL += $" {columnName}=@{property.Name}";
                    }
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
                        foreach (PropertyInfo propertyrelation in GetPropertiesRelationUpdatable(this.GetType()))
                        {
                            
                            var propertyValue = propertyrelation.GetValue(this, null) as dynamic;
                            if (propertyValue != null)
                            {
                                
                                if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    for (int x = 0; x < propertyValue.Count; x++)
                                    {
                                        propertyValue[x].GetType().GetMethod("SetDataBase").Invoke(propertyValue[x], new object[] { _database });
                                        object[] parameters = new object[] { new List<string>(), throwErrors };
                                        ((MethodInfo[])propertyValue[x].GetType().GetMethods()).Where(m => m.Name == "Save" && m.ToString() == "Boolean Save(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue[x], parameters);
                                        addError(Errors, (List<string>)parameters[0]);
                                    }
                                    
                                }
                                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                {
                                    Dictionary<int, string> pv = new Dictionary<int, string>();
                                    foreach (var pkey in propertyValue.Keys)
                                    {
                                        propertyValue[pkey].GetType().GetMethod("SetDataBase").Invoke(propertyValue[pkey], new object[] { _database });
                                        object[] parameters = new object[] { new List<string>(), throwErrors };
                                        ((MethodInfo[])propertyValue[pkey].GetType().GetMethods()).Where(m => m.Name == "Save" && m.ToString() == "Boolean Save(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue[pkey], parameters);
                                        addError(Errors, (List<string>)parameters[0]);
                                    }
                                }
                                else
                                {
                                    propertyValue.GetType().GetMethod("SetDataBase").Invoke(propertyValue, new object[] { _database });
                                    object[] parameters = new object[] { new List<string>(), throwErrors };
                                    ((MethodInfo[])propertyValue.GetType().GetMethods()).Where(m => m.Name == "Save" && m.ToString() == "Boolean Save(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue, parameters);
                                    addError(Errors, (List<string>)parameters[0]);
                                }

                               
                            }
                        }
                        return true;
                    }
                    else 
                        return false;
                }
                
            }
          
            return false;
        }
        public async Task<bool> UpdateAsync()
        {
            List<string> Errors = new List<string>();
            var result = await UpdateAsync(true);
            return result.success;
        }
        public async Task<(bool success, List<string> errors)> UpdateAsync(bool throwErrors = false)
        {
            List<string> Errors = new List<string>();

            PropertyInfo? propertyKey = (PropertyInfo?)GetKeyProperty(GetType());
            if (propertyKey == null)
                setError(Errors, "The entity must have a key property", throwErrors);
            if (_database == null)
                setError(Errors, "The entity must have a database", throwErrors);

            IsValid(out Errors, throwErrors);

            object? idValue = propertyKey.GetValue(this);
            if (idValue == null)
                setError(Errors, "The key value is null", throwErrors);
            else if (idValue.GetType() == typeof(int))
                if ((int)idValue == 0)
                    setError(Errors, "The key value is 0", throwErrors);
                else if (idValue.GetType() == typeof(string))
                    if (string.IsNullOrEmpty((string)idValue))
                        setError(Errors, "The key value is empty", throwErrors);
                    else if (idValue.GetType() == typeof(Guid))
                        if ((Guid)idValue == Guid.Empty)
                            setError(Errors, "The key value is empty", throwErrors);

            if (Errors.Count > 0)
                return (false,Errors);

            if (_columnsModified.Count > 0)
            {
                int resId = -1;
                string sql = $"UPDATE {_tableName} SET ";
                string setSQL = string.Empty;
                foreach (var property in _columnsModified)
                {
                    var columnattr = property.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType() == typeof(ColumnAttribute)) as ColumnAttribute;
                    string columnName = getColumnName(property, out bool isMapColumn);
                    if (isMapColumn && !columnattr.IgnoreInUpdate)
                    {
                        if (setSQL != string.Empty)
                            setSQL += ",";
                        setSQL += $" {columnName}=@{property.Name}";
                    }
                }

                string idName = _columnNameResolver.ResolveKeyColumnName(propertyKey, _database.Encapsulation, out bool mapColum);
                string whereSQL = $" WHERE {idName}=@{idName.Replace("[", "").Replace("]", "")}";
                using (IDbConnection Conexion = _database.Connection)
                {
                    resId = await Conexion.ExecuteAsync(sql + setSQL + whereSQL, this);
                    _columnsModified.Clear();
                    isNew = false;
                    if (resId > 0)
                    {
                        foreach (PropertyInfo propertyrelation in GetPropertiesRelationUpdatable(this.GetType()))
                        {

                            var propertyValue = propertyrelation.GetValue(this, null) as dynamic;
                            if (propertyValue != null)
                            {

                                if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    for (int x = 0; x < propertyValue.Count; x++)
                                    {
                                        propertyValue[x].GetType().GetMethod("SetDataBase").Invoke(propertyValue[x], new object[] { _database });
                                        object[] parameters = new object[] { new List<string>(), throwErrors };
                                        (bool success, List<string> errors)  res = await ((MethodInfo[])propertyValue[x].GetType().GetMethods()).Where(m => m.Name == "SaveAsync" && m.ToString() == "Boolean SaveAsync(Boolean)").FirstOrDefault()?.Invoke(propertyValue[x], parameters);
                                        addError(Errors, res.errors);
                                    }

                                }
                                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                {
                                    Dictionary<int, string> pv = new Dictionary<int, string>();
                                    foreach (var pkey in propertyValue.Keys)
                                    {
                                        propertyValue[pkey].GetType().GetMethod("SetDataBase").Invoke(propertyValue[pkey], new object[] { _database });
                                        object[] parameters = new object[] { new List<string>(), throwErrors };
                                        (bool success, List<string> errors) res = await ((MethodInfo[])propertyValue[pkey].GetType().GetMethods()).Where(m => m.Name == "SaveAsync" && m.ToString() == "Boolean SaveAsync(Boolean)").FirstOrDefault()?.Invoke(propertyValue[pkey], parameters);
                                        addError(Errors, res.errors);
                                    }
                                }
                                else
                                {
                                    propertyValue.GetType().GetMethod("SetDataBase").Invoke(propertyValue, new object[] { _database });
                                    object[] parameters = new object[] { new List<string>(), throwErrors };
                                    (bool success, List<string> errors) res = await ((MethodInfo[])propertyValue.GetType().GetMethods()).Where(m => m.Name == "SaveAsync" && m.ToString() == "Boolean SaveAsync(Boolean)").FirstOrDefault()?.Invoke(propertyValue, parameters);
                                    addError(Errors, res.errors);
                                }


                            }
                        }
                        return (true, Errors);
                    }
                    else
                        return (false, Errors);
                }

            }

            return (false, Errors);
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
            List<string> Errors = new List<string>();
            return Insert(out Errors, true, database);
        }
        public bool Insert(out List<string> Errors,bool throwErrors = false, DataBase database = null)
        {
            Errors = new List<string>();
            if (database != null)
            {
                _database = database;
                _tableName = getTableName(this);
                _primaryKeyColumn = getKeyColumnName(this);
                _columns = getColumnsNames();
            }
            if (_database == null)
                setError(Errors, "The entity must have a database", throwErrors);

            IsValid(out Errors, throwErrors);

            if (Errors.Count > 0)
                return false;

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
                    var columnattr = property.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType() == typeof(ColumnAttribute)) as ColumnAttribute;
                    string columnName = getColumnName(property, out bool isMapColumn);
                    if (isMapColumn && !columnattr.IgnoreInInsert)
                    {
                        if (InserColumnsSQL != string.Empty)
                        {
                            InserColumnsSQL += ",";
                            InsertValuesSQL += ",";
                        }
                        InserColumnsSQL += $" {columnName}";
                        InsertValuesSQL += $" @{property.Name}";
                    }
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

                        foreach (PropertyInfo propertyrelation in GetPropertiesRelationInsertable(this.GetType()))
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
                                            object[] parameters = new object[] { new List<string>(), throwErrors };
                                            ((MethodInfo[])propertyValue[x].GetType().GetMethods()).Where(m => m.Name == "Save" && m.ToString() == "Boolean Save(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue[x], parameters);
                                            addError(Errors, (List<string>)parameters[0]);
                                        }
                                    }
                                }
                                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                {
                                    Dictionary<int, string> pv = new Dictionary<int, string>();
                                    foreach (var pkey in propertyValue.Keys)
                                    {
                                        propertyValue[pkey].GetType().GetMethod("SetDataBase").Invoke(propertyValue[pkey], new object[] { _database });
                                        object[] parameters = new object[] { new List<string>(), throwErrors };
                                        ((MethodInfo[])propertyValue[pkey].GetType().GetMethods()).Where(m => m.Name == "Save" && m.ToString() == "Boolean Save(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue[pkey], parameters);
                                        addError(Errors, (List<string>)parameters[0]);
                                    }
                                }
                                else
                                {
                                    propertyValue.GetType().GetMethod("SetDataBase").Invoke(propertyValue, new object[] { _database });
                                    object[] parameters = new object[] { new List<string>(), throwErrors };
                                    ((MethodInfo[])propertyValue.GetType().GetMethods()).Where(m => m.Name == "Save" && m.ToString() == "Boolean Save(System.Collections.Generic.List`1[System.String] ByRef, Boolean)").FirstOrDefault()?.Invoke(propertyValue, parameters);
                                    addError(Errors, (List<string>)parameters[0]);
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

        public async Task<bool> InsertAsync(DataBase database = null)
        {
            List<string> Errors = new List<string>();
            return (await InsertAsync(true, database)).success;
        }
        public async Task<(bool success, List<string> errors)> InsertAsync(bool throwErrors = false, DataBase database = null)
        {
            List<string> Errors = new List<string>();
            if (database != null)
            {
                _database = database;
                _tableName = getTableName(this);
                _primaryKeyColumn = getKeyColumnName(this);
                _columns = getColumnsNames();
            }
            if (_database == null)
                setError(Errors, "The entity must have a database", throwErrors);

            IsValid(out Errors, throwErrors);

            if (Errors.Count > 0)
                return (false, Errors);

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
                    var columnattr = property.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType() == typeof(ColumnAttribute)) as ColumnAttribute;
                    string columnName = getColumnName(property, out bool isMapColumn);
                    if (isMapColumn && !columnattr.IgnoreInInsert)
                    {
                        if (InserColumnsSQL != string.Empty)
                        {
                            InserColumnsSQL += ",";
                            InsertValuesSQL += ",";
                        }
                        InserColumnsSQL += $" {columnName}";
                        InsertValuesSQL += $" @{property.Name}";
                    }
                }
                if (propertyKey != null)
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

                    if (getIdGen)
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
                        resId = (await Conexion.QueryAsync<int>(sql, this)).Single();
                        propertyKey.SetValue(this, resId);

                        foreach (PropertyInfo propertyrelation in GetPropertiesRelationInsertable(this.GetType()))
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
                                            object[] parameters = new object[] { new List<string>(), throwErrors };
                                            (bool success, List<string> errors) res = await ((MethodInfo[])propertyValue[x].GetType().GetMethods()).Where(m => m.Name == "SaveAsync" && m.ToString() == "Boolean SaveAsync(Boolean)").FirstOrDefault()?.Invoke(propertyValue[x], parameters);
                                            addError(Errors, res.errors);
                                        }
                                    }
                                }
                                else if (propertyrelation.PropertyType.IsGenericType && propertyrelation.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                {
                                    Dictionary<int, string> pv = new Dictionary<int, string>();
                                    foreach (var pkey in propertyValue.Keys)
                                    {
                                        propertyValue[pkey].GetType().GetMethod("SetDataBase").Invoke(propertyValue[pkey], new object[] { _database });
                                        object[] parameters = new object[] { new List<string>(), throwErrors };
                                        (bool success, List<string> errors)  res = await ((MethodInfo[])propertyValue[pkey].GetType().GetMethods()).Where(m => m.Name == "SaveAsync" && m.ToString() == "Boolean SaveAsync(Boolean)").FirstOrDefault()?.Invoke(propertyValue[pkey], parameters);
                                        addError(Errors, res.errors);
                                    }
                                }
                                else
                                {
                                    propertyValue.GetType().GetMethod("SetDataBase").Invoke(propertyValue, new object[] { _database });
                                    object[] parameters = new object[] { new List<string>(), throwErrors };
                                    (bool success, List<string> errors)  res = await ((MethodInfo[])propertyValue.GetType().GetMethods()).Where(m => m.Name == "SaveAsync" && m.ToString() == "Boolean SaveAsync(Boolean)").FirstOrDefault()?.Invoke(propertyValue, parameters);
                                    addError(Errors, res.errors);
                                }
                            }
                        }
                    }
                    else
                        resId = (await Conexion.ExecuteAsync(sql, this));
                    _columnsModified.Clear();
                    return (resId > 0, Errors);
                }
            }
            else
                return (false,Errors);
        }

        private void addError(List<string> groupErrors, List<string> errors)
        {             
            foreach (var error in errors)
            {
                groupErrors.Add(error);
            }
        }
        #endregion

    }
}
