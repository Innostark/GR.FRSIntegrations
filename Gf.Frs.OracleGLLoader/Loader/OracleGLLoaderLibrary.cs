using CsvHelper;
using Gf.Frs.IntegrationCommon.Fault;
using Gf.Frs.OracleGLLoader.Csv.Map;
using Gf.Frs.OracleGLLoader.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gf.Frs.OracleGLLoader.Loader
{
    internal class OracleGLLoaderLibrary
    {
        public List<LoaderFault> OperationFaults = new List<LoaderFault>();

        internal bool ValidateContent(TextReader stream)
        {
            try
            {
                CsvReader csv = new CsvReader(stream);

                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<AccountingEntryMap>();
                
                IEnumerable< AccountingEntry> records = csv.GetRecords<AccountingEntry>().ToList();

                return true;
            }
            catch (Exception ex)
            {
                AddFileLibraryInvalidation(ex);
            }

            return false;
        }

        internal IEnumerable<AccountingEntry> LoadBase64OracleGLContent(string base64Content)
        {
            try
            {
                //DECODE the base 64 back into text form
                byte[] raw = Convert.FromBase64String(base64Content);
                using (MemoryStream decoded = new MemoryStream(raw))
                {
                    CsvReader csv = new CsvReader(new StreamReader(decoded));

                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.RegisterClassMap<AccountingEntryMap>();

                    return csv.GetRecords<AccountingEntry>().ToList();
                }

            }
            catch (Exception ex)
            {
                AddFileLibraryInvalidation(ex);
            }

            return null;
        }

        #region ##START## - Exception and Fault Methods"

        private void AddFileLibraryInvalidation(Exception ex)
        {
            ClearList(OperationFaults);
            OperationFaults.Add(new LoaderFault(FRSValidationMessages.LFV_C_FileFailedLibraryValidationAndLoadToObject,
                                                FRSValidationMessages.LFV_FileFailedLibraryValidationAndLoadToObject));
            string errorMessage = ex.Message;
            if (ex.InnerException != null && !String.IsNullOrEmpty(ex.InnerException.Message))
            {
                errorMessage += Environment.NewLine;
                errorMessage += "Error details: " + ex.InnerException.Message;
            }
            OperationFaults.Add(new LoaderFault(FRSValidationMessages.LFV_C_LibraryError,
                                                string.Format(FRSValidationMessages.LFV_LibraryError, errorMessage)));
        }

        private void ClearList<T>(List<T> list)
        {
            if (list != null)
            {
                list.Clear();
            }
            else
            {
                list = new List<T>();
            }
        }

        #endregion ##END## - Exception and Fault Methods

    }
}
