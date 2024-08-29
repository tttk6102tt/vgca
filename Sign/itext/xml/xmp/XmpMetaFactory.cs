using Sign.itext.xml.xmp.impl;
using Sign.itext.xml.xmp.options;
using System.Runtime.CompilerServices;

namespace Sign.itext.xml.xmp
{
    public static class XmpMetaFactory
    {
        private class XmpVersionInfoImpl : IXmpVersionInfo
        {
            private const int major = 5;

            private const int minor = 1;

            private const int micro = 0;

            private const int engBuild = 3;

            private const bool debug = false;

            private const string message = "Adobe XMP Core 5.1.0-jc003";

            public virtual int Major => 5;

            public virtual int Minor => 1;

            public virtual int Micro => 0;

            public virtual bool Debug => false;

            public virtual int Build => 3;

            public virtual string Message => "Adobe XMP Core 5.1.0-jc003";

            public override string ToString()
            {
                return "Adobe XMP Core 5.1.0-jc003";
            }
        }

        private static IXmpSchemaRegistry _schema = new XmpSchemaRegistryImpl();

        private static IXmpVersionInfo _versionInfo;

        public static IXmpSchemaRegistry SchemaRegistry => _schema;

        public static IXmpMeta Create()
        {
            return new XmpMetaImpl();
        }

        public static IXmpMeta Parse(Stream @in)
        {
            return Parse(@in, null);
        }

        public static IXmpMeta Parse(Stream @in, ParseOptions options)
        {
            return XmpMetaParser.Parse(@in, options);
        }

        public static IXmpMeta ParseFromString(string packet)
        {
            return ParseFromString(packet, null);
        }

        public static IXmpMeta ParseFromString(string packet, ParseOptions options)
        {
            return XmpMetaParser.Parse(packet, options);
        }

        public static IXmpMeta ParseFromBuffer(byte[] buffer)
        {
            return ParseFromBuffer(buffer, null);
        }

        public static IXmpMeta ParseFromBuffer(byte[] buffer, ParseOptions options)
        {
            return XmpMetaParser.Parse(buffer, options);
        }

        public static void Serialize(IXmpMeta xmp, Stream @out)
        {
            Serialize(xmp, @out, null);
        }

        public static void Serialize(IXmpMeta xmp, Stream @out, SerializeOptions options)
        {
            AssertImplementation(xmp);
            XmpSerializerHelper.Serialize((XmpMetaImpl)xmp, @out, options);
        }

        public static byte[] SerializeToBuffer(IXmpMeta xmp, SerializeOptions options)
        {
            AssertImplementation(xmp);
            return XmpSerializerHelper.SerializeToBuffer((XmpMetaImpl)xmp, options);
        }

        public static string SerializeToString(IXmpMeta xmp, SerializeOptions options)
        {
            AssertImplementation(xmp);
            return XmpSerializerHelper.SerializeToString((XmpMetaImpl)xmp, options);
        }

        private static void AssertImplementation(IXmpMeta xmp)
        {
            if (!(xmp is XmpMetaImpl))
            {
                throw new NotSupportedException("The serializing service works onlywith the XMPMeta implementation of this library");
            }
        }

        public static void Reset()
        {
            _schema = new XmpSchemaRegistryImpl();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IXmpVersionInfo GetVersionInfo()
        {
            if (_versionInfo == null)
            {
                try
                {
                    _versionInfo = new XmpVersionInfoImpl();
                }
                catch (Exception value)
                {
                    Console.WriteLine(value);
                }
            }

            return _versionInfo;
        }
    }
}
