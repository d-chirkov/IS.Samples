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
    internal class UserControllerTest
    {
        public RepositoryResponse UnexpectedRepositoryResponse { get; set; } = (RepositoryResponse)100;

        private Mock<IUserRepository> UserRepository { get; set; }

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
            this.UserRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new IdSrvUserDto());
            var controller = new UserController(this.UserRepository.Object);
            var id = new Guid();
            await controller.Get(id);
            this.UserRepository.Verify(v => v.GetByIdAsync(id));
        }

        [Test]
        public async Task Get_ReturnOkWithUserReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var user = new IdSrvUserDto();
            this.UserRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvUserDto>>(httpResult);
            Assert.AreEqual(user, (httpResult as OkNegotiatedContentResult<IdSrvUserDto>).Content);
        }

        [Test]
        public async Task Get_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as IdSrvUserDto);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task GetByUserName_InvokeGetByUserNameFromRepository_With_PassedId()
        {
            this.UserRepository.Setup(v => v.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync(new IdSrvUserDto());
            var controller = new UserController(this.UserRepository.Object);
            var userName = "u";
            await controller.GetByUserName(userName);
            this.UserRepository.Verify(v => v.GetByUserNameAsync(userName));
        }

        [Test]
        public async Task GetByUserName_ReturnOkWithUserReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var user = new IdSrvUserDto();
            this.UserRepository.Setup(v => v.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetByUserName("u");
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvUserDto>>(httpResult);
            Assert.AreEqual(user, (httpResult as OkNegotiatedContentResult<IdSrvUserDto>).Content);
        }

        [Test]
        public async Task GetByUserName_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository.Setup(v => v.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync(null as IdSrvUserDto);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetByUserName("u");
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task GetAll_InvokeGetAllFromUserRepository()
        {
            this.UserRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(new List<IdSrvUserDto>());
            var controller = new UserController(this.UserRepository.Object);
            await controller.GetAll();
            this.UserRepository.Verify(v => v.GetAllAsync());
        }

        [Test]
        public async Task GetAll_ReturnOkWithUsersReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var users = new List<IdSrvUserDto> { new IdSrvUserDto(), new IdSrvUserDto() };
            this.UserRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(users);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<IdSrvUserDto>>>(httpResult);
            Assert.AreEqual(users, (httpResult as OkNegotiatedContentResult<IEnumerable<IdSrvUserDto>>).Content);
        }

        [Test]
        public async Task GetAll_ReturnOkWithEmptyUsersList_When_RepositoryReturnNotNullEmptyUsersList()
        {
            var users = new List<IdSrvUserDto> { };
            this.UserRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(users);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<IdSrvUserDto>>>(httpResult);
            Assert.AreEqual(users, (httpResult as OkNegotiatedContentResult<IEnumerable<IdSrvUserDto>>).Content);
        }

        [Test]
        public async Task GetAll_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(null as IEnumerable<IdSrvUserDto>);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnBadRequest_When_PassingNullDto()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.Create(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnBadRequest_When_PassingDtoWithNullUserName()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.Create(new NewIdSrvUserDto { Password = "p1" });
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnOk_When_PassingDtoWithUsernameAndPassword_And_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDto { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.Create(newUserDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnOk_When_PassingDtoOnlyWithUsername_And_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDto { UserName = "u" };
            IHttpActionResult httpResult = await controller.Create(newUserDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Create_InvokeCreateFromRepository_With_PassedDto_When_PassingUserWithPassword()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var userDto = new NewIdSrvUserDto { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.Create(userDto);
            this.UserRepository.Verify(v => v.CreateAsync(userDto));
        }

        [Test]
        public async Task Create_InvokeCreateFromRepository_With_PassedUser_When_PassingUserWithoutPassword()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var userDto = new NewIdSrvUserDto { UserName = "u" };
            IHttpActionResult httpResult = await controller.Create(userDto);
            this.UserRepository.Verify(v => v.CreateAsync(userDto));
        }

        [Test]
        public async Task Create_ReturnConflict_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDto { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.Create(newUserDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<ConflictResult>(httpResult);
        }

        [Test]
        public void Create_ThrowsUserRepositoryException_When_RepositoryReturnNotFound()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDto { UserName = "u", Password = "p" };
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Create(newUserDto));
        }

        [Test]
        public void Create_ThrowsUserRepositoryException_When_RepositoryReturnUnexpetedResponse()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDto>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDto { UserName = "u", Password = "p" };
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Create(newUserDto));
        }

        [Test]
        public async Task ChangePassword_ReturnBadRequest_When_PassingStructureWithNullArgs()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDto();
            IHttpActionResult httpResult = await controller.ChangePassword(passwordDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task ChangePassword_ReturnBadRequest_When_PassingNull()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.ChangePassword(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task ChangePassword_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDto { Id = Guid.NewGuid(), Password = "p1" };
            IHttpActionResult httpResult = await controller.ChangePassword(passwordDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task ChangePassword_InvokeChangePasswordAsyncFromRepository_With_PassedPassword()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDto { Id = Guid.NewGuid(), Password = "p1" };
            IHttpActionResult httpResult = await controller.ChangePassword(passwordDto);
            this.UserRepository.Verify(v => v.ChangePasswordAsync(passwordDto));
        }

        [Test]
        public async Task ChangePassword_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDto>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDto { Id = Guid.NewGuid(), Password = "p1" };
            IHttpActionResult httpResult = await controller.ChangePassword(passwordDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public void ChangePassword_ThrowsUserRepositoryException_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDto { Id = Guid.NewGuid(), Password = "p1" };
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.ChangePassword(passwordDto));
        }

        [Test]
        public void ChangePassword_ThrowsUserRepositoryException_When_RepositoryReturnUnexpectedResponse()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDto>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDto { Id = Guid.NewGuid(), Password = "p1" };
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.ChangePassword(passwordDto));
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
            IHttpActionResult httpResult = await controller.Delete(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Delete_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.UserRepository.Setup(v => v.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.Get(new Guid());
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
        public async Task GetByAuthInfo_ReturnBadRequest_When_PassingDtoWithNullArgs()
        {
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDto>()))
                .ReturnsAsync(new IdSrvUserDto());
            var controller = new UserController(this.UserRepository.Object);
            var authInfo = new IdSrvUserAuthDto();
            IHttpActionResult httpResult = await controller.GetByAuthInfo(authInfo);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task GetByAuthInfo_ReturnBadRequest_When_PassingNull()
        {
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDto>()))
                .ReturnsAsync(new IdSrvUserDto());
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetByAuthInfo(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task GetByAuthInfo_InvokeDeleteFromRepository_With_PassedId()
        {
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDto>()))
                .ReturnsAsync(new IdSrvUserDto());
            var controller = new UserController(this.UserRepository.Object);
            var authInfoDto = new IdSrvUserAuthDto { UserName = "u", Password = "p" };
            await controller.GetByAuthInfo(authInfoDto);
            this.UserRepository.Verify(v => v.GetByAuthInfoAsync(authInfoDto));
        }

        [Test]
        public async Task GetByAuthInfo_ReturnOkWithUserReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var user = new IdSrvUserDto();
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDto>()))
                .ReturnsAsync(user);
            var controller = new UserController(this.UserRepository.Object);
            var authInfoDto = new IdSrvUserAuthDto { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.GetByAuthInfo(authInfoDto);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvUserDto>>(httpResult);
            Assert.NotNull(httpResult);
            Assert.AreEqual(user, (httpResult as OkNegotiatedContentResult<IdSrvUserDto>).Content);
        }

        [Test]
        public async Task GetByAuthInfo_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDto>()))
                .ReturnsAsync(null as IdSrvUserDto);
            var controller = new UserController(this.UserRepository.Object);
            var authInfoDto = new IdSrvUserAuthDto { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.GetByAuthInfo(authInfoDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task ChangeBlocking_ReturnBadRequest_When_PassingNull()
        {
            this.UserRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvUserBlockDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.ChangeBlocking(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task ChangeBlocking_ReturnOk_When_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvUserBlockDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.ChangeBlocking(new IdSrvUserBlockDto());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task ChangeBlocking_InvokeChangeBlockingAsyncFromRepository_With_PassedBlock()
        {
            this.UserRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvUserBlockDto>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var blockDto = new IdSrvUserBlockDto();
            IHttpActionResult httpResult = await controller.ChangeBlocking(blockDto);
            this.UserRepository.Verify(v => v.ChangeBlockingAsync(blockDto));
        }

        [Test]
        public async Task ChangeBlocking_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.UserRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvUserBlockDto>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            var blockDto = new IdSrvUserBlockDto();
            IHttpActionResult httpResult = await controller.ChangeBlocking(blockDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public void ChangeBlocking_ThrowsUserRepositoryException_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvUserBlockDto>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            var blockDto = new IdSrvUserBlockDto();
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.ChangeBlocking(blockDto));
        }

        [Test]
        public void ChangeBlocking_ThrowsUserRepositoryException_When_RepositoryReturnUnexpectedResponse()
        {
            this.UserRepository
                .Setup(v => v.ChangeBlockingAsync(It.IsAny<IdSrvUserBlockDto>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            var blockDto = new IdSrvUserBlockDto();
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.ChangeBlocking(blockDto));
        }
    }
}
