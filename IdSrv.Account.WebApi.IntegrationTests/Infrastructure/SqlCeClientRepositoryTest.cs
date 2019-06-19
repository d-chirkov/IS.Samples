namespace IdSrv.Account.WebApi.Infrastructure.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.IntegrationTests;
    using NUnit.Framework;
    using SqlKata.Compilers;
    using SqlKata.Execution;

    [TestFixture]
    internal class SqlCeClientRepositoryTest
    {
        public string TestConnectionString { get; set; } = $"Data Source={TestHelper.GetPathToTestDb()}";

        public SqlCeConnectionFactory ConnectionFactory { get; set; }

        private async Task<Guid> InsertDefaultClient(int number, bool hasUri = true, bool isBlocked = false)
        {
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await this.InsertClient(
                    $"n{number}", 
                    (hasUri ? $"u{number}" : null), 
                    $"p{number}", 
                    isBlocked, 
                    db);
            }
        }

        private async Task<Guid> InsertClient(string name, string uri, string secret, bool isBlocked, QueryFactory db)
        {
            await db.Query("Clients").InsertAsync(new
            {
                Name = name,
                Uri = uri,
                Secret = secret,
                IsBlocked = isBlocked
            });
            return await db.Query("Clients").Select("Id").Where(new { Name = name }).FirstAsync<Guid>();
        }

        [SetUp]
        public async Task SetUp()
        {
            this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                await db.Query("Clients").DeleteAsync();
            }
        }

        #region Common

        [Test]
        public void Ctor_DoesNotThrow_When_PassingNotNullFactory()
        {
            var connectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
            Assert.DoesNotThrow(() => new SqlCeClientRepository(connectionFactory));
        }

        [Test]
        public void Ctor_ThrowArgumentNullException_When_PassingNullInsteadFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlCeClientRepository(null));
        }

        #endregion

        #region DatabaseInteractions

        [Test]
        public async Task InsertInDb_InsertNotBlockedClient_When_IsBlockedNotSpecified()
        {
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                await db.Query("Clients").InsertAsync(new
                {
                    Name = "n",
                    Uri = "u",
                    Secret = "s"
                });
                IdSrvClientDTO client = await db.Query("Clients").Where(new { Name = "n" }).FirstOrDefaultAsync<IdSrvClientDTO>();
                Assert.IsFalse(client.IsBlocked);
            }
        }

        [Test]
        public async Task InsertInDb_InsertNullAsUri_When_UriNotSpecified()
        {
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                await db.Query("Clients").InsertAsync(new
                {
                    Name = "n",
                    Secret = "s"
                });
                IdSrvClientDTO client = await db.Query("Clients").Where(new { Name = "n" }).FirstOrDefaultAsync<IdSrvClientDTO>();
                Assert.IsNull(client.Uri);
            }
        }

        [Test]
        public async Task GetAllAsync_ReturnAllClients_When_Invoked()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2, false);
            ids[2] = await this.InsertDefaultClient(3, isBlocked: true);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            IEnumerable<IdSrvClientDTO> clients = await repository.GetAllAsync();
            Assert.IsNotNull(clients);
            Assert.AreEqual(3, clients.Count());
            for (int i = 1; i <= 3; ++i)
            {
                Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                Assert.AreEqual(i == 2 ? null : $"u{i}", clients.ElementAt(i - 1).Uri);
                Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                Assert.AreEqual(i == 3, clients.ElementAt(i - 1).IsBlocked);
            }
        }

        [Test]
        public async Task GetAllAsync_ReturnEmptyList_When_NoClientInDb()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            IEnumerable<IdSrvClientDTO> clients = await repository.GetAllAsync();
            Assert.IsNotNull(clients);
            Assert.AreEqual(0, clients.Count());
        }

        [Test]
        public async Task GetByIdAsync_ReturnClient_When_PassingExistingId_And_ClientInDbContainsUri()
        {
            Guid id = await this.InsertDefaultClient(1);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            IdSrvClientDTO client = await repository.GetByIdAsync(id);
            Assert.AreEqual(id, client.Id);
            Assert.AreEqual($"n1", client.Name);
            Assert.AreEqual($"u1", client.Uri);
            Assert.AreEqual($"p1", client.Secret);
        }

        [Test]
        public async Task GetByIdAsync_ReturnClient_When_PassingExistingId_And_ClientInDbNotContainsUri()
        {
            Guid id = await this.InsertDefaultClient(1, false);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            IdSrvClientDTO client = await repository.GetByIdAsync(id);
            Assert.AreEqual(id, client.Id);
            Assert.AreEqual($"n1", client.Name);
            Assert.AreEqual(null, client.Uri);
            Assert.AreEqual($"p1", client.Secret);
        }

        [Test]
        public async Task GetByIdAsync_ReturnNull_When_DbNotContainAnyClients()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            IdSrvClientDTO client = await repository.GetByIdAsync(Guid.NewGuid());
            Assert.IsNull(client);
        }

        [Test]
        public async Task GetByIdAsync_ReturnNull_When_PassingIdForNotExisingClient()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2, false);
            ids[2] = await this.InsertDefaultClient(3, isBlocked: true);
            var notExistingId = Guid.NewGuid();
            while (ids.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            IdSrvClientDTO client = await repository.GetByIdAsync(notExistingId);
            Assert.IsNull(client);
        }

        [Test]
        public async Task DeleteAsync_ReturnNull_When_DbNotContainAnyClients()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.DeleteAsync(Guid.NewGuid());
            Assert.AreEqual(RepositoryResponse.NotFound, response);
        }

        [Test]
        public async Task DeleteAsync_ReturnNotFound_When_PassingIdForNotExisingClient()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2, false);
            ids[2] = await this.InsertDefaultClient(3, isBlocked: true);
            var notExistingId = Guid.NewGuid();
            while (ids.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.DeleteAsync(notExistingId);
            Assert.AreEqual(RepositoryResponse.NotFound, response);
        }

        [Test]
        public async Task DeleteAsync_ReturnSuccess_And_DeleteClientFromDb_When_PassingExisingIdForClientWithoutUri()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2, false);
            ids[2] = await this.InsertDefaultClient(3, isBlocked: true);
            var notExistingId = Guid.NewGuid();
            while (ids.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.DeleteAsync(ids[1]);
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<Guid> clientsIds = await db.Query("Clients").Select("Id").GetAsync<Guid>();
                Assert.AreEqual(ids.Except(new Guid[] { ids[1] }).ToList().OrderBy(x => x), clientsIds.ToList().OrderBy(x => x));
            }
        }

        [Test]
        public async Task DeleteAsync_ReturnSuccess_And_DeleteClientFromDb_When_PassingExisingIdForClientWithUri()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2, false);
            ids[2] = await this.InsertDefaultClient(3, isBlocked: true);
            var notExistingId = Guid.NewGuid();
            while (ids.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.DeleteAsync(ids[0]);
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<Guid> clientsIds = await db.Query("Clients").Select("Id").GetAsync<Guid>();
                Assert.AreEqual(ids.Except(new Guid[] { ids[0] }).ToList().OrderBy(x => x), clientsIds.ToList().OrderBy(x => x));
            }
        }

        [Test]
        public async Task DeleteAsync_ReturnSuccess_And_DeleteClientFromDb_When_PassingExisingIdForBlockedClient()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2, false);
            ids[2] = await this.InsertDefaultClient(3, isBlocked: true);
            var notExistingId = Guid.NewGuid();
            while (ids.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.DeleteAsync(ids[2]);
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<Guid> clientsIds = await db.Query("Clients").Select("Id").GetAsync<Guid>();
                Assert.AreEqual(ids.Except(new Guid[] { ids[2] }).ToList().OrderBy(x => x), clientsIds.ToList().OrderBy(x => x));
            }
        }

        [Test]
        public async Task DeleteAsync_ReturnSuccess_And_DeleteClientsFromDb_When_DoubleInvokedWithExistingIds()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var notExistingId = Guid.NewGuid();
            while (ids.Contains(notExistingId))
            {
                notExistingId = Guid.NewGuid();
            }
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.DeleteAsync(ids[0]);
            Assert.AreEqual(RepositoryResponse.Success, response);
            response = await repository.DeleteAsync(ids[2]);
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<Guid> clientsIds = await db.Query("Clients").Select("Id").GetAsync<Guid>();
                Assert.AreEqual(ids.Except(new Guid[] { ids[0], ids[2] }).ToList().OrderBy(x => x), clientsIds.ToList().OrderBy(x => x));
            }
        }

        [Test]
        public void CreateAsync_ThrowsArgumentNullException_When_PassingNullAsDto()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(null));
        }

        [Test]
        public void CreateAsync_ThrowsArgumentNullException_When_PassingNullName()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(new NewIdSrvClientDTO { Secret = "s" }));
        }

        [Test]
        public void CreateAsync_ThrowsArgumentNullException_When_PassingNullSecret()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(new NewIdSrvClientDTO { Name = "n" }));
        }

        [Test]
        public async Task CreateAsync_ReturnSuccess_And_CreateNotBlockedUserInDb_When_PassingClientWithoutUri()
        {
            await this.InsertDefaultClient(1);
            await this.InsertDefaultClient(2);
            await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.CreateAsync(new NewIdSrvClientDTO { Name = "n", Secret = "s" });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(4, clients.Count());
                IdSrvClientDTO createdClient = clients.Where(c => c.Name == "n").FirstOrDefault();
                Assert.IsNotNull(createdClient);
                Assert.AreEqual("n", createdClient.Name);
                Assert.AreEqual("s", createdClient.Secret);
                Assert.IsNull(createdClient.Uri);
                Assert.IsFalse(createdClient.IsBlocked);
            }
        }

        [Test]
        public async Task CreateAsync_ReturnSuccess_And_CreateNotBlockedUserInDb_When_PassingClientWithoutUri_And_DbNotContainsAnyClient()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.CreateAsync(new NewIdSrvClientDTO { Name = "n", Secret = "s" });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(1, clients.Count());
                IdSrvClientDTO createdClient = clients.ElementAt(0);
                Assert.AreEqual("n", createdClient.Name);
                Assert.AreEqual("s", createdClient.Secret);
                Assert.IsNull(createdClient.Uri);
                Assert.IsFalse(createdClient.IsBlocked);
            }
        }

        [Test]
        public async Task CreateAsync_ReturnSuccess_And_CreateNotBlockedUserInDb_When_PassingClientWithUri()
        {
            await this.InsertDefaultClient(1);
            await this.InsertDefaultClient(2);
            await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.CreateAsync(new NewIdSrvClientDTO { Name = "n", Secret = "s", Uri = "u" });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(4, clients.Count());
                IdSrvClientDTO createdClient = clients.Where(c => c.Name == "n").FirstOrDefault();
                Assert.IsNotNull(createdClient);
                Assert.AreEqual("n", createdClient.Name);
                Assert.AreEqual("s", createdClient.Secret);
                Assert.AreEqual("u", createdClient.Uri);
                Assert.IsFalse(createdClient.IsBlocked);
            }
        }

        [Test]
        public async Task CreateAsync_ReturnSuccess_And_CreateNotBlockedUserInDb_When_PassingClientWithUri_And_DbNotContainsAnyClient()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.CreateAsync(new NewIdSrvClientDTO { Name = "n", Secret = "s", Uri = "u" });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(1, clients.Count());
                IdSrvClientDTO createdClient = clients.ElementAt(0);
                Assert.AreEqual("n", createdClient.Name);
                Assert.AreEqual("s", createdClient.Secret);
                Assert.AreEqual("u", createdClient.Uri);
                Assert.IsFalse(createdClient.IsBlocked);
            }
        }

        [Test]
        public async Task CreateAsync_ReturnConflict_And_DoNotChangeDb_When_PassingClientNameAlreadyExists()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.CreateAsync(new NewIdSrvClientDTO { Name = "n1", Secret = "s", Uri = "u" });
            Assert.AreEqual(RepositoryResponse.Conflict, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual($"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(false, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public void UpdateAsync_ThrowsArgumentNullException_When_PassingNullAsDto()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));
        }

        [Test]
        public void UpdateAsync_ThrowsArgumentNullException_When_PassingNullName()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(new UpdateIdSrvClientDTO { Secret = "s" }));
        }

        [Test]
        public void UpdateAsync_ThrowsArgumentNullException_When_PassingNullSecret()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(new UpdateIdSrvClientDTO { Name = "n" }));
        }

        [Test]
        public async Task UpdateAsync_ReturnSuccess_And_UpdateClientInDb_When_PassingClientWithoutUri()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.UpdateAsync(new UpdateIdSrvClientDTO
            {
                Id = ids[1],
                Name = "n",
                Secret = "p",
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual(i == 2 ? "n" : $"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual(i == 2 ? null : $"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual(i == 2 ? "p" : $"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(false, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public async Task UpdateAsync_ReturnSuccess_And_UpdateClientInDb_When_PassingClientWithUri()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.UpdateAsync(new UpdateIdSrvClientDTO
            {
                Id = ids[1],
                Name = "n",
                Secret = "p",
                Uri = "u"
            });
            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual(i == 2 ? "n" : $"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual(i == 2 ? "u" : $"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual(i == 2 ? "p" : $"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(false, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public async Task UpdateAsync_ReturnNotFound_When_PassingNotExisintClientId_And_DbNotContainsAnyClient()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.UpdateAsync(new UpdateIdSrvClientDTO
            {
                Id = Guid.NewGuid(),
                Name = "n",
                Secret = "s"
            });
            Assert.AreEqual(RepositoryResponse.NotFound, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(0, clients.Count());
            }
        }

        [Test]
        public async Task UpdateAsync_ReturnNotFound_And_DoNotChangeClientsInDb_When_PassingNotExisintClientId()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            var notExisintGuid = Guid.NewGuid();
            while (ids.Contains(notExisintGuid))
            {
                notExisintGuid = Guid.NewGuid();
            }

            RepositoryResponse response = await repository.UpdateAsync(new UpdateIdSrvClientDTO
            {
                Id = notExisintGuid,
                Name = "n",
                Secret = "p",
                Uri = "u"
            });

            Assert.AreEqual(RepositoryResponse.NotFound, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual($"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(false, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public async Task UpdateAsync_ReturnConflict_And_DoNotChangeClientsInDb_When_PassingAlreadyExistingName()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.UpdateAsync(new UpdateIdSrvClientDTO
            {
                Id = ids[1],
                Name = "n3",
                Secret = "p",
                Uri = "u"
            });

            Assert.AreEqual(RepositoryResponse.Conflict, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual($"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(false, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public void ChangeBlockingAsync_ThrowsArgumentNullException_When_PassingNullAsDto()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            Assert.ThrowsAsync<ArgumentNullException>(() => repository.ChangeBlockingAsync(null));
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_BlocksClientInDb_When_BlockingExistingNotBlockedClient()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvClientBlockDTO
            {
                Id = ids[1],
                IsBlocked = true
            });

            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual($"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(i == 2, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_DoNotChangeClientInDb_When_UnblockingExistingNotBlockedClient()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvClientBlockDTO
            {
                Id = ids[1],
                IsBlocked = false
            });

            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual($"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(false, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_UnblocksClientInDb_When_UnblockingExistingBlockedClient()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2, isBlocked: true);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvClientBlockDTO
            {
                Id = ids[1],
                IsBlocked = false
            });

            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual($"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(false, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnSuccess_And_DoNotChangeClientInDb_When_BlockingExistingBlockedClient()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2, isBlocked: true);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvClientBlockDTO
            {
                Id = ids[1],
                IsBlocked = true
            });

            Assert.AreEqual(RepositoryResponse.Success, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual($"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(i == 2, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnNotFound_And_DoNotChangeClientsInDb_When_PassingNotExistingClientId()
        {
            Guid[] ids = new Guid[3];
            ids[0] = await this.InsertDefaultClient(1);
            ids[1] = await this.InsertDefaultClient(2);
            ids[2] = await this.InsertDefaultClient(3);
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            var notExisintGuid = Guid.NewGuid();
            while (ids.Contains(notExisintGuid))
            {
                notExisintGuid = Guid.NewGuid();
            }

            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvClientBlockDTO
            {
                Id = notExisintGuid,
                IsBlocked = true
            });

            Assert.AreEqual(RepositoryResponse.NotFound, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(3, clients.Count());
                for (int i = 1; i <= 3; ++i)
                {
                    Assert.AreEqual(ids[i - 1], clients.ElementAt(i - 1).Id);
                    Assert.AreEqual($"n{i}", clients.ElementAt(i - 1).Name);
                    Assert.AreEqual($"u{i}", clients.ElementAt(i - 1).Uri);
                    Assert.AreEqual($"p{i}", clients.ElementAt(i - 1).Secret);
                    Assert.AreEqual(false, clients.ElementAt(i - 1).IsBlocked);
                }
            }
        }

        [Test]
        public async Task ChangeBlockingAsync_ReturnNotFound_And_DoNotChangeClientsInDb_When_ClientDbIsEmpty()
        {
            var repository = new SqlCeClientRepository(this.ConnectionFactory);
            RepositoryResponse response = await repository.ChangeBlockingAsync(new IdSrvClientBlockDTO
            {
                Id = Guid.NewGuid(),
                IsBlocked = true
            });

            Assert.AreEqual(RepositoryResponse.NotFound, response);
            using (IDbConnection connection = await this.ConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                this.ConnectionFactory = new SqlCeConnectionFactory(this.TestConnectionString);
                IEnumerable<IdSrvClientDTO> clients = await db.Query("Clients").GetAsync<IdSrvClientDTO>();
                Assert.AreEqual(0, clients.Count());
            }
        }

        #endregion

        // TODO: add scenario tests
    }
}
