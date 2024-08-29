namespace Sign.Org.BouncyCastle.Cms
{
    public class CmsProcessableByteArray : CmsProcessable, CmsReadable
    {
        private readonly byte[] bytes;

        public CmsProcessableByteArray(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public Stream GetInputStream()
        {
            return new MemoryStream(bytes, writable: false);
        }

        public virtual void Write(Stream zOut)
        {
            zOut.Write(bytes, 0, bytes.Length);
        }

        public virtual object GetContent()
        {
            return bytes.Clone();
        }
    }
}
