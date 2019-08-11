namespace Site3.WebForms
{
    using System;
    using System.Web;

    /// <summary>
    /// Страница выхода пользователя. Ничего не выводит, просто передаресует на identity server для выхода.
    /// Сам identity server после этого переадресует на домашнюю страницу этого сайта.
    /// </summary>
    public partial class Logout : System.Web.UI.Page
    {
        /// <summary>
        /// Обработчик события загрузки страницы.
        /// </summary>
        /// <param name="sender">Отправитель сообщения.</param>
        /// <param name="e">Аргументы сообщения.</param>
        /// <remarks>
        /// Аргументу тут не нужны, по факту метод просто вызывает методы выхода Owin-контекста,
        /// который переадресует на identity server.
        /// </remarks>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Request.GetOwinContext().Authentication.SignOut();
        }
    }
}