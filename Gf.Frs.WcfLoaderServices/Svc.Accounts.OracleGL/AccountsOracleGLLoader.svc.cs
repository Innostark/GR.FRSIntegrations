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

namespace Gf.Frs.WcfLoaderServices.Accounts.OracleGL
{
    [ValidateDataAnnotationsBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class AccountsOracleGLLoader : IAccountsOracleGLLoader
    {
        public LoadOracleGLResponse LoadOracleGL(LoadOracleGLRequest request)
        {
            Stopwatch watch = Stopwatch.StartNew();

            #region TODO: Authentication to be added later on project sign off
            //ServiceSecurityContext ssc = ServiceSecurityContext.Current;
            //if (!ssc.IsAnonymous && ssc.PrimaryIdentity != null)
            //{
            //    spDetails.userName = ServiceSecurityContext.Current.PrimaryIdentity.Name;
            //} 
            #endregion

            FrsNLogManager frsLogManager = new FrsNLogManager();
            const string loggerName = "AccountsOracleGLLoader.LoadOracleGL";

            FrsNLogIntegrationServiceStoredProc spDetails = new FrsNLogIntegrationServiceStoredProc();
            LogEventInfo logEventInfo = new LogEventInfo()
            {
                LoggerName = loggerName
            };

            //Set logger details 
            SetLoggerDetails(spDetails, logEventInfo);

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Entering LoadOracleGL and setting up logger configurations...");
            frsLogManager.Instance.Log(logEventInfo);

            SetEventInfo(logEventInfo, LogLevel.Info, "Starting validation... Validating input (LoadOracleGLRequest) request.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            #region ##START## Request Parameters Validation

            if (request == null)
            {
                RaiseException(frsLogManager, logEventInfo, string.Format("There was a fault validating the request.{0}Fault details:{0}{1}",
                                                                        Environment.NewLine,
                                                                        "The request cannot be empty. Please provide a valid request object for a successful execution."));
}
            
            if (request.LoadId <= 0)
            {
                RaiseException(frsLogManager, logEventInfo, string.Format("There was a fault validating passed Load Id = {1}.{0}Fault details:{0}{2}",
                                                                        Environment.NewLine,
                                                                        request.LoadId.ToString(),
                                                                        "The load id is not valid. Please provide a valid Load Id for a successful execution."));
            }

            if (string.IsNullOrEmpty(request.UserId))
            {
                RaiseException(frsLogManager, logEventInfo, string.Format("There was a fault validating passed User Id = {1}.{0}Fault details:{0}{2}",
                                                                        Environment.NewLine,
                                                                        request.UserId,
                                                                        "The user id is not valid. Please provide a valid User Id for a successful execution."));
}

            #endregion

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Validating input (LoadOracleGLRequest) request completed successfully.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            OracleGLLoadHandler oracleGlLoadHandler = new OracleGLLoadHandler();
            List<LoaderFault> faults = new List<LoaderFault>();

            #region ##START## Validation of request Load Id
            try
            {
                #region **LOG ENTRY**
                SetEventInfo(logEventInfo, LogLevel.Info, "Validate the Load object fully, including the associated objects.");
                frsLogManager.Instance.Log(logEventInfo);
                #endregion **LOG ENTRY**

                //Validate the Load object fully, including the associated objects
                faults = oracleGlLoadHandler.ValidateLoadForNullOrEmpty(request.LoadId);
            }
            catch (Exception ex)
            {
                RaiseException(frsLogManager, logEventInfo, string.Format("Unexpected error! during validation of Load Id = {1}.{0}Fault details:{0}{2}",
                                                                          Environment.NewLine,
                                                                          request.LoadId,
                                                                          ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                RaiseException(frsLogManager, logEventInfo, string.Format("There was a fault validating passed Load Id = {1}.{0}Fault details:{0}{2}",
                                                                        Environment.NewLine,
                                                                        request.LoadId.ToString(),
                                                                        DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Get load object from database (to be used through out the function for this call), i.e. requested load.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            //Get load object from database (to be used through out the code for this call)
            Load dbLoad = oracleGlLoadHandler.GetLoad(request.LoadId);

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, string.Format("Load (Load Id = {0}, successfully found in the database.", request.LoadId));
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Loading reference data for the operation.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            #region ##START## Load Reference data from DB            
            //Load required reference data
            List<RefDataLoadStatus> refLoadStatuses = oracleGlLoadHandler.GetLoadStatuses();
            List<RefDataStatus> refStatuses = oracleGlLoadHandler.GetStatuses();

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Reference data loaded successfully.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            #endregion

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Validating the load record against reference data i.e. Read-only & Load Status.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            if (dbLoad.ReadOnly || dbLoad.LoadStatusId != refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSSubmitted)).Value)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, frsLogManager, logEventInfo, oracleGlLoadHandler, refLoadStatuses);

                RaiseException(frsLogManager, logEventInfo, string.Format("Error! during Oracle GL load checking of Load Id = {1}.{0}Fault details:{0}{2}",
                                                                        Environment.NewLine,
                                                                        request.LoadId,
                                                                        "Load is not valid! It's either set to Read Only or Load Status is not submitted. The Loads Status has been updated to 'Failed'."));
            }

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Reference data validation completed successfully.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            //Update Load's Load Status to Parsing in a new context
            oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                             refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSParsing)),
                                                             request.UserId,
                                                             true);
            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL Load status set to 'Parsing'.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            #region ##START## Oracle GL Content validaton
            try
            {
                #region **LOG ENTRY**
                SetEventInfo(logEventInfo, LogLevel.Info, "Validating the Oracle GL file contents.");
                frsLogManager.Instance.Log(logEventInfo);
                #endregion **LOG ENTRY**

                //Validate the Oracle GL Base64 contents
                faults = oracleGlLoadHandler.ValidateOracleGLFileContent(dbLoad.OracleGLLoad.FileContent.FileContentBase64);

                #region **LOG ENTRY**
                SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL file contents validated successfully.");
                frsLogManager.Instance.Log(logEventInfo);
                #endregion **LOG ENTRY**

                //Update Load's Load Status to Transforming in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSTransforming)),
                                                                 request.UserId,
                                                                 true);

                #region **LOG ENTRY**
                SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL Load record status set to 'Transforming'.");
                frsLogManager.Instance.Log(logEventInfo);
                #endregion **LOG ENTRY**
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, frsLogManager, logEventInfo, oracleGlLoadHandler, refLoadStatuses);

