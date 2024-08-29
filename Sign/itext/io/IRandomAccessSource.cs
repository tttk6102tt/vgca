namespace Sign.itext.io
{
    public interface IRandomAccessSource : IDisposable
    {
        long Length { get; }

        int Get(long position);

        int Get(long position, byte[] bytes, int off, int len);

        void Close();
    }
}
