﻿using Sign.Org.BouncyCastle.Asn1.pkcs;
using Sign.Org.BouncyCastle.Asn1.Utilities;
using Sign.Org.BouncyCastle.Utilities.IO;

namespace Sign.Org.BouncyCastle.Cms
{
    public class CmsTypedStream
    {
        private class FullReaderStream : FilterStream
        {
            internal FullReaderStream(Stream input)
                : base(input)
            {
            }

            public override int Read(byte[] buf, int off, int len)
            {
                return Streams.ReadFully(s, buf, off, len);
            }
        }

        private const int BufferSize = 32768;

        private readonly string _oid;

        private readonly Stream _in;

        public string ContentType => _oid;

        public Stream ContentStream => _in;

        public CmsTypedStream(Stream inStream)
            : this(PkcsObjectIdentifiers.Data.Id, inStream, 32768)
        {
        }

        public CmsTypedStream(string oid, Stream inStream)
            : this(oid, inStream, 32768)
        {
        }

        public CmsTypedStream(string oid, Stream inStream, int bufSize)
        {
            _oid = oid;
            _in = new FullReaderStream(new BufferedStream(inStream, bufSize));
        }

        public void Drain()
        {
            Streams.Drain(_in);
            _in.Close();
        }
    }
}