                RaiseException(frsLogManager, logEventInfo, string.Format("Unexpected error! during Oracle GL file contents validation of Load Id = {1}.{0}Fault details:{0}{2}",
                                                                        Environment.NewLine,
                                                                        request.LoadId,
                                                                        ExceptionExtensions.ToDetailedString(ex)));
            }

            if (faults != null && faults.Count > 0)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, frsLogManager, logEventInfo, oracleGlLoadHandler, refLoadStatuses);

                RaiseException(frsLogManager, logEventInfo, string.Format("There was a fault validating passed Load Id = {1}, associated Oracle GL content.{0}Fault details:{0}{2}",
                                                                        Environment.NewLine,
                                                                        request.LoadId.ToString(),
                                                                        DotNetHelper.WrapFaultListToString(faults)));
            }
            #endregion

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL loading started...");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            #region ##START## Load processing into database
            try
            {
                //Update Load's Load Status to Parsing in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSImporting)),
                                                                 request.UserId,
                                                                 true);
                #region **LOG ENTRY**
                SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL Load record status set to 'Importing'.");
                frsLogManager.Instance.Log(logEventInfo);
                #endregion **LOG ENTRY**

                //Load the Oracle GL file data into objects and then to the database
                oracleGlLoadHandler.LoadOracleGL(dbLoad, dbLoad.OracleGLLoad.FileContent.FileContentBase64, request.UserId);

                #region **LOG ENTRY**
                SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL data loaded in Oracle GL entries table.");
                frsLogManager.Instance.Log(logEventInfo);
                #endregion **LOG ENTRY**
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                LoadFailed(request, frsLogManager, logEventInfo, oracleGlLoadHandler, refLoadStatuses);

                RaiseException(frsLogManager, logEventInfo, string.Format("Unexpected error! during the load processing into database of Load Id = {1}.{0}Fault details:{0}{2}",
                                                                        Environment.NewLine,
                                                                        request.LoadId,
                                                                        ExceptionExtensions.ToDetailedString(ex)));
            }

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL data load complete.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            #endregion

            #region ##START## Update Load for the process completion
            
            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL load house keeping started...");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

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
                dbLoad.LoadStatusId = refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSCompleted)).Value;
                dbLoad.ReadOnly = false;

                //Perform the database update
                oracleGlLoadHandler.SummariseOracleGLLoadOnCompletion(dbLoad, dbLoad.OracleGLLoad);
            }
            catch (Exception ex)
            {
                //Update Load's Load Status to Failed in a new context
                oracleGlLoadHandler.UpdateLoadStatusInNewContext(request.LoadId,
                                                                 refLoadStatuses.Find(rls => rls.Name.ToLower().Equals(RefDataLoadStatus.LSFailed)),
                                                                 request.UserId,
                                                                 true);

                RaiseException(frsLogManager, logEventInfo, string.Format("Unexpected error! after load completion of Load Id = {1}, while updating the load record for progress.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL load house keeping completed successfully.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            #endregion

            watch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = watch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            #region **LOG ENTRY**
            SetEventInfo(logEventInfo, LogLevel.Info, "Oracle GL load house keeping completed successfully.");
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            LoadOracleGLResponse response = new LoadOracleGLResponse(LoadOracleGLResponse.SUCCESS_CODE, LoadOracleGLResponse.SUCCESS_MESSAGE);

            #region **LOG ENTRY**
            SetLoggerResponse(spDetails, logEventInfo, response.ToString());
            SetEventInfo(logEventInfo, LogLevel.Info, string.Format("Oracle GL load for Load Id = {0} completed successfully. Time taken was - {1}", request.LoadId, elapsedTime));
            frsLogManager.Instance.Log(logEventInfo);
            #endregion **LOG ENTRY**

            return response;
        }

        private static void LoadFailed(LoadOracleGLRequest request, FrsNLogManager frsLogManager, LogEventInfo logEventInfo, OracleGLLoadHandler oracleGlLoadHandler, List<RefDataLoadStatus> refLoadStatuses)
        {
            //Update Load's Load Status to Failed in a new context
            LoadFailed(request, frsLogManager, logEventInfo, oracleGlLoadHandler, refLoadStatuses);
        }

        private static void SetEventInfo(LogEventInfo lEventInfo, LogLevel level, string message, Exception ex=null)
        {
            lEventInfo.Level = level;
            lEventInfo.Message = message;
            lEventInfo.Exception = ex;
        }

        private static void RaiseException(FrsNLogManager frsLogManager, LogEventInfo logEventInfo, string message)
        {
            try
            {
                throw new FaultException(message);
            }
            catch (FaultException fe)
            {
                #region **LOG ENTRY**
                SetEventInfo(logEventInfo, LogLevel.Fatal, message, fe);
                //frsLogManager.Instance.Log(logEventInfo);
                #endregion **LOG ENTRY**

                //Throw exception back
                throw fe;
            }
        }

        private static void SetLoggerDetails(FrsNLogIntegrationServiceStoredProc spDetails, LogEventInfo logEventInfo)
        {
            spDetails.SiteName = OperationContext.Current.IncomingMessageHeaders.To.Authority;
            if (!logEventInfo.Properties.ContainsKey("SiteName"))
                logEventInfo.Properties["SiteName"] = spDetails.SiteName;

            spDetails.UserID = null;
            if (!logEventInfo.Properties.ContainsKey("UserID"))
                logEventInfo.Properties["UserID"] = spDetails.UserID;

            spDetails.ServerName = OperationContext.Current.IncomingMessageHeaders.To.DnsSafeHost;
            if (!logEventInfo.Properties.ContainsKey("ServerName"))
                logEventInfo.Properties["ServerName"] = spDetails.ServerName;

            spDetails.Port = OperationContext.Current.IncomingMessageHeaders.To.Port.ToString();
            if (!logEventInfo.Properties.ContainsKey("Port"))
                logEventInfo.Properties["Port"] = spDetails.Port;

            spDetails.SessionID = null;
            if (!logEventInfo.Properties.ContainsKey("SessionID"))
                logEventInfo.Properties["SessionID"] = spDetails.SessionID;

            spDetails.CallerDetails = OperationContext.Current.IncomingMessageHeaders.To.ToString();
            if (!logEventInfo.Properties.ContainsKey("CallerDetails"))
                logEventInfo.Properties["CallerDetails"] = spDetails.CallerDetails;

            spDetails.RequestDump = OperationContext.Current.RequestContext.ToString();
            if (!logEventInfo.Properties.ContainsKey("RequestDump"))
                logEventInfo.Properties["RequestDump"] = spDetails.RequestDump;

            spDetails.Url = OperationContext.Current.IncomingMessageHeaders.To.AbsoluteUri;
            if (!logEventInfo.Properties.ContainsKey("Url"))
                logEventInfo.Properties["Url"] = spDetails.Url;

            spDetails.Https = ((string.IsNullOrEmpty(OperationContext.Current.IncomingMessageHeaders.To.Scheme)) ? ((OperationContext.Current.IncomingMessageHeaders.To.Scheme.ToUpper().Equals("HTTPS")) ? true : false) : false);
            if (!logEventInfo.Properties.ContainsKey("Https"))
                logEventInfo.Properties["Https"] = spDetails.Https;


            IPHostEntry heServer = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress curAddress = heServer.AddressList[0];
            spDetails.ServerAddress = curAddress.MapToIPv4().ToString();
            if (!logEventInfo.Properties.ContainsKey("ServerAddress"))
                logEventInfo.Properties["ServerAddress"] = spDetails.ServerAddress;


            OperationContext context = OperationContext.Current;
            RemoteEndpointMessageProperty remoteEndpointMessageProperty = (RemoteEndpointMessageProperty)context.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            spDetails.RemoteAddress = remoteEndpointMessageProperty.Address;
            if (!logEventInfo.Properties.ContainsKey("RemoteAddress"))
                logEventInfo.Properties["RemoteAddress"] = spDetails.RemoteAddress;


            spDetails.CallSite = string.Empty;
            if (!logEventInfo.Properties.ContainsKey("CallSite"))
                logEventInfo.Properties["CallSite"] = spDetails.CallSite;

        }

        private static void SetLoggerResponse(FrsNLogIntegrationServiceStoredProc spDetails, LogEventInfo logEventInfo, string response)
        {
            spDetails.RequestDump = response;
            if (!logEventInfo.Properties.ContainsKey("ResponseDump"))
                logEventInfo.Properties["ResponseDump"] = spDetails.ResponseDump;
        }

        private static void SetLoggerResponse(FrsNLogIntegrationServiceStoredProc spDetails, LoadOracleGLResponse response, LogEventInfo logEventInfo)
        {
            if (response != null)
            {
                spDetails.ResponseDump = response.ToString();
                if (!logEventInfo.Properties.ContainsKey("ResponseDump"))
                    logEventInfo.Properties["ResponseDump"] = spDetails.ResponseDump;
            }

        }

    }
}
