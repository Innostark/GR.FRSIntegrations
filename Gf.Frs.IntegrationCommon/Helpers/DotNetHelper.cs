using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Gf.Frs.IntegrationCommon.Fault;

namespace Gf.Frs.IntegrationCommon.Helpers
{
    public static partial class DotNetHelper
    {
        public static string ReadAppConfigAppSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                return appSettings[key] ?? "Not Found";
                //Console.WriteLine(result);
            }
            catch (ConfigurationErrorsException)
            {
                //Console.WriteLine("Error reading app settings");
            }

            return null;
        }

        public static short ConvertShort(byte value)
        {
            return Convert.ToInt16(value);
        }

        public static bool IsBase64(this string base64String)
        {
            // Credit: oybek http://stackoverflow.com/users/794764/oybek
            if (base64String == null || base64String.Length == 0 || base64String.Length % 4 != 0
               || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception ex)
            {
                //Let it pass as we need to notify that the string is not a valid base64
            }
            return false;
        }

        public static string WrapFaultListToString(List<LoaderFault> faults)
        {
            StringBuilder sb = new StringBuilder();
            foreach (LoaderFault fault in faults)
                sb.AppendLine("Code = " + fault.Code + ", Message = " + fault.Message);

            return sb.ToString();
        }
    }
}
