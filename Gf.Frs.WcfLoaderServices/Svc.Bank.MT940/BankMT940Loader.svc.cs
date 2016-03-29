using DevTrends.WCFDataAnnotations;
using Gf.Frs.IntegrationCommon.DataModel;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940;
using Gf.Frs.MT940Loader;
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
    public class BankMT940Loader : IBankMT940Loader
    {
        const string CurrentLogerName = "BankMT940Loader.LoadMT940";
        FrsNLogManager LogManager = new FrsNLogManager();
        FrsNLogIntegrationServiceStoredProc LogingSPDetails = new FrsNLogIntegrationServiceStoredProc();                
        MT940LoadHandler MT940LoadHndlr = new MT940LoadHandler();
        Stopwatch Timer = null;

        public LoadMT940Response LoadMT940(LoadMT940Request request)
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
                                          "Entering LoadMT940 and setting up logger configurations...",
                                          "#########################################################################################################"));
            #endregion **LOG ENTRY**

            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new FaultException(string.Format("There was a fault validating passed User Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId.ToString(),
                                                        "The user id is not valid. Please provide a valid User Id for a successful execution."));
            }

            #region ##START## Request Parameters Validation

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Starting validation... Validating input (LoadMT940Request) request.");
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
            LogManager.LogFromSPDetails(LogLevel.Info, "Validating input (LoadMT940Request) request completed successfully.");
            #endregion **LOG ENTRY**


            List<LoaderFault> faults = new List<LoaderFault>();

            #region ##START## Validation of request Load Id
            try
            {
                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "Validate the Load object fully, including the associated objects.");
                #endregion **LOG ENTRY**

                //Validate the Load object fully, including the associated objects
                faults = MT940LoadHndlr.ValidateLoadForNullOrEmpty(request.LoadId);
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
            Load dbLoad = MT940LoadHndlr.GetLoad(request.LoadId);

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, string.Format("Load (Load Id = {0}), successfully found in the database.", request.LoadId));
            #endregion **LOG ENTRY**

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Loading reference data for the operation.");
            #endregion **LOG ENTRY**

            #region ##START## Load Reference data from DB            
            //Load required reference data
            List<RefDataLoadStatus> refLoadStatuses = MT940LoadHndlr.GetLoadStatuses();
            List<RefDataStatus> refStatuses = MT940LoadHndlr.GetStatuses();

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

                LogManager.RaiseException(string.Format("Error! during MT940 load checking of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        "Load is not valid! It's either set to Read Only or Load Status is not submitted. The Loads Status has been updated to 'Failed'."));
            }

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Reference data validation completed successfully.");
            #endregion **LOG ENTRY**

            //Update Load's Load Status to Parsing in a new context
            MT940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                        refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSParsing)),
                                                        request.UserId,
                                                        true);
            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 Load status set to 'Parsing'.");
            #endregion **LOG ENTRY**

            #region  ##START## MT940 Header and Trailer setup
            
            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 header and trailer setup starting...");
            #endregion **LOG ENTRY**

            //Very important to set the header and trailer as these are going to be used later throughout this processing
            MT940LoadHndlr.SetHeaderTrailer(dbLoad.LoadMetaData.Header, dbLoad.LoadMetaData.Trailer);

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 header and trailer completed successfuly.");
            #endregion **LOG ENTRY**

            #endregion

            //Update Load's Load Status to Parsing in a new context
            MT940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                             refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSParsing)),
                                                             request.UserId,
                                                             true);
            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 Load status set to 'Parsing'.");
            #endregion **LOG ENTRY**

            #region ##START## MT940 Content validaton
            try
            {
                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "Validating the MT940 file contents.");
                #endregion **LOG ENTRY**

                //Validate the MT940 Base64 contents
                faults = MT940LoadHndlr.ValidateMT940FileContent(dbLoad.MT940Load.FileContent.FileContentBase64);

                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "MT940 file contents validated successfully.");
                #endregion **LOG ENTRY**

                //Update Load's Load Status to Transforming in a new context
                MT940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSTransforming)),
                                                                 request.UserId,
                                                                 true);

                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "MT940 Load record status set to 'Transforming'.");
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
                // Update Load's Load Status to Failed in a new context
                LoadFailed(request, refLoadStatuses);

                LogManager.RaiseException(string.Format("There was a fault validating passed Load Id = {1}, associated MT940 content.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId.ToString(),
                                                        DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 loading started...");
            #endregion **LOG ENTRY**

            #region ##START## Load processing into database
            try
            {
                //Update Load's Load Status to Parsing in a new context
                MT940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                            refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSImporting)),
                                                            request.UserId,
                                                            true);
                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "MT940 Load record status set to 'Importing'.");
                #endregion **LOG ENTRY**

                //Load the MT940 file data into objects and then to the database
                MT940LoadHndlr.LoadMT940(dbLoad, dbLoad.MT940Load.FileContent.FileContentBase64, request.UserId);

                #region **LOG ENTRY**
                LogManager.LogFromSPDetails(LogLevel.Info, "MT940 data loaded in Oracle GL entries table.");
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
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 data load complete.");
            #endregion **LOG ENTRY**

            #endregion

            #region ##START## Update Load for the process completion

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 load house keeping started...");
            #endregion **LOG ENTRY**

            try
            {
                DateTime finish = DateTime.UtcNow;
                //MT940 Load record fields to be updated
                dbLoad.MT940Load.CustomerStatementCount = MT940LoadHndlr.GetProcessedCustomerStatementCount();
                dbLoad.MT940Load.ModifiedBy = request.UserId;

                //Load records field to be updated                                
                dbLoad.InProgress = false;
                dbLoad.Finish = finish;
                dbLoad.ModifiedBy = request.UserId;
                dbLoad.LoadStatusId = refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSCompleted)).Value;
                dbLoad.ReadOnly = false;

                //Perform the database update
                MT940LoadHndlr.SummariseMT940LoadOnCompletion(dbLoad, dbLoad.MT940Load);
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
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 load house keeping completed successfully.");
            #endregion **LOG ENTRY**

            #endregion

            Timer.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = Timer.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 load house keeping completed successfully.");
            #endregion **LOG ENTRY**

            LoadMT940Response response = new LoadMT940Response(LoadMT940Response.SUCCESS_CODE, LoadMT940Response.SUCCESS_MESSAGE);

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, string.Format("MT940 load for Load Id = {0} completed successfully. Time taken - {1}", request.LoadId, elapsedTime), response.ToString());
            #endregion **LOG ENTRY**

            return response;
        }

        private void LoadFailed(LoadMT940Request request, List<RefDataLoadStatus> refLoadStatuses)
        {
            //Update Load's Load Status to Failed in a new context
            MT940LoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                        refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSFailed)),
                                                        request.UserId,
                                                        true);
            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "MT940 Load record status set to 'Failed'.");
            #endregion **LOG ENTRY**
        }

    }
}
