namespace Sign.itext.io
{
    public class RASInputStream : Stream
    {
        private readonly IRandomAccessSource source;

        private long position;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => source.Length;

        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public RASInputStream(IRandomAccessSource source)
        {
            this.source = source;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int len)
        {
            int num = source.Get(position, buffer, offset, len);
            if (num == -1)
            {
                return 0;
            }

            position += num;
            return num;
        }

        public override int ReadByte()
        {
            int num = source.Get(position);
            if (num >= 0)
            {
                position++;
            }

            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position += offset;
                    break;
                default:
                    position = offset + source.Length;
                    break;
            }

            return position;
        }

        public override void SetLength(long value)
        {
            throw new Exception("Not supported.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new Exception("Not supported.");
        }
    }
}
