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
    internal class OracleGLLoaderLibrary: IDisposable
    {
        private List<LoaderFault> _operationFaults = new List<LoaderFault>();
        private bool _disposed = false;

        public List<LoaderFault> OperationFaults
        {
            get
            {
                return _operationFaults;
            }

            set
            {
                _operationFaults = value;
            }
        }

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
                    _operationFaults.Clear();
                    _operationFaults = null;
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.


                // Note disposing has been done.
                _disposed = true;

            }
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
