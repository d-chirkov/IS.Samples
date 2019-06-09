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
    using System.Net;
    using System.Web.Http.Results;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.Infrastructure.Exceptions;

    [TestFixture]
    class UserControllerTest
    {
        private Mock<IUserRepository> UserRepository { get; set; }
        public RepositoryResponse UnexcpectedRepositoryResponse { get; set; } = (RepositoryResponse)100;

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

        [Test]
        public async Task Get_CallGetByIdFromRepository_With_PassedId()
        {
            this.UserRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new IdSrvUserDTO());
            var controller = new UserController(this.UserRepository.Object);
            var id = new Guid();
            await controller.Get(id);
            this.UserRepository.Verify(v => v.GetByIdAsync(id));
        }

        [Test]
        public async Task Get_ReturnOkWithUserReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var user = new IdSrvUserDTO();
            this.UserRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Get(new Guid());
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvUserDTO>>(httpResult);
            Assert.AreEqual(user, (httpResult as OkNegotiatedContentResult<IdSrvUserDTO>).Content);
        }

        [Test]
        public async Task Get_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as IdSrvUserDTO);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Get(new Guid());
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Create(new NewIdSrvUserDTO());
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Create_CallCreateFromRepository_With_PassedUser()
        {
            var user = new NewIdSrvUserDTO();
            this.UserRepository
                .Setup(v => v.CreateAsync(user))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Create(user);
            this.UserRepository.Verify(v => v.CreateAsync(user));
        }

        [Test]
        public async Task Create_ReturnConflict_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Create(new NewIdSrvUserDTO());
            Assert.IsInstanceOf<ConflictResult>(httpResult);
        }

        [Test]
        public void Create_ThrowsUserRepositoryException_When_RepositoryReturnNotFound()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Create(new NewIdSrvUserDTO()));
        }

        [Test]
        public void Create_ThrowsUserRepositoryException_When_RepositoryReturnUnexpetedResponse()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(this.UnexcpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Create(new NewIdSrvUserDTO()));
        }

        [Test]
        public async Task Update_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.UpdateAsync(It.IsAny<IdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Update(new IdSrvUserDTO());
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Update_CallUpdateFromRepository_With_PassedUser()
        {
            var user = new IdSrvUserDTO();
            this.UserRepository
                .Setup(v => v.UpdateAsync(user))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Update(user);
            this.UserRepository.Verify(v => v.UpdateAsync(user));
        }

        [Test]
        public async Task Update_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.UserRepository
                .Setup(v => v.UpdateAsync(It.IsAny<IdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Update(new IdSrvUserDTO());
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task Update_ReturnConflict_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.UpdateAsync(It.IsAny<IdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            var httpResult = await controller.Update(new IdSrvUserDTO());
            Assert.IsInstanceOf<ConflictResult>(httpResult);
        }

        [Test]
        public void Update_ThrowsUserRepositoryException_When_RepositoryReturnUnexpectedResponse()
        {
            this.UserRepository
                .Setup(v => v.UpdateAsync(It.IsAny<IdSrvUserDTO>()))
                .ReturnsAsync(this.UnexcpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Update(new IdSrvUserDTO()));
        }
    }
}
