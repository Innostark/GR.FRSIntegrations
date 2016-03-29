using DevTrends.WCFDataAnnotations;
using Gf.Frs.IntegrationCommon.DataModel;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940;
using Gf.Frs.MT940Loader.DataModel;
using Gf.Frs.MT940Loader.Handlers;
using Gf.Frs.WcfLoaderServices.Loging;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace Gf.Frs.WcfLoaderServices.Bank.MT940
{
    [ValidateDataAnnotationsBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class BankMT940Loader : IBankMT940Loader, IDisposable
    {
        #region #region ##START## PRIVATE DATA

        const string _currentLogerName = "BankMT940Loader.LoadMT940";
        FrsNLogManager _logManager = new FrsNLogManager();
        FrsNLogIntegrationServiceStoredProc _logingSPDetails = new FrsNLogIntegrationServiceStoredProc();
        MT940LoadHandler _mt940LoadHndlr = new MT940LoadHandler();
        Stopwatch _timer = null;
        bool _disposed = false;

        #endregion

        #region #region ##START## SERVICE OPERATIONS
        public LoadMT940Response LoadMT940(LoadMT940Request request)
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
                                          "Entering LoadMT940 and setting up logger configurations...",
                                          "#########################################################################################################"));
            #endregion **LOG ENTRY**

            #region ##START## Request Parameters Validation

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Starting validation... Validating input (LoadMT940Request) request.");
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
            _logManager.LogFromSPDetails(LogLevel.Info, "Validating input (LoadMT940Request) request completed successfully.");
            #endregion **LOG ENTRY**

            List<LoaderFault> faults = new List<LoaderFault>();

            #region ##START## Validation of request Load Id
            try
            {
                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "Validate the Load object fully, including the associated objects.");
                #endregion **LOG ENTRY**

                //Validate the Load object fully, including the associated objects
                faults = _mt940LoadHndlr.ValidateLoadForNullOrEmpty(request.LoadId);
            }
            catch (Exception ex)
            {
                _logManager.RaiseException(string.Format("[d] MT940 unexpected error! during validation of Load Id = {1} / [5].{0}Fault details:{0}{2}",
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
            Load dbLoad = _mt940LoadHndlr.GetLoad(request.LoadId);

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, string.Format("Load (Load Id = {0}), successfully found in the database.", request.LoadId));
            #endregion **LOG ENTRY**

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Loading reference data for the operation.");
            #endregion **LOG ENTRY**

            #region ##START## Load Reference data from DB            
            //Load required reference data
            List<RefDataLoadStatus> refLoadStatuses = _mt940LoadHndlr.GetLoadStatuses();
            List<RefDataStatus> refStatuses = _mt940LoadHndlr.GetStatuses();

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

                _logManager.RaiseException(string.Format("[f] Error! during MT940 load checking of Load Id = {1} / [7].{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        "Load is not valid! It's either set to Read Only or Load Status is not submitted. The Loads Status has been updated to 'Failed'."));
            }

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "Reference data validation completed successfully.");
            #endregion **LOG ENTRY**

            //Update Load's Load Status to Parsing in a new context
            _mt940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                        refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSParsing)),
                                                        request.UserId,
                                                        true);
            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 Load status set to 'Parsing'.");
            #endregion **LOG ENTRY**

            #region  ##START## MT940 Header and Trailer setup

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 header and trailer setup starting...");
            #endregion **LOG ENTRY**

            //Very important to set the header and trailer as these are going to be used later throughout this processing
            _mt940LoadHndlr.SetHeaderTrailer(dbLoad.LoadMetaData.Header, dbLoad.LoadMetaData.Trailer);

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 header and trailer completed successfuly.");
            #endregion **LOG ENTRY**

            #endregion

            //Update Load's Load Status to Parsing in a new context
            _mt940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                             refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSParsing)),
                                                             request.UserId,
                                                             true);
            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 Load status set to 'Parsing'.");
            #endregion **LOG ENTRY**

            #region ##START## MT940 Content validaton
            try
            {
                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "Validating the MT940 file contents.");
                #endregion **LOG ENTRY**

                //Validate the MT940 Base64 contents
                faults = _mt940LoadHndlr.ValidateMT940FileContent(dbLoad.MT940Load.FileContent.FileContentBase64);

                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "MT940 file contents validated successfully.");
                #endregion **LOG ENTRY**

                //Update Load's Load Status to Transforming in a new context
                _mt940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSTransforming)),
                                                                 request.UserId,
                                                                 true);

                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "MT940 Load record status set to 'Transforming'.");
                #endregion **LOG ENTRY**
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[g] MT940 unexpected error! during MT940 file contents validation of Load Id = {1} / [8].{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                // Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[h] There was a fault validating passed Load Id = {1} / [9], associated MT940 content.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId.ToString(),
                                                        DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 loading started...");
            #endregion **LOG ENTRY**

            #region ##START## Load processing into database
            try
            {
                //Update Load's Load Status to Parsing in a new context
                _mt940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                            refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSImporting)),
                                                            request.UserId,
                                                            true);
                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "MT940 Load record status set to 'Importing'.");
                #endregion **LOG ENTRY**

                //Load the MT940 file data into objects and then to the database
                _mt940LoadHndlr.LoadMT940(dbLoad, dbLoad.MT940Load.FileContent.FileContentBase64, request.UserId);

                #region **LOG ENTRY**
                _logManager.LogFromSPDetails(LogLevel.Info, "MT940 data loaded in MT940 entries table.");
                #endregion **LOG ENTRY**
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[i] MT940 unexpected error! during the load processing into database of Load Id = {1} / [10].{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 data load complete.");
            #endregion **LOG ENTRY**

            #endregion

            #region ##START## Update Load for the process completion

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 load house keeping started...");
            #endregion **LOG ENTRY**

            try
            {
                DateTime finish = DateTime.UtcNow;
                //MT940 Load record fields to be updated
                dbLoad.MT940Load.CustomerStatementCount = _mt940LoadHndlr.GetProcessedCustomerStatementCount();
                dbLoad.MT940Load.ModifiedBy = request.UserId;

                //Load records field to be updated                                
                dbLoad.InProgress = false;
                dbLoad.Finish = finish;
                dbLoad.ModifiedBy = request.UserId;
                dbLoad.LoadStatusId = refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSCompleted)).Value;
                dbLoad.ReadOnly = false;

                //Perform the database update
                _mt940LoadHndlr.SummariseMT940LoadOnCompletion(dbLoad, dbLoad.MT940Load);
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                _logManager.RaiseException(string.Format("[j] MT940 unexpected error! after load completion of Load Id = {1} / [11], while updating the load record for progress.{0}Fault details:{0}{2}",
                                            Environment.NewLine,
                                            request.LoadId,
                                            ExceptionExtensions.ToDetailedString(ex)));
            }

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 load house keeping completed successfully.");
            #endregion **LOG ENTRY**

            #endregion

            _timer.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = _timer.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 load house keeping completed successfully.");
            #endregion **LOG ENTRY**

            LoadMT940Response response = new LoadMT940Response(LoadMT940Response.SUCCESS_CODE, LoadMT940Response.SUCCESS_MESSAGE);

            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, string.Format("MT940 load for Load Id = {0} completed successfully. Time taken - {1}", request.LoadId, elapsedTime), response.ToString());
            #endregion **LOG ENTRY**

            return response;
        }

        #endregion

        #region #region ##START## Private functions

        private void LoadFailed(LoadMT940Request request, List<RefDataLoadStatus> refLoadStatuses)
        {
            //Update Load's Load Status to Failed in a new context
            _mt940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                        refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSFailed)),
                                                        request.UserId,
                                                        true);
            #region **LOG ENTRY**
            _logManager.LogFromSPDetails(LogLevel.Info, "MT940 Load record status set to 'Failed'.");
            #endregion **LOG ENTRY**
        }

        #endregion

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
                    _logManager.Dispose();
                    _logingSPDetails.Dispose();
                    _mt940LoadHndlr.Dispose();
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

    }
}
