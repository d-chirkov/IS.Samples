namespace IdSrv.Account.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using IdSrv.Account.WebApi.Infrastructure.Exceptions;
    using Swashbuckle.Swagger.Annotations;

    /// <summary>
    /// Контроллер для управления пользователями identity server-а.
    /// </summary>
    [RoutePrefix("Api/User")]
    public class UserController : ApiController
    {
        /// <summary>
        /// Создаёт экземпляр контроллера.
        /// </summary>
        /// <param name="userRepository">Репозиторий пользователей.</param>
        public UserController(IUserRepository userRepository)
        {
            this.UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Получает или задает репозиторий пользователей.
        /// </summary>
        private IUserRepository UserRepository { get; set; }

        /// <summary>
        /// Получить всех пользователей.
        /// </summary>
        /// <returns>
        /// Ok со списком объектов IdSrvUserDto,
        /// либо NotFound, если получить такой список из репозитория не удалось.
        /// </returns>
        [HttpGet]
        [Route("GetAll")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdSrvUserDto>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetAll()
        {
            IEnumerable<IdSrvUserDto> users = await this.UserRepository.GetAllAsync();
            return users != null ? this.Ok(users) : this.NotFound() as IHttpActionResult;
        }

        /// <summary>
        /// Получить пользователя по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>
        /// Ok с dto типа IdSrvUserDto,
        /// либо NotFound, если такой пользователь не найден в репозитории.
        /// </returns>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdSrvUserDto))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            IdSrvUserDto user = await this.UserRepository.GetByIdAsync(id);
            return user != null ? this.Ok(user) : this.NotFound() as IHttpActionResult;
        }

        /// <summary>
        /// Получить пользователя по логину.
        /// </summary>
        /// <param name="userName">Логин пользователя.</param>
        /// <returns>
        /// Ok с dto типа IdSrvUserDto,
        /// либо NotFound, если такой пользователь не найден в репозитории.
        /// </returns>
        [HttpGet]
        [Route("GetByUserName")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdSrvUserDto))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetByUserName(string userName)
        {
            IdSrvUserDto user = await this.UserRepository.GetByUserNameAsync(userName);
            return user != null ? this.Ok(user) : this.NotFound() as IHttpActionResult;
        }

        /// <summary>
        /// Получить пользователя по его аутентификационным данным (логину и паролю).
        /// Данные метод необходим для проверки аутентификационных данных.
        /// </summary>
        /// <param name="authInfo">Аутентификационные данные пользователя.</param>
        /// <returns>
        /// Ok с dto типа IdSrvUserDto,
        /// либо NotFound, если такой пользователь не найден в репозитории, т.е. 
        /// логин и/или пароль не верные.
        /// </returns>
        [HttpPost]
        [Route("GetByAuthInfo")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdSrvUserDto))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetByAuthInfo(IdSrvUserAuthDto authInfo)
        {
            // This action check credentials only for simple users, not windows users.
            // So it's necessary to get password from client
            if (authInfo == null || authInfo.UserName == null || authInfo.Password == null)
            {
                return this.BadRequest();
            }

            IdSrvUserDto user = await this.UserRepository.GetByAuthInfoAsync(authInfo);
            return user != null ? this.Ok(user) : this.NotFound() as IHttpActionResult;
        }

        /// <summary>
        /// Создать пользователя в репозитории.
        /// </summary>
        /// <param name="user">Dto с данными для создания пользователя.</param>
        /// <returns>
        /// Ok, если пользователь успешно создан, Conflict в противном случае.
        /// Если пользвоаетеля не удалось создать, то это означает, что пользователь
        /// с такими данными (логином) уже существует в репозитории.
        /// </returns>
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<IHttpActionResult> Create(NewIdSrvUserDto user)
        {
            // Password can be null for windows users
            if (user == null || user.UserName == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.UserRepository.CreateAsync(user);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.Conflict ? this.Conflict() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        /// <summary>
        /// Сменить пароль пользователя.
        /// </summary>
        /// <param name="password">Dto с идентификатором пользователя и новым паролем для него.</param>
        /// <returns>
        /// Ok, если пароль был успешно изменён в репозитории;
        /// NotFound, если пользователь с таким идентификатором не найден.
        /// </returns>
        [HttpPost]
        [Route("ChangePassword")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> ChangePassword(IdSrvUserPasswordDto password)
        {
            if (password == null || password.Password == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.UserRepository.ChangePasswordAsync(password);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        /// <summary>
        /// Изменить статус блокировки пользователя.
        /// </summary>
        /// <param name="block">Dto с новым статусом блокироваки.</param>
        /// <returns>
        /// Ok, если статус блокироваки изменён в репозитории;
        /// NotFound, если такого пользователя не найдено.
        /// </returns>
        [HttpPost]
        [Route("ChangeBlocking")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> ChangeBlocking(IdSrvUserBlockDto block)
        {
            if (block == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.UserRepository.ChangeBlockingAsync(block);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        /// <summary>
        /// Удалить пользователя по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>
        /// Ok, если пользователь успешно удалён из репозитория;
        /// NotFound, если пользователь с таким идентификатором не найден.
        /// </returns>
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            RepositoryResponse response = await this.UserRepository.DeleteAsync(id);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }
    }
}
