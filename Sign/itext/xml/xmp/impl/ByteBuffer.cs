namespace Sign.itext.xml.xmp.impl
{
    public class ByteBuffer
    {
        private byte[] _buffer;

        private string _encoding;

        private int _length;

        public virtual Stream ByteStream => new MemoryStream(_buffer, 0, _length);

        public virtual int Length => _length;

        public virtual string Encoding
        {
            get
            {
                if (_encoding == null)
                {
                    if (_length < 2)
                    {
                        _encoding = "UTF-8";
                    }
                    else if (_buffer[0] == 0)
                    {
                        if (_length < 4 || _buffer[1] != 0)
                        {
                            _encoding = "UTF-16BE";
                        }
                        else if ((_buffer[2] & 0xFF) == 254 && (_buffer[3] & 0xFF) == 255)
                        {
                            _encoding = "UTF-32BE";
                        }
                        else
                        {
                            _encoding = "UTF-32";
                        }
                    }
                    else if ((_buffer[0] & 0xFF) < 128)
                    {
                        if (_buffer[1] != 0)
                        {
                            _encoding = "UTF-8";
                        }
                        else if (_length < 4 || _buffer[2] != 0)
                        {
                            _encoding = "UTF-16LE";
                        }
                        else
                        {
                            _encoding = "UTF-32LE";
                        }
                    }
                    else if ((_buffer[0] & 0xFF) == 239)
                    {
                        _encoding = "UTF-8";
                    }
                    else if ((_buffer[0] & 0xFF) == 254)
                    {
                        _encoding = "UTF-16";
                    }
                    else if (_length < 4 || _buffer[2] != 0)
                    {
                        _encoding = "UTF-16";
                    }
                    else
                    {
                        _encoding = "UTF-32";
                    }
                }

                return _encoding;
            }
        }

        public ByteBuffer(int initialCapacity)
        {
            _buffer = new byte[initialCapacity];
            _length = 0;
        }

        public ByteBuffer(byte[] buffer)
        {
            _buffer = buffer;
            _length = buffer.Length;
        }

        public ByteBuffer(byte[] buffer, int length)
        {
            if (length > buffer.Length)
            {
                throw new IndexOutOfRangeException("Valid length exceeds the buffer length.");
            }

            _buffer = buffer;
            _length = length;
        }

        public ByteBuffer(Stream inp)
        {
            _length = 0;
            _buffer = new byte[16384];
            int num;
            while ((num = inp.Read(_buffer, _length, 16384)) > 0)
            {
                _length += num;
                if (num == 16384)
                {
                    EnsureCapacity(_length + 16384);
                    continue;
                }

                break;
            }
        }

        public ByteBuffer(byte[] buffer, int offset, int length)
        {
            if (length > buffer.Length - offset)
            {
                throw new IndexOutOfRangeException("Valid length exceeds the buffer length.");
            }

            _buffer = new byte[length];
            Array.Copy(buffer, offset, _buffer, 0, length);
            _length = length;
        }

        public virtual byte ByteAt(int index)
        {
            if (index < _length)
            {
                return _buffer[index];
            }

            throw new IndexOutOfRangeException("The index exceeds the valid buffer area");
        }

        public virtual int CharAt(int index)
        {
            if (index < _length)
            {
                return _buffer[index] & 0xFF;
            }

            throw new IndexOutOfRangeException("The index exceeds the valid buffer area");
        }

        public virtual void Append(byte b)
        {
            EnsureCapacity(_length + 1);
            _buffer[_length++] = b;
        }

        public virtual void Append(byte[] bytes, int offset, int len)
        {
            EnsureCapacity(_length + len);
            Array.Copy(bytes, offset, _buffer, _length, len);
            _length += len;
        }

        public virtual void Append(byte[] bytes)
        {
            Append(bytes, 0, bytes.Length);
        }

        public virtual void Append(ByteBuffer anotherBuffer)
        {
            Append(anotherBuffer._buffer, 0, anotherBuffer._length);
        }

        private void EnsureCapacity(int requestedLength)
        {
            if (requestedLength > _buffer.Length)
            {
                byte[] buffer = _buffer;
                _buffer = new byte[buffer.Length * 2];
                Array.Copy(buffer, 0, _buffer, 0, buffer.Length);
            }
        }
    }
}
