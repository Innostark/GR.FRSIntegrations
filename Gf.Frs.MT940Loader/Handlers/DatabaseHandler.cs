using Gf.Frs.MT940Loader.DataModels;

namespace Gf.Frs.MT940Loader.Handlers
{
    internal class DatabaseHandler
    {
        private FRSMT940LoaderContext _dbContext;

        internal FRSMT940LoaderContext DbContext
        {
            get
            {
                return _dbContext;
            }

            set
            {
                _dbContext = value;
            }
        }

        public DatabaseHandler()
        {
            _dbContext = new FRSMT940LoaderContext();
        }

        public Load GetLoadById(long loadId)
        {
            return _dbContext.Loads.Find(loadId);
        }

        public LoadMetaData GetLoadMetadataById(short id)
        {
            return _dbContext.LoadMetaDatas.Find(id);
        }

        public MT940Load GetMT940LoadById(long id)
        {
            return _dbContext.MT940Load.Find(id);
        }

        public void PreDispose()
        {
            _dbContext.Dispose();
            _dbContext = null;
        }
    }
}
