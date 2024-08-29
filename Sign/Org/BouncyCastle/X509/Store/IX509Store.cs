using System.Collections;

namespace Sign.Org.BouncyCastle.X509.Store
{
    public interface IX509Store
    {
        ICollection GetMatches(IX509Selector selector);
    }
}
