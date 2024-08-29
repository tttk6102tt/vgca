using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Utilities.IO;

namespace Sign.Org.BouncyCastle.Cms
{
    internal class DigOutputStream : BaseOutputStream
    {
        private readonly IDigest dig;

        internal DigOutputStream(IDigest dig)
        {
            this.dig = dig;
        }

        public override void WriteByte(byte b)
        {
            dig.Update(b);
        }

        public override void Write(byte[] b, int off, int len)
        {
            dig.BlockUpdate(b, off, len);
        }
    }
}
