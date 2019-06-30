namespace IdSrv.Account.WebApi.Infrastructure.Tests
{
    using System;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Threading.Tasks;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.IntegrationTests;
    using NUnit.Framework;

    [TestFixture]
    internal class SqlCeConnectionFactoryTest
    {
        public string TestConnectionString { get; set; } = $"Data Source={TestHelper.GetPathToTestDb()}";

        [Test]
        public void Ctor_DoesNotThrow_When_AnyNotNullStringPassed()
        {
            Assert.DoesNotThrow(() => new SqlCeConnectionFactory("a"));
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_When_NullStringPassed()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlCeConnectionFactory(null));
        }

        [Test]
        public void GetConnectionAsync_DoesNoThrow_When_PassingRealConnectionStringToCtor()
        {
            var factory = new SqlCeConnectionFactory(this.TestConnectionString);
            Assert.DoesNotThrowAsync(async () =>
            {
                using (await factory.GetConnectionAsync())
                {
                }
            });
        }

        [Test]
        public async Task GetConnectionAsync_ReturnOpenedConnection_When_PassingRealConnectionStringToCtor()
        {
            var factory = new SqlCeConnectionFactory(this.TestConnectionString);
            using (IDbConnection connection = await factory.GetConnectionAsync())
            {
                Assert.AreEqual(ConnectionState.Open, connection.State);
            }
        }

        [Test]
        public void GetConnectionAsync_Throws_When_PassingRealConnectionStringToCtor()
        {
            var factory = new SqlCeConnectionFactory(@"Data Source=C:\Users\test_compact_db.sdf");
            Assert.ThrowsAsync<SqlCeException>(async () =>
                {
                    using (IDbConnection connection = await factory.GetConnectionAsync())
                    {
                        Assert.AreEqual(ConnectionState.Open, connection.State);
                    }
                }
            );
        }
    }
}
