using System.Runtime.Serialization;

namespace Gf.Frs.LoaderServices.InputOutput
{
    /// <summary>
    /// Input parameter class for the load MT940 data method call.
    /// </summary>
    [DataContract( Name = "LoadMT940AfterInsertInput", Namespace = "http://www.gulffinance.com.sa/frs/v1/mt940/operations/loadafterinsert/request")]
    public class LoadMT940AfterInsertRequest
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
