using Sign.Org.BouncyCastle.Crypto;
using Sign.Org.BouncyCastle.Security;
using Sign.Org.BouncyCastle.Utilities.IO;

namespace Sign.Org.BouncyCastle.Cms
{
    internal class SigOutputStream : BaseOutputStream
    {
        private readonly ISigner sig;

        internal SigOutputStream(ISigner sig)
        {
            this.sig = sig;
        }

        public override void WriteByte(byte b)
        {
            try
            {
                sig.Update(b);
            }
            catch (SignatureException ex)
            {
                throw new CmsStreamException("signature problem: " + ex);
            }
        }

        public override void Write(byte[] b, int off, int len)
        {
            try
            {
                sig.BlockUpdate(b, off, len);
            }
            catch (SignatureException ex)
            {
                throw new CmsStreamException("signature problem: " + ex);
            }
        }
    }
}
