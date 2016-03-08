namespace Gf.Frs.MT940Loader.Faults
{
    public class MT940ValidationMessages
    {
        public const string HNF_HeaderSeparatorCannotBeNullOrEmpty = "The file 'header separator' argument cannot be null or empty.";
        public const string HNF_TrailerSeparatorCanotBeNullOrEmpty = "The file 'trailer separator' argument cannot be null or empty.";

        public const int FNF_C_FileNotFoundOnPath = 1;
        public const string FNF_FileNotFoundOnPath = "The file on the mentioned path was not found. Please provide a valid MT940 file path or make sure the MT940 file exists on the stated path.";

        public const int LFV_C_FileFailedLibraryValidationAndLoadToObject = 666;
        public const string LFV_FileFailedLibraryValidationAndLoadToObject = "The MT940 file has failed detailed validation and loading into an object.";

        public const int LFV_C_LibraryError = 667;
        public const string LFV_LibraryError = "Internal library error - *** {0} ***";
    }

    public class FRSLoadValidationFaults
    {
        public const int NRF_C_NoRecordFoundWithId = 100;
        public const string NRF_NoRecordFoundWithId = "There was no '{0}' record found in the database for the '{1}' - '{2}.";

        public const int LRNF_C_LinkedRecordNotFound = 101;
        public const string LRNF_LinkedRecordNotFound = "A '{0}' record linked with a '{1}' record is required. There is surely an error in setup up of this '{1}' record.";
    }

    public class FRSLoadMetadataValidationFaults
    {
        public const int NLTF_C_NoLoadTypeFound = 201;
        public const string NLTF_NoLoadTypeFound = "The Load Meta Data associated with this load record is not valid, Load Type is missing and is required.";

        public const int RNM_C_RecordIsNotLoadTypeMT940 = 201;
        public const string RNM_RecordIsNotLoadTypeMT940 = "The Load Meta Data associated with this load record is not of Load Type MT940, expecting LoadTypeId of LoadMetada to be '{0}', found was '{1}'.";
    }

    public class FRSFileContentValidationFaults
    {
        public const int NBD_C_NoBase64Data = 300;
        public const string NBD_NoBase64Data = "The associated 'FileContent' had no Base 64 data.";

        public const int NBD_C_Base64DataNotValid = 301;
        public const string NBD_Base64DataNotValid = "The associated 'FileContent' records contains invalid 'FileContentBase64'.";
    }

}
