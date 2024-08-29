using Sign.itext.xml.xmp.impl;
using Sign.itext.xml.xmp.impl.xpath;

namespace Sign.itext.xml.xmp
{
    public static class XmpPathFactory
    {
        public static string ComposeArrayItemPath(string arrayName, int itemIndex)
        {
            if (itemIndex > 0)
            {
                return arrayName + "[" + itemIndex + "]";
            }

            if (itemIndex == -1)
            {
                return arrayName + "[last()]";
            }

            throw new XmpException("Array index must be larger than zero", 104);
        }

        public static string ComposeStructFieldPath(string fieldNs, string fieldName)
        {
            AssertFieldNs(fieldNs);
            AssertFieldName(fieldName);
            XmpPath xmpPath = XmpPathParser.ExpandXPath(fieldNs, fieldName);
            if (xmpPath.Size() != 2)
            {
                throw new XmpException("The field name must be simple", 102);
            }

            return "/" + xmpPath.GetSegment(1).Name;
        }

        public static string ComposeQualifierPath(string qualNs, string qualName)
        {
            AssertQualNs(qualNs);
            AssertQualName(qualName);
            XmpPath xmpPath = XmpPathParser.ExpandXPath(qualNs, qualName);
            if (xmpPath.Size() != 2)
            {
                throw new XmpException("The qualifier name must be simple", 102);
            }

            return "/?" + xmpPath.GetSegment(1).Name;
        }

        public static string ComposeLangSelector(string arrayName, string langName)
        {
            return arrayName + "[?xml:lang=\"" + Utils.NormalizeLangValue(langName) + "\"]";
        }

        public static string ComposeFieldSelector(string arrayName, string fieldNs, string fieldName, string fieldValue)
        {
            XmpPath xmpPath = XmpPathParser.ExpandXPath(fieldNs, fieldName);
            if (xmpPath.Size() != 2)
            {
                throw new XmpException("The fieldName name must be simple", 102);
            }

            return arrayName + "[" + xmpPath.GetSegment(1).Name + "=\"" + fieldValue + "\"]";
        }

        private static void AssertQualNs(string qualNs)
        {
            if (string.IsNullOrEmpty(qualNs))
            {
                throw new XmpException("Empty qualifier namespace URI", 101);
            }
        }

        private static void AssertQualName(string qualName)
        {
            if (string.IsNullOrEmpty(qualName))
            {
                throw new XmpException("Empty qualifier name", 102);
            }
        }

        private static void AssertFieldNs(string fieldNs)
        {
            if (string.IsNullOrEmpty(fieldNs))
            {
                throw new XmpException("Empty field namespace URI", 101);
            }
        }

        private static void AssertFieldName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new XmpException("Empty f name", 102);
            }
        }
    }
}
