using System.ServiceModel;
using Gf.Frs.MT940Loader.Services.InputOutput;

namespace Gf.Frs.MT940LoaderService.Wcf
{
    /// <summary>
    /// Service contract for the FRS MT940 WCF Loader service
    /// </summary>
    [ServiceContract(Name = "IFRSMT940LoaderWcfService", Namespace = "http://www.gulffinance.com.sa/frs/v1/mt940/operations")]
    public interface IFRSMT940WcfLoaderService
    {
        [OperationContract]
        ProcessMT940AfterInsertReturn LoadMT940AfterInsert(LoadMT940AfterInsertRequest input);
    }
}
