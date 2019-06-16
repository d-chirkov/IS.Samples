namespace IdSrv.Account.WebApi.Infrastructure.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.IntegrationTests;
    using NUnit.Framework;
    using SqlKata.Compilers;
    using SqlKata.Execution;

    [TestFixture]
    internal class SqlCompactUserRepositoryTest
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
        public void CreateAsync_ThrowsArgumentNullException_When_ArgIsNullOrContainsAnyNull()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(new NewIdSrvUserDTO
            {
                UserName = null,
                Password = "p"
            }));
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(new NewIdSrvUserDTO
            {
                UserName = "a",
                Password = null
            }));
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(new NewIdSrvUserDTO
            {
                UserName = null,
                Password = null
            }));
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
                IEnumerable<dynamic> rows = await db.Query("Users").GetAsync();
                Assert.AreEqual(rows.Count(), 1);
                dynamic createdUser = rows.FirstOrDefault();
                Assert.IsNotNull(createdUser);
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
                IEnumerable<dynamic> rows = await db.Query("Users").GetAsync();
                Assert.AreEqual(rows.Count(), 1);
                dynamic createdUser = rows.FirstOrDefault();
                Assert.IsNotNull(createdUser);
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
            Assert.IsNotNull(users);
            Assert.AreEqual(users.ElementAt(0).UserName, "u1");
            Assert.AreEqual(users.ElementAt(1).UserName, "u2");
        }

        [Test]
        public async Task GetAllAsync_ReturnEmptyUsersEnumFromDb_When_ThereAreNoUsersInDb()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            IEnumerable<IdSrvUserDTO> users = await repository.GetAllAsync();
            Assert.IsNotNull(users);
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
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
                    PasswordHash = this.GetB64PasswordHash("p3", "s3"),
                    PasswordSalt = "s3"
                });
                searchingId = await db.Query("Users").Select("Id").Where(new { UserName = "u2" }).FirstAsync<Guid>();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IdSrvUserDTO user = await repository.GetByIdAsync(searchingId);
            Assert.IsNotNull(user);
            Assert.AreEqual(user.Id, searchingId);
            Assert.AreEqual(user.UserName, "u2");
        }

        [Test]
        public async Task GetByIdAsync_ReturnNull_When_PassingNotExistingId()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            Guid existingId = Guid.Empty;
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
                existingId = await db.Query("Users").Select("Id").Where(new { UserName = "u1" }).FirstAsync<Guid>();
            }
            Guid searchingId;
            do
            {
                searchingId = Guid.NewGuid();
            } while (searchingId == existingId);
            var repository = new SqlCompactUserRepository(connectionFactory);
            IdSrvUserDTO user = await repository.GetByIdAsync(searchingId);
            Assert.IsNull(user);
        }

        [Test]
        public void GetByAuthInfoAsync_ThrowsArgumentNullException_When_ArgIsNullOrContainsAnyNull()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetByAuthInfoAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO
            {
                UserName = null,
                Password = "p"
            }));
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO
            {
                UserName = "a",
                Password = null
            }));
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO
            {
                UserName = null,
                Password = null
            }));
        }

        [Test]
        public async Task GetByAuthInfoAsync_ReturnUserFromDb_When_PassingExistingAuthInfo()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            Guid userId = Guid.Empty;
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
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
                    PasswordHash = this.GetB64PasswordHash("p3", "s3"),
                    PasswordSalt = "s3"
                });
                userId = await db.Query("Users").Select("Id").Where(new { UserName = "u2" }).FirstAsync<Guid>();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u2", Password = "p2" });
            Assert.IsNotNull(user);
            Assert.AreEqual(user.Id, userId);
            Assert.AreEqual(user.UserName, "u2");
        }

        [Test]
        public async Task GetByAuthInfoAsync_ReturnNull_When_PassingNotExistingUserName()
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
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
                    PasswordHash = this.GetB64PasswordHash("p3", "s3"),
                    PasswordSalt = "s3"
                });
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u4", Password = "p4" });
            Assert.IsNull(user);
        }

        [Test]
        public async Task GetByAuthInfoAsync_ReturnNull_When_PassingInvalidPasswordForExistingUserName()
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
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
                    PasswordHash = this.GetB64PasswordHash("p3", "s3"),
                    PasswordSalt = "s3"
                });
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u2", Password = "p4" });
            Assert.IsNull(user);
        }

        [Test]
        public void ChangePasswordAsync_ThrowArgumentNullException_When_PassingNullPasswords()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.ChangePasswordAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.ChangePasswordAsync(new IdSrvUserPasswordDTO
            {
                UserId = Guid.NewGuid(),
                Password = null
            }));
        }

        [Test]
        public async Task ChangePasswordAsync_ReturnSuccessAndChangePasswordInDb_When_PassingExistingUserGuidWithNewPassword()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            Guid userId;
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
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
                    PasswordHash = this.GetB64PasswordHash("p3", "s3"),
                    PasswordSalt = "s3"
                });
                userId = await db.Query("Users").Select("Id").Where(new { UserName = "u2" }).FirstAsync<Guid>();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse response = await repository.ChangePasswordAsync(new IdSrvUserPasswordDTO
            {
                UserId = userId,
                Password = "p4"
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                dynamic insertedPassword = await db
                    .Query("Users")
                    .Select("PasswordHash", "PasswordSalt")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync();
                Assert.IsNotNull(insertedPassword);
                Assert.AreEqual(this.GetB64PasswordHash("p4", insertedPassword.PasswordSalt), insertedPassword.PasswordHash);
            }
        }

        [Test]
        public async Task ChangePasswordAsync_ReturnNotFound_When_PassingNotExistingUserGuid()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var existingIds = new List<Guid>();
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
                existingIds = (await db.Query("Users").Select("Id").GetAsync<Guid>()).ToList();
            }
            var notExistingId = Guid.NewGuid();
            while (existingIds.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse response = await repository.ChangePasswordAsync(new IdSrvUserPasswordDTO
            {
                UserId = notExistingId,
                Password = "p4"
            });
            Assert.AreEqual(RepositoryResponse.NotFound, response);
        }

        [Test]
        public async Task DeleteAsync_ReturnSuccessAndDeleteUserFromDb_When_PassingExistingUserGuid()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            Guid userId;
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
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
                    PasswordHash = this.GetB64PasswordHash("p3", "s3"),
                    PasswordSalt = "s3"
                });
                userId = await db.Query("Users").Select("Id").Where(new { UserName = "u2" }).FirstAsync<Guid>();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse response = await repository.DeleteAsync(userId);
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                dynamic deletedUser = await db
                    .Query("Users")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync();
                Assert.IsNull(deletedUser);
            }
        }

        [Test]
        public async Task DeleteAsync_ReturnNotFound_When_PassingNotExistingUserGuid()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var existingIds = new List<Guid>();
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
                existingIds = (await db.Query("Users").Select("Id").GetAsync<Guid>()).ToList();
            }
            var notExistingId = Guid.NewGuid();
            while (existingIds.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse response = await repository.DeleteAsync(notExistingId);
            Assert.AreEqual(RepositoryResponse.NotFound, response);
        }
    }
}
