namespace IdSrv.Account.WebApi.Infrastructure.Tests
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using Moq;
    using IdSrv.Account.Models;
    using SqlKata;
    using SqlKata.Execution;
    using SqlKata.Compilers;
    using System.Data.SqlServerCe;

    [TestFixture]
    class SqlCompactUserRepositoryTest
    {
        public string TestConnectionString { get; set; } = @"Data Source=C:\Users\dchir\source\repos\IS.Samples\IdSrv.Account.WebApi.IntegrationTests\Infrastructure\test_compact_db.sdf";

        [SetUp]
        public async Task SetUp()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                await db.Query("Users").Where(new { }).DeleteAsync();
            }
        }

        [Test]
        public void Ctor_DoesNotThrow_When_PassingNotNullFactory()
        {
            var connectionFactoryMock = new Mock<IDatabaseConnectionFactory>();
            Assert.DoesNotThrow(() => new SqlCompactUserRepository(connectionFactoryMock.Object));
        }

        [Test]
        public void Ctor_ThrowArgumentNullException_When_PassingNullInsteadFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlCompactUserRepository(null));
        }

        [Test]
        public async Task CreateAsync_ReturnSuccess_And_CreateUserInDb_When_ThereIsNoUsersWithTheSameNameInDb()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            var user = new NewIdSrvUserDTO { UserName = "t1", Password = "p1" };
            RepositoryResponse response = await repository.CreateAsync(user);
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                var rows = await db.Query("Users").Select("UserName").Where(new { UserName = user.UserName }).GetAsync();
                Assert.AreEqual(1, rows.Count());
            }
        }
    }
}
