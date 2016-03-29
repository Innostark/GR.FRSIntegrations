using Gf.Frs.IntegrationCommon.DataModel;
using System;

namespace Gf.Frs.MT940Loader.DataModel.Mappers
{
    public static class DBLoadStatusToRefLoadStatus
    {
        public static RefDataLoadStatus MapToLoader(LoadStatus dbLoadStatus, out RefDataLoadStatus refLoadStatus)
        {
            if (dbLoadStatus == null)
                throw new ArgumentNullException("dbLoadStatus", "A valid LoadStatus object should be passed as first argument - 'dbLoadStatus'. The object was null.");

            refLoadStatus = new RefDataLoadStatus()
            {
                Name = dbLoadStatus.Name,
                Value = dbLoadStatus.Value,
                StatusId = dbLoadStatus.StatusId
            };

            return refLoadStatus;
        }
    }
}
