using Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940;
using System;
using System.ServiceModel;

namespace Gf.Frs.LoaderWcfServices.Bank.MT940
{
    /// <summary>
    /// Service contract for the FRS MT940 WCF Loader service
    /// </summary>
    [ServiceContract(Name = "BankMT940Loader", Namespace = "http://www.gulffinance.com.sa/frs/v1/bank/mt940/operations")]
    public interface IBankMT940Loader
    {
        [OperationContract(IsOneWay = false, AsyncPattern = true)]
        IAsyncResult BeginMT940Load(LoadMT940Request request, AsyncCallback callback, object state);

        IAsyncResult EndMT940Load(IAsyncResult asyncResult);
    }
}
