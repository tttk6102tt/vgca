namespace Sign.itext.xml.xmp.impl
{
    public class PushbackReader : FilterReader
    {
        private char[] _buf;

        private int _pos;

        public PushbackReader(TextReader inp, int size)
            : base(inp)
        {
            if (size <= 0)
            {
                throw new ArgumentException("size <= 0");
            }

            _buf = new char[size];
            _pos = size;
        }

        public PushbackReader(TextReader inp)
            : this(inp, 1)
        {
        }

        private void EnsureOpen()
        {
            if (_buf == null)
            {
                throw new IOException("Stream closed");
            }
        }

        public override int Read()
        {
            EnsureOpen();
            if (_pos < _buf.Length)
            {
                return _buf[_pos++];
            }

            return base.Read();
        }

        public override int Read(char[] cbuf, int off, int len)
        {
            EnsureOpen();
            try
            {
                if (len <= 0)
                {
                    if (len < 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    if (off < 0 || off > cbuf.Length)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    return 0;
                }

                int num = _buf.Length - _pos;
                if (num > 0)
                {
                    if (len < num)
                    {
                        num = len;
                    }

                    Array.Copy(_buf, _pos, cbuf, off, num);
                    _pos += num;
                    off += num;
                    len -= num;
                }

                if (len > 0)
                {
                    len = base.Read(cbuf, off, len);
                    if (len == -1)
                    {
                        return (num == 0) ? (-1) : num;
                    }

                    return num + len;
                }

                return num;
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public virtual void Unread(int c)
        {
            EnsureOpen();
            if (_pos == 0)
            {
                throw new IOException("Pushback buffer overflow");
            }

            _buf[--_pos] = (char)c;
        }

        public virtual void Unread(char[] cbuf, int off, int len)
        {
            EnsureOpen();
            if (len > _pos)
            {
                throw new IOException("Pushback buffer overflow");
            }

            _pos -= len;
            Array.Copy(cbuf, off, _buf, _pos, len);
        }

        public virtual void Unread(char[] cbuf)
        {
            Unread(cbuf, 0, cbuf.Length);
        }

        public override void Close()
        {
            base.Close();
            _buf = null;
        }
    }
}
