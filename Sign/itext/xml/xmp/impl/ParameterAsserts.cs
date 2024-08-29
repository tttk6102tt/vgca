namespace Sign.itext.xml.xmp.impl
{
    internal class ParameterAsserts : XmpConst
    {
        private ParameterAsserts()
        {
        }

        public static void AssertArrayName(string arrayName)
        {
            if (string.IsNullOrEmpty(arrayName))
            {
                throw new XmpException("Empty array name", 4);
            }
        }

        public static void AssertPropName(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                throw new XmpException("Empty property name", 4);
            }
        }

        public static void AssertSchemaNs(string schemaNs)
        {
            if (string.IsNullOrEmpty(schemaNs))
            {
                throw new XmpException("Empty schema namespace URI", 4);
            }
        }

        public static void AssertPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw new XmpException("Empty prefix", 4);
            }
        }

        public static void AssertSpecificLang(string specificLang)
        {
            if (string.IsNullOrEmpty(specificLang))
            {
                throw new XmpException("Empty specific language", 4);
            }
        }

        public static void AssertStructName(string structName)
        {
            if (string.IsNullOrEmpty(structName))
            {
                throw new XmpException("Empty array name", 4);
            }
        }

        public static void AssertNotNull(object param)
        {
            if (param == null)
            {
                throw new XmpException("Parameter must not be null", 4);
            }

            if (param is string && ((string)param).Length == 0)
            {
                throw new XmpException("Parameter must not be null or empty", 4);
            }
        }

        public static void AssertImplementation(IXmpMeta xmp)
        {
            if (xmp == null)
            {
                throw new XmpException("Parameter must not be null", 4);
            }

            if (!(xmp is XmpMetaImpl))
            {
                throw new XmpException("The XMPMeta-object is not compatible with this implementation", 4);
            }
        }
    }
}
