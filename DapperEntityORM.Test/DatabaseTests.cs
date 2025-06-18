namespace DapperEntityORM.Test
{
    using DapperEntityORM.Database.Interfaces;
    using DapperEntityORM.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Data.SqlClient;

    [TestClass]
    public class DatabaseTests
    {
        [TestMethod]
        public void GivenCustomDialectAndFactoryWhenCreatingDatabaseShouldUseProvidedInstancesThenExposesValues()
        {
            // Given
            var connection = new SqlConnection("Data Source=.;Initial Catalog=Test;");
            var factory = new Mock<IConnectionFactory>();
            factory.Setup(f => f.CreateConnection()).Returns(connection);

            var dialect = new Mock<IDatabaseDialect>();
            dialect.SetupGet(d => d.DataBaseType).Returns(DataBaseTypes.SQLServer);
            dialect.SetupGet(d => d.Encapsulation).Returns("[{0}]");
            dialect.SetupGet(d => d.GetIdentitySql).Returns("IDS");
            dialect.SetupGet(d => d.GetPagedListSql).Returns("PAGE");

            // When
            var db = new DataBase(dialect.Object, factory.Object);

            // Then
            Assert.AreEqual(connection, db.Connection);
            Assert.AreEqual("[{0}]", db.Encapsulation);
            Assert.AreEqual("IDS", db.GetIdentitySql);
            Assert.AreEqual("PAGE", db.GetPagedListSql);
            Assert.AreEqual(DataBaseTypes.SQLServer, db.DataBaseType);
            factory.Verify(f => f.CreateConnection(), Times.Once);
        }

        [TestMethod]
        public void GivenDatabaseTypeAndConnectionStringWhenInstantiatedShouldCreateDefaultFactoryThenSetDialectValues()
        {
            // Given
            const string conn = "Server=my;Database=db;";

            // When
            var db = new DataBase(DataBaseTypes.MySQL, conn);

            // Then
            Assert.AreEqual(conn, db.Connection.ConnectionString);
            Assert.AreEqual(DataBaseTypes.MySQL, db.DataBaseType);
            Assert.AreEqual("`{0}`", db.Encapsulation);
            Assert.AreEqual("SELECT LAST_INSERT_ID() AS id", db.GetIdentitySql);
        }
    }
}
