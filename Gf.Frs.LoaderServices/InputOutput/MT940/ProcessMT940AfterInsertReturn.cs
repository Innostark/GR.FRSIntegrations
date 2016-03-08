using System.Runtime.Serialization;

namespace Gf.Frs.LoaderServices.InputOutput.MT940
{
    [DataContract]
    public class ProcessMT940AfterInsertReturn
    {
        [DataMember(Order = 0, IsRequired = true, Name = "Code")]
        public long Code;

        [DataMember(Order = 0, IsRequired = true, Name = "Message")]
        public string Message;
    }
}
