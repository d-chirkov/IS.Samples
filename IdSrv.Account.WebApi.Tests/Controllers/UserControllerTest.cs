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
            IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IdSrvUserDTO>>(httpResult);
            Assert.AreEqual(user, (httpResult as OkNegotiatedContentResult<IdSrvUserDTO>).Content);
        }

        [Test]
        public async Task Get_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository.Setup(v => v.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as IdSrvUserDTO);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.Get(new Guid());
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task GetAll_InvokeGetAllFromRepository()
        {
            this.UserRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(new List<IdSrvUserDTO>());
            var controller = new UserController(this.UserRepository.Object);
            await controller.GetAll();
            this.UserRepository.Verify(v => v.GetAllAsync());
        }

        [Test]
        public async Task GetAll_ReturnOkWithUsersReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var users = new List<IdSrvUserDTO> { new IdSrvUserDTO(), new IdSrvUserDTO() };
            this.UserRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(users);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<IdSrvUserDTO>>>(httpResult);
            Assert.AreEqual(users, (httpResult as OkNegotiatedContentResult<IEnumerable<IdSrvUserDTO>>).Content);
        }

        [Test]
        public async Task GetAll_ReturnOkWithEmptyUsersList_When_RepositoryReturnNotNullEmptyUsersList()
        {
            var users = new List<IdSrvUserDTO> { };
            this.UserRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(users);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<IdSrvUserDTO>>>(httpResult);
            Assert.AreEqual(users, (httpResult as OkNegotiatedContentResult<IEnumerable<IdSrvUserDTO>>).Content);
        }

        [Test]
        public async Task GetAll_ReturnNotFound_When_RepositoryReturnNull()
        {
            this.UserRepository.Setup(v => v.GetAllAsync()).ReturnsAsync(null as IEnumerable<IdSrvUserDTO>);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetAll();
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnBadRequest_When_PassingNullDto()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
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
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.Create(new NewIdSrvUserDTO { Password = "p1" });
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnBadRequest_When_PassingNull()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.Create(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnOk_When_PassingDtoWithUsernameAndPassword_And_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDTO { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.Create(newUserDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Create_ReturnOk_When_PassingDtoOnlyWithUsername_And_RepositoryReturnSuccess()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDTO { UserName = "u"};
            IHttpActionResult httpResult = await controller.Create(newUserDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<OkResult>(httpResult);
        }

        [Test]
        public async Task Create_InvokeCreateFromRepository_With_PassedUser_When_PassingUserWithPassword()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var userDto = new NewIdSrvUserDTO { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.Create(userDto);
            this.UserRepository.Verify(v => v.CreateAsync(userDto));
        }

        [Test]
        public async Task Create_InvokeCreateFromRepository_With_PassedUser_When_PassingUserWithoutPassword()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var userDto = new NewIdSrvUserDTO { UserName = "u" };
            IHttpActionResult httpResult = await controller.Create(userDto);
            this.UserRepository.Verify(v => v.CreateAsync(userDto));
        }

        [Test]
        public async Task Create_ReturnConflict_When_RepositoryReturnConflict()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(RepositoryResponse.Conflict);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDTO { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.Create(newUserDto);
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
            var newUserDto = new NewIdSrvUserDTO { UserName = "u", Password = "p" };
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Create(newUserDto));
        }

        [Test]
        public void Create_ThrowsUserRepositoryException_When_RepositoryReturnUnexpetedResponse()
        {
            this.UserRepository
                .Setup(v => v.CreateAsync(It.IsAny<NewIdSrvUserDTO>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            var newUserDto = new NewIdSrvUserDTO { UserName = "u", Password = "p" };
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.Create(newUserDto));
        }

        [Test]
        public async Task ChangePassword_ReturnBadRequest_When_PassingStructureWithNullArgs()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDTO();
            IHttpActionResult httpResult = await controller.ChangePassword(passwordDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task ChangePassword_ReturnBadRequest_When_PassingNull()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
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
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(RepositoryResponse.Success);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDTO { UserId = Guid.NewGuid(), Password = "p1" };
            IHttpActionResult httpResult = await controller.ChangePassword(passwordDto);
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
            var passwordDto = new IdSrvUserPasswordDTO { UserId = Guid.NewGuid(), Password = "p1" };
            IHttpActionResult httpResult = await controller.ChangePassword(passwordDto);
            this.UserRepository.Verify(v => v.ChangePasswordAsync(passwordDto));
        }

        [Test]
        public async Task ChangePassword_ReturnNotFound_When_RepositoryReturnNotFound()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(RepositoryResponse.NotFound);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDTO { UserId = Guid.NewGuid(), Password = "p1" };
            IHttpActionResult httpResult = await controller.ChangePassword(passwordDto);
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
            var passwordDto = new IdSrvUserPasswordDTO { UserId = Guid.NewGuid(), Password = "p1" };
            Assert.ThrowsAsync<UserRepositoryException>(() => controller.ChangePassword(passwordDto));
        }

        [Test]
        public void ChangePassword_ThrowsUserRepositoryException_When_RepositoryReturnUnexpectedResponse()
        {
            this.UserRepository
                .Setup(v => v.ChangePasswordAsync(It.IsAny<IdSrvUserPasswordDTO>()))
                .ReturnsAsync(this.UnexpectedRepositoryResponse);
            var controller = new UserController(this.UserRepository.Object);
            var passwordDto = new IdSrvUserPasswordDTO { UserId = Guid.NewGuid(), Password = "p1" };
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
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDTO>()))
                .ReturnsAsync(new IdSrvUserDTO());
            var controller = new UserController(this.UserRepository.Object);
            var authInfo = new IdSrvUserAuthDTO();
            IHttpActionResult httpResult = await controller.GetByAuthInfo(authInfo);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task GetByAuthInfo_ReturnBadRequest_When_PassingNull()
        {
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDTO>()))
                .ReturnsAsync(new IdSrvUserDTO());
            var controller = new UserController(this.UserRepository.Object);
            IHttpActionResult httpResult = await controller.GetByAuthInfo(null);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<BadRequestResult>(httpResult);
        }

        [Test]
        public async Task GetByAuthInfo_InvokeDeleteFromRepository_With_PassedId()
        {
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDTO>()))
                .ReturnsAsync(new IdSrvUserDTO());
            var controller = new UserController(this.UserRepository.Object);
            var authInfoDto = new IdSrvUserAuthDTO { UserName = "u", Password = "p" };
            await controller.GetByAuthInfo(authInfoDto);
            this.UserRepository.Verify(v => v.GetByAuthInfoAsync(authInfoDto));
        }

        [Test]
        public async Task GetByAuthInfo_ReturnOkWithUserReceivedFromRepository_When_RepositoryReturnNotNull()
        {
            var user = new IdSrvUserDTO();
            this.UserRepository
                .Setup(v => v.GetByAuthInfoAsync(It.IsAny<IdSrvUserAuthDTO>()))
                .ReturnsAsync(user);
            var controller = new UserController(this.UserRepository.Object);
            var authInfoDto = new IdSrvUserAuthDTO { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.GetByAuthInfo(authInfoDto);
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
            var authInfoDto = new IdSrvUserAuthDTO { UserName = "u", Password = "p" };
            IHttpActionResult httpResult = await controller.GetByAuthInfo(authInfoDto);
            Assert.NotNull(httpResult);
            Assert.IsInstanceOf<NotFoundResult>(httpResult);
        }
    }
}
