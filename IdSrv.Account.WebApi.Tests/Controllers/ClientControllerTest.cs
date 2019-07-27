namespace IdSrv.Account.WebApi.Controllers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Results;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using IdSrv.Account.WebApi.Infrastructure.Exceptions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class ClientControllerTest
    {
        private Mock<IClientRepository> ClientRepository { get; set; }
        public RepositoryResponse UnexpectedRepositoryResponse { get; set; } = (RepositoryResponse)100;

        [SetUp]
        public void SetUp()
        {
            this.ClientRepository = new Mock<IClientRepository>();
        }

        [Test]
        public void Ctor_DoesNotThrow_When_PassingNotNullClientRepository()
        {
            Assert.DoesNotThrow(() => new ClientController(this.ClientRepository.Object));
        }

        [Test]
        public void Ctor_ThrowArgumentNullException_When_PassingNullInsteadClientRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new ClientController(null));
        }

        [Test]
        public async Task Get_InvokeGetByIdFromRepository_With_PassedId()
        {
            this.ClientRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new IdSrvClientDto());
            var controller = new ClientController(this.ClientRepository.Object);
            var id = new Guid();
            await controller.Get(id);
            this.ClientRepository.Verify(v => v.GetByIdAsync(id));
        }

        [Test]
        public async Task Get_ReturnOkWithClientReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var client = new IdSrvClientDto();
            this.ClientRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(client);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvClientDto>>(httpResult);
            Assert.AreEqual(client, (httpResult as OkNegotiatedContentResult<IdSrvClientDto>).Content);
        }

        [Test]
        public async Task Get_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.ClientRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as IdSrvClientDto);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task GetByName_ReturnBadRequest_When_PassingNullName()
        {
            this.ClientRepository.Setup(v => v.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(null as IdSrvClientDto);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetByName(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task GetByName_InvokeGetByNameFromRepository_With_PassedName()
        {
            this.ClientRepository.Setup(v => v.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdSrvClientDto());
            var controller = new ClientController(this.ClientRepository.Object);
            var name = "n";
            await controller.GetByName(name);
            this.ClientRepository.Verify(v => v.GetByNameAsync(name));
        }

        [Test]
        public async Task GetByName_ReturnOkWithClientReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var client = new IdSrvClientDto();
            this.ClientRepository.Setup(v => v.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(client);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetByName("n");
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvClientDto>>(httpResult);
            Assert.AreEqual(client, (httpResult as OkNegotiatedContentResult<IdSrvClientDto>).Content);
        }

        [Test]
        public async Task GetByName_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.ClientRepository.Setup(v => v.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(null as IdSrvClientDto);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetByName("n1");
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task GetAll_InvokeGetAllFromClientRepository()
        {
            this.ClientRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(new List<IdSrvClientDto>());
            var controller = new ClientController(this.ClientRepository.Object);
            await controller.GetAll();
            this.ClientRepository.Verify(v => v.GetAllAsync());
        }

        [Test]
        public async Task GetAll_ReturnOkWithClientsReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var clients = new List<IdSrvClientDto> { new IdSrvClientDto(), new IdSrvClientDto() };
            this.ClientRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(clients);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<IdSrvClientDto>>>(httpResult);
            Assert.AreEqual(clients, (httpResult as OkNegotiatedContentResult<IEnumerable<IdSrvClientDto>>).Content);
        }

        [Test]
        public async Task GetAll_ReturnOkWithEmptyClientsList_When_RepositoryReturnNotNullEmptyClientsList()
        {
            var clients = new List<IdSrvClientDto> { };
            this.ClientRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(clients);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<IdSrvClientDto>>>(httpResult);
            Assert.AreEqual(clients, (httpResult as OkNegotiatedContentResult<IEnumerable<IdSrvClientDto>>).Content);
        }

        [Test]
        public async Task GetAll_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.ClientRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(null as IEnumerable<IdSrvClientDto>);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task GetAllUris_InvokeGetAllUrisFromClientRepository()
        {
            this.ClientRepository.Setup(v => v.GetAllUrisAsync()).ReturnsAsync(new List<string>());
            var controller = new ClientController(this.ClientRepository.Object);
            await controller.GetAllUris();
            this.ClientRepository.Verify(v => v.GetAllUrisAsync());
        }

        [Test]
        public async Task GetAllUris_ReturnOkWithUrisReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var uris = new List<string> { "a", "b" };
            this.ClientRepository.Setup(v => v.GetAllUrisAsync()).ReturnsAsync(uris);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetAllUris();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<string>>>(httpResult);
            Assert.AreEqual(uris, (httpResult as OkNegotiatedContentResult<IEnumerable<string>>).Content);
        }

        [Test]
        public async Task GetAllUris_ReturnOkWithEmptyUrisList_When_RepositoryReturnNotNullEmptyUrisList()
        {
            var uris = new List<string> { };
            this.ClientRepository.Setup(v => v.GetAllUrisAsync()).ReturnsAsync(uris);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetAllUris();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<string>>>(httpResult);
            Assert.AreEqual(uris, (httpResult as OkNegotiatedContentResult<IEnumerable<string>>).Content);
        }

        [Test]
        public async Task GetAllUris_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.ClientRepository.Setup(v => v.GetAllUrisAsync()).ReturnsAsync(null as IEnumerable<string>);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.GetAllUris();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnBadRequest_When_PassingNullDto()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Create(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnBadRequest_When_PassingDtoWithNullName()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Create(new NewIdSrvClientDto { Secret = "s" });
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnBadRequest_When_PassingDtoWithNullSecret()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Create(new NewIdSrvClientDto { Name = "n" });
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnOk_When_PassingDtoWithNameAndSecret_And_RepositoryReturnSuccess()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var newClientDto = new NewIdSrvClientDto { Name = "u", Secret = "p" };
            IHttpActionResult httpResult = await controller.Create(newClientDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnOk_When_PassingDtoWithNameAndSecretAndUri_And_RepositoryReturnSuccess()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var newClientDto = new NewIdSrvClientDto { Name = "u", Secret = "p", Uri = "u" };
            IHttpActionResult httpResult = await controller.Create(newClientDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Create_InvokeCreateFromRepository_With_PassedDto_When_PassingNameAndSecretInDto()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new NewIdSrvClientDto { Name = "u", Secret = "p" };
            IHttpActionResult httpResult = await controller.Create(clientDto);
            this.ClientRepository.Verify(v => v.CreateAsync(clientDto));
        }

        [Test]
        public async Task Create_InvokeCreateFromRepository_With_PassedDto_When_PassingNameAndSecretAndUriInDto()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new NewIdSrvClientDto { Name = "u", Secret = "p", Uri = "u" };
            IHttpActionResult httpResult = await controller.Create(clientDto);
            this.ClientRepository.Verify(v => v.CreateAsync(clientDto));
        }

        [Test]
        public async Task Create_ReturnConflict_When_RepositoryReturnConflict()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new NewIdSrvClientDto { Name = "u", Secret = "p" };
            IHttpActionResult httpResult = await controller.Create(clientDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<ConflictResult>(httpResult);
        }

        [Test]
        public void Create_ThrowsClientRepositoryException_When_RepositoryReturnNotFound()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new NewIdSrvClientDto { Name = "u", Secret = "p" };
            Assert.ThrowsAsync<ClientRepositoryException>(() => controller.Create(clientDto));
        }

        [Test]
        public void Create_ThrowsClientRepositoryException_When_RepositoryReturnUnexpetedResponse()
        {
            this.ClientRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvClientDto>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new NewIdSrvClientDto { Name = "u", Secret = "p" };
            Assert.ThrowsAsync<ClientRepositoryException>(() => controller.Create(clientDto));
        }

        [Test]
        public async Task Update_ReturnBadRequest_When_PassingNullDto()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Update(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Update_ReturnBadRequest_When_PassingDtoWithNullName()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Update(new UpdateIdSrvClientDto { Secret = "s" });
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Update_ReturnBadRequest_When_PassingDtoWithNullSecret()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Update(new UpdateIdSrvClientDto { Name = "n" });
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Update_ReturnOk_When_PassingDtoWithNameAndSecret_And_RepositoryReturnSuccess()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new UpdateIdSrvClientDto { Name = "u", Secret = "p" };
            IHttpActionResult httpResult = await controller.Update(clientDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Update_ReturnOk_When_PassingDtoWithNameAndSecretAndUri_And_RepositoryReturnSuccess()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new UpdateIdSrvClientDto { Name = "u", Secret = "p", Uri = "u" };
            IHttpActionResult httpResult = await controller.Update(clientDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Update_InvokeUpdateFromRepository_With_PassedDto_When_PassingNameAndSecretInDto()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new UpdateIdSrvClientDto { Name = "u", Secret = "p" };
            IHttpActionResult httpResult = await controller.Update(clientDto);
            this.ClientRepository.Verify(v => v.UpdateAsync(clientDto));
        }

        [Test]
        public async Task Update_InvokeUpdateFromRepository_With_PassedDto_When_PassingNameAndSecretAndUriInDto()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new UpdateIdSrvClientDto { Name = "u", Secret = "p", Uri = "u" };
            IHttpActionResult httpResult = await controller.Update(clientDto);
            this.ClientRepository.Verify(v => v.UpdateAsync(clientDto));
        }

        [Test]
        public async Task Update_ReturnConflict_When_RepositoryReturnConflict()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new UpdateIdSrvClientDto { Name = "u", Secret = "p" };
            IHttpActionResult httpResult = await controller.Update(clientDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<ConflictResult>(httpResult);
        }

        [Test]
        public async Task Update_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new UpdateIdSrvClientDto { Name = "u", Secret = "p" };
            IHttpActionResult httpResult = await controller.Update(clientDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public void Update_ThrowsClientRepositoryException_When_RepositoryReturnUnexpetedResponse()
        {
            this.ClientRepository
                .Setup(v => v.UpdateAsync(It.IsAny<UpdateIdSrvClientDto>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new ClientController(this.ClientRepository.Object);
            var clientDto = new UpdateIdSrvClientDto { Name = "u", Secret = "p" };
            Assert.ThrowsAsync<ClientRepositoryException>(() => controller.Update(clientDto));
        }

        [Test]
        public async Task Delete_InvokeDeleteFromRepository_With_PassedId()
        {
            this.ClientRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var id = new Guid();
            await controller.Delete(id);
            this.ClientRepository.Verify(v => v.DeleteAsync(id));
        }

        [Test]
        public async Task Delete_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.ClientRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Delete(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Delete_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.ClientRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public void Delete_ThrowsClientRepositoryException_When_RepositoryReturnConflict()
        {
            this.ClientRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            Assert.ThrowsAsync<ClientRepositoryException>(() => controller.Delete(new Guid()));
        }

        [Test]
        public void Delete_ThrowsClientRepositoryException_When_RepositoryReturnUnexpetedResponse()
        {
            this.ClientRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new ClientController(this.ClientRepository.Object);
            Assert.ThrowsAsync<ClientRepositoryException>(() => controller.Delete(new Guid()));
        }

        [Test]
        public async Task ChangeBlocking_ReturnBadRequest_When_PassingNull()
        {
            this.ClientRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvClientBlockDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.ChangeBlocking(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task ChangeBlocking_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.ClientRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvClientBlockDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            IHttpActionResult httpResult = await controller.ChangeBlocking(new IdSrvClientBlockDto());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task ChangeBlocking_InvokeChangeBlockingAsyncFromRepository_With_PassedBlock()
        {
            this.ClientRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvClientBlockDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new ClientController(this.ClientRepository.Object);
            var blockDto = new IdSrvClientBlockDto();
            IHttpActionResult httpResult = await controller.ChangeBlocking(blockDto);
            this.ClientRepository.Verify(v => v.ChangeBlockingAsync(blockDto));
        }

        [Test]
        public async Task ChangeBlocking_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.ClientRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvClientBlockDto>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new ClientController(this.ClientRepository.Object);
            var blockDto = new IdSrvClientBlockDto();
            IHttpActionResult httpResult = await controller.ChangeBlocking(blockDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public void ChangeBlocking_ThrowsClientRepositoryException_When_RepositoryReturnConflict()
        {
            this.ClientRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvClientBlockDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new ClientController(this.ClientRepository.Object);
            var blockDto = new IdSrvClientBlockDto();
            Assert.ThrowsAsync<ClientRepositoryException>(() => controller.ChangeBlocking(blockDto));
        }

        [Test]
        public void ChangeBlocking_ThrowsClientRepositoryException_When_RepositoryReturnUnexpectedResponse()
        {
            this.ClientRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvClientBlockDto>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new ClientController(this.ClientRepository.Object);
            var blockDto = new IdSrvClientBlockDto();
            Assert.ThrowsAsync<ClientRepositoryException>(() => controller.ChangeBlocking(blockDto));
        }
    }
}
