using System.Runtime.Serialization;

namespace Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940
{
    [DataContract(Name = "LoadMT940Response", Namespace = "http://www.gulffinance.com.sa/frs/v1/bank/mt940/operations/load/response")]
    public class LoadMT940Response
    {
        [DataMember(Order = 0, IsRequired = true, Name = "Code")]
        public long Code;

        [DataMember(Order = 0, IsRequired = true, Name = "Message")]
        public string Message;

        public const long SUCCESS_CODE = 0;
        public const string SUCCESS_MESSAGE = "Request completed successfully! HURRAY!!!";

        public LoadMT940Response(long code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
