using Gf.Frs.MT940Loader.DataModel;
using System;

namespace Gf.Frs.MT940Loader.Handlers
{
    internal class MT940DatabaseHandler: IDisposable
    {
        private FRSMT940LoaderContext _dbContext;
        private bool _disposed = false;

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

        public MT940DatabaseHandler()
        {
            _dbContext = new FRSMT940LoaderContext();
        }

        public Load GetLoadById(long loadId)
        {
            return _dbContext.Loads.Find(loadId);
        }

        public Load GetLoadById(FRSMT940LoaderContext context, long loadId)
        {
            return context.Loads.Find(loadId);
        }

        public LoadMetaData GetLoadMetadataById(short id)
        {
            return _dbContext.LoadMetaDatas.Find(id);
        }

        public MT940Load GetMT940LoadById(long id)
        {
            return _dbContext.MT940Load.Find(id);
        }

        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    _dbContext.Dispose();
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.


                // Note disposing has been done.
                _disposed = true;

            }
        }
    }
}
