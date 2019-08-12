namespace IdSrv.Server.Services.LoggedDecorators
{
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer3.Core;
    using IdentityServer3.Core.Extensions;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using IdSrv.Server.Loggers.Abstractions;

    /// <summary>
    /// Декоратор сервиса пользователя, добавляющий логирование входа и выхода пользователя.
    /// </summary>
    internal class LoggedUserServiceDecorator : UserServiceBase
    {
        /// <summary>
        /// Инициализировать декоратор.
        /// </summary>
        /// <param name="userService">Декорируемый сервис.</param>
        /// <param name="logger">Логгер для логирования операций входа и выхода пользователей.</param>
        public LoggedUserServiceDecorator(UserServiceBase userService, IAuthLogger logger)
        {
            this.UserService = userService;
            this.Logger = logger;
        }

        /// <summary>
        /// Получает или задает декорируемый сервис пользователя.
        /// </summary>
        private UserServiceBase UserService { get; set; }

        /// <summary>
        /// Получает или задает логгер для логирования операций входа и выхода пользователей.
        /// </summary>
        private IAuthLogger Logger { get; set; }

        /// <inheritdoc/>
        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            await this.UserService.AuthenticateLocalAsync(context);
            var user = context.AuthenticateResult?.User;
            if (user != null)
            {
                await this.Logger?.UserSignedInAsync(
                    userId: user.GetSubjectId(),
                    userName: user.GetName(),
                    clientId: context.SignInMessage?.ClientId);
            }
            else
            {
                await this.Logger?.UnsuccessfulSigningInAsync(
                    userName: context.UserName,
                    clientId: context.SignInMessage?.ClientId,
                    reason: context.AuthenticateResult?.ErrorMessage ?? "bad username or password");
            }
        }

        /// <inheritdoc/>
        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            await this.UserService.AuthenticateExternalAsync(context);
            var user = context.AuthenticateResult?.User;
            if (user != null)
            {
                await this.Logger?.UserSignedInAsync(
                    userId: user.GetSubjectId(),
                    userName: user.GetName(),
                    clientId: context.SignInMessage?.ClientId);
            }
            else
            {
                string userName = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name)?.Value;
                await this.Logger?.UnsuccessfulSigningInAsync(
                    userName: userName,
                    clientId: context.SignInMessage?.ClientId,
                    reason: context.AuthenticateResult?.ErrorMessage ?? "bad username or password");
            }
        }

        /// <inheritdoc/>
        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await this.UserService.GetProfileDataAsync(context);
            await this.Logger?.ProfileDataAccessedAsync(
                userId: context.Subject.GetSubjectId(),
                clientId: context.Client?.ClientId,
                clientName: context.Client?.ClientName,
                isSuccess: context.IssuedClaims?.Count() > 0);
        }

        /// <inheritdoc/>
        public override async Task SignOutAsync(SignOutContext context)
        {
            await this.UserService.SignOutAsync(context);
            var userClaims = context.Subject?.Claims;
            if (userClaims != null)
            {
                await this.Logger?.UserSignedOutAsync(
                        userId: userClaims?.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject)?.Value,
                        userName: userClaims?.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Name)?.Value,
                        clientId: context.ClientId);
            }
        }
    }
}