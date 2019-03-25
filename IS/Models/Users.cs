using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;
using System.Collections.Generic;
using System.Security.Claims;

namespace IS.Models
{
    // Конфигурация пользователей в памяти, для работы с бд см. https://identityserver.github.io/Documentation/docsv2/advanced/userService.html
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    // Имя пользователя, вводится в поле логина при входе
                    Username = "bob",

                    // Пароль пользователя
                    Password = "123",

                    // ID пользователя
                    Subject = "1",

                    // Также можно добавлять свои данные, например если мы хотим получить доступ к 
                    // имени пользователя на сайте, то надо явно добавить его как Claim. Это не касается
                    // ID пользователя, оно автоматически добавляется как Claim.
                    Claims = new[]
                    {
                        new Claim(Constants.ClaimTypes.Name, "bob"),
                    }
                }
            };
        }
    }
}