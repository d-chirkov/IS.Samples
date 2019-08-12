namespace IdSrv.Server.Services
{
    /*
     * ВАЖНО:
     * Ставим NuGet пакет System.IdentityModel.Tokens.Jwt версии 4.0.4 (это последняя из 4.x на момент написания)
     * С версиями 5.x не работает.
     * Остальные пакеты самые новые (на момент написания)
     * 
     * Делать Scopes именно таким, какой он есть тоже надо, чтобы Claim-ы пользователя подцепились
     *
     * Для развертывания необходимо включить Windows Authentication
     * В IIS Express это ставится через Property проекты (там уже стоит Enabled, но по-умолчанию оно выключено)
     * Для развёртывания на полноценном IIS - смотрим Web.config и комментарий в нём (строка 18)
     */

    using System.DirectoryServices.AccountManagement;
    using System.Threading.Tasks;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Validation;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    /// <summary>
    /// Валидатор аутентификационных данных windows-пользователей. Класс необходим для расширения
    /// функционала identity server, чтобы он мог работать с windows-пользователями.
    /// Именно этот класс отвечает за общение с возможным контроллером домена (или просто
    /// windows ПК), который содержит учётные записи пользователей, с целью проверки логина и пароля.
    /// </summary>
    internal class CustomGrantValidator : ICustomGrantValidator
    {
        /// <summary>
        /// Инициализирует валидатор.
        /// </summary>
        /// <param name="userRepository">Репозиторий пользователей.</param>
        /// <param name="clientRepository">Репозиторий клиентов.</param>
        public CustomGrantValidator(
            IUserRepository userRepository,
            IClientRepository clientRepository)
        {
            this.UserRepository = userRepository;
            this.ClientRepository = clientRepository;
        }

        /// <inheritdoc/>
        public string GrantType => "winauth";

        /// <summary>
        /// Получает или задает репозиторий пользователей.
        /// </summary>
        private IUserRepository UserRepository { get; set; }

        /// <summary>
        /// Получает или задает репозиторий клиентов.
        /// </summary>
        private IClientRepository ClientRepository { get; set; }

        /// <inheritdoc/>
        public async Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            string userName = request.Raw.Get("name");
            IdSrvUserDto user = await this.UserRepository.GetUserByUserNameAsync(userName);
            IdSrvClientDto client = await this.ClientRepository.GetClientByIdAsync(request.Client.ClientId);
            string password = request.Raw.Get("password");

            // Выполняем проверку учетки пользователя
            // Значение ContextType.Machine для домена вероятно надо будет поменять на ContextType.Domain (не тестировал)
            var pc = new PrincipalContext(ContextType.Machine);
            bool isCredentialValid = false;
            if (user != null && !user.IsBlocked && client != null && !client.IsBlocked)
            {
                isCredentialValid = pc.ValidateCredentials(userName, password);
            }

            var authResult = new AuthenticateResult(
                subject: user != null ? user.Id.ToString() : "-",
                name: userName);
            var grantResult = new CustomGrantValidationResult
            {
                IsError = !isCredentialValid,
                Error = authResult.ErrorMessage,
                ErrorDescription = (user != null && user.IsBlocked) ? $"User \"{userName}\" is blocked" : authResult.ErrorMessage,
                Principal = authResult.User,
            };

            return grantResult;
        }
    }
}
