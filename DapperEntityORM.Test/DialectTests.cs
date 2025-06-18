namespace DapperEntityORM.Test
{
    using DapperEntityORM.Database.Dialects;
    using DapperEntityORM.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DialectTests
    {
        [TestMethod]
        public void GivenSqlServerDialectWhenCreatedShouldExposeConstantsThenReturnCorrectSql()
        {
            // Given
            var dialect = new SqlServerDialect();

            // Then
            Assert.AreEqual(DataBaseTypes.SQLServer, dialect.DataBaseType);
            Assert.AreEqual("[{0}]", dialect.Encapsulation);
            Assert.AreEqual("SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [id]", dialect.GetIdentitySql);
            Assert.AreEqual("SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {OrderBy}) AS PagedNumber, {SelectColumns} FROM {TableName} {WhereClause}) AS u WHERE PagedNUMBER BETWEEN (({PageNumber}-1) * {RowsPerPage} + 1) AND ({PageNumber} * {RowsPerPage})", dialect.GetPagedListSql);
        }

        [TestMethod]
        public void GivenMySqlDialectWhenCreatedShouldExposeConstantsThenReturnCorrectSql()
        {
            // Given
            var dialect = new MySqlDialect();

            // Then
            Assert.AreEqual(DataBaseTypes.MySQL, dialect.DataBaseType);
            Assert.AreEqual("`{0}`", dialect.Encapsulation);
            Assert.AreEqual("SELECT LAST_INSERT_ID() AS id", dialect.GetIdentitySql);
            Assert.AreEqual("Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {Offset},{RowsPerPage}", dialect.GetPagedListSql);
        }

        [TestMethod]
        public void GivenPostgreSqlDialectWhenCreatedShouldExposeConstantsThenReturnCorrectSql()
        {
            // Given
            var dialect = new PostgreSqlDialect();

            // Then
            Assert.AreEqual(DataBaseTypes.PostgreSQL, dialect.DataBaseType);
            Assert.AreEqual("\"{0}\"", dialect.Encapsulation);
            Assert.AreEqual("SELECT LASTVAL() AS id", dialect.GetIdentitySql);
            Assert.AreEqual("Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})", dialect.GetPagedListSql);
        }

        [TestMethod]
        public void GivenSqliteDialectWhenCreatedShouldExposeConstantsThenReturnCorrectSql()
        {
            // Given
            var dialect = new SQLiteDialect();

            // Then
            Assert.AreEqual(DataBaseTypes.SQLite, dialect.DataBaseType);
            Assert.AreEqual("\"{0}\"", dialect.Encapsulation);
            Assert.AreEqual("SELECT LAST_INSERT_ROWID() AS id", dialect.GetIdentitySql);
            Assert.AreEqual("Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})", dialect.GetPagedListSql);
        }
    }
}
