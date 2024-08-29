using Sign.itext.xml.xmp.impl;
using Sign.itext.xml.xmp.options;

namespace Sign.itext.xml.xmp
{
    public class XmpBasicProperties
    {
        public static readonly string ADVISORY = "Advisory";

        public static readonly string BASEURL = "BaseURL";

        public static readonly string CREATEDATE = "CreateDate";

        public static readonly string CREATORTOOL = "CreatorTool";

        public static readonly string IDENTIFIER = "Identifier";

        public static readonly string METADATADATE = "MetadataDate";

        public static readonly string MODIFYDATE = "ModifyDate";

        public static readonly string NICKNAME = "Nickname";

        public static readonly string THUMBNAILS = "Thumbnails";

        public static void SetCreatorTool(IXmpMeta xmpMeta, string creator)
        {
            xmpMeta.SetProperty("http://ns.adobe.com/xap/1.0/", CREATORTOOL, creator);
        }

        public static void SetCreateDate(IXmpMeta xmpMeta, string date)
        {
            xmpMeta.SetProperty("http://ns.adobe.com/xap/1.0/", CREATEDATE, date);
        }

        public static void SetModDate(IXmpMeta xmpMeta, string date)
        {
            xmpMeta.SetProperty("http://ns.adobe.com/xap/1.0/", MODIFYDATE, date);
        }

        public static void SetMetaDataDate(IXmpMeta xmpMeta, string date)
        {
            xmpMeta.SetProperty("http://ns.adobe.com/xap/1.0/", METADATADATE, date);
        }

        public static void SetIdentifiers(IXmpMeta xmpMeta, string[] id)
        {
            XmpUtils.RemoveProperties(xmpMeta, "http://purl.org/dc/elements/1.1/", IDENTIFIER, doAllProperties: true, includeAliases: true);
            for (int i = 0; i < id.Length; i++)
            {
                xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", IDENTIFIER, new PropertyOptions(512u), id[i], null);
            }
        }

        public static void SetNickname(IXmpMeta xmpMeta, string name)
        {
            xmpMeta.SetProperty("http://ns.adobe.com/xap/1.0/", NICKNAME, name);
        }
    }
}
