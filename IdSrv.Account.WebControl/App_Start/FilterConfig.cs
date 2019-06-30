using System.Web;
using System.Web.Mvc;

namespace IdSrv.Account.WebControl
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
