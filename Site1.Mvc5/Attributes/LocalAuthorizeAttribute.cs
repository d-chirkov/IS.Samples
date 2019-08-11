namespace Site1.Mvc5.Attributes
{
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using IdSrv.AspNet.Helpers;
    using Site1.Mvc5.Models;

    /// <summary>
    /// Собственная реализация атрибута авторизации - проверяет доступ пользователя именно к этому сайту.
    /// Пользователи, разрешённые для этого сайта, регулируются самим сайтом (содержатся в БД).
    /// </summary>
    public class LocalAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Проверять ли локальный доступ.
        /// </summary>
        private bool checkLocalAccess;

        /// <summary>
        /// Инициализирует атрибут авторизации, в аргументах указывается, надо ли проеврять локальный доступ.
        /// </summary>
        /// <param name="checkLocalAccess">
        /// Если true (по-умолчанию) - проверяется локальный доступ (наличие пользователя в собственной БД),
        /// если false - ведёт себя как обычный <see cref="AuthorizeAttribute"/>.</param>
        public LocalAuthorizeAttribute(bool checkLocalAccess = true)
        {
            this.checkLocalAccess = checkLocalAccess;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Когда <see cref="checkLocalAccess"/> - true (передаётся через конструктор атрибута),
        /// то помимо наследуюемой логики также проверяется наличие пользователя в локльной БД.
        /// Если пользователя нет, то атрибут передаресует на страницу ~/Account/AccessDenied,
        /// (см <see cref="Site1.Mvc5.Controllers.AccountController.AccessDenied"/>).
        /// </remarks>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorized = base.AuthorizeCore(httpContext);
            if (!authorized)
            {
                return false;
            }

            if (!IdSrvConnection.IsAccessBlocked(httpContext) && this.checkLocalAccess)
            {
                string idsrvUserId = IdSrvConnection.GetUserId(httpContext);
                string userLogin = IdSrvConnection.GetUserName(httpContext);

                UserProfile userProfile = null;
                using (var context = new AccountsContext())
                {
                    userProfile =
                        context.UserProfiles.Where(p => p.IdSrvId == idsrvUserId).FirstOrDefault() ??
                        context.UserProfiles.Where(p => p.Login == userLogin).FirstOrDefault();
                }

                if (userProfile == null)
                {
                    httpContext.Response.Redirect("~/Account/AccessDenied");
                }

                return userProfile != null;
            }

            return true;
        }
    }
}