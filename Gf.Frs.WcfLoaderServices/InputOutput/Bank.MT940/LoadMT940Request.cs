using System.Runtime.Serialization;

namespace Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940
{
    /// <summary>
    /// Input parameter class for the load MT940 data method call.
    /// </summary>
    [DataContract( Name = "LoadMT940Request", Namespace = "http://www.gulffinance.com.sa/frs/v1/bank/mt940/operations/load/request")]
    public class LoadMT940Request
    {
        /// <summary>
        /// Load Id is the primary key of the Load record created by the front end and passed to the service for processing.
        /// </summary>
        [DataMember(Order = 0, IsRequired = true, Name = "LoadId")]
        
        public long LoadId;
        
        /// <summary>
        /// The systems user who started this MT940 load.
        /// </summary>
        [DataMember(Order = 1, IsRequired = true, Name = "UserId")]
        public string UserId;
    }
}
