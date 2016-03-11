using System.ServiceModel;
using Gf.Frs.LoaderServices.InputOutput.MT940;
using System;

namespace Gf.Frs.LoaderServices.Wcf.MT940
{
    /// <summary>
    /// Service contract for the FRS MT940 WCF Loader service
    /// </summary>
    [ServiceContract(Name = "FrsMT940LoaderService", Namespace = "http://www.gulffinance.com.sa/frs/v1/mt940/operations")]
    public interface IFrsMT940WcfLoaderService
    {
        [OperationContract]
        LoadMT940AfterInsertResponse LoadMT940AfterInsert(LoadMT940AfterInsertRequest request);

        //[OperationContract(IsOneWay = false, AsyncPattern = true)]
        //IAsyncResult BeginLoadMT940AfterInsert(LoadMT940AfterInsertRequest request, AsyncCallback callback, object state);

        //IAsyncResult EndLoadMT940AfterInsert(IAsyncResult asyncResult);
    }
}
