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
    using System.Collections.Generic;

    [TestFixture]
    class SqlCompactUserRepositoryTest
    {
        public string TestConnectionString { get; set; } = $"Data Source={TestHelper.GetPathToTestDb()}";

        private string GetB64PasswordHash(string password, string salt)
        {
            SHA512 sha512 = new SHA512Managed();
            byte[] rawPasswordHash = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + salt));
            return Convert.ToBase64String(rawPasswordHash);
        }

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
                Assert.AreEqual(createdUser.PasswordHash, this.GetB64PasswordHash("p1", createdUser.PasswordSalt));
            }
        }

        [Test]
        public async Task GetAllAsync_ReturnAllUsersFromDb_When_UsersExistsInDb()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u1",
                    PasswordHash = this.GetB64PasswordHash("p1", "s1"),
                    PasswordSalt = "s1"
                });
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u2",
                    PasswordHash = this.GetB64PasswordHash("p2", "s2"),
                    PasswordSalt = "s2"
                });
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IEnumerable<IdSrvUserDTO> users = await repository.GetAllAsync();
            Assert.NotNull(users);
            Assert.AreEqual(users.ElementAt(0).UserName, "u1");
            Assert.AreEqual(users.ElementAt(1).UserName, "u2");
        }

        [Test]
        public async Task GetAllAsync_ReturnEmptyUsersEnumFromDb_When_ThereAreNoUsersInDb()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            IEnumerable<IdSrvUserDTO> users = await repository.GetAllAsync();
            Assert.NotNull(users);
            Assert.AreEqual(users.Count(), 0);
        }

        [Test]
        public async Task GetByIdAsync_ReturnUserFromDb_When_PassingExistingId()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            Guid searchingId = Guid.Empty;
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u1",
                    PasswordHash = this.GetB64PasswordHash("p1", "s1"),
                    PasswordSalt = "s1"
                });
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u2",
                    PasswordHash = this.GetB64PasswordHash("p2", "s2"),
                    PasswordSalt = "s2"
                });
                searchingId = await db.Query("Users").Select("Id").Where(new { UserName = "u1" }).FirstAsync<Guid>();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IdSrvUserDTO user = await repository.GetByIdAsync(searchingId);
            Assert.NotNull(user);
            Assert.AreEqual(user.UserName, "u1");
        }
    }
}
