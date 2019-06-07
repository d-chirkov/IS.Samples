namespace IdSrv.Account.WebControl.Tests.Controllers
{
    using System;
    using NUnit.Framework;
    using Moq;
    using IdSrv.Account.WebControl.Controllers;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;
    using System.Collections.Generic;
    using IdSrv.Account.WebControl.Models;
    using System.Web.Mvc;
    using System.Threading.Tasks;

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

        private async Task _Index_ReturnViewWithUsers(List<IdSrvUserDTO> users)
        {
            this.AccountServiceMock.Setup(v => v.GetUsersAsync()).ReturnsAsync(users);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.Index();
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
        public async Task Create_RedirectToIndex_When_NewUserCreatedByService()
        {
            var newUser = new NewIdSrvUserDTO { UserName = "u1", Password = "p1" };
            this.AccountServiceMock.Setup(v => v.CreateUserAsync(newUser)).Returns(Task.FromResult(true));
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.Create(newUser);
            Assert.AreEqual(viewResult.ViewName, nameof(controller.Index));
        }

        [Test]
        public async Task Create_CallSeriveseCreateUser_When_PassingNewUser()
        {
            var newUser = new NewIdSrvUserDTO { UserName = "u1", Password = "p1" };
            this.AccountServiceMock.Setup(v => v.CreateUserAsync(newUser)).ReturnsAsync(true);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.Create(newUser);
            this.AccountServiceMock.Verify(v => v.CreateUserAsync(newUser), Times.Once);
        }

        [Test]
        public async Task Create_ReturnSelfWithPassedModel_When_ModelIsInvalid()
        {
            var newUser = new NewIdSrvUserDTO { UserName = "u1", Password = "p1" };
            this.AccountServiceMock.Setup(v => v.CreateUserAsync(newUser)).ReturnsAsync(true);
            var controller = new UsersController(this.AccountServiceMock.Object);
            controller.ModelState.AddModelError("", "error");
            ViewResult viewResult = await controller.Create(newUser);
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName);
            this.AccountServiceMock.Verify(v => v.CreateUserAsync(It.IsAny<NewIdSrvUserDTO>()), Times.Never);
        }

        [Test]
        public async Task Create_ReturnViewWithPassedModel_When_ServiseReturnFalse()
        {
            var newUser = new NewIdSrvUserDTO { UserName = "u1", Password = "p1" };
            this.AccountServiceMock.Setup(v => v.CreateUserAsync(newUser)).ReturnsAsync(false);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.Create(newUser);
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName); // empty view name means asp.net returns the same view
            Assert.AreEqual(controller.ViewData.Model, newUser);
        }

        [Test]
        public void UpdatePassword_ReturnSelf_WithModelContainsPassedUserIdAndNullPasswords_When_NoArgs()
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

        [Test]
        public async Task ChangePassword_RedirectToIndex_CallSerive_When_ServiceReturnTrue()
        {
            var passwords = new ChangeIdSrvUserPasswordDTO
            {
                UserId = new Guid(),
                OldPassword = "a",
                NewPassword = "b",
                RepeatNewPassword = "b"
            };
            this.AccountServiceMock.Setup(v => v.ChangePasswordForUserAsync(passwords)).ReturnsAsync(true);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.ChangePassword(passwords);
            Assert.NotNull(viewResult);
            Assert.AreEqual(viewResult.ViewName, nameof(controller.Index));
            this.AccountServiceMock.Verify(v => v.ChangePasswordForUserAsync(passwords), Times.Once);
        }

        [Test]
        public async Task ChangePassword_ReturnSelf_WithPassedModel_WithoutPasswords_WithInvlaidModelState_CallService_When_ServiceReturnFalse()
        {
            var passwords = new ChangeIdSrvUserPasswordDTO
            {
                UserId = new Guid(),
                OldPassword = "a",
                NewPassword = "b",
                RepeatNewPassword = "b"
            };
            this.AccountServiceMock.Setup(v => v.ChangePasswordForUserAsync(passwords)).ReturnsAsync(false);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.ChangePassword(passwords);
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName);
            var model = controller.ViewData.Model;
            Assert.IsInstanceOf<ChangeIdSrvUserPasswordDTO>(model);
            var actualModel = model as ChangeIdSrvUserPasswordDTO;
            Assert.AreEqual(actualModel.UserId, passwords.UserId);
            Assert.IsNull(actualModel.OldPassword);
            Assert.IsNull(actualModel.NewPassword);
            Assert.IsNull(actualModel.RepeatNewPassword);
            Assert.IsFalse(controller.ModelState.IsValid);
            this.AccountServiceMock.Verify(v => v.ChangePasswordForUserAsync(passwords), Times.Once);
        }

        [Test]
        public async Task ChangePassword_ReturnSelf_WithPassedModel_WithoutPasswords_WithInvlaidModelState_When_NewPasswordsAreDifferent()
        {
            var passwords = new ChangeIdSrvUserPasswordDTO
            {
                UserId = new Guid(),
                OldPassword = "a",
                NewPassword = "b",
                RepeatNewPassword = "c"
            };
            this.AccountServiceMock.Setup(v => v.ChangePasswordForUserAsync(passwords)).ReturnsAsync(false);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.ChangePassword(passwords);
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName);
            var model = controller.ViewData.Model;
            Assert.IsInstanceOf<ChangeIdSrvUserPasswordDTO>(model);
            var actualModel = model as ChangeIdSrvUserPasswordDTO;
            Assert.AreEqual(actualModel.UserId, passwords.UserId);
            Assert.IsNull(actualModel.OldPassword);
            Assert.IsNull(actualModel.NewPassword);
            Assert.IsNull(actualModel.RepeatNewPassword);
            Assert.IsFalse(controller.ModelState.IsValid);
            this.AccountServiceMock.Verify(v => v.ChangePasswordForUserAsync(passwords), Times.Never);
        }

        // TODO: обработка ошибок сервиса, если он вернёт null или кинет исключение
    }
}
