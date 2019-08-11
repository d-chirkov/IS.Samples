namespace Site1.Mvc5
{
    using System.Web.Optimization;

    /// <summary>
    /// Конфигурация для свертков (js, css). Стандартная реализация из шаблона.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// Добавить новые свёртки в колекцию.
        /// </summary>
        /// <param name="bundles">
        /// Коллекция свёртков, является выходным параметром.
        /// </param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
