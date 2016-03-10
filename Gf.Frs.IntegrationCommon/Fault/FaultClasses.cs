namespace Gf.Frs.IntegrationCommon.Fault
{
    public class FrsLoadValidationFaults
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
    }

    public class FrsFileContentValidationFaults
    {
        public const int NBD_C_NoBase64Data = 300;
        public const string NBD_NoBase64Data = "The associated 'FileContent' had no Base 64 data.";

        public const int NBD_C_Base64DataNotValid = 301;
        public const string NBD_Base64DataNotValid = "The associated 'FileContent' records contains invalid 'FileContentBase64'.";
    }


    public class FRSValidationMessages
    {
        public const int LFV_C_FileFailedLibraryValidationAndLoadToObject = 666;
        public const string LFV_FileFailedLibraryValidationAndLoadToObject = "The Oracle GL file has failed detailed validation and loading into an object.";

        public const int LFV_C_LibraryError = 667;
        public const string LFV_LibraryError = "Internal library error - *** {0} ***";
    }

}
