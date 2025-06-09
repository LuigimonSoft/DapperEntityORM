using DapperEntityORM;
using DapperEntityORM.Enums;
using System.Collections.Generic;

namespace DapperEntityORM.Test.UnitTests
{
    public class DataBaseTests
    {
        public static IEnumerable<object[]> EncapsulationData => new List<object[]>
        {
            new object[]{ DataBaseTypes.SQLServer, "[{0}]" },
            new object[]{ DataBaseTypes.PostgreSQL, "\"{0}\"" },
            new object[]{ DataBaseTypes.SQLite, "\"{0}\"" },
            new object[]{ DataBaseTypes.MySQL, "`{0}`" }
        };

        [Theory]
        [MemberData(nameof(EncapsulationData))]
        public void GivenDatabaseType_WhenCreatingDatabase_ShouldSetEncapsulation_ThenMatchesExpected(DataBaseTypes type, string expected)
        {
            // Given
            var db = new DataBase(type, "conn");

            // When

            var result = db.Encapsulation;

            // Then
            Assert.Equal(expected, result);
        }
    }
}
