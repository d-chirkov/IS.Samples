namespace IdSrv.Account.WebControl.Tests.Controllers
{
    using System;
    using NUnit.Framework;
    using Moq;
    using IdSrv.Account.WebControl.Controllers;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;
    using System.Collections.Generic;
    using IdSrv.Account.WebControl.Infrastructure.Entities;
    using System.Web.Mvc;

    [TestFixture]
    public class UsersControllerTest
    {
        public Mock<IAccountService> AccountServiceMock { get; set; }
        [SetUp]
        public void SetUp()
        {
            AccountServiceMock = new Mock<IAccountService>();
        }

        [Test]
        public void Ctor_DoesNotThrow_When_PassingAnyAccountService()
        {
            Assert.DoesNotThrow(() => new UsersController(this.AccountServiceMock.Object));
        }

        private void _Index_ReturnViewWithUsers(List<IdSrvUser> users)
        {
            this.AccountServiceMock.Setup(v => v.GetUsers()).Returns(users);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = controller.Index();
            object actualUsers = controller.ViewData.Model;
            Assert.NotNull(viewResult);
            Assert.IsInstanceOf<IEnumerable<IdSrvUser>>(actualUsers);
            Assert.AreEqual(actualUsers, users);
        }

        [Test]
        public void Index_ReturnViewWithUsers_When_AccountServiceReturnNotEmptyUsersCollection()
        {
            var users = new List<IdSrvUser>()
            {
                new IdSrvUser { UserName = "u1", Id = new Guid(), Enabled = true },
                new IdSrvUser { UserName = "u2", Id = new Guid(), Enabled = true },
                new IdSrvUser { UserName = "u3", Id = new Guid(), Enabled = false },
            };
            _Index_ReturnViewWithUsers(users);
        }

        [Test]
        public void Index_ReturnViewWithNoUsers_When_AccountServiceReturnEmptyUsersCollection()
        {
            _Index_ReturnViewWithUsers(new List<IdSrvUser>());
        }

        // TODO: обработка ошибок сервиса, если он вернёт null или кинет исключение
    }
}
