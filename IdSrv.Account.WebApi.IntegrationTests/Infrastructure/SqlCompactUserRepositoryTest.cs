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
    using System.IO;
    using IdSrv.Account.WebApi.IntegrationTests;
    using System.Security.Cryptography;

    [TestFixture]
    class SqlCompactUserRepositoryTest
    {
        public string TestConnectionString { get; set; } = $"Data Source={TestHelper.GetPathToTestDb()}";

        [SetUp]
        public async Task SetUp()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                await db.Query("Users").DeleteAsync();
            }
        }

        [Test]
        public void Ctor_DoesNotThrow_When_PassingNotNullFactory()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            Assert.DoesNotThrow(() => new SqlCompactUserRepository(connectionFactory));
        }

        [Test]
        public void Ctor_ThrowArgumentNullException_When_PassingNullInsteadFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlCompactUserRepository(null));
        }

        [Test]
        public async Task CreateAsync_ReturnSuccess_And_CreateUserInDb_When_ThereIsNoUsersWithTheSameNameInDb()
        {
            string userName = "u1";
            string userPassword = "p1";
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            var user = new NewIdSrvUserDTO { UserName = userName, Password = userPassword };
            RepositoryResponse response = await repository.CreateAsync(user);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                var rows = await db.Query("Users").Select().GetAsync();
                Assert.AreEqual(rows.Count(), 1);
                var createdUser = rows.FirstOrDefault();
                Assert.NotNull(createdUser);
                Assert.IsInstanceOf<Guid>(createdUser.Id);
                Assert.AreEqual(createdUser.UserName, userName);
                SHA512 sha512 = new SHA512Managed();
                byte[] rawPasswordHash = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userPassword + createdUser.PasswordSalt));
                string b64PasswordHash = Convert.ToBase64String(rawPasswordHash);
                Assert.AreEqual(createdUser.PasswordHash, b64PasswordHash);
            }
        }

        [Test]
        public async Task CreateAsync_ReturnConflict_And_DoNotChangeDbData_When_TryingToCreateUserWithTheSameNameTwice()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u1", Password = "p1" });
            RepositoryResponse response = await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u1", Password = "p2" });
            Assert.AreEqual(RepositoryResponse.Conflict, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                var rows = await db.Query("Users").Select().GetAsync();
                Assert.AreEqual(rows.Count(), 1);
                var createdUser = rows.FirstOrDefault();
                Assert.NotNull(createdUser);
                Assert.IsInstanceOf<Guid>(createdUser.Id);
                Assert.AreEqual(createdUser.UserName, "u1");
                SHA512 sha512 = new SHA512Managed();
                byte[] rawPasswordHash = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes("p1" + createdUser.PasswordSalt));
                string b64PasswordHash = Convert.ToBase64String(rawPasswordHash);
                Assert.AreEqual(createdUser.PasswordHash, b64PasswordHash);
            }
        }
    }
}
