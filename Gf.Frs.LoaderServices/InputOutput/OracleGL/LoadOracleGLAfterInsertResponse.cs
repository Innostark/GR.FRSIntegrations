using System.Runtime.Serialization;

namespace Gf.Frs.LoaderServices.InputOutput.OracleGL
{
    [DataContract(Name = "LoadOracleGLAfterInsertResponse", Namespace = "http://www.gulffinance.com.sa/frs/v1/oraclegl/operations/loadafterinsert/response")]
    public class LoadOracleGLAfterInsertResponse
    {
        [DataMember(Order = 0, IsRequired = true, Name = "Code")]
        public long Code;

        [DataMember(Order = 0, IsRequired = true, Name = "Message")]
        public string Message;
    }
}
