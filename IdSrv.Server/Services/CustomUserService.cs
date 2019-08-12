namespace IdSrv.Server.Services
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityServer3.Core;
    using IdentityServer3.Core.Extensions;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using IdSrv.Account.Models;
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
        public CustomUserService(IUserRepository userRepository)
        {
            this.UserRepository = userRepository;
        }

        /// <summary>
        /// Получает или задает репозиторий пользователей.
        /// </summary>
        private IUserRepository UserRepository { get; set; }

        /// <inheritdoc/>
        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            IdSrvUserDto user = await this.UserRepository.GetUserByUserNameAndPasswordAsync(context.UserName, context.Password);
            if (user != null)
            {
                context.AuthenticateResult = user.IsBlocked ?
                    new AuthenticateResult(errorMessage: $"User \"{user.UserName}\" is blocked") :
                    new AuthenticateResult(user.Id.ToString(), user.UserName);
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