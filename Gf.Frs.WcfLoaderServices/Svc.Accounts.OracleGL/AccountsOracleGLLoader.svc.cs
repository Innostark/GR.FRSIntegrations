using DevTrends.WCFDataAnnotations;

using Gf.Frs.LoaderWcfServices.InputOutput.Accounts.OracleGL;
using Gf.Frs.OracleGLLoader.Handlers;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.OracleGLLoader.DataModel;

using System;
using System.Collections.Generic;
using System.ServiceModel;
using NLog;
using System.Net;
using System.ServiceModel.Channels;
using Gf.Frs.WcfLoaderServices.Loging;
using System.Diagnostics;
using Gf.Frs.IntegrationCommon.DataModel;

namespace Gf.Frs.WcfLoaderServices.Accounts.OracleGL
{
    [ValidateDataAnnotationsBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class AccountsOracleGLLoader : IAccountsOracleGLLoader
    {
        const string CurrentLogerName = "AccountsOracleGLLoader.LoadOracleGL";
        FrsNLogManager LogManager = new FrsNLogManager();
        FrsNLogIntegrationServiceStoredProc LogingSPDetails = new FrsNLogIntegrationServiceStoredProc();
        OracleGLLoadHandler OracleGLLoadHndlr = new OracleGLLoadHandler();
        Stopwatch Timer = null;

        public LoadOracleGLResponse LoadOracleGL(LoadOracleGLRequest request)
        {
            Timer = Stopwatch.StartNew();

            #region TODO: Authentication to be added later on project sign off
            //ServiceSecurityContext ssc = ServiceSecurityContext.Current;
            //if (!ssc.IsAnonymous && ssc.PrimaryIdentity != null)
            //{
            //    spDetails.userName = ServiceSecurityContext.Current.PrimaryIdentity.Name;
            //} 
            #endregion

            #region **LOG ENTRY**
            LogManager.SetSPDetailsAndLog(LogLevel.Info, string.Format("{2}{0}{2}{0}{1}{0}{2}{0}{2}",
                                                            Environment.NewLine,
                                                            "Entering LoadOracleGL and setting up logger configurations...",
                                                            "#########################################################################################################"));
            #endregion **LOG ENTRY**

            #region ##START## Request Parameters Validation

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Starting validation... Validating input (LoadOracleGLRequest) request.");
            #endregion **LOG ENTRY**

            if (request == null)
            {
                LogManager.RaiseException(string.Format("There was a fault validating the request.{0}Fault details:{0}{1}",
                                            Environment.NewLine,
                                            "The request cannot be empty. Please provide a valid request object for a successful execution."));
            }

            if (request.LoadId <= 0)
            {
                LogManager.RaiseException(string.Format("There was a fault validating passed Load Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId.ToString(),
                                            "The load id is not valid. Please provide a valid Load Id for a successful execution."));
            }

            if (string.IsNullOrEmpty(request.UserId))
            {
                LogManager.RaiseException(string.Format("There was a fault validating passed User Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.UserId,
                                            "The user id is not valid. Please provide a valid User Id for a successful execution."));
            }

            #endregion

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Validating input (LoadOracleGLRequest) request completed successfully.");
            #endregion **LOG ENTRY**

            
            List<LoaderFault> faults = new List<LoaderFault>();

            #region ##START## Validation of request Load Id
            try
            {
                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "Validate the Load object fully, including the associated objects.");
                #endregion **LOG ENTRY**

                //Validate the Load object fully, including the associated objects
                faults = OracleGLLoadHndlr.ValidateLoadForNullOrEmpty(request.LoadId);
            }
            catch (Exception ex)
            {
                LogManager.RaiseException(string.Format("Unexpected error! during validation of Load Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                LogManager.RaiseException(string.Format("There was a fault validating passed Load Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId.ToString(),
                                            DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Get load object from database (to be used through out the function for this call), i.e. requested load.");
            #endregion **LOG ENTRY**

            //Get load object from database (to be used through out the code for this call)
            Load dbLoad = OracleGLLoadHndlr.GetLoad(request.LoadId);

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, string.Format("Load (Load Id = {0}), successfully found in the database.", request.LoadId));
            #endregion **LOG ENTRY**

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Loading reference data for the operation.");
            #endregion **LOG ENTRY**

            #region ##START## Load Reference data from DB            
            //Load required reference data
            List<RefDataLoadStatus> refLoadStatuses = OracleGLLoadHndlr.GetLoadStatuses();
            List<RefDataStatus> refStatuses = OracleGLLoadHndlr.GetStatuses();

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Reference data loaded successfully.");
            #endregion **LOG ENTRY**

            #endregion

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Validating the load record against reference data i.e. Read-only & Load Status.");
            #endregion **LOG ENTRY**

            if (dbLoad.ReadOnly || dbLoad.LoadStatusId != refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSSubmitted)).Value)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                LogManager.RaiseException(string.Format("Error! during Oracle GL load checking of Load Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            "Load is not valid! It's either set to Read Only or Load Status is not submitted. The Loads Status has been updated to 'Failed'."));
            }

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Reference data validation completed successfully.");
            #endregion **LOG ENTRY**

            //Update Load's Load Status to Parsing in a new context
            OracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                             refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSParsing)),
                                                             request.UserId,
                                                             true);
            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL Load status set to 'Parsing'.");
            #endregion **LOG ENTRY**

