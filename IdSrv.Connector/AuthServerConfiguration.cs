namespace IdSrv.Connector
{
    internal class AuthServerConfiguration
    {
        public string IdSrvAddress { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string OwnAddress { get; set; }

        public bool UseWebForms { get; set; } = false;

        public bool IsComplete {
            get
            {
                return 
                    this.IdSrvAddress != null && 
                    this.ClientId != null && 
                    this.ClientSecret != null && 
                    this.OwnAddress != null;
            }
        }
    }
}
