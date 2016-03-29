using System;

namespace Gf.Frs.WcfLoaderServices.Loging
{
    public class FrsNLogIntegrationServiceStoredProc: IDisposable
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
        private bool _disposed = false;

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

        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    _callerDetails = null;
                    _callSite = null;
                    _port = null;
                    _remoteAddress = null;
                    _requestDump = null;
                    _responseDump = null;
                    _serverAddress = null;
                    _serverName = null;
                    _sessionID = null;
                    _siteName = null;
                    _url = null;
                    _userID = null;
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.


                // Note disposing has been done.
                _disposed = true;

            }
        }

    }
}