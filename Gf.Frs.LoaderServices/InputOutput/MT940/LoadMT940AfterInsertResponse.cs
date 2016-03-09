using System.Runtime.Serialization;

namespace Gf.Frs.LoaderServices.InputOutput.MT940
{
    [DataContract(Name = "LoadMT940AfterInsertResponse", Namespace = "http://www.gulffinance.com.sa/frs/v1/mt940/operations/loadafterinsert/response")]
    public class LoadMT940AfterInsertResponse
    {
        [DataMember(Order = 0, IsRequired = true, Name = "Code")]
        public long Code;

        [DataMember(Order = 0, IsRequired = true, Name = "Message")]
        public string Message;
    }
}
