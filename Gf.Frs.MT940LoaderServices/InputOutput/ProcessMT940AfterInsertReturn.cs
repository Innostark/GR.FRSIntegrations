using System.Runtime.Serialization;

namespace Gf.Frs.MT940Loader.Services.InputOutput
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
