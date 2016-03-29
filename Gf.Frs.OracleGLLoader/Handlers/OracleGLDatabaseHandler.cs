using Gf.Frs.OracleGLLoader.DataModel;

namespace Gf.Frs.OracleGLLoader.Handlers
{
    internal class OracleGLDatabaseHandler
    {
        private FrsOracleGLLoaderContext _dbContext;

        public FrsOracleGLLoaderContext DbContext
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

        public OracleGLDatabaseHandler()
        {
            base.DbContext = new FrsOracleGLLoaderContext();
        }

        public Load GetLoadById(long loadId)
        {
            return _dbContext.Loads.Find(loadId);
        }

        public Load GetLoadById(FrsOracleGLLoaderContext context, long loadId)
        {
            return context.Loads.Find(loadId);
        }

        public LoadMetaData GetLoadMetadataById(short id)
        {
            return _dbContext.LoadMetaDatas.Find(id);
        }

        public OracleGLLoad GetOracleGLLoadById(long id)
        {
            return _dbContext.OracleGLLoads.Find(id);
        }

        public void PreDispose()
        {
            DbContext.Dispose();
            DbContext = null;
        }
    }
}
