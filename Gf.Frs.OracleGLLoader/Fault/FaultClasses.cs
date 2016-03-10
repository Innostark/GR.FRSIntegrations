namespace Gf.Frs.OracleGLLoader.Fault
{
    public class FrsOracleGLLoadMetadataValidationFaults
    {
        public const int RNM_C_RecordIsNotLoadTypeCsv = 201;
        public const string RNM_RecordIsNotLoadTypeCsv = "The Load Meta Data associated with this load record is not of Load Type CSV, expecting LoadTypeId of LoadMetada to be '{0}', found was '{1}'.";
    }
}
