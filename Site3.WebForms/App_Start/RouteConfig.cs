namespace Site3.WebForms
{
    using System.Web.Routing;
    using Microsoft.AspNet.FriendlyUrls;

    /// <summary>
    /// ������������ ��������� ASP.NET.
    /// </summary>
    public static class RouteConfig
    {
        /// <summary>
        /// �������� �������� � ���������.
        /// </summary>
        /// <param name="routes">��������� ���������, �������� ��������.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Permanent;
            routes.EnableFriendlyUrls(settings);
        }
    }
}
