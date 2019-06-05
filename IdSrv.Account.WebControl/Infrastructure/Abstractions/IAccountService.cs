namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    using IdSrv.Account.WebControl.Models;
    using System.Collections.Generic;

    public interface IAccountService
    {
        IEnumerable<IdSrvUserDTO> GetUsers();

        bool CreateUser(NewIdSrvUserDTO newUser);

        bool ChangePasswordForUser(ChangeIdSrvUserPasswordDTO passwords);

        IEnumerable<IdSrvApplicationDTO> GetApplications();
    }
}