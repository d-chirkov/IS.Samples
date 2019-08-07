namespace IdSrv.Server.Loggers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using IdSrv.Server.Loggers.Abstractions;

    public class FileAuthLogger : IAuthLogger
    {
        public FileAuthLogger(string pathToFile)
        {
            this.PathToFile = pathToFile ?? throw new ArgumentNullException(nameof(pathToFile));
        }

        private string PathToFile { get; set; }

        public Task NotRegisteredUserTryToSignInAsync(string userName, string clientId, string clientName = null)
        {
            string output = $"{this.GetTimeString()} [ERR] failed attempt to sign in, user not found";
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        public Task ProfileDataAccessedAsync(string userId, string clientId, string userName = null, string clientName = null, bool isBlocked = false)
        {
            string output = $"{this.GetTimeString()} [INF] access to user data";
            if (userId != null) output += $", user-id: {userId}" + (isBlocked ? " [BLOCKED]" : string.Empty);
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        public Task UnsuccessfulSigningInAsync(string userName, string clientId, string clientName = null)
        {
            string output = $"{this.GetTimeString()} [ERR] failed attempt to sign in, invalid credentials";
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        public Task UserSignedInAsync(string userId, string clientId, string userName = null, string clientName = null, bool isBlocked = false)
        {
            string output = $"{this.GetTimeString()} [INF] successufuly signed in";
            if (userId != null) output += $", user-id: {userId}" + (isBlocked ? " [BLOCKED]" : string.Empty);
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        public Task UserSignedOutAsync(string userId, string clientId, string userName = null, string clientName = null)
        {
            string output = $"{this.GetTimeString()} [INF] signed out";
            if (userId != null) output += $", user-id: {userId}";
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        private string GetTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}