namespace Gf.Frs.WcfLoaderServices.Loging
{
    public class FrsNLogIntegrationServiceStoredProc
    {
        private string _siteName;
        private string _userID;
        private string _serverName;
        private string _port;
        private string _sessionID;
        private string _callerDetails;
        private string _requestDump;
        private string _responseDump;
        private string _url;
        private bool _https;
        private string _serverAddress;
        private string _remoteAddress;
        private string _callSite;

        public string SiteName
        {
            get
            {
                return _siteName;
            }

            set
            {
                _siteName = value;
            }
        }

        public string UserID
        {
            get
            {
                return _userID;
            }

            set
            {
                _userID = value;
            }
        }

        public string ServerName
        {
            get
            {
                return _serverName;
            }

            set
            {
                _serverName = value;
            }
        }

        public string Port
        {
            get
            {
                return _port;
            }

            set
            {
                _port = value;
            }
        }

        public string SessionID
        {
            get
            {
                return _sessionID;
            }

            set
            {
                _sessionID = value;
            }
        }

        public string CallerDetails
        {
            get
            {
                return _callerDetails;
            }

            set
            {
                _callerDetails = value;
            }
        }

        public string RequestDump
        {
            get
            {
                return _requestDump;
            }

            set
            {
                _requestDump = value;
            }
        }

        public string ResponseDump
        {
            get
            {
                return _responseDump;
            }

            set
            {
                _responseDump = value;
            }
        }

        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                _url = value;
            }
        }

        public bool Https
        {
            get
            {
                return _https;
            }

            set
            {
                _https = value;
            }
        }

        public string ServerAddress
        {
            get
            {
                return _serverAddress;
            }

            set
            {
                _serverAddress = value;
            }
        }

        public string RemoteAddress
        {
            get
            {
                return _remoteAddress;
            }

            set
            {
                _remoteAddress = value;
            }
        }

        public string CallSite
        {
            get
            {
                return _callSite;
            }

            set
            {
                _callSite = value;
            }
        }
    }
}