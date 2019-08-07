namespace IdSrv.Server.Services
{
    using System.IO;
    using System.Threading.Tasks;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using IdentityServer3.Core.ViewModels;

    internal class CustomViewService : DefaultViewService
    {
        public CustomViewService(DefaultViewServiceOptions config, IViewLoader viewLoader)
            : base(config, viewLoader)
        {
        }

        public override Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
            model.ClientName = null;
            return base.LoggedOut(model, message);
        }
    }
}