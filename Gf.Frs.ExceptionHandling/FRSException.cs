using System;

namespace Gf.Frs.ExceptionHandling
{
    /// <summary>
    /// Cares Exception
    /// </summary>
    public sealed class FRSException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of FRS Exception
        /// </summary>
        public FRSException(string message): base(message)
        {            
        }
        /// <summary>
        /// Initializes a new instance of FRS Exception
        /// </summary>
        public FRSException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
