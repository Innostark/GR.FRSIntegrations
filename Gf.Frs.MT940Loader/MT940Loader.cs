using Gf.Frs.MT940Loader.Faults;
using Raptorious.SharpMt940Lib;
using Raptorious.SharpMt940Lib.Mt940Format;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Gf.Frs.MT940Loader
{
    internal class MT940Loader
    {
        private string _filename;
        private string _path;
        private string _headerSeperator;
        private string _trailerSeperator;

        public List<MT940LoaderFault> OperationFaults;

        public string Filename
        {
            get
            {
                return _filename;
            }

            set
            {
                _filename = value;
            }
        }

        public string FilePath
        {
            get
            {
                return _path;
            }

            set
            {
                _path = value;
            }
        }

        public string HeaderSeperator
        {
            get
            {
                return _headerSeperator;
            }

            set
            {
                _headerSeperator = value;
            }
        }

        public string TrailerSeperator
        {
            get
            {
                return _trailerSeperator;
            }

            set
            {
                _trailerSeperator = value;
            }
        }

        public MT940Loader()
        {
            OperationFaults = new List<MT940LoaderFault>();
        }

        public MT940Loader(string filePath, string headerSeperator, string trailerSeperator)
        {
            if (string.IsNullOrEmpty(headerSeperator))
                throw new ArgumentException(MT940ValidationMessages.HNF_HeaderSeparatorCannotBeNullOrEmpty, "headerSeperator");

            if (string.IsNullOrEmpty(trailerSeperator))
                throw new ArgumentException(MT940ValidationMessages.HNF_TrailerSeparatorCanotBeNullOrEmpty, "trailerSeperator");

            if (ValidateFilePhysically(filePath))
            {
                _path = filePath;
                _filename = GetFileNameFromPath(filePath);
                HeaderSeperator = headerSeperator;
                TrailerSeperator = trailerSeperator;

                return;
            }

            AddFilePhysicalValidatoinFaultAndThrowException();
        }        

        public bool ValidateFile()
        {
            return ValidateFile(_path);
        }

        public bool ValidBase64MT940Content(string base64MT940Content)
        {
            return ValidateContent(base64MT940Content);
        }

        public ICollection<CustomerStatementMessage> LoadBase64MT940Content(string base64MT940Content)
        {
            try
            {
                Separator header = new Separator(HeaderSeperator);
                Separator trailer = new Separator(TrailerSeperator);
                GenericFormat genericFormat = new GenericFormat(header, trailer);
                string fileData = Encoding.ASCII.GetString(Convert.FromBase64String(base64MT940Content));
                return Mt940Parser.ParseData(genericFormat, fileData, CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                AddFileLibraryInvalidation(ex);
            }

            return null;
        }

        private string GetFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        private string GetFileNameWithoutExtensionFromPath(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        private bool ValidateFilePhysically(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Exists;
        }

        private bool ValidateContent(string base64Content)
        {
            try
            {
                Separator header = new Separator(HeaderSeperator);
                Separator trailer = new Separator(TrailerSeperator);
                GenericFormat genericFomat = new GenericFormat(header, trailer);
                string fileData = Encoding.ASCII.GetString(Convert.FromBase64String(base64Content));
                var parsed = Mt940Parser.ParseData(genericFomat, fileData, CultureInfo.CurrentCulture);

                return true;
            }
            catch (Exception ex)
            {
                AddFileLibraryInvalidation(ex);
            }

            return false;
        }

        private bool ValidateFile(string path)
        {
            if(!ValidateFilePhysically(path))
            {
                AddFilePhysicalValidationFault();
                return false;
            }

            try
            {
                var header = new Separator(HeaderSeperator);
                var trailer = new Separator(TrailerSeperator);
                var genericFomat = new GenericFormat(header, trailer);
                var parsed = Mt940Parser.Parse(genericFomat, FilePath, CultureInfo.CurrentCulture);
            }
            catch(Exception ex)
            {
                AddFileLibraryInvalidation(ex);

                return false;
            }

            return true;
        }

        #region ##START## - Exception and Fault Methods"

        private void ClearList<T>(List<T> list)
        {
            if(list!=null)
                list.Clear();
        }

        private void AddFilePhysicalValidatoinFaultAndThrowException()
        {
            ClearList(OperationFaults);
            AddFilePhysicalValidationFault();

            throw new FileNotFoundException(MT940ValidationMessages.FNF_FileNotFoundOnPath);
        }

        private void AddFilePhysicalValidationFault()
        {
            ClearList(OperationFaults);
            OperationFaults.Add(new MT940LoaderFault(MT940ValidationMessages.FNF_C_FileNotFoundOnPath, MT940ValidationMessages.FNF_FileNotFoundOnPath));
        }

        private void AddFileLibraryInvalidation(Exception ex)
        {
            ClearList(OperationFaults);
            OperationFaults.Add(new MT940LoaderFault(MT940ValidationMessages.LFV_C_FileFailedLibraryValidationAndLoadToObject,
                                                            MT940ValidationMessages.LFV_FileFailedLibraryValidationAndLoadToObject));
            string errorMessage = ex.Message;
            if (ex.InnerException != null && !String.IsNullOrEmpty(ex.InnerException.Message))
            {
                errorMessage += Environment.NewLine;
                errorMessage += "Error details: " + ex.InnerException.Message;
            }
            OperationFaults.Add(new MT940LoaderFault(MT940ValidationMessages.LFV_C_LibraryError,
                                                            string.Format(MT940ValidationMessages.LFV_LibraryError, errorMessage)));
        }

        #endregion ##END## - Exception and Fault Methods

    }
}
