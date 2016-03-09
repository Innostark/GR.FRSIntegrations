using Gf.Frs.LoaderServices.InputOutput.OracleGL;
using System.ServiceModel;

namespace Gf.Frs.LoaderServices.Wcf.OracleGL
{
    /// <summary>
    /// Service contract for the FRS ORACLE GL WCF Loader service
    /// </summary>
    [ServiceContract(Name = "FrsOracleGLLoaderService", Namespace = "http://www.gulffinance.com.sa/frs/v1/oraclegl/operations")]
    public interface IFrsOracleGLWcfLoaderService
    {
        [OperationContract]
        LoadOracleGLAfterInsertResponse LoadOracleGLAfterInsert(LoadOracleGLAfterInsertRequest request);
    }
}
