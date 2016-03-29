namespace Gf.Frs.IntegrationCommon.DataModel
{
    public class RefDataLoadStatus
    {
        public byte Value { get; set; }
        public string Name { get; set; }
        public byte StatusId { get; set; }

        public const string LSCreated = "created";
        public const string LSSubmitted = "submitted";
        public const string LSParsing = "parsing";
        public const string LSTransforming = "transforming";
        public const string LSImporting = "importing";
        public const string LSCompleted = "completed";
        public const string LSFailed = "failed";
    }
}
