using Gf.Frs.LoaderServices.InputOutput.MT940;
using Gf.Frs.MT940Loader.Handlers;
using System.Collections.Generic;
using System.ServiceModel;
using System;
using DevTrends.WCFDataAnnotations;
using Gf.Frs.MT940Loader;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.MT940LoaderService.InputOutput.MT940;
using System.Threading;

namespace Gf.Frs.LoaderServices.Wcf.MT940
{
    [ValidateDataAnnotationsBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class FrsMT940WcfLoaderService : IFrsMT940WcfLoaderService
    {
        public IAsyncResult BeginLoadMT940AfterInsert(LoadMT940AfterInsertRequest request, AsyncCallback callback, object state)
        {
            LoadMT940AfterInsertResponse response = LoadMT940AfterInsert(request);

            var asyncResult = new LoadMT940AfterInsertResponseAsyncResponse(response.Code, response.Message, callback, state);

            ThreadPool.QueueUserWorkItem(CompleteProcess, asyncResult);

            return asyncResult;
        }

        private void CompleteProcess(object state)
        {
            var asyncResult = state as LoadMT940AfterInsertResponseAsyncResponse;
            asyncResult.Completed();
        }

        public IAsyncResult EndLoadMT940AfterInsert(IAsyncResult asyncResult)
        {
            var operationAsyncResult = asyncResult as LoadMT940AfterInsertResponseAsyncResponse;
            operationAsyncResult.AsyncWait.WaitOne();
            return operationAsyncResult;
        }

        public LoadMT940AfterInsertResponse LoadMT940AfterInsert(LoadMT940AfterInsertRequest request)
        {
            if(string.IsNullOrEmpty(request.UserId))
            {
                throw new FaultException(string.Format("There was a fault validating passed User Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId.ToString(),
                                                        "The user id is not valid. Please provide a valid User Id for a successful execution."));
            }

            MT940LoadHandler mt940LoadHandler = new MT940LoadHandler();
            List<LoaderFault> faults = new List<LoaderFault>();

            #region Validation of request Load Id
            try
            {
                //Validate the Load object fully, including the associated objects
                faults = mt940LoadHandler.ValidateLoadForNullOrEmpty(request.LoadId);
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
            Load load = mt940LoadHandler.GetLoad(request.LoadId);

            //Very important to set the header and trailer as these are going to be used later throughout this processing
            mt940LoadHandler.SetHeaderTrailer(load.LoadMetaData.Header, load.LoadMetaData.Trailer);

            #region MT940 Content validaton
            try
            {
                //Validate the MT940 Base64 contents
                faults = mt940LoadHandler.ValidateMT940FileContent(load.MT940Load.FileContent.FileContentBase64);
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
                mt940LoadHandler.LoadMT940(load, load.MT940Load.FileContent.FileContentBase64, request.UserId);
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
                load.MT940Load.CustomerStatementCount = mt940LoadHandler.GetProcessedCustomerStatementCount();
                load.MT940Load.ModifiedBy = request.UserId;

                //Load records field to be updated                                
                load.InProgress = false;
                load.Finish = updateTime;
                load.ModifiedBy = request.UserId;

                //Perform the database update
                mt940LoadHandler.SummariseMT940LoadOnCompletion(load, load.MT940Load);
            }
            catch (Exception ex)
            {
                throw new FaultException(string.Format("Unexpected error! after load completion of Load Id = {1}, while updating the load record for progress.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            } 
            #endregion

            return new LoadMT940AfterInsertResponse(LoadMT940AfterInsertResponse.SUCCESS_CODE, LoadMT940AfterInsertResponse.SUCCESS_MESSAGE);
        }
    }

}
