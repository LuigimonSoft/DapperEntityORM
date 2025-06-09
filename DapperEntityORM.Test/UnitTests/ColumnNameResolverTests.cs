using DapperEntityORM.Attributes;
using DapperEntityORM.Resolvers;
using System.Reflection;

namespace DapperEntityORM.Test.UnitTests
{
    public class ColumnNameResolverTests
    {
        [Fact]
        public void GivenPropertyWithColumnAttribute_WhenResolvingColumnName_ShouldUseAttributeName_ThenReturnMappedName()
        {
            // Given
            var columnAttr = new ColumnAttribute("MyCol");
            var mockProperty = new Mock<PropertyInfo>();
            mockProperty.Setup(p => p.Name).Returns("Original");
            mockProperty.Setup(p => p.GetCustomAttributes(true)).Returns(new object[] { columnAttr });
            var resolver = new ColumnNameResolver();

            // When
            var result = resolver.ResolveColumnName(mockProperty.Object, "[{0}]", out bool mapped);

            // Then
            Assert.True(mapped);
            Assert.Equal("[MyCol]", result);
        }
    }
}
