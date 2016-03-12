using System;
using System.Threading;

namespace Gf.Frs.LoaderWcfServices.Async
{
    public class AsyncResponse : IAsyncResult, IDisposable
    {
        private AsyncCallback _callback;
        private object _state;
        private ManualResetEvent _manualResetEvent;

        public AsyncResponse(AsyncCallback callback, object state)
        {
            _callback = callback;
            _state = state;
            _manualResetEvent = new ManualResetEvent(false);
        }

        public bool IsCompleted
        {
            get { return _manualResetEvent.WaitOne(0, false); }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _manualResetEvent; }
        }

        public object AsyncState
        {
            get { return _state; }
        }
        public ManualResetEvent AsyncWait
        {
            get { return _manualResetEvent; }

        }
        public bool CompletedSynchronously
        {
            get { return false; }
        }
        public void Completed()
        {
            _manualResetEvent.Set();
            if (_callback != null)
                _callback(this);
        }
        public void Dispose()
        {
            _manualResetEvent.Close();
            _manualResetEvent = null;
            _state = null;
            _callback = null;
        }
    }
}
