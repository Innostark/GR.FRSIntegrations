using Gf.Frs.MT940Loader.Faults;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Raptorious.SharpMt940Lib;

namespace Gf.Frs.MT940Loader.Helpers
{
    public static class DotNetHelper
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

        public static string WrapFaultListToString(List<MT940LoaderFault> faults)
        {
            StringBuilder sb = new StringBuilder();
            foreach (MT940LoaderFault fault in faults)
                sb.AppendLine("Code = " + fault.Code + ", Message = " + fault.Message);

            return sb.ToString();
        }

        public static MT940Balance ConvertTransactionBalanceToMT940Balance(this TransactionBalance source, byte currencyValue, string userId)
        {
            return new MT940Balance
            {
                CurrencyId = currencyValue,
                DebitOrCredit = source.DebitCredit.ConvertToDebitOrCredit(),
                EntryDate = source.EntryDate,
                Value = source.Balance.Value,
                CreatedBy = userId,
                ModifiedBy = userId
            };
        }

        public static string ConvertToDebitOrCredit(this DebitCredit source)
        {
           return (source == DebitCredit.RC || source == DebitCredit.Credit) ? "C" : "D";
        }
    }
}
