namespace Sign.itext.xml.xmp
{
    public class XmpException : Exception
    {
        private readonly int _errorCode;

        public virtual int ErrorCode => _errorCode;

        public XmpException(string message, int errorCode)
            : base(message)
        {
            _errorCode = errorCode;
        }

        public XmpException(string message, int errorCode, Exception t)
            : base(message, t)
        {
            _errorCode = errorCode;
        }
    }
}
