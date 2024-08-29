namespace Sign.Org.BouncyCastle.Cms
{
    public interface CmsProcessable
    {
        void Write(Stream outStream);

        [Obsolete]
        object GetContent();
    }
}
