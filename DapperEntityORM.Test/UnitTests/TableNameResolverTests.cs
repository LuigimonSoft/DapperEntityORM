using DapperEntityORM;
using DapperEntityORM.Attributes;
using DapperEntityORM.Resolvers;
using DapperEntityORM.Enums;

namespace DapperEntityORM.Test.UnitTests
{
    [Table("People")]
    class PersonWithTable : Entity<PersonWithTable>
    {
        [Key]
        public int Id { get; set; }
    }

    class PersonDefault : Entity<PersonDefault>
    {
        [Key]
        public int Id { get; set; }
    }

    public class TableNameResolverTests
    {
        [Theory]
        [InlineData(typeof(PersonWithTable), "[People]")]
        [InlineData(typeof(PersonDefault), "[PersonDefault]")]
        public void GivenEntityType_WhenResolvingTableName_ShouldReturnEncapsulatedName_ThenMatchesExpected(Type type, string expected)
        {
            // Given
            var resolver = new TableNameResolver();
            var db = new DataBase(DataBaseTypes.SQLServer, "conn");

            // When
            var result = resolver.ResolveTableName(type, db.Encapsulation);

            // Then
            Assert.Equal(expected, result);
        }
    }
}
