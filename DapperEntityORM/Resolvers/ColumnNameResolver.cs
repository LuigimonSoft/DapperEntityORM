using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DapperEntityORM.Resolvers.Interfaces;
using DapperEntityORM.Attributes;

namespace DapperEntityORM.Resolvers
{
    public class ColumnNameResolver: IColumnNameResolver
    {
        /// <summary>
        /// Resolves the column name for the property
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="encapsulation"></param>
        /// <param name="MapColumn"></param>
        /// <returns></returns>
        public virtual string ResolveColumnName(PropertyInfo propertyInfo, string encapsulation, out bool MapColumn)
        {
            MapColumn = false;
            var columnName = Encapsulate(propertyInfo.Name, encapsulation);

            var columnattr = propertyInfo.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) as dynamic;
            if (columnattr != null)
            {
                if (!String.IsNullOrEmpty(columnattr.ColumnName))
                {
                    MapColumn = true;
                    columnName = Encapsulate(columnattr.ColumnName, encapsulation);
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Trace.WriteLine(String.Format("Column name for type overridden from {0} to {1}", propertyInfo.Name, columnName));
                }
            }
            return columnName;
        }

        public virtual string ResolveKeyColumnName(PropertyInfo propertyInfo, string encapsulation, out bool MapColumn)
        {
            MapColumn = false;
            var columnName = Encapsulate(propertyInfo.Name, encapsulation);

            var columnattr = propertyInfo.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(KeyAttribute).Name) as dynamic;
            if (columnattr != null)
            {
                if (!String.IsNullOrEmpty(columnattr.Name))
                {
                    MapColumn = true;
                    columnName = Encapsulate(columnattr.Name, encapsulation);
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Trace.WriteLine(String.Format("Key column name for type overridden from {0} to {1}", propertyInfo.Name, columnName));
                }
            }
            return columnName;
        }

        /// <summary>
        /// Encapsulates the column name with the encapsulation string
        /// </summary>
        /// <param name="databaseword"></param>
        /// <param name="_encapsulation"></param>
        /// <returns></returns>
        private static string Encapsulate(string databaseword, string encapsulation)
        {
            return string.Format(encapsulation, databaseword);
        }
    }
}
