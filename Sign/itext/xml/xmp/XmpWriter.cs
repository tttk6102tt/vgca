using Sign.itext.pdf;
using Sign.itext.xml.xmp.impl;
using Sign.itext.xml.xmp.options;

namespace Sign.itext.xml.xmp
{
    public class XmpWriter
    {
        public static string UTF8 = "UTF-8";

        public static string UTF16 = "UTF-16";

        public static string UTF16BE = "UTF-16BE";

        public static string UTF16LE = "UTF-16LE";

        protected IXmpMeta xmpMeta;

        protected Stream outputStream;

        protected SerializeOptions serializeOptions;

        public virtual IXmpMeta XmpMeta => xmpMeta;

        public virtual bool ReadOnly
        {
            get
            {
                return serializeOptions.ReadOnlyPacket;
            }
            set
            {
                serializeOptions.ReadOnlyPacket = value;
            }
        }

        public virtual string About
        {
            get
            {
                return xmpMeta.ObjectName;
            }
            set
            {
                xmpMeta.ObjectName = value;
            }
        }

        public XmpWriter(Stream os, string utfEncoding, int extraSpace)
        {
            outputStream = os;
            serializeOptions = new SerializeOptions();
            if (UTF16BE.Equals(utfEncoding) || UTF16.Equals(utfEncoding))
            {
                serializeOptions.EncodeUtf16Be = true;
            }
            else if (UTF16LE.Equals(utfEncoding))
            {
                serializeOptions.EncodeUtf16Le = true;
            }

            serializeOptions.Padding = extraSpace;
            xmpMeta = XmpMetaFactory.Create();
            xmpMeta.ObjectName = "xmpmeta";
            xmpMeta.ObjectName = "";
            try
            {
                xmpMeta.SetProperty("http://purl.org/dc/elements/1.1/", DublinCoreProperties.FORMAT, "application/pdf");
                xmpMeta.SetProperty("http://ns.adobe.com/pdf/1.3/", PdfProperties.PRODUCER, Sign.itext.text.Version.GetInstance().GetVersion);
            }
            catch (XmpException)
            {
            }
        }

        public XmpWriter(Stream os)
            : this(os, UTF8, 2000)
        {
        }

        public XmpWriter(Stream os, PdfDictionary info)
            : this(os)
        {
            if (info == null)
            {
                return;
            }

            foreach (PdfName key in info.Keys)
            {
                PdfObject pdfObject = info.Get(key);
                if (pdfObject != null && pdfObject.IsString())
                {
                    string value = ((PdfString)pdfObject).ToUnicodeString();
                    try
                    {
                        AddDocInfoProperty(key, value);
                    }
                    catch (XmpException ex)
                    {
                        throw new IOException(ex.Message);
                    }
                }
            }
        }

        public XmpWriter(Stream os, IDictionary<string, string> info)
            : this(os)
        {
            if (info == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> item in info)
            {
                string key = item.Key;
                string value = item.Value;
                if (value != null)
                {
                    try
                    {
                        AddDocInfoProperty(key, value);
                    }
                    catch (XmpException ex)
                    {
                        throw new IOException(ex.Message);
                    }
                }
            }
        }

        [Obsolete]
        public virtual void AddRdfDescription(string xmlns, string content)
        {
            try
            {
                XmpUtils.AppendProperties(XmpMetaFactory.ParseFromString("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><rdf:Description rdf:about=\"" + xmpMeta.ObjectName + "\" " + xmlns + ">" + content + "</rdf:Description></rdf:RDF>\n"), xmpMeta, doAllProperties: true, replaceOldValues: true);
            }
            catch (XmpException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        [Obsolete]
        public virtual void AddRdfDescription(XmpSchema s)
        {
            try
            {
                XmpUtils.AppendProperties(XmpMetaFactory.ParseFromString("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><rdf:Description rdf:about=\"" + xmpMeta.ObjectName + "\" " + s.Xmlns + ">" + s.ToString() + "</rdf:Description></rdf:RDF>\n"), xmpMeta, doAllProperties: true, replaceOldValues: true);
            }
            catch (XmpException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        public virtual void SetProperty(string schemaNS, string propName, object value)
        {
            xmpMeta.SetProperty(schemaNS, propName, value);
        }

        public virtual void AppendArrayItem(string schemaNS, string arrayName, string value)
        {
            xmpMeta.AppendArrayItem(schemaNS, arrayName, new PropertyOptions(512u), value, null);
        }

        public virtual void AppendOrderedArrayItem(string schemaNS, string arrayName, string value)
        {
            xmpMeta.AppendArrayItem(schemaNS, arrayName, new PropertyOptions(1024u), value, null);
        }

        public virtual void AppendAlternateArrayItem(string schemaNS, string arrayName, string value)
        {
            xmpMeta.AppendArrayItem(schemaNS, arrayName, new PropertyOptions(2048u), value, null);
        }

        public virtual void Serialize(Stream externalOutputStream)
        {
            XmpMetaFactory.Serialize(xmpMeta, externalOutputStream, serializeOptions);
        }

        public virtual void Close()
        {
            if (outputStream != null)
            {
                try
                {
                    XmpMetaFactory.Serialize(xmpMeta, outputStream, serializeOptions);
                    outputStream = null;
                }
                catch (XmpException ex)
                {
                    throw new IOException(ex.Message);
                }
            }
        }

        public virtual void AddDocInfoProperty(object key, string value)
        {
            if (key is string)
            {
                key = new PdfName((string)key);
            }

            if (PdfName.TITLE.Equals(key))
            {
                xmpMeta.SetLocalizedText("http://purl.org/dc/elements/1.1/", DublinCoreProperties.TITLE, "x-default", "x-default", value);
            }
            else if (PdfName.AUTHOR.Equals(key))
            {
                xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", DublinCoreProperties.CREATOR, new PropertyOptions(1024u), value, null);
            }
            else if (PdfName.SUBJECT.Equals(key))
            {
                xmpMeta.SetLocalizedText("http://purl.org/dc/elements/1.1/", DublinCoreProperties.DESCRIPTION, "x-default", "x-default", value);
            }
            else if (PdfName.KEYWORDS.Equals(key))
            {
                string[] array = value.Split(',', ';');
                foreach (string text in array)
                {
                    if (text.Trim().Length > 0)
                    {
                        xmpMeta.AppendArrayItem("http://purl.org/dc/elements/1.1/", DublinCoreProperties.SUBJECT, new PropertyOptions(512u), text.Trim(), null);
                    }
                }

                xmpMeta.SetProperty("http://ns.adobe.com/pdf/1.3/", PdfProperties.KEYWORDS, value);
            }
            else if (PdfName.PRODUCER.Equals(key))
            {
                xmpMeta.SetProperty("http://ns.adobe.com/pdf/1.3/", PdfProperties.PRODUCER, value);
            }
            else if (PdfName.CREATOR.Equals(key))
            {
                xmpMeta.SetProperty("http://ns.adobe.com/xap/1.0/", XmpBasicProperties.CREATORTOOL, value);
            }
            else if (PdfName.CREATIONDATE.Equals(key))
            {
                xmpMeta.SetProperty("http://ns.adobe.com/xap/1.0/", XmpBasicProperties.CREATEDATE, PdfDate.GetW3CDate(value));
            }
            else if (PdfName.MODDATE.Equals(key))
            {
                xmpMeta.SetProperty("http://ns.adobe.com/xap/1.0/", XmpBasicProperties.MODIFYDATE, PdfDate.GetW3CDate(value));
            }
        }
    }
}
