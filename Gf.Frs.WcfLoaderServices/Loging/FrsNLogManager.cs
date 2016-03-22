using NLog;
using NLog.Config;
using NLog.Targets;
using System.ComponentModel;
using System.Globalization;

namespace Gf.Frs.WcfLoaderServices.Loging
{
    public class FrsNLogManager: Logger, ILog
    {
        public Logger Instance { get; private set; }
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

    }
}