            #region ##START## Oracle GL Content validaton
            try
            {
                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "Validating the Oracle GL file contents.");
                #endregion **LOG ENTRY**

                //Validate the Oracle GL Base64 contents
                faults = OracleGLLoadHndlr.ValidateOracleGLFileContent(dbLoad.OracleGLLoad.FileContent.FileContentBase64);

                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL file contents validated successfully.");
                #endregion **LOG ENTRY**

                //Update Load's Load Status to Transforming in a new context
                OracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSTransforming)),
                                                                 request.UserId,
                                                                 true);

                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL Load record status set to 'Transforming'.");
                #endregion **LOG ENTRY**
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                LogManager.RaiseException(string.Format("Unexpected error! during Oracle GL file contents validation of Load Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                LogManager.RaiseException(string.Format("There was a fault validating passed Load Id = {1}, associated Oracle GL content.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId.ToString(),
                                            DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL loading started...");
            #endregion **LOG ENTRY**

            #region ##START## Load processing into database
            try
            {
                //Update Load's Load Status to Parsing in a new context
                OracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSImporting)),
                                                                 request.UserId,
                                                                 true);
                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL Load record status set to 'Importing'.");
                #endregion **LOG ENTRY**

                //Load the Oracle GL file data into objects and then to the database
                OracleGLLoadHndlr.LoadOracleGL(dbLoad, dbLoad.OracleGLLoad.FileContent.FileContentBase64, request.UserId);

                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL data loaded in Oracle GL entries table.");
                #endregion **LOG ENTRY**
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                LogManager.RaiseException(string.Format("Unexpected error! during the load processing into database of Load Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL data load complete.");
            #endregion **LOG ENTRY**

            #endregion

            #region ##START## Update Load for the process completion

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL load house keeping started...");
            #endregion **LOG ENTRY**

            try
            {
                DateTime finish = DateTime.UtcNow;
                //Oracle GL Load record fields to be updated
                dbLoad.OracleGLLoad.OracleGLEntryCount = OracleGLLoadHndlr.GetProcessedCustomerStatementCount();
                dbLoad.OracleGLLoad.ModifiedBy = request.UserId;

                //Load records field to be updated                                
                dbLoad.InProgress = false;
                dbLoad.Finish = finish;
                dbLoad.ModifiedBy = request.UserId;
                dbLoad.LoadStatusId = refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSCompleted)).Value;
                dbLoad.ReadOnly = false;

                //Perform the database update
                OracleGLLoadHndlr.SummariseOracleGLLoadOnCompletion(dbLoad, dbLoad.OracleGLLoad);
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                LogManager.RaiseException(string.Format("Unexpected error! after load completion of Load Id = {1}, while updating the load record for progress.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL load house keeping completed successfully.");
            #endregion **LOG ENTRY**

            #endregion

            Timer.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = Timer.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL load house keeping completed successfully.");
            #endregion **LOG ENTRY**

            LoadOracleGLResponse response = new LoadOracleGLResponse(LoadOracleGLResponse.SUCCESS_CODE, LoadOracleGLResponse.SUCCESS_MESSAGE);

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, string.Format("Oracle GL load for Load Id = {0} completed successfully. Time taken - {1}", request.LoadId, elapsedTime), response.ToString());
            #endregion **LOG ENTRY**

            return response;
        }

        private void LoadFailed(LoadOracleGLRequest request, List<RefDataLoadStatus> refLoadStatuses)
        {
            //Update Load's Load Status to Failed in a new context
            OracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                        refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSFailed)),
                                                        request.UserId,
                                                        true);
            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Oracle GL Load record status set to 'Failed'.");
            #endregion **LOG ENTRY**
        }

    }
}
