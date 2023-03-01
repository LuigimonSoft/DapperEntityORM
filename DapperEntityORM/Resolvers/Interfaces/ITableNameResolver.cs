using System;

namespace DapperEntityORM.Resolvers.Interfaces
{
    public interface ITableNameResolver
    {
        string ResolveTableName(Type type, string _encapsulation);
    }
}
