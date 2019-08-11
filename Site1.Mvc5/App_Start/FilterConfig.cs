namespace Site1.Mvc5
{
    using System.Web.Mvc;

    /// <summary>
    /// Конфигарция фильтров ASP.NET, стандартная реализация.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Зарегистрировать глобальные фаильтры.
        /// </summary>
        /// <param name="filters">Коллекция глоабльных фильтров.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
