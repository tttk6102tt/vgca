using Sign.itext.xml.xmp.impl;
using Sign.itext.xml.xmp.options;

namespace Sign.itext.xml.xmp
{
    public class DublinCoreProperties
    {
        public static readonly string CONTRIBUTOR = "contributor";

        public static readonly string COVERAGE = "coverage";

        public static readonly string CREATOR = "creator";

        public static readonly string DATE = "date";

        public static readonly string DESCRIPTION = "description";

        public static readonly string FORMAT = "format";

        public static readonly string IDENTIFIER = "identifier";

        public static readonly string LANGUAGE = "language";

        public static readonly string PUBLISHER = "publisher";

        public static readonly string RELATION = "relation";

        public static readonly string RIGHTS = "rights";

        public static readonly string SOURCE = "source";

        public static readonly string SUBJECT = "subject";

        public static readonly string TITLE = "title";

        public static readonly string TYPE = "type";

        public static void AddTitle(IXmpMeta xmpMeta, string title)
        {
            xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", TITLE, new PropertyOptions(2048u), title, null);
        }

        public static void SetTitle(IXmpMeta xmpMeta, string title, string genericLang, string specificLang)
        {
            xmpMeta.SetLocalizedText("http://purl.org/dc/elements/1.1/", TITLE, genericLang, specificLang, title);
        }

        public static void AddDescription(IXmpMeta xmpMeta, string desc)
        {
            xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", DESCRIPTION, new PropertyOptions(2048u), desc, null);
        }

        public static void SetDescription(IXmpMeta xmpMeta, string desc, string genericLang, string specificLang)
        {
            xmpMeta.SetLocalizedText("http://purl.org/dc/elements/1.1/", DESCRIPTION, genericLang, specificLang, desc);
        }

        public static void AddSubject(IXmpMeta xmpMeta, string subject)
        {
            xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", SUBJECT, new PropertyOptions(512u), subject, null);
        }

        public static void SetSubject(IXmpMeta xmpMeta, string[] subject)
        {
            XmpUtils.RemoveProperties(xmpMeta, "http://purl.org/dc/elements/1.1/", SUBJECT, doAllProperties: true, includeAliases: true);
            for (int i = 0; i < subject.Length; i++)
            {
                xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", SUBJECT, new PropertyOptions(512u), subject[i], null);
            }
        }

        public static void AddAuthor(IXmpMeta xmpMeta, string author)
        {
            xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", CREATOR, new PropertyOptions(1024u), author, null);
        }

        public static void SetAuthor(IXmpMeta xmpMeta, string[] author)
        {
            XmpUtils.RemoveProperties(xmpMeta, "http://purl.org/dc/elements/1.1/", CREATOR, doAllProperties: true, includeAliases: true);
            for (int i = 0; i < author.Length; i++)
            {
                xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", CREATOR, new PropertyOptions(1024u), author[i], null);
            }
        }

        public static void AddPublisher(IXmpMeta xmpMeta, string publisher)
        {
            xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", PUBLISHER, new PropertyOptions(1024u), publisher, null);
        }

        public static void SetPublisher(IXmpMeta xmpMeta, string[] publisher)
        {
            XmpUtils.RemoveProperties(xmpMeta, "http://purl.org/dc/elements/1.1/", PUBLISHER, doAllProperties: true, includeAliases: true);
            for (int i = 0; i < publisher.Length; i++)
            {
                xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", PUBLISHER, new PropertyOptions(1024u), publisher[i], null);
            }
        }
    }
}
