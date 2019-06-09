namespace IdSrv.Account.WebApi.Infrastructure.Tests
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using IdSrv.Account.WebApi.Infrastructure;
    using System.Data.SqlServerCe;

    [TestFixture]
    class SqlCompactConnectionFactoryTest
    {
        public string TestConnectionString { get; set; } = @"Data Source=C:\Users\dchir\source\repos\IS.Samples\IdSrv.Account.WebApi.IntegrationTests\Infrastructure\test_compact_db.sdf";

        [Test]
        public void Ctor_DoesNotThrow_When_AnyNotNullStringPassed()
        {
            Assert.DoesNotThrow(() => new SqlCompactConnectionFactory("a"));
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_When_NullStringPassed()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlCompactConnectionFactory(null));
        }

        [Test]
        public void GetConnectionAsync_DoesNoThrow_When_PassingRealConnectionStringToCtor()
        {
            var factory = new SqlCompactConnectionFactory(this.TestConnectionString);
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
            var factory = new SqlCompactConnectionFactory(this.TestConnectionString);
            using (IDbConnection connection = await factory.GetConnectionAsync())
            {
                Assert.AreEqual(ConnectionState.Open, connection.State);
            }
        }

        [Test]
        public void GetConnectionAsync_Throws_When_PassingRealConnectionStringToCtor()
        {
            var factory = new SqlCompactConnectionFactory(@"Data Source=C:\Users\test_compact_db.sdf");
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
