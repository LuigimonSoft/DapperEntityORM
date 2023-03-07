using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DapperEntityORM.Resolvers.Interfaces
{
    public interface IColumnNameResolver
    {
        string ResolveColumnName(PropertyInfo propertyInfo, string _encapsulation, out bool MapColumn);
        string ResolveKeyColumnName(PropertyInfo propertyInfo, string _encapsulation, out bool MapColumn);
    }
}
