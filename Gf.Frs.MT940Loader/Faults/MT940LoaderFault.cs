namespace Gf.Frs.MT940Loader.Faults
{
    public class MT940LoaderFault
    {
        private int _code;
        private string _message;

        public int Code
        {
            get
            {
                return _code;
            }

            set
            {
                _code = value;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                _message = value;
            }
        }

        public MT940LoaderFault(int code, string message)
        {
            _code = code;
            _message = message;
        }
    }
}
