﻿namespace IdSrv.Account.WebControl.Tests.Controllers
{
    using System;
    using NUnit.Framework;
    using Moq;
    using IdSrv.Account.WebControl.Controllers;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;
    using System.Collections.Generic;
    using IdSrv.Account.WebControl.Models;
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

        private void _Index_ReturnViewWithUsers(List<IdSrvUserDTO> users)
        {
            this.AccountServiceMock.Setup(v => v.GetUsers()).Returns(users);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = controller.Index();
            object actualUsers = controller.ViewData.Model;
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName);
            Assert.IsInstanceOf<IEnumerable<IdSrvUserDTO>>(actualUsers);
            Assert.AreEqual(actualUsers, users);
        }

        [Test]
        public void Index_ReturnViewWithUsers_When_AccountServiceReturnNotEmptyUsersCollection()
        {
            var users = new List<IdSrvUserDTO>()
            {
                new IdSrvUserDTO { UserName = "u1", Id = new Guid(), Enabled = true },
                new IdSrvUserDTO { UserName = "u2", Id = new Guid(), Enabled = true },
                new IdSrvUserDTO { UserName = "u3", Id = new Guid(), Enabled = false },
            };
            _Index_ReturnViewWithUsers(users);
        }

        [Test]
        public void Index_ReturnViewWithNoUsers_When_AccountServiceReturnEmptyUsersCollection()
        {
            _Index_ReturnViewWithUsers(new List<IdSrvUserDTO>());
        }

        [Test]
        public void Create_ReturnEmptyView_When_NoArgs()
        {
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = controller.Create();
            Assert.NotNull(viewResult);
        }

        [Test]
        public void Create_RedirectToIndex_When_NewUserCreatedByService()
        {
            var newUser = new NewIdSrvUserDTO { UserName = "u1", Password = "p1" };
            this.AccountServiceMock.Setup(v => v.CreateUser(newUser)).Returns(true);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = controller.Create(newUser);
            Assert.AreEqual(viewResult.ViewName, nameof(controller.Index));
        }

        [Test]
        public void Create_CallSeriveseCreateUser_When_PassingNewUser()
        {
            var newUser = new NewIdSrvUserDTO { UserName = "u1", Password = "p1" };
            this.AccountServiceMock.Setup(v => v.CreateUser(newUser)).Returns(true);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = controller.Create(newUser);
            this.AccountServiceMock.Verify(v => v.CreateUser(newUser), Times.Once);
        }

        [Test]
        public void Create_ReturnViewWithPassedModel_When_ServiseReturnFalse()
        {
            var newUser = new NewIdSrvUserDTO { UserName = "u1", Password = "p1" };
            this.AccountServiceMock.Setup(v => v.CreateUser(newUser)).Returns(false);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = controller.Create(newUser);
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName); // empty view name means asp.net returns the same view
            Assert.AreEqual(controller.ViewData.Model, newUser);
        }

        [Test]
        public void UpdatePassword_ReturnModelWithUserIdAndEmptyPasswords_When_NoArgs()
        {
            var passwords = new ChangeIdSrvUserPasswordDTO { UserId = new Guid() };
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = controller.ChangePassword(passwords.UserId);
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName); // empty view name means asp.net returns the same view
            object model = controller.ViewData.Model;
            Assert.IsInstanceOf<ChangeIdSrvUserPasswordDTO>(model);
            var actualModel = model as ChangeIdSrvUserPasswordDTO;
            Assert.AreEqual(actualModel.UserId, passwords.UserId);
            Assert.IsNull(actualModel.OldPassword);
            Assert.IsNull(actualModel.NewPassword);
            Assert.IsNull(actualModel.RepeatNewPassword);
        }

        // TODO: обработка ошибок сервиса, если он вернёт null или кинет исключение
    }
}
