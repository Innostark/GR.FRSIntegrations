using DevTrends.WCFDataAnnotations;
using Gf.Frs.LoaderWcfServices.InputOutput.Accounts.OracleGL;
using Gf.Frs.OracleGLLoader.Handlers;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.OracleGLLoader.DataModel;
using Gf.Frs.WcfLoaderServices.Loging;
using Gf.Frs.IntegrationCommon.DataModel;
using NLog;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Diagnostics;

namespace Gf.Frs.WcfLoaderServices.Accounts.OracleGL
{
    [ValidateDataAnnotationsBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class AccountsOracleGLLoader : IAccountsOracleGLLoader, IDisposable
    {
        #region ##START## PRIVATE DATA
        private bool _disposed = false;
        private const string _currentLogerName = "AccountsOracleGLLoader.LoadOracleGL";        
        private FrsNLogManager _logManager = new FrsNLogManager();
        private FrsNLogIntegrationServiceStoredProc _logingSPDetails = new FrsNLogIntegrationServiceStoredProc();
        private OracleGLLoadHandler _oracleGLLoadHndlr = new OracleGLLoadHandler();
        private Stopwatch _timer = null;

        #endregion

        #region ##START## SERVICE OPERATIONS

        public LoadOracleGLResponse LoadOracleGL(LoadOracleGLRequest request)
        {
            _timer = Stopwatch.StartNew();

            #region TODO: Authentication to be added later on project sign off
            //ServiceSecurityContext ssc = ServiceSecurityContext.Current;
            //if (!ssc.IsAnonymous && ssc.PrimaryIdentity != null)
            //{
            //    spDetails.userName = ServiceSecurityContext.Current.PrimaryIdentity.Name;
            //} 
            #endregion

            #region **LOG ENTRY**
            _logManager.SetSPDetailsAndLog(LogLevel.Info, string.Format("{2}{0}{2}{0}{1}{0}{2}{0}{2}",
                                                            Environment.NewLine,
                                                            "Entering LoadOracleGL and setting up logger configurations...",
                                                            "#########################################################################################################"));
            #endregion **LOG ENTRY**

            #region ##START## Request Parameters Validation

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Starting validation... Validating input (LoadOracleGLRequest) request.");
            #endregion **LOG ENTRY**

            if (request == null)
            {
                _logManager.RaiseException(string.Format("[a] There was a fault validating the request.{0}Fault details:{0}{1}",
                                            Environment.NewLine,
                                            "The request cannot be empty. Please provide a valid request object for a successful execution. [2]"));
            }

            if (request.LoadId <= 0)
            {
                _logManager.RaiseException(string.Format("[b] There was a fault validating passed Load Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId.ToString(),
                                            "The load id is not valid. Please provide a valid Load Id for a successful execution. [3]"));
            }

            if (string.IsNullOrEmpty(request.UserId))
            {
                _logManager.RaiseException(string.Format("[c] There was a fault validating passed User Id = {1}.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.UserId,
                                            "The user id is not valid. Please provide a valid User Id for a successful execution. [4]"));
            }

            #endregion

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Validating input (LoadOracleGLRequest) request completed successfully.");
            #endregion **LOG ENTRY**
            
            List<LoaderFault> faults = new List<LoaderFault>();

            #region ##START## Validation of request Load Id
            try
            {
                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "Validate the Load object fully, including the associated objects.");
                #endregion **LOG ENTRY**

                //Validate the Load object fully, including the associated objects
                faults = _oracleGLLoadHndlr.ValidateLoadForNullOrEmpty(request.LoadId);
            }
            catch (Exception ex)
            {
                _logManager.RaiseException(string.Format("[d] Oracle GL unexpected error! during validation of Load Id = {1} / [5].{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                _logManager.RaiseException(string.Format("[e] There was a fault validating passed Load Id = {1} / [6].{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId.ToString(),
                                            DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Get load object from database (to be used through out the function for this call), i.e. requested load.");
            #endregion **LOG ENTRY**

            //Get load object from database (to be used through out the code for this call)
            Load dbLoad = _oracleGLLoadHndlr.GetLoad(request.LoadId);

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, string.Format("Load (Load Id = {0}), successfully found in the database.", request.LoadId));
            #endregion **LOG ENTRY**

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Loading reference data for the operation.");
            #endregion **LOG ENTRY**

            #region ##START## Load Reference data from DB            
            //Load required reference data
            List<RefDataLoadStatus> refLoadStatuses = _oracleGLLoadHndlr.GetLoadStatuses();
            List<RefDataStatus> refStatuses = _oracleGLLoadHndlr.GetStatuses();

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Reference data loaded successfully.");
            #endregion **LOG ENTRY**

            #endregion

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Validating the load record against reference data i.e. Read-only & Load Status.");
            #endregion **LOG ENTRY**

            if (dbLoad.ReadOnly || dbLoad.LoadStatusId != refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSSubmitted)).Value)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[f] Error! during Oracle GL load checking of Load Id = {1} / [7].{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            "Load is not valid! It's either set to Read Only or Load Status is not submitted. The Loads Status has been updated to 'Failed'."));
            }

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Reference data validation completed successfully.");
            #endregion **LOG ENTRY**

            //Update Load's Load Status to Parsing in a new context
            _oracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                             refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSParsing)),
                                                             request.UserId,
                                                             true);
            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL Load status set to 'Parsing'.");
            #endregion **LOG ENTRY**

            #region ##START## Oracle GL Content validaton
            try
            {
                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "Validating the Oracle GL file contents.");
                #endregion **LOG ENTRY**

                //Validate the Oracle GL Base64 contents
                faults = _oracleGLLoadHndlr.ValidateOracleGLFileContent(dbLoad.OracleGLLoad.FileContent.FileContentBase64);

                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL file contents validated successfully.");
                #endregion **LOG ENTRY**

                //Update Load's Load Status to Transforming in a new context
                _oracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSTransforming)),
                                                                 request.UserId,
                                                                 true);

                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL Load record status set to 'Transforming'.");
                #endregion **LOG ENTRY**
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[g] Oracle GL unexpected error! during Oracle GL file contents validation of Load Id = {1} / [8].{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[h] There was a fault validating passed Load Id = {1} / [9], associated Oracle GL content.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId.ToString(),
                                            DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL loading started...");
            #endregion **LOG ENTRY**

            #region ##START## Load processing into database
            try
            {
                //Update Load's Load Status to Parsing in a new context
                _oracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSImporting)),
                                                                 request.UserId,
                                                                 true);
                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL Load record status set to 'Importing'.");
                #endregion **LOG ENTRY**

                //Load the Oracle GL file data into objects and then to the database
                _oracleGLLoadHndlr.LoadOracleGL(dbLoad, dbLoad.OracleGLLoad.FileContent.FileContentBase64, request.UserId);

                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL data loaded in Oracle GL entries table.");
                #endregion **LOG ENTRY**
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[i] Oracle GL unexpected error! during the load processing into database of Load Id = {1} / [10].{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL data load complete.");
            #endregion **LOG ENTRY**

            #endregion

            #region ##START## Update Load for the process completion

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL load house keeping started...");
            #endregion **LOG ENTRY**

            try
            {
                DateTime finish = DateTime.UtcNow;
                //Oracle GL Load record fields to be updated
                dbLoad.OracleGLLoad.OracleGLEntryCount = _oracleGLLoadHndlr.GetProcessedCustomerStatementCount();
                dbLoad.OracleGLLoad.ModifiedBy = request.UserId;

                //Load records field to be updated                                
                dbLoad.InProgress = false;
                dbLoad.Finish = finish;
                dbLoad.ModifiedBy = request.UserId;
                dbLoad.LoadStatusId = refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSCompleted)).Value;
                dbLoad.ReadOnly = false;

                //Perform the database update
                _oracleGLLoadHndlr.SummariseOracleGLLoadOnCompletion(dbLoad, dbLoad.OracleGLLoad);
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[j] Oracle GL unexpected error! after load completion of Load Id = {1} / [11], while updating the load record for progress.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL load house keeping completed successfully.");
            #endregion **LOG ENTRY**

            #endregion

            _timer.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = _timer.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL load house keeping completed successfully.");
            #endregion **LOG ENTRY**

            LoadOracleGLResponse response = new LoadOracleGLResponse(LoadOracleGLResponse.SUCCESS_CODE, LoadOracleGLResponse.SUCCESS_MESSAGE);

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, string.Format("Oracle GL load for Load Id = {0} completed successfully. Time taken - {1}", request.LoadId, elapsedTime), response.ToString());
            #endregion **LOG ENTRY**

            return response;
        }

        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    _logingSPDetails.Dispose();
                    _logManager.Dispose();
                    _oracleGLLoadHndlr.Dispose();
                    _timer = null;
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.


                // Note disposing has been done.
                _disposed = true;

            }
        }

        #endregion


        #region ##START## Private functions

        /// <summary>
        /// Method to log a failure within the operation. Changes the loads status to 'Failed' and adds a failure log entry too.
        /// </summary>
        /// <param name="request">Request object for the parent operation.</param>
        /// <param name="refLoadStatuses">Collection of Load Status reference list.</param>
        private void LoadFailed(LoadOracleGLRequest request, List<RefDataLoadStatus> refLoadStatuses)
        {
            //Update Load's Load Status to Failed in a new context
            _oracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                        refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSFailed)),
                                                        request.UserId,
                                                        true);
            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Oracle GL Load record status set to 'Failed'.");
            #endregion **LOG ENTRY**
        } 

        #endregion

    }
}
