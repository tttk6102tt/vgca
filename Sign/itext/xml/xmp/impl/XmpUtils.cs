using Sign.itext.xml.xmp.options;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpUtils
    {
        private XmpUtils()
        {
        }

        public static string CatenateArrayItems(IXmpMeta xmp, string schemaNs, string arrayName, string separator, string quotes, bool allowCommas)
        {
            return XmpUtilsImpl.CatenateArrayItems(xmp, schemaNs, arrayName, separator, quotes, allowCommas);
        }

        public static void SeparateArrayItems(IXmpMeta xmp, string schemaNs, string arrayName, string catedStr, PropertyOptions arrayOptions, bool preserveCommas)
        {
            XmpUtilsImpl.SeparateArrayItems(xmp, schemaNs, arrayName, catedStr, arrayOptions, preserveCommas);
        }

        public static void RemoveProperties(IXmpMeta xmp, string schemaNs, string propName, bool doAllProperties, bool includeAliases)
        {
            XmpUtilsImpl.RemoveProperties(xmp, schemaNs, propName, doAllProperties, includeAliases);
        }

        public static void AppendProperties(IXmpMeta source, IXmpMeta dest, bool doAllProperties, bool replaceOldValues)
        {
            AppendProperties(source, dest, doAllProperties, replaceOldValues, deleteEmptyValues: false);
        }

        public static void AppendProperties(IXmpMeta source, IXmpMeta dest, bool doAllProperties, bool replaceOldValues, bool deleteEmptyValues)
        {
            XmpUtilsImpl.AppendProperties(source, dest, doAllProperties, replaceOldValues, deleteEmptyValues);
        }

        public static bool ConvertToBoolean(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new XmpException("Empty convert-string", 5);
            }

            value = value.ToLower();
            try
            {
                return Convert.ToInt32(value) != 0;
            }
            catch (FormatException)
            {
                return "true".Equals(value) || "t".Equals(value) || "on".Equals(value) || "yes".Equals(value);
            }
        }

        public static string ConvertFromBoolean(bool value)
        {
            if (!value)
            {
                return "False";
            }

            return "True";
        }

        public static int ConvertToInteger(string rawValue)
        {
            try
            {
                if (string.IsNullOrEmpty(rawValue))
                {
                    throw new XmpException("Empty convert-string", 5);
                }

                if (rawValue.StartsWith("0x"))
                {
                    return Convert.ToInt32(rawValue.Substring(2), 16);
                }

                return Convert.ToInt32(rawValue);
            }
            catch (FormatException)
            {
                throw new XmpException("Invalid integer string", 5);
            }
        }

        public static string ConvertFromInteger(int value)
        {
            return Convert.ToString(value);
        }

        public static long ConvertToLong(string rawValue)
        {
            try
            {
                if (string.IsNullOrEmpty(rawValue))
                {
                    throw new XmpException("Empty convert-string", 5);
                }

                if (rawValue.StartsWith("0x"))
                {
                    return Convert.ToInt64(rawValue.Substring(2), 16);
                }

                return Convert.ToInt64(rawValue);
            }
            catch (FormatException)
            {
                throw new XmpException("Invalid long string", 5);
            }
        }

        public static string ConvertFromLong(long value)
        {
            return Convert.ToString(value);
        }

        public static double ConvertToDouble(string rawValue)
        {
            try
            {
                if (string.IsNullOrEmpty(rawValue))
                {
                    throw new XmpException("Empty convert-string", 5);
                }

                return Convert.ToDouble(rawValue);
            }
            catch (FormatException)
            {
                throw new XmpException("Invalid double string", 5);
            }
        }

        public static string ConvertFromDouble(double value)
        {
            return Convert.ToString(value);
        }

        public static IXmpDateTime ConvertToDate(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                throw new XmpException("Empty convert-string", 5);
            }

            return Iso8601Converter.Parse(rawValue);
        }

        public static string ConvertFromDate(IXmpDateTime value)
        {
            return Iso8601Converter.Render(value);
        }

        public static string EncodeBase64(byte[] buffer)
        {
            return GetString(Base64.Encode(buffer));
        }

        public static byte[] DecodeBase64(string base64String)
        {
            try
            {
                return Base64.Decode(GetBytes(base64String));
            }
            catch (Exception t)
            {
                throw new XmpException("Invalid base64 string", 5, t);
            }
        }

        private static byte[] GetBytes(string str)
        {
            byte[] array = new byte[str.Length * 2];
            Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
            return array;
        }

        private static string GetString(byte[] bytes)
        {
            char[] array = new char[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
            return new string(array);
        }
    }
}
