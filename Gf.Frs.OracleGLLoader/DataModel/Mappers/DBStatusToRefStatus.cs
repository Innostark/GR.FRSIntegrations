using System;

namespace Gf.Frs.OracleGLLoader.DataModel.Mappers
{
    class DBStatusToRefStatus
    {
        internal static RefDataStatus MapToLoader(Status dbStatus, out RefDataStatus refStatus)
        {
            if (dbStatus == null)
                throw new ArgumentNullException("dbStatus", "A valid Status object should be passed as first argument - 'dbStatus'. The object was null.");

            refStatus = new RefDataStatus()
            {
                Name = dbStatus.Name,
                Value = dbStatus.Value
            };

            return refStatus;
        }
    }
}
