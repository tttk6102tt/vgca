using Sign.itext.xml.simpleparser;
using Sign.itext.xml.xmp.options;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpSerializerHelper
    {
        public static void Serialize(XmpMetaImpl xmp, Stream @out, SerializeOptions options)
        {
            options = options ?? new SerializeOptions();
            if (options.Sort)
            {
                xmp.Sort();
            }

            new XmpSerializerRdf().Serialize(xmp, @out, options);
        }

        public static string SerializeToString(XmpMetaImpl xmp, SerializeOptions options)
        {
            options = options ?? new SerializeOptions();
            options.EncodeUtf16Be = true;
            MemoryStream memoryStream = new MemoryStream(2048);
            Serialize(xmp, memoryStream, options);
            try
            {
                return new EncodingNoPreamble(IanaEncodings.GetEncodingEncoding(options.Encoding)).GetString(memoryStream.GetBuffer());
            }
            catch (Exception)
            {
                return GetString(memoryStream.GetBuffer());
            }
        }

        public static byte[] SerializeToBuffer(XmpMetaImpl xmp, SerializeOptions options)
        {
            MemoryStream memoryStream = new MemoryStream(2048);
            Serialize(xmp, memoryStream, options);
            return memoryStream.GetBuffer();
        }

        private static string GetString(byte[] bytes)
        {
            char[] array = new char[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
            return new string(array);
        }
    }
}
