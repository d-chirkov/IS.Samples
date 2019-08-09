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
    using IdSrv.Server.Loggers.Abstractions;
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
        /// <param name="logger">Логгер для логирования операций входа и выхода пользователей.</param>
        public ExternalRegistrationUserService(IUserRepository userRepository, IAuthLogger logger = null)
        {
            this.UserRepository = userRepository;
            this.Logger = logger;
        }

        /// <summary>
        /// Получает или задает репозиторий пользователей.
        /// </summary>
        private IUserRepository UserRepository { get; set; }

        /// <summary>
        /// Получает или задает логгер для логирования операций входа и выхода пользователей.
        /// </summary>
        private IAuthLogger Logger { get; set; }

        /// <inheritdoc/>
        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            string userName = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name)?.Value;
            if (userName == null)
            {
                await this.Logger?.UnsuccessfulSigningInAsync(
                    userName: userName,
                    clientId: context.SignInMessage.ClientId);
                return;
            }

            IdSrvUserDto user = await this.UserRepository.GetUserByUserNameAsync(userName);
            if (user != null && !user.IsBlocked)
            {
                context.AuthenticateResult = user.IsBlocked ?
                    new AuthenticateResult($"User is \"{userName}\" blocked") :
                    new AuthenticateResult(
                        user.Id.ToString(),
                        userName,
                        identityProvider: context.ExternalIdentity.Provider);

                await this.Logger?.UserSignedInAsync(
                    userId: user.Id.ToString(),
                    clientId: context.SignInMessage.ClientId,
                    userName: user.UserName,
                    isBlocked: user.IsBlocked);
            }
            else
            {
                context.AuthenticateResult = new AuthenticateResult($"User is \"{userName}\" not registered");
                await this.Logger?.NotRegisteredUserTryToSignInAsync(
                    userName: user.UserName,
                    clientId: context.SignInMessage.ClientId);
            }
        }

        /// <inheritdoc/>
        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            IdSrvUserDto user = await this.UserRepository.GetUserByIdAsync(context.Subject.GetSubjectId());
            await this.Logger?.ProfileDataAccessedAsync(
                userId: user?.Id.ToString(),
                userName: user?.UserName,
                clientId: context.Client?.ClientId,
                clientName: context.Client?.ClientName,
                isBlocked: user != null ? user.IsBlocked : false);

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

        /// <inheritdoc/>
        public override async Task SignOutAsync(SignOutContext context)
        {
            string userId = context.Subject?.Claims?.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject)?.Value;
            if (userId != null && context.ClientId != null)
            {
                await this.Logger?.UserSignedOutAsync(
                    userId: userId,
                    userName: context.Subject?.Claims?.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Name)?.Value,
                    clientId: context.ClientId);
            }
        }
    }
}