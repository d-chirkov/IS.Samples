namespace IdSrv.Server.Services.LoggedDecorators
{
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer3.Core;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Validation;
    using IdSrv.Server.Loggers.Abstractions;

    /// <summary>
    /// Декоратор валидатора аутентификационных данных windows-пользователей,
    /// добавляющий логирование входа и выхода пользователя.
    /// </summary>
    internal class LoggedGrantValidatorDecorator : ICustomGrantValidator
    {
        /// <summary>
        /// Инициализирует декоратор.
        /// </summary>
        /// <param name="validator">Декорируемый валидатор.</param>
        /// <param name="logger">Логгер.</param>
        public LoggedGrantValidatorDecorator(ICustomGrantValidator validator, IAuthLogger logger = null)
        {
            this.Validator = validator;
            this.Logger = logger;
        }

        /// <inheritdoc/>
        public string GrantType => this.Validator.GrantType;

        /// <summary>
        /// Получает или задает декорируемый валидатор.
        /// </summary>
        private ICustomGrantValidator Validator { get; set; }

        /// <summary>
        /// Получает или задает логгер, с помощью которого будут логироваться операции входа
        /// windows-пользователей.
        /// </summary>
        private IAuthLogger Logger { get; set; }

        /// <inheritdoc/>
        public async Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            CustomGrantValidationResult result = await this.Validator.ValidateAsync(request);
            var userId = result.Principal.Claims.FirstOrDefault(v => v.Type == Constants.ClaimTypes.Subject)?.Value;
            var userName = result.Principal.Claims.FirstOrDefault(v => v.Type == Constants.ClaimTypes.Name)?.Value;
            if (!result.IsError)
            {
                await this.Logger?.UserSignedInAsync(
                    userId: userId,
                    userName: userName,
                    clientId: request.Client?.ClientId,
                    clientName: request.Client?.ClientName);
            }
            else
            {
                await this.Logger?.UnsuccessfulSigningInAsync(
                    userName: userName,
                    clientId: request.Client?.ClientId,
                    clientName: request.Client?.ClientName,
                    reason: result?.ErrorDescription);
            }

            return result;
        }
    }
}
