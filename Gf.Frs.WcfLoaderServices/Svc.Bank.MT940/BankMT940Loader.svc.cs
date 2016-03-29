using DevTrends.WCFDataAnnotations;
using Gf.Frs.IntegrationCommon.DataModel;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.LoaderWcfServices.InputOutput.Bank.MT940;
using Gf.Frs.MT940Loader;
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
        MT940LoadHandler Mt940LoadHandler = new MT940LoadHandler();
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
                faults = Mt940LoadHandler.ValidateLoadForNullOrEmpty(request.LoadId);
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
            Load dbLoad = Mt940LoadHandler.GetLoad(request.LoadId);

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, string.Format("Load (Load Id = {0}), successfully found in the database.", request.LoadId));
            #endregion **LOG ENTRY**

            #region **LOG ENTRY**
            LogManager.LogFromSPDetails(LogLevel.Info, "Loading reference data for the operation.");
            #endregion **LOG ENTRY**

            #region ##START## Load Reference data from DB            
            //Load required reference data
            List<RefDataLoadStatus> refLoadStatuses = Mt940LoadHandler.GetLoadStatuses();
            List<RefDataStatus> refStatuses = Mt940LoadHandler.GetStatuses();

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
                LogManager.LoadFailedMT940(request, refLoadStatuses);

                LogManager.RaiseException(string.Format("Error! during Oracle GL load checking of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        "Load is not valid! It's either set to Read Only or Load Status is not submitted. The Loads Status has been updated to 'Failed'."));
            }

            #region **LOG ENTRY**
            LogFromSPDetails(LogLevel.Info, "Reference data validation completed successfully.");
            #endregion **LOG ENTRY**

            //Update Load's Load Status to Parsing in a new context
            OracleGLLoadHndlr.UpdateLoadStatusInNewContext(request.LoadId,
                                                             refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSParsing)),
                                                             request.UserId,
                                                             true);
            #region **LOG ENTRY**
            LogFromSPDetails(LogLevel.Info, "Oracle GL Load status set to 'Parsing'.");
            #endregion **LOG ENTRY**







            //Very important to set the header and trailer as these are going to be used later throughout this processing
            Mt940LoadHandler.SetHeaderTrailer(dbLoad.LoadMetaData.Header, dbLoad.LoadMetaData.Trailer);

            #region MT940 Content validaton
            try
            {
                //Validate the MT940 Base64 contents
                faults = Mt940LoadHandler.ValidateMT940FileContent(dbLoad.MT940Load.FileContent.FileContentBase64);
            }
            catch (Exception ex)
            {
                throw new FaultException(string.Format("Unexpected error! during MT940 file contents validation of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                throw new FaultException(string.Format("There was a fault validating passed Load Id = {1}, associated MT940 content.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId.ToString(),
                                                        DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region Load processing into database
            try
            {
                //Load the MT940 file data into objects and then to the database
                Mt940LoadHandler.LoadMT940(dbLoad, dbLoad.MT940Load.FileContent.FileContentBase64, request.UserId);
            }
            catch (Exception ex)
            {
                throw new FaultException(string.Format("Unexpected error! during the load processing into database of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }
            #endregion

            #region Update Load for the process completion

            try
            {
                DateTime updateTime = DateTime.UtcNow;
                //MT940 Load record fields to be updated
                dbLoad.MT940Load.CustomerStatementCount = Mt940LoadHandler.GetProcessedCustomerStatementCount();
                dbLoad.MT940Load.ModifiedBy = request.UserId;

                //Load records field to be updated                                
                dbLoad.InProgress = false;
                dbLoad.Finish = updateTime;
                dbLoad.ModifiedBy = request.UserId;

                //Perform the database update
                Mt940LoadHandler.SummariseMT940LoadOnCompletion(dbLoad, dbLoad.MT940Load);
            }
            catch (Exception ex)
            {
                throw new FaultException(string.Format("Unexpected error! after load completion of Load Id = {1}, while updating the load record for progress.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }
            #endregion

            return new LoadMT940Response(LoadMT940Response.SUCCESS_CODE, LoadMT940Response.SUCCESS_MESSAGE);
        }
    }
}
