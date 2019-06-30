namespace IdSrv.Server.Loggers.Abstractions
{
    using System.Threading.Tasks;

    internal interface IAuthLogger
    {
        Task UserSignedInAsync(string userId, string clientId, string userName = null, string clientName = null, bool isBlocked = false);

        Task UserSignedOutAsync(string userId, string clientId, string userName = null, string clientName = null);

        Task ProfileDataAccessedAsync(string userId, string clientId, string userName = null, string clientName = null, bool isBlocked = false);

        Task UnsuccessfulSigningInAsync(string userName, string clientId, string clientName = null);

        Task NotRegisteredUserTryToSignInAsync(string userName, string clientId, string clientName = null);
    }
}
