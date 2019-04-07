// ВАЖНО:
// Ставим NuGet пакет System.IdentityModel.Tokens.Jwt версии 4.0.4 (это последняя из 4.x на момент написания)
// С версиями 5.x не работает.
// Остальные пакеты самые навые (на момент написания)
// 
// Делать Scopes именно таким, какой он есть тоже надо, чтобы Claim-ы пользователя подцепились
//
// Для развертывания необходимо включить Windows Authentication
// В IIS Express это ставится через Property проекты (там уже стоит Enabled, но по-умолчанию оно выключено)
// Для развёртывания на полноценном IIS - смотрим Web.config и комментарий в нём (строка 18)


using Microsoft.Owin;
using IdentityServer3.Core.Services;
using System.Threading.Tasks;
using IdentityServer3.Core.Validation;
using System.DirectoryServices.AccountManagement;
using IdentityServer3.Core.Models;

[assembly: OwinStartup(typeof(IS.WindowsAuth.Startup))]

namespace IS.WindowsAuth
{
    class CustomGrantValidator : ICustomGrantValidator
    {
        public string GrantType => "winauth";

        public Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            string name = request.Raw.Get("name");
            string password = request.Raw.Get("password");
            // Выполняем проверку учетки пользователя
            // Значение ContextType.Machine для домена вероятно надо будет поменять на ContextType.Domain (не тестировал)
            PrincipalContext pc = new PrincipalContext(ContextType.Machine);
            bool isCredentialValid = pc.ValidateCredentials(name, password);

            var authResult = new AuthenticateResult(
                // subject тут не нужен особо
                subject: "-",
                name: name
            );
            var grantResult = new CustomGrantValidationResult
            {
                IsError = !isCredentialValid,
                Error = authResult.ErrorMessage,
                ErrorDescription = authResult.ErrorMessage,
                Principal = authResult.User
            };
            return Task.FromResult(grantResult);
        }
    }
}
