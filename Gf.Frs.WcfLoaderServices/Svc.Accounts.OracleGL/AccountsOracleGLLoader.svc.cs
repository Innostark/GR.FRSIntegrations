using DevTrends.WCFDataAnnotations;

using Gf.Frs.LoaderWcfServices.InputOutput.Accounts.OracleGL;
using Gf.Frs.OracleGLLoader.Handlers;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.OracleGLLoader.DataModel;

using System;
using System.Collections.Generic;
using System.ServiceModel;
using Gf.Frs.LoaderServices.Logging;
using NLog;

namespace Gf.Frs.WcfLoaderServices.Accounts.OracleGL
{
    [ValidateDataAnnotationsBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class AccountsOracleGLLoader : IAccountsOracleGLLoader
    {
        public LoadOracleGLResponse LoadOracleGL(LoadOracleGLRequest request)
        {
            FrsNLogManager.Instance.Info("123");
            FrsNLogManager.Instance.Trace("123456");

            int k = 42;
            int l = 100;

            FrsNLogManager.Instance.Trace("Sample trace message, k={0}, l={1}", k, l);
            FrsNLogManager.Instance.Debug("Sample debug message, k={0}, l={1}", k, l);
            FrsNLogManager.Instance.Info("Sample informational message, k={0}, l={1}", k, l);
            FrsNLogManager.Instance.Warn("Sample warning message, k={0}, l={1}", k, l);
            FrsNLogManager.Instance.Error("Sample error message, k={0}, l={1}", k, l);
            FrsNLogManager.Instance.Fatal("Sample fatal error message, k={0}, l={1}", k, l);
            FrsNLogManager.Instance.Log(LogLevel.Info, "Sample informational message, k={0}, l={1}", k, l);


            #region ##START## Request Parameters Validation

            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new FaultException(string.Format("There was a fault validating passed User Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId.ToString(),
                                                        "The user id is not valid. Please provide a valid User Id for a successful execution."));
            }

            #endregion

            OracleGLLoadHandler oracleGlLoadHandler = new OracleGLLoadHandler();
            List<LoaderFault> faults = new List<LoaderFault>();

            #region ##START## Validation of request Load Id
            try
            {
                //Validate the Load object fully, including the associated objects
                faults = oracleGlLoadHandler.ValidateLoadForNullOrEmpty(request.LoadId);
            }
            catch (Exception ex)
            {
                throw new FaultException(string.Format("Unexpected error! during validation of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                throw new FaultException(string.Format("There was a fault validating passed Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId.ToString(),
                                                        DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            //Get load object from database (to be used through out the code for this call)
            Load dbLoad = oracleGlLoadHandler.GetLoad(request.LoadId);

            #region ##START## Load Reference data from DB            
            //Load required reference data
            List<RefDataLoadStatus> refLoadStatuses = oracleGlLoadHandler.GetLoadStatuses();
            List<RefDataStatus> refStatuses = oracleGlLoadHandler.GetStatuses();
            #endregion

            if(dbLoad.ReadOnly || dbLoad.LoadStatusId != refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCSubmitted)).Value)
            {
                //Update Load's Load Status to Failed in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCFailed)),
                                                                 request.UserId,
                                                                 true);

                throw new FaultException(string.Format("Error! during Oracle GL load checking of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        "Load is not valid! It's either set to Read Only or Load Status in submitted."));
            }

            //Update Load's Load Status to Parsing in a new context
            oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId, 
                                                             refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCParsing)),
                                                             request.UserId,
                                                             true);

            #region ##START## Oracle GL Content validaton
            try
            {
                //Validate the Oracle GL Base64 contents
                faults = oracleGlLoadHandler.ValidateOracleGLFileContent(dbLoad.OracleGLLoad.FileContent.FileContentBase64);
                //Update Load's Load Status to Transforming in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCTransforming)),
                                                                 request.UserId,
                                                                 true);
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCFailed)),
                                                                 request.UserId,
                                                                 true);

                throw new FaultException(string.Format("Unexpected error! during Oracle GL file contents validation of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                throw new FaultException(string.Format("There was a fault validating passed Load Id = {1}, associated Oracle GL content.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId.ToString(),
                                                        DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion
            

            #region ##START## Load processing into database
            try
            {
                //Update Load's Load Status to Parsing in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCImporting)),
                                                                 request.UserId,
                                                                 true);
                //Load the Oracle GL file data into objects and then to the database
                oracleGlLoadHandler.LoadOracleGL(dbLoad, dbLoad.OracleGLLoad.FileContent.FileContentBase64, request.UserId);
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCFailed)),
                                                                 request.UserId,
                                                                 true);

                throw new FaultException(string.Format("Unexpected error! during the load processing into database of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }
            #endregion

            #region ##START## Update Load for the process completion

            try
            {
                DateTime finish = DateTime.UtcNow;
                //Oracle GL Load record fields to be updated
                dbLoad.OracleGLLoad.OracleGLEntryCount = oracleGlLoadHandler.GetProcessedCustomerStatementCount();
                dbLoad.OracleGLLoad.ModifiedBy = request.UserId;

                //Load records field to be updated                                
                dbLoad.InProgress = false;
                dbLoad.Finish = finish;
                dbLoad.ModifiedBy = request.UserId;
                dbLoad.LoadStatusId = refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCCompleted)).Value;
                dbLoad.ReadOnly = false;

                //Perform the database update
                oracleGlLoadHandler.SummariseOracleGLLoadOnCompletion(dbLoad, dbLoad.OracleGLLoad);
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LCFailed)),
                                                                 request.UserId,
                                                                 true);

                throw new FaultException(string.Format("Unexpected error! after load completion of Load Id = {1}, while updating the load record for progress.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }
            #endregion

            return new LoadOracleGLResponse(LoadOracleGLResponse.SUCCESS_CODE, LoadOracleGLResponse.SUCCESS_MESSAGE);
        }
    }
}
