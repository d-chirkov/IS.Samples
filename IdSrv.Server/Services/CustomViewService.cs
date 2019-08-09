namespace IdSrv.Server.Services
{
    using System.IO;
    using System.Threading.Tasks;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using IdentityServer3.Core.ViewModels;

    /// <summary>
    /// Перегруженная версия <see cref="DefaultViewService"/>, в которой при операции
    /// выхода не указывается имя клиента (по-умолчанию оно присутствует). Это связано с тем, что
    /// identity server запоминает для вошедшего пользователя того клиента, на котором был произведён
    /// вход. При выходе на другом клиенте выводится имя клиента, на котором был произведён вход,
    /// что немного странно выглядит. Было решено просто убрать имя клиента со страницы 
    /// (какого-то особого смысла оно всё равно не несёт).
    /// </summary>
    internal class CustomViewService : DefaultViewService
    {
        /// <summary>
        /// Конструктор просто инициализирует базовый класс.
        /// </summary>
        /// <param name="config">Объект <see cref="DefaultViewServiceOptions"/>.</param>
        /// <param name="viewLoader">Реализация <see cref="IViewLoader"/>.</param>
        public CustomViewService(DefaultViewServiceOptions config, IViewLoader viewLoader)
            : base(config, viewLoader)
        {
        }

        /// <summary>
        /// Перегруженная версия функции <see cref="DefaultViewService.LoggedOut(LoggedOutViewModel, SignOutMessage)"/>,
        /// из которой удалено вывод имени клиента.
        /// </summary>
        /// <param name="model">
        /// Объект <see cref="LoggedOutViewModel"/>, содержит имя клиента, которое в методе приравнивается null.
        /// </param>
        /// <param name="message">Объект <see cref="SignOutMessage"/>.</param>
        /// <returns>
        /// Возвращаемое значение базового класса <see cref="DefaultViewService"/>, на вход которого
        /// подаётся модель <see cref="LoggedOutViewModel"/> с отстутствующим именем клиента.
        /// </returns>
        public override Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
            model.ClientName = null;
            return base.LoggedOut(model, message);
        }
    }
}