using System;

namespace Gf.Frs.ExceptionHandling
{
    /// <summary>
    /// Cares Exception
    /// </summary>
    [Serializable]
    public sealed class FrsException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of FRS Exception
        /// </summary>
        public FrsException(string message): base(message)
        {            
        }
        /// <summary>
        /// Initializes a new instance of FRS Exception
        /// </summary>
        public FrsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
