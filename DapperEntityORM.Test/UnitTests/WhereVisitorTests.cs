using Dapper;
using System.Linq.Expressions;

namespace DapperEntityORM.Test.UnitTests
{
    class VisitorModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class WhereVisitorTests
    {
        [Fact]
        public void GivenExpression_WhenVisiting_ShouldBuildWhereClause_ThenCreateParameters()
        {
            // Given
            var parameters = new DynamicParameters();
            var visitor = new WhereVisitor(parameters);
            Expression<Func<VisitorModel, bool>> expr = x => x.Id == 5 && x.Name == "John";

            // When
            visitor.Visit(expr);

            // Then
            Assert.Equal(" WHERE Id = @Id AND Name = @Name", visitor.WhereClause);
            Assert.Equal(5, parameters.Get<int>("@Id"));
            Assert.Equal("John", parameters.Get<string>("@Name"));
        }
    }
}
