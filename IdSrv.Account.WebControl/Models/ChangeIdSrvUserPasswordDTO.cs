namespace IdSrv.Account.WebControl.Models
{
    using System;

    public class ChangeIdSrvUserPasswordDTO
    {
        public Guid UserId { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string RepeatNewPassword { get; set; }
    }
}