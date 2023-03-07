using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DapperEntityORM.Resolvers;
using DapperEntityORM.Resolvers.Interfaces;
using DapperEntityORM.Attributes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;

namespace DapperEntityORM
{
    public class Entity<T> : INotifyPropertyChanged
    {
        private DataBase? _database;
        private ITableNameResolver _tableNameResolver = new TableNameResolver();
        private IColumnNameResolver _columnNameResolver = new ColumnNameResolver();

        private string _tableName;
        private string _primaryKeyColumn;
        private List<string> _columns;
        private List<PropertyInfo> _columnsModified;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null)
            {
                PropertyInfo? property = typeof(T).GetProperty(e.PropertyName);
                if(property != null)
                {
                    if (!_columnsModified.Contains(property))
                        _columnsModified.Add(property);
                }
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
        }

        private void init()
        {
            _tableName = getTableName(this);
            _primaryKeyColumn = getKeyColumnName(this);
            _columns = getColumnsNames();
            _columnsModified = new List<PropertyInfo>();
        }

        public void SetDataBase(DataBase dataBase)
        {
            _database = dataBase;
            init();
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
            string idName = _columnNameResolver.ResolveKeyColumnName(GetKeyProperty(this.GetType()), _database.Encapsulation,out mapColum);
            object? idValue = GetKeyProperty(GetType()).GetValue(this);
            var EntityLoaded = Select(_database).Where($"{idName}=@{idName.Replace("[","").Replace("]","")}",new List<object>() { idValue }).Single();
            if (EntityLoaded != null)
            {
                foreach (var property in EntityLoaded.GetType().GetProperties())
                {
                    var value = property.GetValue(EntityLoaded);
                    property.SetValue(this, value);
                }
                _columnsModified.Clear();
                return true;
            }
           
            return false;
        }

        public bool Update() 
        {
            _columnsModified.Clear();
            return false; 
        }

        public bool Delete()
        {
            _columnsModified.Clear();
            return false; 
        }

        public bool Insert()
        {
            _columnsModified.Clear();
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
