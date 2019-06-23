﻿// ВАЖНО:
// Ставим NuGet пакет System.IdentityModel.Tokens.Jwt версии 4.0.4 (это последняя из 4.x на момент написания)
// С версиями 5.x не работает.
// Остальные пакеты самые навые (на момент написания)
// 
// Делать Scopes именно таким, какой он есть тоже надо, чтобы Claim-ы пользователя подцепились
//
// Для развертывания необходимо включить Windows Authentication
// В IIS Express это ставится через Property проекты (там уже стоит Enabled, но по-умолчанию оно выключено)
// Для развёртывания на полноценном IIS - смотрим Web.config и комментарий в нём (строка 18)


namespace IdSrv.Server.Services
{
    using System.DirectoryServices.AccountManagement;
    using System.Threading.Tasks;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Validation;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    internal class CustomGrantValidator : ICustomGrantValidator
    {
        public string GrantType => "winauth";

        public IUserRepository UserRepository { get; set; }

        public IClientRepository ClientRepository { get; set; }

        public CustomGrantValidator(IUserRepository userRepository, IClientRepository clientRepository)
        {
            this.UserRepository = userRepository;
            this.ClientRepository = clientRepository;
        }

        public async Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            string name = request.Raw.Get("name");
            IdSrvUserDTO user = await this.UserRepository.GetUserByUserNameAsync(name);
            IdSrvClientDTO client = await this.ClientRepository.GetClientByIdAsync(request.Client.ClientId);
            string password = request.Raw.Get("password");
            // Выполняем проверку учетки пользователя
            // Значение ContextType.Machine для домена вероятно надо будет поменять на ContextType.Domain (не тестировал)
            var pc = new PrincipalContext(ContextType.Machine);
            bool isCredentialValid = false;
            if (user != null && !user.IsBlocked && client != null && !client.IsBlocked)
            {
                isCredentialValid = pc.ValidateCredentials(name, password);
            }
            var authResult = new AuthenticateResult(
                subject: user != null ? user.Id.ToString() : "-",
                name: name
            );
            var grantResult = new CustomGrantValidationResult
            {
                IsError = !isCredentialValid,
                Error = authResult.ErrorMessage,
                ErrorDescription = authResult.ErrorMessage,
                Principal = authResult.User
            };
            return grantResult;
        }
    }
}