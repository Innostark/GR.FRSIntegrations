using Gf.Frs.LoaderWcfServices.InputOutput.Accounts.OracleGL;
using System;
using System.ServiceModel;

namespace Gf.Frs.LoaderWcfServices.Accounts.OracleGL
{
    /// <summary>
    /// Service contract for the FRS ORACLE GL WCF Loader service
    /// </summary>
    [ServiceContract(Name = "FrsOracleGLLoaderService", Namespace = "http://www.gulffinance.com.sa/frs/v1/accounts/oraclegl/operations")]
    public interface IAccountsOracleGLLoader
    {
        [OperationContract(IsOneWay = false, AsyncPattern = true)]
        IAsyncResult BeginOracleGLLoad(LoadOracleGLRequest request, AsyncCallback callback, object state);

        IAsyncResult EndOracleGLLoad(IAsyncResult asyncResult);
    }
}
