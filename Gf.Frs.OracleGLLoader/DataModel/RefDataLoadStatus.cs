namespace Gf.Frs.OracleGLLoader.DataModel
{
    public class RefDataLoadStatus
    {
        public byte Value { get; set; }
        public string Name { get; set; }
        public byte StatusId { get; set; }

        public const string LCCreated = "created";
        public const string LCSubmitted = "submitted";
        public const string LCParsing = "parsing";
        public const string LCTransforming = "transforming";
        public const string LCImporting = "importing";
        public const string LCCompleted = "completed";
        public const string LCFailed = "failed";
    }
}
