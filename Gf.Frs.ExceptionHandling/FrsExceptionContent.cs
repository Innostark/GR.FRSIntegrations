
namespace Gf.Frs.ExceptionHandling
{
    /// <summary>
    /// Cares Exception Contents
    /// </summary>
    public sealed class FRSExceptionContent
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// FRS Exception Type
        /// </summary>
        public string ExceptionType { get { return FrsExceptionTypes.FRSGeneralException; }}
    }
}
