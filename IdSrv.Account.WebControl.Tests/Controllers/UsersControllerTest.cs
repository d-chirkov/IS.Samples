namespace IdSrv.Account.WebControl.Controllers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class UsersControllerTest
    {
        public Mock<IUserService> UserServiceMock { get; set; }

        [SetUp]
        public void SetUp()
        {
            this.UserServiceMock = new Mock<IUserService>();
        }

        [Test]
        public void Ctor_DoesNotThrow_When_PassingAnyUserService()
        {
            Assert.DoesNotThrow(() => new UsersController(this.UserServiceMock.Object));
        }

        [Test]
        public void Ctor_ThrowArgumentNullException_When_PassingNullInsteadUserService()
        {
            Assert.Throws<ArgumentNullException>(() => new UsersController(null));
        }

        [Test]
        public async Task Index_ReturnViewWithUsers_When_UserServiceReturnAnyUsersCollection()
        {
            Func<IEnumerable<IdSrvUserDto>, Task> testWhenServiceReturns = async (users) =>
            {
                this.UserServiceMock.Setup(v => v.GetUsersAsync()).ReturnsAsync(users);
                var controller = new UsersController(this.UserServiceMock.Object);
                ViewResult viewResult = await controller.Index();
                object actualUsers = controller.ViewData.Model;
                Assert.NotNull(viewResult);
                Assert.IsEmpty(viewResult.ViewName);
                Assert.IsInstanceOf<IEnumerable<IdSrvUserDto>>(actualUsers);
                Assert.AreEqual(users, actualUsers);
            };
            await testWhenServiceReturns(new[]
            {
                new IdSrvUserDto(),
                new IdSrvUserDto()
            });
            await testWhenServiceReturns(new IdSrvUserDto[] { });
        }

        [Test]
        public void Create_ReturnNotNullViewResult_When_NoArgs()
        {
            var controller = new UsersController(this.UserServiceMock.Object);
            ViewResult viewResult = controller.Create();
            Assert.NotNull(viewResult);
        }

        [Test]
        public async Task Create_CallServiceCreateUser_When_PassingNewUser()
        {
            var newUser = new NewIdSrvUserDto { UserName = "u1", Password = "p1" };
            this.UserServiceMock.Setup(v => v.CreateUserAsync(newUser)).ReturnsAsync(true);
            var controller = new UsersController(this.UserServiceMock.Object);
            await controller.Create(newUser);
            this.UserServiceMock.Verify(v => v.CreateUserAsync(newUser), Times.Once);
        }

        [Test]
        public async Task Create_RedirectToIndex_When_UserServiceCanCreateUser()
        {
            this.UserServiceMock.Setup(v => v.CreateUserAsync(It.IsAny<NewIdSrvUserDto>())).Returns(Task.FromResult(true));
            var controller = new UsersController(this.UserServiceMock.Object);
            ActionResult result = await controller.Create(new NewIdSrvUserDto { UserName = "u1", Password = "p1" });
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual(nameof(controller.Index), (result as RedirectToRouteResult).RouteValues["action"]);
        }

        [Test]
        public async Task Create_ReturnSelf_With_InvalilModelState_And_PassedModel_When_UserServiceCanNotCreateUser()
        {
            var newUser = new NewIdSrvUserDto { UserName = "u1", Password = "p1" };
            this.UserServiceMock.Setup(v => v.CreateUserAsync(It.IsAny<NewIdSrvUserDto>())).Returns(Task.FromResult(false));
            var controller = new UsersController(this.UserServiceMock.Object);
            ActionResult result = await controller.Create(newUser);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual(string.Empty, viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(controller.ViewData.Model, newUser);
        }

        [Test]
        public void UpdatePassword_ReturnSelf_With_ModelContainsPassedUserIdAndNullPassword_When_NoArgs()
        {
            var passwords = new IdSrvUserPasswordDto { Id = new Guid() };
            var controller = new UsersController(this.UserServiceMock.Object);
            ViewResult viewResult = controller.ChangePassword(passwords.Id);
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName); // empty view name means asp.net returns the same view
            object model = controller.ViewData.Model;
            Assert.IsInstanceOf<IdSrvUserPasswordDto>(model);
            var actualModel = model as IdSrvUserPasswordDto;
            Assert.AreEqual(actualModel.Id, passwords.Id);
            Assert.IsNull(actualModel.Password);
        }

        [Test]
        public async Task ChangePassword_CallServiceChangePasswordForUser_When_PassingModel()
        {
            var passwords = new IdSrvUserPasswordDto();
            this.UserServiceMock.Setup(v => v.ChangePasswordForUserAsync(passwords)).ReturnsAsync(true);
            var controller = new UsersController(this.UserServiceMock.Object);
            await controller.ChangePassword(passwords);
            this.UserServiceMock.Verify(v => v.ChangePasswordForUserAsync(passwords), Times.Once);
        }

        [Test]
        public async Task ChangePassword_RedirectToIndex_With_TempDataContainsNoError_When_ServiceReturnTrue()
        {
            var passwords = new IdSrvUserPasswordDto();
            this.UserServiceMock.Setup(v => v.ChangePasswordForUserAsync(passwords)).ReturnsAsync(true);
            var controller = new UsersController(this.UserServiceMock.Object);
            ActionResult result = await controller.ChangePassword(passwords);
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            var redirectResult = result as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(nameof(controller.Index), redirectResult.RouteValues["action"]);
            Assert.IsTrue(controller.TempData.ContainsKey("_IsError"));
            Assert.IsFalse(controller.TempData["_IsError"] as bool?);
        }

        [Test]
        public async Task ChangePassword_ReturnSelf_With_ModelIsInvalidAndHasNullPassword_When_ServiceReturnFalse()
        {
            var passwords = new IdSrvUserPasswordDto
            {
                Id = new Guid(),
                Password = "b",
            };
            this.UserServiceMock.Setup(v => v.ChangePasswordForUserAsync(passwords)).ReturnsAsync(false);
            var controller = new UsersController(this.UserServiceMock.Object);
            ActionResult result = await controller.ChangePassword(passwords);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName);
            object model = controller.ViewData.Model;
            Assert.IsInstanceOf<IdSrvUserPasswordDto>(model);
            var actualModel = model as IdSrvUserPasswordDto;
            Assert.AreEqual(actualModel.Id, passwords.Id);
            Assert.IsNull(actualModel.Password);
            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [Test]
        public async Task Delete_CallServiceDeleteUser()
        {
            var userId = new Guid();
            this.UserServiceMock.Setup(v => v.DeleteUserAsync(userId)).ReturnsAsync(true);
            var controller = new UsersController(this.UserServiceMock.Object);
            await controller.Delete(userId);
            this.UserServiceMock.Verify(v => v.DeleteUserAsync(userId), Times.Once);
        }

        [Test]
        public async Task Delete_RedirectToIndex_When_ServiceReturnsAny()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var userId = new Guid();
                this.UserServiceMock.Setup(v => v.DeleteUserAsync(userId)).ReturnsAsync(what);
                var controller = new UsersController(this.UserServiceMock.Object);
                ActionResult result = await controller.Delete(userId);
                Assert.IsInstanceOf<RedirectToRouteResult>(result);
                var redirectResult = result as RedirectToRouteResult;
                Assert.AreEqual(nameof(controller.Index), redirectResult.RouteValues["action"]);
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
                this.UserServiceMock.Setup(v => v.DeleteUserAsync(userId)).ReturnsAsync(status);
                var controller = new UsersController(this.UserServiceMock.Object);
                await controller.Delete(userId);
                Assert.IsTrue(controller.TempData.ContainsKey("_IsError"));
                Assert.IsInstanceOf<bool?>(controller.TempData["_IsError"]);
                Assert.AreEqual(!status, controller.TempData["_IsError"] as bool?);
            };
            await testWithServiceReturns(true);
            await testWithServiceReturns(false);
        }

        [Test]
        public async Task Block_CallServiceChangeBlock()
        {
            var userId = new Guid();
            IdSrvUserBlockDto block = null;
            this.UserServiceMock
                .Setup(v => v.ChangeBlock(It.IsAny<IdSrvUserBlockDto>()))
                .ReturnsAsync(true)
                .Callback<IdSrvUserBlockDto>(r => block = r);
            var controller = new UsersController(this.UserServiceMock.Object);
            await controller.Block(userId);
            Assert.NotNull(block);
            Assert.AreEqual(userId, block.Id);
            Assert.IsTrue(block.IsBlocked);
        }

        [Test]
        public async Task Block_RedirectToIndex_When_ServiceReturnsAny()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var userId = new Guid();
                this.UserServiceMock.Setup(v => v.ChangeBlock(It.IsAny<IdSrvUserBlockDto>())).ReturnsAsync(what);
                var controller = new UsersController(this.UserServiceMock.Object);
                ActionResult result = await controller.Block(userId);
                Assert.IsInstanceOf<RedirectToRouteResult>(result);
                var redirectResult = result as RedirectToRouteResult;
                Assert.AreEqual(nameof(controller.Index), redirectResult.RouteValues["action"]);
            };
            await testWithServiceReturns(true);
            await testWithServiceReturns(false);
        }

        [Test]
        public async Task Block_PutErrorStatusToTempData_When_ServiceReturnsThisErrorStatus()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var userId = new Guid();
                this.UserServiceMock.Setup(v => v.ChangeBlock(It.IsAny<IdSrvUserBlockDto>())).ReturnsAsync(what);
                var controller = new UsersController(this.UserServiceMock.Object);
                await controller.Block(userId);
                Assert.IsTrue(controller.TempData.ContainsKey("_IsError"));
                Assert.IsInstanceOf<bool?>(controller.TempData["_IsError"]);
                Assert.AreEqual(!what, controller.TempData["_IsError"] as bool?);
            };
            await testWithServiceReturns(true);
            await testWithServiceReturns(false);
        }

        [Test]
        public async Task Unblock_CallServiceChangeBlock()
        {
            var userId = new Guid();
            IdSrvUserBlockDto block = null;
            this.UserServiceMock
                .Setup(v => v.ChangeBlock(It.IsAny<IdSrvUserBlockDto>()))
                .ReturnsAsync(true)
                .Callback<IdSrvUserBlockDto>(r => block = r);
            var controller = new UsersController(this.UserServiceMock.Object);
            await controller.Unblock(userId);
            Assert.NotNull(block);
            Assert.AreEqual(userId, block.Id);
            Assert.IsFalse(block.IsBlocked);
        }

        [Test]
        public async Task Unblock_RedirectToIndex_When_ServiceReturnsAny()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var userId = new Guid();
                this.UserServiceMock.Setup(v => v.ChangeBlock(It.IsAny<IdSrvUserBlockDto>())).ReturnsAsync(what);
                var controller = new UsersController(this.UserServiceMock.Object);
                ActionResult result = await controller.Unblock(userId);
                Assert.IsInstanceOf<RedirectToRouteResult>(result);
                var redirectResult = result as RedirectToRouteResult;
                Assert.AreEqual(nameof(controller.Index), redirectResult.RouteValues["action"]);
            };
            await testWithServiceReturns(true);
            await testWithServiceReturns(false);
        }

        [Test]
        public async Task Unblock_PutErrorStatusToTempData_When_ServiceReturnsThisErrorStatus()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var userId = new Guid();
                this.UserServiceMock.Setup(v => v.ChangeBlock(It.IsAny<IdSrvUserBlockDto>())).ReturnsAsync(what);
                var controller = new UsersController(this.UserServiceMock.Object);
                await controller.Unblock(userId);
                Assert.IsTrue(controller.TempData.ContainsKey("_IsError"));
                Assert.IsInstanceOf<bool?>(controller.TempData["_IsError"]);
                Assert.AreEqual(!what, controller.TempData["_IsError"] as bool?);
            };
            await testWithServiceReturns(true);
            await testWithServiceReturns(false);
        }

        // TODO: обработка ошибок сервиса, если он вернёт null или кинет исключение
    }
}
