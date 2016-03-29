using Gf.Frs.IntegrationCommon.DataModel;
using Gf.Frs.LoaderWcfServices.InputOutput.Accounts.OracleGL;
using Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940;
using Gf.Frs.OracleGLLoader.DataModel;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Gf.Frs.WcfLoaderServices.Loging
{
    public class FrsNLogManager: Logger, ILog
    {
        string _currentLogerName;

        public Logger Instance { get; private set; }
        FrsNLogIntegrationServiceStoredProc LogingSPDetails = new FrsNLogIntegrationServiceStoredProc();

        public string CurrentLogerName
        {
            get
            {
                return _currentLogerName;
            }

            set
            {
                _currentLogerName = value;
            }
        }



        public FrsNLogManager()
        {
            #region ##START## Sentinal & Harvester Logging Only in DEBUG mode
#if DEBUG
            // Setup the logging view for Sentinel - http://sentinel.codeplex.com
            var sentinalTarget = new NLogViewerTarget()
            {
                Name = "sentinal",
                Address = "udp://127.0.0.1:9999",
                IncludeNLogData = false
            };
            var sentinalRule = new LoggingRule("*", LogLevel.Trace, sentinalTarget);
            LogManager.Configuration.AddTarget("sentinal", sentinalTarget);
            LogManager.Configuration.LoggingRules.Add(sentinalRule);

            // Setup the logging view for Harvester - http://harvester.codeplex.com
            var harvesterTarget = new OutputDebugStringTarget()
            {
                Name = "harvester",
                Layout = "${log4jxmlevent:includeNLogData=false}"
            };
            var harvesterRule = new LoggingRule("*", LogLevel.Trace, harvesterTarget);
            LogManager.Configuration.AddTarget("harvester", harvesterTarget);
            LogManager.Configuration.LoggingRules.Add(harvesterRule);
#endif 
            #endregion ##END## Sentinal & Harvester Logging Only in DEBUG mode

            LogManager.ReconfigExistingLoggers();

            Instance = LogManager.GetCurrentClassLogger();
        }

        public void Write(LogType type, object properties, string message, params object[] args)
        {
            var info = new LogEventInfo(LogLevel.FromOrdinal((int)type), Name, CultureInfo.CurrentCulture, message, args);

            if (null != properties)
            {
                foreach (PropertyDescriptor propertyDescriptor
                    in TypeDescriptor.GetProperties(properties))
                {
                    var value = propertyDescriptor.GetValue(properties);
                    info.Properties[propertyDescriptor.Name] = value;
                }
            }

            Log(info);
        }

        public void SetSPDetailsAndLog(LogLevel level, string message)
        {
            LogEventInfo lei = new LogEventInfo()
            {
                LoggerName = CurrentLogerName
            };

            //Set logger details - called only once to set the stored procedure objects values
            SetLoggerDetails(lei);
            SetEventInfo(lei, level, message);
            Instance.Log(lei);
        }

        private void SetLoggerDetails(LogEventInfo logEventInfo)
        {
            LogingSPDetails.SiteName = OperationContext.Current.IncomingMessageHeaders.To.Authority;
            if (!logEventInfo.Properties.ContainsKey("SiteName"))
                logEventInfo.Properties["SiteName"] = LogingSPDetails.SiteName;

            LogingSPDetails.UserID = null;
            if (!logEventInfo.Properties.ContainsKey("UserID"))
                logEventInfo.Properties["UserID"] = LogingSPDetails.UserID;

            LogingSPDetails.ServerName = OperationContext.Current.IncomingMessageHeaders.To.DnsSafeHost;
            if (!logEventInfo.Properties.ContainsKey("ServerName"))
                logEventInfo.Properties["ServerName"] = LogingSPDetails.ServerName;

            LogingSPDetails.Port = OperationContext.Current.IncomingMessageHeaders.To.Port.ToString();
            if (!logEventInfo.Properties.ContainsKey("Port"))
                logEventInfo.Properties["Port"] = LogingSPDetails.Port;

            LogingSPDetails.SessionID = null;
            if (!logEventInfo.Properties.ContainsKey("SessionID"))
                logEventInfo.Properties["SessionID"] = LogingSPDetails.SessionID;

            LogingSPDetails.CallerDetails = OperationContext.Current.IncomingMessageHeaders.To.ToString();
            if (!logEventInfo.Properties.ContainsKey("CallerDetails"))
                logEventInfo.Properties["CallerDetails"] = LogingSPDetails.CallerDetails;

            LogingSPDetails.RequestDump = OperationContext.Current.RequestContext.ToString();
            if (!logEventInfo.Properties.ContainsKey("RequestDump"))
                logEventInfo.Properties["RequestDump"] = LogingSPDetails.RequestDump;

            LogingSPDetails.Url = OperationContext.Current.IncomingMessageHeaders.To.AbsoluteUri;
            if (!logEventInfo.Properties.ContainsKey("Url"))
                logEventInfo.Properties["Url"] = LogingSPDetails.Url;

            LogingSPDetails.Https = ((string.IsNullOrEmpty(OperationContext.Current.IncomingMessageHeaders.To.Scheme)) ? ((OperationContext.Current.IncomingMessageHeaders.To.Scheme.ToUpper().Equals("HTTPS")) ? true : false) : false);
            if (!logEventInfo.Properties.ContainsKey("Https"))
                logEventInfo.Properties["Https"] = LogingSPDetails.Https;


            IPHostEntry heServer = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress curAddress = heServer.AddressList[0];
            LogingSPDetails.ServerAddress = curAddress.MapToIPv4().ToString();
            if (!logEventInfo.Properties.ContainsKey("ServerAddress"))
                logEventInfo.Properties["ServerAddress"] = LogingSPDetails.ServerAddress;


            OperationContext context = OperationContext.Current;
            RemoteEndpointMessageProperty remoteEndpointMessageProperty = (RemoteEndpointMessageProperty)context.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            LogingSPDetails.RemoteAddress = remoteEndpointMessageProperty.Address;
            if (!logEventInfo.Properties.ContainsKey("RemoteAddress"))
                logEventInfo.Properties["RemoteAddress"] = LogingSPDetails.RemoteAddress;


            LogingSPDetails.CallSite = string.Empty;
            if (!logEventInfo.Properties.ContainsKey("CallSite"))
                logEventInfo.Properties["CallSite"] = LogingSPDetails.CallSite;

        }

        private void SetEventInfo(LogEventInfo lEventInfo, LogLevel level, string message, Exception ex = null)
        {
            lEventInfo.Level = level;
            lEventInfo.Message = message;
            lEventInfo.Exception = ex;
        }

        public void LogFromSPDetails(LogLevel level, string message, string response = null, FaultException fe = null)
        {
            LogEventInfo lei = new LogEventInfo()
            {
                LoggerName = CurrentLogerName
            };
            //Set logger details 
            SetLoggerDetailsFromSPDetails(lei, response);
            SetEventInfo(lei, level, message);

            if (fe != null)
                lei.Exception = fe;

            Instance.Log(lei);
        }

        private void SetLoggerDetailsFromSPDetails(LogEventInfo logEventInfo, string response = null)
        {
            logEventInfo.Properties["SiteName"] = LogingSPDetails.SiteName;
            logEventInfo.Properties["UserID"] = LogingSPDetails.UserID;
            logEventInfo.Properties["ServerName"] = LogingSPDetails.ServerName;
            logEventInfo.Properties["Port"] = LogingSPDetails.Port;
            logEventInfo.Properties["SessionID"] = LogingSPDetails.SessionID;
            logEventInfo.Properties["CallerDetails"] = LogingSPDetails.CallerDetails;
            logEventInfo.Properties["RequestDump"] = LogingSPDetails.RequestDump;
            logEventInfo.Properties["Url"] = LogingSPDetails.Url;
            logEventInfo.Properties["Https"] = LogingSPDetails.Https;
            logEventInfo.Properties["ServerAddress"] = LogingSPDetails.ServerAddress;
            logEventInfo.Properties["RemoteAddress"] = LogingSPDetails.RemoteAddress;
            logEventInfo.Properties["CallSite"] = LogingSPDetails.CallSite;

            if (response != null)
            {
                LogingSPDetails.ResponseDump = response.ToString();
                if (!logEventInfo.Properties.ContainsKey("ResponseDump"))
                    logEventInfo.Properties["ResponseDump"] = LogingSPDetails.ResponseDump;
            }
        }

        public void RaiseException(string message)
        {
            FaultException fe = new FaultException(message);
            LogFromSPDetails(LogLevel.Fatal, message, null, fe);
            throw fe;
        }    

    }
}