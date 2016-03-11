﻿using DevTrends.WCFDataAnnotations;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.LoaderServices.InputOutput.OracleGL;
using Gf.Frs.OracleGLLoader.DataModel;
using Gf.Frs.OracleGLLoader.Handlers;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Gf.Frs.LoaderServices.Wcf.OracleGL
{
    [ValidateDataAnnotationsBehavior]
    public class FrsOracleGLWcfLoaderService : IFrsOracleGLWcfLoaderService
    {
        public LoadOracleGLAfterInsertResponse LoadOracleGLAfterInsert(LoadOracleGLAfterInsertRequest request)
        {
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
            Load load = oracleGlLoadHandler.GetLoad(request.LoadId);

            #region ##START## Oracle GL Content validaton
            try
            {
                //Validate the MT940 Base64 contents
                faults = oracleGlLoadHandler.ValidateOracleGLFileContent(load.OracleGLLoad.FileContent.FileContentBase64);
            }
            catch (Exception ex)
            {
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
                //Load the Oracle GL file data into objects and then to the database
                oracleGlLoadHandler.LoadOracleGL(load, load.OracleGLLoad.FileContent.FileContentBase64, request.UserId);
            }
            catch (Exception ex)
            {
                throw new FaultException(string.Format("Unexpected error! during the load processing into database of Load Id = {1}.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }
            #endregion

            #region ##START## Update Load for the process completion

            try
            {
                DateTime updateTime = DateTime.UtcNow;
                //MT940 Load record fields to be updated
                load.OracleGLLoad.OracleGLEntryCount = oracleGlLoadHandler.GetProcessedCustomerStatementCount();
                load.OracleGLLoad.ModifiedBy = request.UserId;

                //Load records field to be updated                                
                load.InProgress = false;
                load.Finish = updateTime;
                load.ModifiedBy = request.UserId;

                //Perform the database update
                oracleGlLoadHandler.SummariseOracleGLLoadOnCompletion(load, load.OracleGLLoad);
            }
            catch (Exception ex)
            {
                throw new FaultException(string.Format("Unexpected error! after load completion of Load Id = {1}, while updating the load record for progress.{0}Fault details:{0}{2}",
                                                        Environment.NewLine,
                                                        request.LoadId,
                                                        ExceptionExtensions.ToDetailedString(ex)));
            }
            #endregion

            return new LoadOracleGLAfterInsertResponse(LoadOracleGLAfterInsertResponse.SUCCESS_CODE, LoadOracleGLAfterInsertResponse.SUCCESS_MESSAGE);
        }
    }
}
