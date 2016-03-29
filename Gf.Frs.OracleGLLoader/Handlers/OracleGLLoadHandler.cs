using System;
using System.Linq;
using System.Collections.Generic;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.OracleGLLoader.Loader;
using Gf.Frs.OracleGLLoader.DataModel;
using Gf.Frs.IntegrationCommon.Helpers;
using Gf.Frs.OracleGLLoader.Fault;
using System.IO;
using Gf.Frs.OracleGLLoader.DataModel.Mappers;
using Gf.Frs.IntegrationCommon.DataModel;

namespace Gf.Frs.OracleGLLoader.Handlers
{
    public class OracleGLLoadHandler
    {
        private OracleGLDatabaseHandler _dbHandler;
        private OracleGLLoaderLibrary _oracleGlLoader;
        private short _AppConfigLoadTypeOracleGLId = short.MinValue;
        private byte _processedOracleGLEntryCount = 0;

        internal OracleGLDatabaseHandler DbHandler
        {
            get
            {
                return _dbHandler;
            }

            set
            {
                _dbHandler = value;
            }
        }

        internal OracleGLLoaderLibrary OracleGLLoader
        {
            get
            {
                return _oracleGlLoader;
            }

            set
            {
                _oracleGlLoader = value;
            }
        }

        internal byte ProcessedOracleGLEntryCount
        {
            get
            {
                return _processedOracleGLEntryCount;
            }

            set
            {
                _processedOracleGLEntryCount = value;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public OracleGLLoadHandler()
        {
            _dbHandler = new OracleGLDatabaseHandler();
            _oracleGlLoader = new OracleGLLoaderLibrary();

            short.TryParse(DotNetHelper.ReadAppConfigAppSetting(LoaderConstants.RefDataLoadTypeOracleGLId), out _AppConfigLoadTypeOracleGLId);
        }

        
        #region **START - Private Methods **
        /// <summary>
        /// Internal validation method to check if the loads associated Load Metadata record's valid.
        /// </summary>
        /// <param name="loadMetadata">Load metadata object</param>
        /// <param name="faults">Faults collection</param>
        private void ValidateAssociatedLoadMetadataForNullOrEmpty(LoadMetaData loadMetadata, List<LoaderFault> faults)
        {
            //Validate the associated Load Metadata
            if (loadMetadata == null)
            {
                faults.Add(new LoaderFault(FrsLoadValidationFaults.LRNF_C_LinkedRecordNotFound,
                                                string.Format(FrsLoadValidationFaults.LRNF_LinkedRecordNotFound, "LoadMetada", "Load")));
            }
            else
            {
                if (loadMetadata.LoadType == null)
                {
                    faults.Add(new LoaderFault(FRSLoadMetadataValidationFaults.NLTF_C_NoLoadTypeFound, FRSLoadMetadataValidationFaults.NLTF_NoLoadTypeFound));
                }
                else
                {
                    short metadataLoadTypeValue = DotNetHelper.ConvertShort(loadMetadata.LoadType.Value);
                    if (metadataLoadTypeValue != _AppConfigLoadTypeOracleGLId)
                        faults.Add(new LoaderFault(FrsOracleGLLoadMetadataValidationFaults.RNM_C_RecordIsNotLoadTypeCsv,
                                                   string.Format(FrsOracleGLLoadMetadataValidationFaults.RNM_RecordIsNotLoadTypeCsv, _AppConfigLoadTypeOracleGLId.ToString(), metadataLoadTypeValue.ToString())));
                }
            }
        }

        /// <summary>
        /// Internal validation method to check if the loads associated MT940 Load record's valid.
        /// </summary>
        /// <param name="mt940Load">MT940 load object</param>
        /// <param name="faults">Fault collection</param>
        private void ValidateAssociatedOracleGLLoadForNullOrEmpty(OracleGLLoad mt940Load, List<LoaderFault> faults)
        {
            if (mt940Load == null)
            {
                faults.Add(new LoaderFault(FrsLoadValidationFaults.LRNF_C_LinkedRecordNotFound,
                                                string.Format(FrsLoadValidationFaults.LRNF_LinkedRecordNotFound, "OracleGLLoad", "Load")));
            }
            else
            {
                ValidateAssociatedFileContent(mt940Load.FileContent, faults);
            }
        }

        /// <summary>
        /// Internal validation method to check if the Oracle GL loads associated File Content's valid.
        /// </summary>
        /// <param name="fileContent">File content object</param>
        /// <param name="faults">Faults collection</param>
        private void ValidateAssociatedFileContent(FileContent fileContent, List<LoaderFault> faults)
        {
            if (fileContent == null)
            {
                faults.Add(new LoaderFault(FrsLoadValidationFaults.LRNF_C_LinkedRecordNotFound,
                                                string.Format(FrsLoadValidationFaults.LRNF_LinkedRecordNotFound, "FileContent", "OracleGLLoad")));
            }
            else
            {
                if (string.IsNullOrEmpty(fileContent.FileContentBase64))
                    faults.Add(new LoaderFault(FrsFileContentValidationFaults.NBD_C_NoBase64Data, FrsFileContentValidationFaults.NBD_NoBase64Data));
            }

        }       

        private OracleGLEntry AddOracleGLEntry(OracleGLEntry oracleGlEntry)
        {            
            return _dbHandler.DbContext.OracleGLEntries.Add(oracleGlEntry); ;
        }
        #endregion **END - Private Methods **

        #region **STRAT - Public Methods**

        /// <summary>
        /// Method to validate the Load id passed to process this Oracle GL load.
        /// </summary>
        /// <param name="id">Load record's primary key</param>
        /// <returns>Faults collection</returns>
        public List<LoaderFault> ValidateLoadForNullOrEmpty(long id)
        {
            List<LoaderFault> faults = new List<LoaderFault>();
            Load load = null;

            try
            {
                load = _dbHandler.GetLoadById(id);
            }
            catch(Exception ex)
            {
                faults.Add(new LoaderFault(FrsLoadValidationFaults.NRF_C_NoRecordFoundWithId,
                                                string.Format(FrsLoadValidationFaults.NRF_NoRecordFoundWithId, "Load", "LoadId", (id.ToString() + " Exception Details: " + ex.Message))));
            }

            //Validate the Load record
            //If no load record was returned
            if (load == null)
            {
                if (faults.Count == 0)
                {
                    faults.Add(new LoaderFault(FrsLoadValidationFaults.NRF_C_NoRecordFoundWithId,
                                                    string.Format(FrsLoadValidationFaults.NRF_NoRecordFoundWithId, "Load", "LoadId", id.ToString())));
                }
            }
            else
            {
                //Validate the associated Load Metadata record
                ValidateAssociatedLoadMetadataForNullOrEmpty(load.LoadMetaData, faults);

                //Validate the associated OracleGLLoad
                ValidateAssociatedOracleGLLoadForNullOrEmpty(load.OracleGLLoad, faults);
            }

            //Return null if there are no faults
            return faults.Count > 0 ? faults : null;
        }

        /// <summary>
        /// Method to check the base 64 contents of the Oracle GL files
        /// </summary>
        /// <param name="base64Content">Base64 string contents of Oracle GL file</param>
        /// <returns>Faults collection</returns>
        public List<LoaderFault> ValidateOracleGLFileContent(string base64Content)
        {
            List<LoaderFault> faults = new List<LoaderFault>();
            // DECODE
            byte[] raw = Convert.FromBase64String(base64Content);
            using (MemoryStream decoded = new MemoryStream(raw))
            {
                _oracleGlLoader.ValidateContent(new StreamReader(decoded));
                faults = _oracleGlLoader.OperationFaults;
            }            
                
            return faults;
        }

        /// <summary>
        /// Method to get the Load record from FRS database based on the passed id.
        /// </summary>
        /// <param name="id">Primary Key of Load record</param>
        /// <returns>Load object</returns>
        public Load GetLoad(long id)
        {
            return _dbHandler.GetLoadById(id);
        }

        /// <summary>
        /// Method to load the Oracle GL contents into the FRS database as per the schema.
        /// </summary>
        /// <param name="load">Load object for this load</param>
        /// <param name="base64Content">Base64 string contents of Oracle GL file</param>
        public void LoadOracleGL(Load load, string base64Content, string userId)
        {
            IEnumerable<AccountingEntry> accountingEnteries = _oracleGlLoader.LoadBase64OracleGLContent(base64Content);
            bool isSaveChanges = false;
            foreach (AccountingEntry accountingEntry in accountingEnteries)
            {
                OracleGLEntry oracleGlEntry = _dbHandler.DbContext.OracleGLEntries.Create();

                oracleGlEntry.OracleGLLoadId = load.OracleGLLoad.OracleGLLoadId;

                #region **START** Set common customer statement data
                oracleGlEntry.AccountDescription = accountingEntry.AccountDescription;
                oracleGlEntry.AccountedCr = accountingEntry.AccountedCr;
                oracleGlEntry.AccountedDr = accountingEntry.AccountedDr;
                oracleGlEntry.AccountNumber = accountingEntry.AccountNumber;
                oracleGlEntry.Currency = accountingEntry.Currency;
                oracleGlEntry.EffectiveDate = accountingEntry.EffectiveDate;
                oracleGlEntry.EnteredCr = accountingEntry.EnteredCr;
                oracleGlEntry.EnteredDr = accountingEntry.EnteredDr;
                oracleGlEntry.EntrySource = accountingEntry.EntrySource;
                oracleGlEntry.ExchangeRate = accountingEntry.ExchangeRate;
                if (accountingEntry.FiscalYear != null && accountingEntry.FiscalYear.HasValue)
                {
                    oracleGlEntry.FiscalYearId = (short)accountingEntry.FiscalYear.Value;
                }
                oracleGlEntry.JECreationDate = accountingEntry.JECreationDate;
                oracleGlEntry.JELastUpdateDate = accountingEntry.JELastUpdateDate;
                oracleGlEntry.JournalEntryDescription = accountingEntry.JournalEntryDescription;
                oracleGlEntry.JournalEntryHeaderNumber = accountingEntry.JournalEntryHeaderNumber;
                oracleGlEntry.LineDescription = accountingEntry.LineDescription;
                oracleGlEntry.LineNumber = accountingEntry.LineNumber;
                oracleGlEntry.Period = accountingEntry.Period;
                oracleGlEntry.SubAccountDescription = accountingEntry.SubAccountDescription;
                oracleGlEntry.UniqueReferenceKey = accountingEntry.UniqueReferenceKey;

                oracleGlEntry.CreatedBy = userId;
                oracleGlEntry.ModifiedBy = userId;
                oracleGlEntry.StatusId = 1;

                #endregion **END** Set common customer statement data

                AddOracleGLEntry(oracleGlEntry);

                isSaveChanges = true;

                //break;
            }

            
            _processedOracleGLEntryCount = Convert.ToByte(accountingEnteries.Count());

            //If required commit changes to database
            if (isSaveChanges)
            {
                _dbHandler.DbContext.SaveChanges();
            }            
        }

        /// <summary>
        /// Method to get the number number of customer statements processed by current Oracle GL file load process
        /// </summary>
        /// <returns>Number of Customer Statement records processed</returns>
        public int GetProcessedCustomerStatementCount()
        {
            return Convert.ToInt16(_processedOracleGLEntryCount);
        }

        /// <summary>
        /// Method to load the Reference data Load Statuses
        /// </summary>
        /// <returns>A list of Ref Load Statuses</returns>
        public List<RefDataLoadStatus> GetLoadStatuses()
        {
            List<RefDataLoadStatus> refDataLoadStatuses = new List<RefDataLoadStatus>();

            foreach (LoadStatus loadStatus in _dbHandler.DbContext.LoadStatus.ToList())
            {
                RefDataLoadStatus refDataLoadStatus = new RefDataLoadStatus();
                refDataLoadStatuses.Add(DBLoadStatusToRefLoadStatus.MapToLoader(loadStatus, out refDataLoadStatus));
            }
            
            return refDataLoadStatuses;
        }

        /// <summary>
        /// Method to load the Reference data Statuses
        /// </summary>
        /// <returns></returns>
        public List<RefDataStatus> GetStatuses()
        {
            List<RefDataStatus> refDataStatuses = new List<RefDataStatus>();

            foreach (Status status in _dbHandler.DbContext.Status.ToList())
            {
                RefDataStatus refDataStatus = new RefDataStatus();
                refDataStatuses.Add(DBStatusToRefStatus.MapToLoader(status, out refDataStatus));
            }

            return refDataStatuses;
        }

        /// <summary>
        /// Method to uupdate the load summary information after an Oracle GL load has successfully completed.
        /// </summary>
        /// <param name="load">Load</param>
        /// <param name="oracleGlLoad">Oracle GL Load</param>
        public void SummariseOracleGLLoadOnCompletion(Load load, OracleGLLoad oracleGlLoad)
        {
            //Load modified fields
            _dbHandler.DbContext.Entry(load).Property(e => e.Finish).IsModified = true;
            _dbHandler.DbContext.Entry(load).Property(e => e.InProgress).IsModified = true;
            _dbHandler.DbContext.Entry(load).Property(e => e.ModifiedBy).IsModified = true;
            _dbHandler.DbContext.Entry(load).Property(e => e.LoadStatusId).IsModified = true;
            _dbHandler.DbContext.Entry(load).Property(e => e.ReadOnly).IsModified = true;


            //OracleGLLoad modified fields
            _dbHandler.DbContext.Entry(oracleGlLoad).Property(e => e.OracleGLEntryCount).IsModified = true;
            _dbHandler.DbContext.Entry(oracleGlLoad).Property(e => e.ModifiedBy).IsModified = true;

            //Commit changes to database
            _dbHandler.DbContext.SaveChanges();
        }

        public void UpdateLoadStatusInNewContext(long loadId, RefDataLoadStatus loadStatus, string userId, bool readOnly)
        {
            using (FrsOracleGLLoaderContext context = new FrsOracleGLLoaderContext())
            {
                Load load = DbHandler.GetLoadById(context, loadId);

                if(load != null)
                {
                    load.InProgress = true;
                    load.LoadStatusId = loadStatus.StatusId;
                    load.ModifiedBy = userId;
                    load.ReadOnly = readOnly;

                    context.Entry(load).Property(e => e.InProgress).IsModified = true;
                    context.Entry(load).Property(e => e.LoadStatusId).IsModified = true;
                    context.Entry(load).Property(e => e.ModifiedBy).IsModified = true;
                    context.Entry(load).Property(e => e.ReadOnly).IsModified = true;

                    context.SaveChanges();
                }
            }
        }

        #endregion **END - Public Methods**

    }
}
