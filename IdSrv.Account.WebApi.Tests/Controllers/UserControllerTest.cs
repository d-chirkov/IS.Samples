namespace IdSrv.Account.WebApi.Controllers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http.Results;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using IdSrv.Account.WebApi.Infrastructure.Exceptions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class UserControllerTest
    {
        private Mock<IUserRepository> UserRepository { get; set; }
        public RepositoryResponse UnexpectedRepositoryResponse { get; set; } = (RepositoryResponse)100;

        [SetUp]
        public void SetUp()
        {
            this.UserRepository = new Mock<IUserRepository>();
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
        public async Task Get_InvokeGetByIdFromRepository_With_PassedId()
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
            System.Web.Http.IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvUserDTO>>(httpResult);
            Assert.AreEqual(user, (httpResult as OkNegotiatedContentResult<IdSrvUserDTO>).Content);
        }

        [Test]
        public async Task Get_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as IdSrvUserDTO);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task GetAll_InvokeGetAllFromRepository()
        {
            this.UserRepository.Setup(v => v.GetAll()).ReturnsAsync(new List<IdSrvUserDTO>());
            var controller = new UserController(this.UserRepository.Object);
            await controller.GetAll();
            this.UserRepository.Verify(v => v.GetAll());
        }

        [Test]
        public async Task GetAll_ReturnOkWithUsersReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var users = new List<IdSrvUserDTO> { new IdSrvUserDTO(), new IdSrvUserDTO() };
            this.UserRepository.Setup(v => v.GetAll()).ReturnsAsync(users);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<IdSrvUserDTO>>>(httpResult);
            Assert.AreEqual(users, (httpResult as OkNegotiatedContentResult<IEnumerable<IdSrvUserDTO>>).Content);
        }

        [Test]
        public async Task GetAll_ReturnOkWithEmptyUsersList_When_RepositoryReturnNotNullEmptyUsersList()
        {
            var users = new List<IdSrvUserDTO> { };
            this.UserRepository.Setup(v => v.GetAll()).ReturnsAsync(users);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<IdSrvUserDTO>>>(httpResult);
            Assert.AreEqual(users, (httpResult as OkNegotiatedContentResult<IEnumerable<IdSrvUserDTO>>).Content);
        }

        [Test]
        public async Task GetAll_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository.Setup(v => v.GetAll()).ReturnsAsync(null as IEnumerable<IdSrvUserDTO>);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.Create(new NewIdSrvUserDTO());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Create_InvokeCreateFromRepository_With_PassedUser()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var user = new NewIdSrvUserDTO();
            System.Web.Http.IHttpActionResult httpResult = await controller.Create(user);
            this.UserRepository.Verify(v => v.CreateAsync(user));
        }

        [Test]
        public async Task Create_ReturnConflict_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.Create(new NewIdSrvUserDTO());
            Assert.NotNull(httpResult);
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
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
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
            System.Web.Http.IHttpActionResult httpResult = await controller.Update(new IdSrvUserDTO());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Update_InvokeUpdateFromRepository_With_PassedUser()
        {
            this.UserRepository
                .Setup(v => v.UpdateAsync(It.IsAny<IdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var user = new IdSrvUserDTO();
            System.Web.Http.IHttpActionResult httpResult = await controller.Update(user);
            this.UserRepository.Verify(v => v.UpdateAsync(user));
        }

        [Test]
        public async Task Update_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.UserRepository
                .Setup(v => v.UpdateAsync(It.IsAny<IdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.Update(new IdSrvUserDTO());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task Update_ReturnConflict_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.UpdateAsync(It.IsAny<IdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.Update(new IdSrvUserDTO());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<ConflictResult>(httpResult);
        }

        [Test]
        public void Update_ThrowsUserRepositoryException_When_RepositoryReturnUnexpectedResponse()
        {
            this.UserRepository
                .Setup(v => v.UpdateAsync(It.IsAny<IdSrvUserDTO>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Update(new IdSrvUserDTO()));
        }

        [Test]
        public async Task ChangePassword_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.ChangePassword(new IdSrvUserPasswordDTO());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task ChangePassword_InvokeUpdateFromRepository_With_PassedPassword()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var password = new IdSrvUserPasswordDTO();
            System.Web.Http.IHttpActionResult httpResult = await controller.ChangePassword(password);
            this.UserRepository.Verify(v => v.ChangePasswordAsync(password));
        }

        [Test]
        public async Task ChangePassword_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.ChangePassword(new IdSrvUserPasswordDTO());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public void ChangePassword_ThrowsUserRepositoryException_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.ChangePassword(new IdSrvUserPasswordDTO()));
        }

        [Test]
        public void ChangePassword_ThrowsUserRepositoryException_When_RepositoryReturnUnexpectedResponse()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.ChangePassword(new IdSrvUserPasswordDTO()));
        }

        [Test]
        public async Task Delete_InvokeDeleteFromRepository_With_PassedId()
        {
            this.UserRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var id = new Guid();
            await controller.Delete(id);
            this.UserRepository.Verify(v => v.DeleteAsync(id));
        }

        [Test]
        public async Task Delete_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.UserRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.Delete(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Delete_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.UserRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public void Delete_ThrowsUserRepositoryException_When_RepositoryReturnConflict()
        {
            this.UserRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Delete(new Guid()));
        }

        [Test]
        public void Delete_ThrowsUserRepositoryException_When_RepositoryReturnUnexpetedResponse()
        {
            this.UserRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Delete(new Guid()));
        }

        [Test]
        public async Task GetByAuthInfo_InvokeDeleteFromRepository_With_PassedId()
        {
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDTO>()))
                .ReturnsAsync(new IdSrvUserDTO());
            var controller = new UserController(this.UserRepository.Object);
            var authInfo = new IdSrvUserAuthDTO();
            await controller.GetByAuthInfo(authInfo);
            this.UserRepository.Verify(v => v.GetByAuthInfoAsync(authInfo));
        }

        [Test]
        public async Task GetByAuthInfo_ReturnOkWithUserReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var user = new IdSrvUserDTO();
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDTO>()))
                .ReturnsAsync(user);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.GetByAuthInfo(new IdSrvUserAuthDTO());
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvUserDTO>>(httpResult);
            Assert.NotNull(httpResult);
            Assert.AreEqual(user, (httpResult as OkNegotiatedContentResult<IdSrvUserDTO>).Content);
        }

        [Test]
        public async Task GetByAuthInfo_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDTO>()))
                .ReturnsAsync(null as IdSrvUserDTO);
            var controller = new UserController(this.UserRepository.Object);
            System.Web.Http.IHttpActionResult httpResult = await controller.GetByAuthInfo(new IdSrvUserAuthDTO());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }
    }
}
