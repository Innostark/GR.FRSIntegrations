using Gf.Frs.LoaderWcfServices.InputOutput.Accounts.OracleGL;
using System.ServiceModel;

namespace Gf.Frs.WcfLoaderServices.Accounts.OracleGL
{
    /// <summary>
    /// Service contract for the FRS ORACLE GL WCF Loader service
    /// </summary>
    [ServiceContract(Name = "FrsOracleGLLoaderService", Namespace = "http://www.gulffinance.com.sa/frs/v1/accounts/oraclegl/operations")]
    public interface IAccountsOracleGLLoader
    {
        [OperationContract]
        LoadOracleGLResponse LoadOracleGL(LoadOracleGLRequest request);
    }
}
