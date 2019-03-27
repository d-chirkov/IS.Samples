//using IdentityServer3.Core.Models;
//using System.Collections.Generic;

//namespace IS.Models
//{
//    // Конфигурация клиентов в памяти, для работы с бд см. https://identityserver.github.io/Documentation/docsv2/ef/overview.html
//    public static class Clients
//    {
//        public static IEnumerable<Client> Get()
//        {
//            return new[]
//            {
//                new Client
//                {
//                    Enabled = true,
//                    ClientName = "Site1",

//                    // ID клиента, также указывается в настройках самого клиента, см. конфигарцию в классе Startup
//                    // проектов Site1 и Site2
//                    ClientId = "site1",

//                    ClientSecrets = new List<Secret>
//                    {
//                        new Secret("secret".Sha256()),

//                    },

//                    // Используется для относительно коротких сессий, 
//                    // см. https://www.scottbrady91.com/OpenID-Connect/OpenID-Connect-Flows
//                    Flow = Flows.Hybrid,

//                    // Время жизни identity токена в секундах, то есть токена идентификации.
//                    // Также есть access токен, то есть токен для доступа к данным,
//                    // он обычно имеет короткое время жизни.
//                    // Для того, чтобы сервер аутентификации "забывал" логин и пароль пользователя, 
//                    // надо также выставить время жизни cookie сессии сервера аутентификации, см.
//                    // класс Startup.
//                    IdentityTokenLifetime = 5 * 60,

//                    // При входе на сайте через сервер аутентификации в первый раз у пользователя
//                    // спрашивают, какие данные сайт может использовать (например, может ли сайт просматривать
//                    // профиль пользователя). Для текущих целей не нужно, поэтому пропускаем.
//                    RequireConsent = false,

//                    // Адрес сайта, куда будет редиректить после входа (по идее должен совпадать с адресом
//                    // самого сайта)
//                    RedirectUris = new List<string>
//                    {
//                        "http://localhost:51542/"
//                    },

//                    // Адрес, на который редиректит после выхода
//                    PostLogoutRedirectUris = new List<string>
//                    {
//                        "http://localhost:51542/"
//                    },

//                    // Scope-ы в данном примере не освещаются, по идее с помощью них можно разделить 
//                    // клиентов (сайты) на области и рудить ими по-отдельности, допускать пользователей
//                    // в разные области. Для текущих целей пока не нужно.
//                    AllowAccessToAllScopes = true
//                },

//                new Client
//                {
//                    Enabled = true,
//                    ClientName = "Site2",
//                    ClientId = "site2",
//                    Flow = Flows.Implicit,

//                    IdentityTokenLifetime = 5 * 60,

//                    RequireConsent = false,

//                    RedirectUris = new List<string>
//                    {
//                        "http://localhost:51566/"
//                    },

//                    PostLogoutRedirectUris = new List<string>
//                    {
//                        "http://localhost:51566/"
//                    },

//                    AllowAccessToAllScopes = true
//                },

//                new Client
//                {
//                    Enabled = true,
//                    ClientName = "Site3(WebForms)",
//                    ClientId = "site3",
//                    Flow = Flows.Implicit,

//                    IdentityTokenLifetime = 5 * 60,

//                    RequireConsent = false,

//                    RedirectUris = new List<string>
//                    {
//                        "http://localhost:56140/"
//                    },

//                    PostLogoutRedirectUris = new List<string>
//                    {
//                        "http://localhost:56140/"
//                    },

//                    AllowAccessToAllScopes = true
//                }
//            };
//        }
//    }
//}