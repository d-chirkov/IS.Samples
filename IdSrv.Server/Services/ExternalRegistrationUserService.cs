namespace IdSrv.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityServer3.Core;
    using IdentityServer3.Core.Extensions;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    /// <summary>
    /// Реализация <see cref="UserServiceBase"/> для windows-пользователей.
    /// Данный класс не общается с контроллером домена, а просто работает с результатом
    /// вызова <see cref="CustomGrantValidator"/>. Включает дополнительную логику для проверки,
    /// что пользователь сущетсвует, не заблокирован и т.п.
    /// Используется только сайтами, во время входа через прилоежния методы данного класса не выполняются.
    /// </summary>
    internal class ExternalRegistrationUserService : UserServiceBase
    {
        /// <summary>
        /// Инициализировать сервис пользователей.
        /// </summary>
        /// <param name="userRepository">Репозиторий пользователей.</param>
        public ExternalRegistrationUserService(IUserRepository userRepository)
        {
            this.UserRepository = userRepository;
        }

        /// <summary>
        /// Получает или задает репозиторий пользователей.
        /// </summary>
        private IUserRepository UserRepository { get; set; }

        /// <inheritdoc/>
        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            string userName = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name)?.Value;
            if (userName == null)
            {
                return;
            }

            IdSrvUserDto user = await this.UserRepository.GetUserByUserNameAsync(userName);
            if (user != null)
            {
                context.AuthenticateResult = user.IsBlocked ?
                    new AuthenticateResult($"User is \"{userName}\" blocked") :
                    new AuthenticateResult(
                        user.Id.ToString(),
                        userName,
                        identityProvider: context.ExternalIdentity.Provider);
            }
            else
            {
                context.AuthenticateResult = new AuthenticateResult($"User is \"{userName}\" not registered");
            }
        }

        /// <inheritdoc/>
        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            IdSrvUserDto user = await this.UserRepository.GetUserByIdAsync(context.Subject.GetSubjectId());
            if (user != null)
            {
                if (!user.IsBlocked)
                {
                    context.IssuedClaims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.Subject, user.Id.ToString()),
                        new Claim(Constants.ClaimTypes.Name, user.UserName.ToString()),
                    };
                }
            }
        }
    }
}