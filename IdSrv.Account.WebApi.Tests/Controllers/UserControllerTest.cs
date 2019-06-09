namespace IdSrv.Account.WebApi.Tests.Controllers
{
    using System;
    using NUnit.Framework;
    using Moq;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using IdSrv.Account.WebApi.Controllers;
    using System.Collections.Generic;
    using IdSrv.Account.Models;
    using System.Threading.Tasks;

    [TestFixture]
    class UserControllerTest
    {
        public Mock<IUserRepository> UserRepository { get; set; }

        [SetUp]
        public void SetUp()
        {
            UserRepository = new Mock<IUserRepository>();
        }

        [Test]
        public void Ctor_DoesNotThrow_When_PassingNotNullUserRepository()
        {
            Assert.DoesNotThrow(() => new UserController(this.UserRepository.Object));
        }

        [Test]
        public void Ctor_ThrowArgumentNullException_When_PassingNullInsteadUserRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new UserController(null));
        }
    }
}
