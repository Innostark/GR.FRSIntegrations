using Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940;
using System.ServiceModel;

namespace Gf.Frs.WcfLoaderServices.Bank.MT940
{
    /// <summary>
    /// Service contract for the FRS MT940 WCF Loader service
    /// </summary>
    [ServiceContract(Name = "BankMT940Loader", Namespace = "http://www.gulffinance.com.sa/frs/v1/bank/mt940/operations")]
    public interface IBankMT940Loader
    {
        [OperationContract]
        LoadMT940Response LoadMT940(LoadMT940Request request);
    }
}
