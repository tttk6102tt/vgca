using System.Runtime.Serialization;

namespace Sign.itext.text.exceptions
{
    [Serializable]
    public class BadPasswordException : IOException
    {
        public BadPasswordException(string message)
            : base(message)
        {
        }

        protected BadPasswordException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
