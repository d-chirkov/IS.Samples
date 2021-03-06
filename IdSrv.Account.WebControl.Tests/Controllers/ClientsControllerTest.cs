﻿namespace IdSrv.Account.WebControl.Controllers.Tests
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
    public class ClientsControllerTest
    {
        public Mock<IClientService> ClientServiceMock { get; set; }

        [SetUp]
        public void SetUp()
        {
            this.ClientServiceMock = new Mock<IClientService>();
        }

        [Test]
        public void Ctor_DoesNotThrow_When_PassingAnyClientService()
        {
            Assert.DoesNotThrow(() => new ClientsController(this.ClientServiceMock.Object));
        }

        [Test]
        public void Ctor_ThrowArgumentNullException_When_PassingNullInsteadClientService()
        {
            Assert.Throws<ArgumentNullException>(() => new ClientsController(null));
        }

        [Test]
        public async Task Index_ReturnViewWithClients_When_ClientServiceReturnAnyClientsCollection()
        {
            Func<IEnumerable<IdSrvClientDto>, Task> testWhenServiceReturns = async (clients) =>
            {
                this.ClientServiceMock.Setup(v => v.GetClientsAsync()).ReturnsAsync(clients);
                var controller = new ClientsController(this.ClientServiceMock.Object);
                ViewResult viewResult = await controller.Index();
                object actualClients = controller.ViewData.Model;
                Assert.NotNull(viewResult);
                Assert.IsEmpty(viewResult.ViewName);
                Assert.IsInstanceOf<IEnumerable<IdSrvClientDto>>(actualClients);
                Assert.AreEqual(clients, actualClients);
            };
            await testWhenServiceReturns(new[]
            {
                new IdSrvClientDto(),
                new IdSrvClientDto()
            });
            await testWhenServiceReturns(new IdSrvClientDto[] { });
        }

        [Test]
        public void Create_ReturnNotNullViewResult_When_NoArgs()
        {
            var controller = new ClientsController(this.ClientServiceMock.Object);
            ViewResult viewResult = controller.Create();
            Assert.NotNull(viewResult);
        }

        [Test]
        public async Task Create_CallServiceCreateClient_When_PassingNewClient()
        {
            var newClient = new NewIdSrvClientDto { Name = "c", Secret = "s" };
            this.ClientServiceMock.Setup(v => v.CreateClientAsync(newClient)).ReturnsAsync(true);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            await controller.Create(newClient);
            this.ClientServiceMock.Verify(v => v.CreateClientAsync(newClient), Times.Once);
        }

        [Test]
        public async Task Create_RedirectToIndex_When_ClientServiceCanCreateClientUri()
        {
            this.ClientServiceMock
                .Setup(v => v.CreateClientAsync(It.IsAny<NewIdSrvClientDto>()))
                .Returns(Task.FromResult(true));
            var controller = new ClientsController(this.ClientServiceMock.Object);
            ActionResult result = await controller.Create(new NewIdSrvClientDto { Name = "c", Secret = "s" });
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual(nameof(controller.Index), (result as RedirectToRouteResult).RouteValues["action"]);
        }

        [Test]
        public async Task Create_ReturnSelf_With_InvalilModelState_And_PassedModel_When_ClientServiceCanNotCreateClient()
        {
            var newClient = new NewIdSrvClientDto { Name = "n", Secret = "s" };
            this.ClientServiceMock
                .Setup(v => v.CreateClientAsync(It.IsAny<NewIdSrvClientDto>()))
                .Returns(Task.FromResult(false));
            var controller = new ClientsController(this.ClientServiceMock.Object);
            ActionResult result = await controller.Create(newClient);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual(string.Empty, viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(newClient, controller.ViewData.Model);
        }

        [Test]
        public async Task Create_ReturnSelf_With_InvalilModelState_And_PassedModel_When_ModelIsInvalid()
        {
            var newClient = new NewIdSrvClientDto { };
            this.ClientServiceMock
                .Setup(v => v.CreateClientAsync(It.IsAny<NewIdSrvClientDto>()))
                .Returns(Task.FromResult(false));
            var controller = new ClientsController(this.ClientServiceMock.Object);
            controller.ModelState.AddModelError(string.Empty, string.Empty);
            ActionResult result = await controller.Create(newClient);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual(string.Empty, viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(newClient, controller.ViewData.Model);
        }

        [Test]
        public async Task Update_ReturnSelf_With_ClientReturnedFromRepo_OfType_UpdateIdSrvClientDTO_When_PassingClientId()
        {
            var clientId = Guid.NewGuid();
            var client = new IdSrvClientDto()
            {
                Id = Guid.NewGuid(),
                Name = "n",
                Secret = "s",
                Uri = "u"
            };

            this.ClientServiceMock
                .Setup(v => v.GetClientByIdAsync(clientId))
                .ReturnsAsync(client);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            ActionResult result = await controller.Update(clientId);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.AreEqual(string.Empty, viewResult.ViewName);
            Assert.IsInstanceOf<UpdateIdSrvClientDto>(controller.ViewData.Model);
            var model = controller.ViewData.Model as UpdateIdSrvClientDto;
            Assert.AreEqual(client.Id, model.Id);
            Assert.AreEqual(client.Name, model.Name);
            Assert.AreEqual(client.Secret, model.Secret);
            Assert.AreEqual(client.Uri, model.Uri);
        }

        [Test]
        public async Task Update_CallServiceGetClientById_When_PassingClientId()
        {
            var clientId = Guid.NewGuid();
            var client = new IdSrvClientDto();
            this.ClientServiceMock
                .Setup(v => v.GetClientByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(client);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            ActionResult result = await controller.Update(clientId);
            this.ClientServiceMock.Verify(v => v.GetClientByIdAsync(clientId));
        }

        [Test]
        public async Task Update_RedirectToIndex_With_TempDataCointainsError_When_RepositoryReturnNullClientForPassedId()
        {
            this.ClientServiceMock
                .Setup(v => v.GetClientByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(null as IdSrvClientDto);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            ActionResult result = await controller.Update(Guid.NewGuid());
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual(nameof(controller.Index), (result as RedirectToRouteResult).RouteValues["action"]);
            Assert.IsTrue(controller.TempData.ContainsKey("_IsError"));
            Assert.IsTrue(controller.TempData["_IsError"] as bool?);
        }

        [Test]
        public async Task Update_CallServiceUpdateForClient_When_PassingModel()
        {
            var model = new UpdateIdSrvClientDto();
            this.ClientServiceMock.Setup(v => v.UpdateClientAsync(model)).ReturnsAsync(true);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            await controller.Update(model);
            this.ClientServiceMock.Verify(v => v.UpdateClientAsync(model), Times.Once);
        }

        [Test]
        public async Task Update_RedirectToIndex_With_TempDataContainsNoError_When_ServiceReturnTrue()
        {
            this.ClientServiceMock.Setup(v => v.UpdateClientAsync(It.IsAny<UpdateIdSrvClientDto>())).ReturnsAsync(true);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            ActionResult result = await controller.Update(new UpdateIdSrvClientDto());
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            var redirectResult = result as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(nameof(controller.Index), redirectResult.RouteValues["action"]);
            Assert.IsTrue(controller.TempData.ContainsKey("_IsError"));
            Assert.IsFalse(controller.TempData["_IsError"] as bool?);
        }

        [Test]
        public async Task Update_ReturnSelf_With_InvalidModel_When_ServiceReturnFalse()
        {
            var model = new UpdateIdSrvClientDto() { Id = Guid.NewGuid() };
            this.ClientServiceMock.Setup(v => v.UpdateClientAsync(It.IsAny<UpdateIdSrvClientDto>())).ReturnsAsync(false);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            ActionResult result = await controller.Update(model);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName);
            object modelFromController = controller.ViewData.Model;
            Assert.IsInstanceOf<UpdateIdSrvClientDto>(modelFromController);
            var actualModel = modelFromController as UpdateIdSrvClientDto;
            Assert.AreEqual(actualModel, model);
            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [Test]
        public async Task Update_ReturnSelf_With_InvalidModel_When_PassingInvalidModel()
        {
            var model = new UpdateIdSrvClientDto() { Id = Guid.NewGuid() };
            this.ClientServiceMock.Setup(v => v.UpdateClientAsync(It.IsAny<UpdateIdSrvClientDto>())).ReturnsAsync(true);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            controller.ModelState.AddModelError(string.Empty, string.Empty);
            ActionResult result = await controller.Update(model);
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.IsEmpty(viewResult.ViewName);
            object modelFromController = controller.ViewData.Model;
            Assert.IsInstanceOf<UpdateIdSrvClientDto>(modelFromController);
            var actualModel = modelFromController as UpdateIdSrvClientDto;
            Assert.AreEqual(actualModel, model);
            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [Test]
        public async Task Delete_CallServiceDeleteClient()
        {
            var clientId = Guid.NewGuid();
            this.ClientServiceMock.Setup(v => v.DeleteClientAsync(clientId)).ReturnsAsync(true);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            await controller.Delete(clientId);
            this.ClientServiceMock.Verify(v => v.DeleteClientAsync(clientId), Times.Once);
        }

        [Test]
        public async Task Delete_RedirectToIndex_When_ServiceReturnsAny()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var clientId = Guid.NewGuid();
                this.ClientServiceMock.Setup(v => v.DeleteClientAsync(clientId)).ReturnsAsync(what);
                var controller = new ClientsController(this.ClientServiceMock.Object);
                ActionResult result = await controller.Delete(clientId);
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
                var ClientId = Guid.NewGuid();
                this.ClientServiceMock.Setup(v => v.DeleteClientAsync(ClientId)).ReturnsAsync(status);
                var controller = new ClientsController(this.ClientServiceMock.Object);
                await controller.Delete(ClientId);
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
            var clientId = Guid.NewGuid();
            IdSrvClientBlockDto block = null;
            this.ClientServiceMock
                .Setup(v => v.ChangeBlock(It.IsAny<IdSrvClientBlockDto>()))
                .ReturnsAsync(true)
                .Callback<IdSrvClientBlockDto>(r => block = r);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            await controller.Block(clientId);
            Assert.NotNull(block);
            Assert.AreEqual(clientId, block.Id);
            Assert.IsTrue(block.IsBlocked);
        }

        [Test]
        public async Task Block_RedirectToIndex_When_ServiceReturnsAny()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var clientId = Guid.NewGuid();
                this.ClientServiceMock.Setup(v => v.ChangeBlock(It.IsAny<IdSrvClientBlockDto>())).ReturnsAsync(what);
                var controller = new ClientsController(this.ClientServiceMock.Object);
                ActionResult result = await controller.Block(clientId);
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
                var clientId = Guid.NewGuid();
                this.ClientServiceMock.Setup(v => v.ChangeBlock(It.IsAny<IdSrvClientBlockDto>())).ReturnsAsync(what);
                var controller = new ClientsController(this.ClientServiceMock.Object);
                await controller.Block(clientId);
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
            var clientId = Guid.NewGuid();
            IdSrvClientBlockDto block = null;
            this.ClientServiceMock
                .Setup(v => v.ChangeBlock(It.IsAny<IdSrvClientBlockDto>()))
                .ReturnsAsync(true)
                .Callback<IdSrvClientBlockDto>(r => block = r);
            var controller = new ClientsController(this.ClientServiceMock.Object);
            await controller.Unblock(clientId);
            Assert.NotNull(block);
            Assert.AreEqual(clientId, block.Id);
            Assert.IsFalse(block.IsBlocked);
        }

        [Test]
        public async Task Unblock_RedirectToIndex_When_ServiceReturnsAny()
        {
            Func<bool, Task> testWithServiceReturns = async (bool what) =>
            {
                var clientId = Guid.NewGuid();
                this.ClientServiceMock.Setup(v => v.ChangeBlock(It.IsAny<IdSrvClientBlockDto>())).ReturnsAsync(what);
                var controller = new ClientsController(this.ClientServiceMock.Object);
                ActionResult result = await controller.Unblock(clientId);
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
                var clientId = Guid.NewGuid();
                this.ClientServiceMock.Setup(v => v.ChangeBlock(It.IsAny<IdSrvClientBlockDto>())).ReturnsAsync(what);
                var controller = new ClientsController(this.ClientServiceMock.Object);
                await controller.Unblock(clientId);
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
