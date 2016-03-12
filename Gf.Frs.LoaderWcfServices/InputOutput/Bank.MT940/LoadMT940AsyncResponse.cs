using Gf.Frs.LoaderWcfServices.Async;
using System;
using System.Runtime.Serialization;

namespace Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940
{
    [DataContract(Name = "LoadMT940AsyncResponse", Namespace = "http://www.gulffinance.com.sa/frs/v1/bank/mt940/operations/load/response")]
    public class LoadMT940AsyncResponse : AsyncResponse
    {
        [DataMember(Order = 0, IsRequired = true, Name = "Code")]
        public long Code;

        [DataMember(Order = 0, IsRequired = true, Name = "Message")]
        public string Message;

        public const long SUCCESS_CODE = 0;
        public const string SUCCESS_MESSAGE = "Request completed successfully! HURRAY!!!";

        public LoadMT940AsyncResponse(long code, string message, AsyncCallback callback, object state)
        : base(callback, state)
        {
            Code = code;
            Message = message;
        }
    }
}
