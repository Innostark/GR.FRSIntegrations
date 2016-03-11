using Gf.Frs.MT940LoaderService.Async;
using System;
using System.Runtime.Serialization;

namespace Gf.Frs.MT940LoaderService.InputOutput.MT940
{    public class LoadMT940AfterInsertResponseAsyncResponse : AsyncResponse
    {
        [DataMember(Order = 0, IsRequired = true, Name = "Code")]
        public long Code;

        [DataMember(Order = 0, IsRequired = true, Name = "Message")]
        public string Message;

        public const long SUCCESS_CODE = 0;
        public const string SUCCESS_MESSAGE = "Request completed successfully! HURRAY!!!";

        public LoadMT940AfterInsertResponseAsyncResponse(long code, string message, AsyncCallback callback, object state)
        : base(callback, state)
        {
            Code = code;
            Message = message;
        }
    }
}
