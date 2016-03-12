using Gf.Frs.LoaderWcfServices.Async;
using System;
using System.Runtime.Serialization;

namespace Gf.Frs.LoaderWcfServices.InputOutput.Accounts.OracleGL
{
    [DataContract(Name = "LoadOracleGLAsyncResponse", Namespace = "http://www.gulffinance.com.sa/frs/v1/accounts/oraclegl/operations/load/response")]
    public class LoadOracleGLAsyncResponse : AsyncResponse
    {

        [DataMember(Order = 0, IsRequired = true, Name = "Code")]
        public long Code;

        [DataMember(Order = 0, IsRequired = true, Name = "Message")]
        public string Message;

        public const long SUCCESS_CODE = 0;
        public const string SUCCESS_MESSAGE = "Request completed successfully! HURRAY!!!";

        public LoadOracleGLAsyncResponse(long code, string message, AsyncCallback callback, object state)
        : base(callback, state)
        {
            Code = code;
            Message = message;
        }
    }
}