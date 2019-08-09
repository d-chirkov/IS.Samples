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
    /// Сервис пользователей, реализаует логику проверки аутетификационных данных пользовтелей и
    /// логику получения claim-ов (данных профиля) пользователей. Расширяет базоый класс
    /// identity server <see cref="UserServiceBase"/> (часть api identity server), который не предоставляет
    /// никакой стандартной реализации для своих методов (все они просто возвращают void).
    /// </summary>
    internal class CustomUserService : UserServiceBase
    {
        /// <summary>
        /// Инициализировать сервис пользователей.
        /// </summary>
        /// <param name="userRepository">Репозиторий пользователей.</param>
        /// <param name="logger">Логгер для логирования операций входа и выхода пользователей.</param>
        public CustomUserService(IUserRepository userRepository, IAuthLogger logger = null)
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
        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            IdSrvUserDto user = await this.UserRepository.GetUserByUserNameAndPasswordAsync(context.UserName, context.Password);
            if (user != null)
            {
                context.AuthenticateResult = user.IsBlocked ?
                    new AuthenticateResult(errorMessage: $"User \"{user.UserName}\" is blocked") :
                    new AuthenticateResult(user.Id.ToString(), user.UserName);
                await this.Logger?.UserSignedInAsync(
                    userId: user.Id.ToString(),
                    userName: user.UserName,
                    clientId: context.SignInMessage.ClientId,
                    isBlocked: user.IsBlocked);
            }
            else
            {
                await this.Logger?.UnsuccessfulSigningInAsync(
                    userName: context.UserName,
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