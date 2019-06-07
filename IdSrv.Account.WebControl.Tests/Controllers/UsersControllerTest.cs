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

        [Test]
        public void Ctor_Throw_NullReferenceException_When_PassingNullInsteadAccountService()
        {
            Assert.Throws<NullReferenceException>(() => new UsersController(null));
        }

        [Test]
        public async Task Index_ReturnViewWithUsers_When_AccountServiceReturnAnyUsersCollection()
        {
            Func<IEnumerable<IdSrvUserDTO>, Task> testWhenServiceReturns = async (users) =>
            {
                this.AccountServiceMock.Setup(v => v.GetUsersAsync()).ReturnsAsync(users);
                var controller = new UsersController(this.AccountServiceMock.Object);
                ViewResult viewResult = await controller.Index();
                object actualUsers = controller.ViewData.Model;
                Assert.NotNull(viewResult);
                Assert.IsEmpty(viewResult.ViewName);
                Assert.IsInstanceOf<IEnumerable<IdSrvUserDTO>>(actualUsers);
                Assert.AreEqual(actualUsers, users);
            };
            await testWhenServiceReturns(new []
            {
                new IdSrvUserDTO { UserName = "u1", Id = new Guid(), Enabled = true },
                new IdSrvUserDTO { UserName = "u2", Id = new Guid(), Enabled = true },
                new IdSrvUserDTO { UserName = "u3", Id = new Guid(), Enabled = false },
            });
            await testWhenServiceReturns(new IdSrvUserDTO[] { });
        }

        [Test]
        public void Create_ReturnNotNullViewResult_When_NoArgs()
        {
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = controller.Create();
            Assert.NotNull(viewResult);
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
        public async Task Create_RedirectToIndex_When_AccountServiceCanCreateUser()
        {
            this.AccountServiceMock.Setup(v => v.CreateUserAsync(It.IsAny<NewIdSrvUserDTO>())).Returns(Task.FromResult(true));
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.Create(new NewIdSrvUserDTO { UserName = "u1", Password = "p1" });
            Assert.AreEqual(nameof(controller.Index), viewResult.ViewName);
        }

        [Test]
        public async Task Create_ReturnSelf_With_InvalilModelState_And_PassedModel_When_AccountServiceCanNotCreateUser()
        {
            var newUser = new NewIdSrvUserDTO { UserName = "u1", Password = "p1" };
            this.AccountServiceMock.Setup(v => v.CreateUserAsync(It.IsAny<NewIdSrvUserDTO>())).Returns(Task.FromResult(false));
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.Create(newUser);
            Assert.NotNull(viewResult);
            Assert.AreEqual(string.Empty, viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(controller.ViewData.Model, newUser);
        }
        [Test]
        public void UpdatePassword_ReturnSelf_With_ModelContainsPassedUserIdAndNullPasswords_When_NoArgs()
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
        public async Task ChangePassword_CallSeriveChangePasswordForUser_When_PassingModel()
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
            this.AccountServiceMock.Verify(v => v.ChangePasswordForUserAsync(passwords), Times.Once);
        }

        [Test]
        public async Task ChangePassword_RedirectToIndex_With_TempDataContainsNoError_When_ServiceReturnTrue()
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
            Assert.AreEqual(nameof(controller.Index), viewResult.ViewName);
            Assert.IsTrue(controller.TempData.ContainsKey("_IsError"));
            Assert.IsFalse(controller.TempData["_IsError"] as bool?);
        }

        [Test]
        public async Task ChangePassword_ReturnSelf_With_ModelIsInvalidAndHasNullPasswords_When_ServiceReturnFalseOrDiffPasswords()
        {
            Func<ChangeIdSrvUserPasswordDTO, Task> testWithPasswordsModel = async (passwords) =>
            {
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
            };
            await testWithPasswordsModel(new ChangeIdSrvUserPasswordDTO
            {
                UserId = new Guid(),
                OldPassword = "a",
                NewPassword = "b",
                RepeatNewPassword = "b"
            });
            await testWithPasswordsModel(new ChangeIdSrvUserPasswordDTO
            {
                UserId = new Guid(),
                OldPassword = "a",
                NewPassword = "b",
                RepeatNewPassword = "c"
            });
        }

        [Test]
        public async Task Delete_CallServiceDeleteUser()
        {
            var userId = new Guid();
            this.AccountServiceMock.Setup(v => v.DeleteUserAsync(userId)).ReturnsAsync(true);
            var controller = new UsersController(this.AccountServiceMock.Object);
            ViewResult viewResult = await controller.Delete(userId);
            this.AccountServiceMock.Verify(v => v.DeleteUserAsync(userId), Times.Once);
        }

        [Test]
        public async Task Delete_RedirectToIndex_When_ServiceReturnsAny()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var userId = new Guid();
                this.AccountServiceMock.Setup(v => v.DeleteUserAsync(userId)).ReturnsAsync(true);
                var controller = new UsersController(this.AccountServiceMock.Object);
                ViewResult viewResult = await controller.Delete(userId);
                Assert.AreEqual(nameof(controller.Index), viewResult.ViewName);
            };
            await testWithServiceReturns(true);
            await testWithServiceReturns(false);
        }

        [Test]
        public async Task Delete_PutErrorStatusToTempData_When_ServiceReturnsThisErrorStatus()
        {
            Func<bool, Task> testWithServiceReturns = async (bool status) =>
            {
                var userId = new Guid();
                this.AccountServiceMock.Setup(v => v.DeleteUserAsync(userId)).ReturnsAsync(status);
                var controller = new UsersController(this.AccountServiceMock.Object);
                ViewResult viewResult = await controller.Delete(userId);
                Assert.IsInstanceOf<bool?>(controller.TempData["_IsError"]);
                Assert.AreEqual(!status, controller.TempData["_IsError"] as bool?);
                Assert.AreEqual(nameof(controller.Index), viewResult.ViewName);
            };
            await testWithServiceReturns(true);
            await testWithServiceReturns(false);
        }
        // TODO: обработка ошибок сервиса, если он вернёт null или кинет исключение
    }
}
