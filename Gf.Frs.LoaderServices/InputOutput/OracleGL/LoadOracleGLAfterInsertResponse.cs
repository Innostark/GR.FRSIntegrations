﻿using System.Runtime.Serialization;

namespace Gf.Frs.LoaderServices.InputOutput.OracleGL
{
    [DataContract(Name = "LoadOracleGLAfterInsertResponse", Namespace = "http://www.gulffinance.com.sa/frs/v1/oraclegl/operations/loadafterinsert/response")]
    public class LoadOracleGLAfterInsertResponse
    {
        [DataMember(Order = 0, IsRequired = true, Name = "Code")]
        public long Code;

        [DataMember(Order = 0, IsRequired = true, Name = "Message")]
        public string Message;

        public const long SUCCESS_CODE = 0;
        public const string SUCCESS_MESSAGE = "Request completed successfully! HURRAY!!!";

        public LoadOracleGLAfterInsertResponse(long code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
