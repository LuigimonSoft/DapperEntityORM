using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperEntityORM.Resolvers.Interfaces;
using DapperEntityORM.Attributes;

namespace DapperEntityORM.Resolvers
{
    public class TableNameResolver: ITableNameResolver
    {
        /// <summary>
        ///  Resolves the table name from the type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="encapsulation"></param>
        /// <returns></returns>
        public virtual string ResolveTableName(Type type, string encapsulation)
        {
            var tableName = Encapsulate(type.Name, encapsulation);

            var tableattr = type.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name) as dynamic;
            if (tableattr != null)
            {
                tableName = Encapsulate(tableattr.Name, encapsulation);
                try
                {
                    if (!String.IsNullOrEmpty(tableattr.Schema))
                    {
                        string schemaName = Encapsulate(tableattr.Schema, encapsulation);
                        tableName = String.Format("{0}.{1}", schemaName, tableName);
                    }
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                {
                    //Schema doesn't exist on this attribute.
                }
            }

            return tableName;
        }

        /// <summary>
        /// Encapsulates the database word with the encapsulation string.
        /// </summary>
        /// <param name="databaseword"></param>
        /// <param name="_encapsulation"></param>
        /// <returns></returns>
        private static string Encapsulate(string databaseword, string _encapsulation)
        {
            return string.Format(_encapsulation, databaseword);
        }
    }
}
