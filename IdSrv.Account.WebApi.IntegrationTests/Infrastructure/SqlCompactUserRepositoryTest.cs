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

        #region DatabaseInteractions

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
            Assert.AreEqual(RepositoryResponse.Success, response);
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
        public async Task CreateAsync_ReturnSuccess_And_CreateUserInDb_When_PassingNullPasswordForNewUser()
        {
            string userName = "u1";
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            var user = new NewIdSrvUserDTO { UserName = userName };
            RepositoryResponse response = await repository.CreateAsync(user);
            Assert.AreEqual(RepositoryResponse.Success, response);
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
                Assert.IsNull(createdUser.PasswordHash, null);
                Assert.IsNull(createdUser.PasswordSalt, null);
            }
        }

        [Test]
        public async Task CreateAsync_CreateUnblockedUserInDb_When_PassingUserWithPassword()
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
                Assert.IsFalse(createdUser.IsBlocked);
            }
        }

        [Test]
        public async Task CreateAsync_CreateUnblockedUserInDb_When_PassingUserWithoutPassword()
        {
            string userName = "u1";
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            var user = new NewIdSrvUserDTO { UserName = userName };
            RepositoryResponse response = await repository.CreateAsync(user);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                IEnumerable<dynamic> rows = await db.Query("Users").GetAsync();
                Assert.AreEqual(rows.Count(), 1);
                dynamic createdUser = rows.FirstOrDefault();
                Assert.IsNotNull(createdUser);
                Assert.IsFalse(createdUser.IsBlocked);
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
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
                });
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IEnumerable<IdSrvUserDTO> users = await repository.GetAllAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(3, users.Count());
            Assert.AreEqual(users.ElementAt(0).UserName, "u1");
            Assert.AreEqual(users.ElementAt(1).UserName, "u2");
            Assert.AreEqual(users.ElementAt(2).UserName, "u3");
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
        public async Task GetByIdAsync_ReturnUserFromDb_When_PassingExistingIdForUserWithPassword()
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
        public async Task GetByIdAsync_ReturnUserFromDb_When_PassingExistingIdForUserWithoutPassword()
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
                    UserName = "u2"
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
        public async Task GetByAuthInfoAsync_ReturnUserFromDb_When_PassingExistingAuthInfoForUserWithPassword()
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
                    UserName = "u3"
                });
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u4",
                    PasswordHash = this.GetB64PasswordHash("p4", "s4"),
                    PasswordSalt = "s4"
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
        public async Task GetByAuthInfoAsync_ReturnNull_When_PassingExistingAuthInfoForUserWithoutPassword()
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
                    UserName = "u3"
                });
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u4",
                    PasswordHash = this.GetB64PasswordHash("p4", "s4"),
                    PasswordSalt = "s4"
                });
                userId = await db.Query("Users").Select("Id").Where(new { UserName = "u3" }).FirstAsync<Guid>();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u3", Password = "p3" });
            Assert.IsNull(user);
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
                    UserName = "u3"
                });
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u4", Password = "p4" });
            Assert.IsNull(user);
        }

        [Test]
        public async Task GetByAuthInfoAsync_ReturnNull_When_PassingInvalidPasswordForExistingUserWithPassword()
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
        public async Task GetByAuthInfoAsync_ReturnNull_When_PassingInvalidPasswordForExistingUserWithoutPassword()
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
                    UserName = "u2"
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
        public async Task ChangePasswordAsync_ReturnSuccess_And_ChangePasswordInDb_When_PassingUserThatHasPasswordInDb()
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
        public async Task ChangePasswordAsync_ReturnNotFound_And_DoNotChangePasswordInDb_When_PassingUserThatHasNotPasswordInDb()
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
                    UserName = "u2"
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
            Assert.AreEqual(RepositoryResponse.NotFound, response);
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
                Assert.IsNull(insertedPassword.PasswordHash);
                Assert.IsNull(insertedPassword.PasswordSalt);
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
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
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
        public async Task DeleteAsync_ReturnSuccess_And_DeleteUserFromDb_When_PassingExistingUserThatHasPassword()
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
        public async Task DeleteAsync_ReturnSuccess_And_DeleteUserFromDb_When_PassingExistingUserThatHasNotPassword()
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
                    UserName = "u2"
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

        [Test]
        public void ChangeBlockingAsync_ThrowArgumentNullException_When_PassingNull()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.ChangePasswordAsync(null));
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_BlockUserInDb_When_BlockUserWithPassword()
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
                    PasswordSalt = "s2",
                    IsBlocked = false
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
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = userId,
                IsBlocked = true
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                bool? isBlocked = await db
                    .Query("Users")
                    .Select("IsBlocked")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync<bool?>();
                Assert.IsNotNull(isBlocked);
                Assert.IsTrue(isBlocked);
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_BlockUserInDb_When_BlockUserWithoutPassword()
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
                    IsBlocked = false
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
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = userId,
                IsBlocked = true
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                bool? isBlocked = await db
                    .Query("Users")
                    .Select("IsBlocked")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync<bool?>();
                Assert.IsNotNull(isBlocked);
                Assert.IsTrue(isBlocked);
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_UnblockUserInDb_When_UnblockingUserWithPassword()
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
                    PasswordSalt = "s2",
                    IsBlocked = true
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
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = userId,
                IsBlocked = false
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                bool? isBlocked = await db
                    .Query("Users")
                    .Select("IsBlocked")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync<bool?>();
                Assert.IsNotNull(isBlocked);
                Assert.IsFalse(isBlocked);
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_UnblockUserInDb_When_UnblockingUserWithoutPassword()
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
                    IsBlocked = true
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
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = userId,
                IsBlocked = false
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                bool? isBlocked = await db
                    .Query("Users")
                    .Select("IsBlocked")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync<bool?>();
                Assert.IsNotNull(isBlocked);
                Assert.IsFalse(isBlocked);
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_DoNotChangeDb_When_BlockingAlreadyBlockedUserWithPassword()
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
                    PasswordSalt = "s2",
                    IsBlocked = true
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
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = userId,
                IsBlocked = true
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                bool? isBlocked = await db
                    .Query("Users")
                    .Select("IsBlocked")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync<bool?>();
                Assert.IsNotNull(isBlocked);
                Assert.IsTrue(true);
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_DoNotChangeDb_When_BlockingAlreadyBlockedUserWithoutPassword()
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
                    IsBlocked = true
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
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = userId,
                IsBlocked = true
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                bool? isBlocked = await db
                    .Query("Users")
                    .Select("IsBlocked")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync<bool?>();
                Assert.IsNotNull(isBlocked);
                Assert.IsTrue(true);
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_DoNotChangeDb_When_UnblockingAlreadyUnblockedUserWithPassword()
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
                    PasswordSalt = "s2",
                    IsBlocked = false
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
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = userId,
                IsBlocked = false
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                bool? isBlocked = await db
                    .Query("Users")
                    .Select("IsBlocked")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync<bool?>();
                Assert.IsNotNull(isBlocked);
                Assert.AreEqual(false, isBlocked);
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_DoNotChangeDb_When_UnblockingAlreadyUnblockedUserWithoutPassword()
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
                    IsBlocked = false
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
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = userId,
                IsBlocked = false
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await connectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                bool? isBlocked = await db
                    .Query("Users")
                    .Select("IsBlocked")
                    .Where(new { Id = userId })
                    .FirstOrDefaultAsync<bool?>();
                Assert.IsNotNull(isBlocked);
                Assert.AreEqual(false, isBlocked);
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnNotFound_When_PassingNotExistingUser()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            List<Guid> existingIds = new List<Guid>();
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
                    UserName = "u2"
                });
                await db.Query("Users").InsertAsync(new
                {
                    UserName = "u3",
                    PasswordHash = this.GetB64PasswordHash("p3", "s3"),
                    PasswordSalt = "s3"
                });
                existingIds = (await db.Query("Users").Select("Id").GetAsync<Guid>()).ToList();
            }
            var notExistingId = Guid.NewGuid();
            while (existingIds.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO
            {
                UserId = notExistingId,
                IsBlocked = true
            });
            Assert.AreEqual(RepositoryResponse.NotFound, response);
        }

        #endregion

        #region Scenarios

        [Test]
        public async Task Create_GetByAuthInfo_Delete_GetByAuthInfo_GetById_GetAll()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse result = await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u1", Password = "p1" });
            Assert.AreEqual(RepositoryResponse.Success, result);
            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u1", Password = "p1" });
            Assert.IsNotNull(user);
            Assert.AreEqual("u1", user.UserName);
            Assert.IsFalse(user.IsBlocked);
            Guid storedId = user.Id;
            IEnumerable<IdSrvUserDTO> users = await repository.GetAllAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count());
            result = await repository.DeleteAsync(storedId);
            Assert.AreEqual(RepositoryResponse.Success, result);
            user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u1", Password = "p1" });
            Assert.IsNull(user);
            user = await repository.GetByIdAsync(storedId);
            Assert.IsNull(user);
            users = await repository.GetAllAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(0, users.Count());
        }

        [Test]
        public async Task Create_GetByAuthInfo_Block_GetById_Unblock_GetById()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse result = await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u1", Password = "p1" });
            Assert.AreEqual(RepositoryResponse.Success, result);
            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u1", Password = "p1" });
            Assert.IsNotNull(user);
            Assert.AreEqual("u1", user.UserName);
            Assert.IsFalse(user.IsBlocked);
            result = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO { UserId = user.Id, IsBlocked = true});
            Assert.AreEqual(RepositoryResponse.Success, result);
            user = await repository.GetByIdAsync(user.Id);
            Assert.IsNotNull(user);
            Assert.IsTrue(user.IsBlocked);
            result = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO { UserId = user.Id, IsBlocked = false });
            Assert.AreEqual(RepositoryResponse.Success, result);
            user = await repository.GetByIdAsync(user.Id);
            Assert.IsNotNull(user);
            Assert.IsFalse(user.IsBlocked);
        }

        [Test]
        public async Task Create_GetByAuthInfo_ChangePassword_GetByAuthInfo()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse result = await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u1", Password = "p1" });
            Assert.AreEqual(RepositoryResponse.Success, result);
            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u1", Password = "p1" });
            Assert.IsNotNull(user);
            Assert.AreEqual("u1", user.UserName);
            Assert.IsFalse(user.IsBlocked);
            result = await repository.ChangePasswordAsync(new IdSrvUserPasswordDTO { UserId = user.Id, Password = "p2" });
            Assert.AreEqual(RepositoryResponse.Success, result);
            user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u1", Password = "p1" });
            Assert.IsNull(user);
            user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u1", Password = "p2" });
            Assert.IsNotNull(user);
            Assert.AreEqual("u1", user.UserName);
            Assert.IsFalse(user.IsBlocked);
        }

        [Test]
        public async Task Create1_Create2_Create1_GetAll_GetById1_GetById2_Delete1_GetAll_Delete1_GetAll()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse result = await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u1", Password = "p1" });
            Assert.AreEqual(RepositoryResponse.Success, result);
            result = await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u2", Password = "p2" });
            Assert.AreEqual(RepositoryResponse.Success, result);
            result = await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u1", Password = "p2" });
            Assert.AreEqual(RepositoryResponse.Conflict, result);

            IEnumerable<IdSrvUserDTO> users = await repository.GetAllAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count());
            IdSrvUserDTO user1 = users.OrderBy(u => u.UserName).ElementAt(0);
            IdSrvUserDTO user2 = users.OrderBy(u => u.UserName).ElementAt(1);
            Assert.AreEqual("u1", user1.UserName);
            Assert.AreEqual("u2", user2.UserName);
            Assert.IsFalse(user1.IsBlocked);
            Assert.IsFalse(user2.IsBlocked);

            IdSrvUserDTO user = await repository.GetByIdAsync(user1.Id);
            Assert.IsNotNull(user);
            Assert.AreEqual("u1", user.UserName);
            Assert.IsFalse(user.IsBlocked);

            user = await repository.GetByIdAsync(user2.Id);
            Assert.IsNotNull(user);
            Assert.AreEqual("u2", user.UserName);
            Assert.IsFalse(user.IsBlocked);

            result = await repository.DeleteAsync(user1.Id);
            Assert.AreEqual(RepositoryResponse.Success, result);

            users = await repository.GetAllAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count());
            user = users.First();
            Assert.AreEqual("u2", user.UserName);
            Assert.IsFalse(user.IsBlocked);

            result = await repository.DeleteAsync(user1.Id);
            Assert.AreEqual(RepositoryResponse.NotFound, result);

            users = await repository.GetAllAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count());
            user = users.First();
            Assert.AreEqual("u2", user.UserName);
            Assert.IsFalse(user.IsBlocked);
        }

        [Test]
        public async Task Create_GetByAuthInfo_Block_GetById_Block_GetById_Unblock_Unblock_GetById()
        {
            var connectionFactory = new SqlCompactConnectionFactory(this.TestConnectionString);
            var repository = new SqlCompactUserRepository(connectionFactory);
            RepositoryResponse result = await repository.CreateAsync(new NewIdSrvUserDTO { UserName = "u1", Password = "p1" });
            Assert.AreEqual(RepositoryResponse.Success, result);

            IdSrvUserDTO user = await repository.GetByAuthInfoAsync(new IdSrvUserAuthDTO { UserName = "u1", Password = "p1" });
            Assert.IsNotNull(user);
            Assert.AreEqual("u1", user.UserName);
            Assert.IsFalse(user.IsBlocked);

            result = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO { UserId = user.Id, IsBlocked = true });
            Assert.AreEqual(RepositoryResponse.Success, result);

            user = await repository.GetByIdAsync(user.Id);
            Assert.IsNotNull(user);
            Assert.IsTrue(user.IsBlocked);

            result = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO { UserId = user.Id, IsBlocked = true });
            Assert.AreEqual(RepositoryResponse.Success, result);

            user = await repository.GetByIdAsync(user.Id);
            Assert.IsNotNull(user);
            Assert.IsTrue(user.IsBlocked);

            result = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO { UserId = user.Id, IsBlocked = false });
            Assert.AreEqual(RepositoryResponse.Success, result);

            result = await repository.ChangeBlockingAsync(new IdSrvUserBlockDTO { UserId = user.Id, IsBlocked = false });
            Assert.AreEqual(RepositoryResponse.Success, result);

            user = await repository.GetByIdAsync(user.Id);
            Assert.IsNotNull(user);
            Assert.IsFalse(user.IsBlocked);
        }

        // TODO: add more scenarious

        #endregion
    }
}
