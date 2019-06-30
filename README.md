# IS.Samples
- <b>IdSrv.Account.Models</b> - 
  общие модели dto для использования между проектами
  
- <b>IdSrv.Server</b> - сам IdentityServer (v3), одновременно запускается обычный и для windows (имеет логирование), знает про WebApi
- <b>IdSrv.Account.WebApi</b> - rest-сервис для доступа к пользователям и клиентам, только он знает про БД (нет ни валидации, ни логирования, самый минимальный функционал)
- <b>IdSrv.Account.WebControl</b> - сайт для управления пользователями и клиентами (создание, удаление, блокировака, редактирование), знает про WebApi
- <b>IdSrv.Connector</b> - содержит класс-расширение IAppBuilder для подключения сайта к IdSrv
- <b>IdSrv.AspNet.Helpers</b> - содержит статический класс для упрощения интеграции сайтов с IdSrv


Остальные проекты - примеры клиентов.

# Интеграция существующего сайта:
- Необходимо установить сборки Microsoft.Owin.Host.SystemWeb, IdSrv.Connector, IdSrv.AspNet.Helpers
- Залезаем в Web.config, среди прочего должны быть такие строки:
```
  <dependentAssembly>
    <assemblyIdentity name="Microsoft.Owin" publicKeyToken="31bf3856ad364e35" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-4.0.1.0" newVersion="4.0.1.0" />
  </dependentAssembly>
  <dependentAssembly>
    <assemblyIdentity name="Microsoft.Owin.Security" publicKeyToken="31bf3856ad364e35" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-4.0.1.0" newVersion="4.0.1.0" />
  </dependentAssembly>
  <dependentAssembly>
    <assemblyIdentity name="System.IdentityModel.Tokens.Jwt" publicKeyToken="31bf3856ad364e35" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-4.0.40306.1554" newVersion="4.0.40306.1554" />
  </dependentAssembly>
  <dependentAssembly>
    <assemblyIdentity name="Microsoft.IdentityModel.Protocol.Extensions" publicKeyToken="31bf3856ad364e35" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-1.0.40306.1554" newVersion="1.0.40306.1554" />
  </dependentAssembly>
  <dependentAssembly>
    <assemblyIdentity name="Microsoft.IdentityModel.Protocols" publicKeyToken="31bf3856ad364e35" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-5.3.0.0" newVersion="5.3.0.0" />
  </dependentAssembly>
  <dependentAssembly>
    <assemblyIdentity name="Microsoft.IdentityModel.Tokens" publicKeyToken="31bf3856ad364e35" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-5.3.0.0" newVersion="5.3.0.0" />
  </dependentAssembly>
  <dependentAssembly>
    <assemblyIdentity name="Microsoft.IdentityModel.Logging" publicKeyToken="31bf3856ad364e35" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-5.3.0.0" newVersion="5.3.0.0" />
  </dependentAssembly>
```
- Создаём/открываем Startup.cs, метод Configuration можно наполнить примерно следующим содержанием:
```
  string idsrvAddress = "https://localhost:44363/identity";
  app
      .UseAuthServer(idsrvAddress)
      .WithClientId("2111f461-02f0-4376-96e6-7cd69796c67f")
      .WithClientSecret("secret3")
      .WithOwnAddress("http://localhost:56140/");

  IdSrvConnection.IdSrvAddress = idsrvAddress;
  IdSrvConnection.UseAutoLogoutWhenNoAccess = true;
```
Тут важно, что клиента предварительно нужно создать через WebControl, посмотреть его guid, и забить его сюда.

# Логирование:
На данный момент есть 3 раздельных лога:
- IdSrv.Server.System.log - лог самого IdentityServer, если что-то пошло не так, можно лезть туда
- IdSrv.Server.identity.log - лог той части IdSrv, что отвечает за обычных пользователей
- IdSrv.Server.winidentity.log - лог той части IdSrv, что отвечает за windows пользователей

Логируется вход, выход, доступ к данным (в основном это проверка того, что пользователь заблокирован, см. ниже)
К сожалению, IdentityServer привязвает к токену доступа того клиента, который его создавал (производил вход). 
Поэтому при логировании операции доступа к данным и выхода указывают не того клиента, который совершает операции, а того, кто создал этот токен.
Как исправить пока не знаю.

# Автовыход
При каждом обновлении страницы на сайте-клиенте (если пользователь вошёл под учёткой), сайт будет запрашивать данные у IdSrv, не заблокирован ли пользователь.
Может быть небольшой оверхед (хотя json маленький, всего два поля), но других идей пока нет (backchannel для оповещения клиентов появился только в IdentityServer4, да и не факт, что применимо, клиенты могут быть за натом)
Как результат, если заблокировать пользователя или клиента, то пользователя автоматически выкинет (с сообщением) с клиента.

# Создание пользователей
На WebControl можно не указывать пароль пользователя, тогда WebApi не будет предоставлять возможность проверить его учётные данные.
Так, например, для Windows пользователей не надо указывать пароль, потому что проверка учётных данных происходит не через WebApi.

На WebControl можно не указывать uri клиента, это нужно для настольных клиентов (wpf, winforms, ...), у которых этого uri и нет.

Идентификаторы (GUID-ы) назначаются автоматически, так что перед добавлением конфигурации в коде клиениа (там нужен id клиента), сначала создайте его в WebControl

# Пример локальной политики для пользователей сайта:
Решается перегрузкой стандартного атрибута AuthorizeAttribute, пример есть в проекте Site1.Mvc5, там же пример регистрации новых пользователей через клиента (он просто дёргает WebApi)

# P.S.
Если уже тестировали эти проекты, почистите кэш для localhost, на всякий случай.
