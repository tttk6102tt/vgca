using Sign.itext.xml.xmp.options;
using System.Text;
using System.Xml;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpMetaParser
    {
        private static readonly object XmpRdf = new object();

        private XmpMetaParser()
        {
        }

        public static IXmpMeta Parse(object input, ParseOptions options)
        {
            ParameterAsserts.AssertNotNull(input);
            options = options ?? new ParseOptions();
            XmlDocument root = ParseXml(input, options);
            bool requireXmpMeta = options.RequireXmpMeta;
            object[] result = new object[3];
            result = FindRootNode(root, requireXmpMeta, result);
            if (result != null && result[1] == XmpRdf)
            {
                XmpMetaImpl xmpMetaImpl = ParseRdf.Parse((XmlNode)result[0]);
                xmpMetaImpl.PacketHeader = (string)result[2];
                if (!options.OmitNormalization)
                {
                    return XmpNormalizer.Process(xmpMetaImpl, options);
                }

                return xmpMetaImpl;
            }

            return new XmpMetaImpl();
        }

        private static XmlDocument ParseXml(object input, ParseOptions options)
        {
            if (input is Stream)
            {
                return ParseXmlFromInputStream((Stream)input, options);
            }

            if (input is byte[])
            {
                return ParseXmlFromBytebuffer(new ByteBuffer((byte[])input), options);
            }

            return ParseXmlFromString((string)input, options);
        }

        private static XmlDocument ParseXmlFromInputStream(Stream stream, ParseOptions options)
        {
            if (!options.AcceptLatin1 && !options.FixControlChars)
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(GetSecureXmlReader(stream));
                return xmlDocument;
            }

            try
            {
                return ParseXmlFromBytebuffer(new ByteBuffer(stream), options);
            }
            catch (IOException t)
            {
                throw new XmpException("Error reading the XML-file", 204, t);
            }
        }

        private static XmlDocument ParseXmlFromBytebuffer(ByteBuffer buffer, ParseOptions options)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(GetSecureXmlReader(buffer.ByteStream));
                return xmlDocument;
            }
            catch (XmlException t)
            {
                XmlDocument xmlDocument2 = new XmlDocument();
                if (options.AcceptLatin1)
                {
                    buffer = Latin1Converter.Convert(buffer);
                }

                if (options.FixControlChars)
                {
                    try
                    {
                        FixAsciiControlsReader textReader = new FixAsciiControlsReader(new StreamReader(buffer.ByteStream, Encoding.GetEncoding(buffer.Encoding)));
                        xmlDocument2.Load(GetSecureXmlReader(textReader));
                        return xmlDocument2;
                    }
                    catch (Exception)
                    {
                        throw new XmpException("Unsupported Encoding", 9, t);
                    }
                }

                xmlDocument2.Load(buffer.ByteStream);
                return xmlDocument2;
            }
        }

        private static XmlDocument ParseXmlFromString(string input, ParseOptions options)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(GetSecureXmlReader(input));
                return xmlDocument;
            }
            catch (XmpException ex)
            {
                if (ex.ErrorCode == 201 && options.FixControlChars)
                {
                    XmlDocument xmlDocument2 = new XmlDocument();
                    xmlDocument2.Load(GetSecureXmlReader(new FixAsciiControlsReader(new StringReader(input))));
                    return xmlDocument2;
                }

                throw ex;
            }
        }

        private static object[] FindRootNode(XmlNode root, bool xmpmetaRequired, object[] result)
        {
            XmlNodeList childNodes = root.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                root = childNodes[i];
                if (XmlNodeType.ProcessingInstruction == root.NodeType && "xpacket".Equals(((XmlProcessingInstruction)root).Target))
                {
                    if (result != null)
                    {
                        result[2] = ((XmlProcessingInstruction)root).Data;
                    }
                }
                else
                {
                    if (XmlNodeType.Text == root.NodeType || XmlNodeType.ProcessingInstruction == root.NodeType)
                    {
                        continue;
                    }

                    string namespaceURI = root.NamespaceURI;
                    string localName = root.LocalName;
                    if (("xmpmeta".Equals(localName) || "xapmeta".Equals(localName)) && "adobe:ns:meta/".Equals(namespaceURI))
                    {
                        return FindRootNode(root, xmpmetaRequired: false, result);
                    }

                    if (!xmpmetaRequired && "RDF".Equals(localName) && "http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(namespaceURI))
                    {
                        if (result != null)
                        {
                            result[0] = root;
                            result[1] = XmpRdf;
                        }

                        return result;
                    }

                    object[] array = FindRootNode(root, xmpmetaRequired, result);
                    if (array != null)
                    {
                        return array;
                    }
                }
            }

            return null;
        }

        private static XmlReaderSettings GetSecureReaderSettings()
        {
            return new XmlReaderSettings
            {
                ProhibitDtd = true
            };
        }

        private static XmlReader GetSecureXmlReader(Stream stream)
        {
            return XmlReader.Create(stream, GetSecureReaderSettings());
        }

        private static XmlReader GetSecureXmlReader(TextReader textReader)
        {
            return XmlReader.Create(textReader, GetSecureReaderSettings());
        }

        private static XmlReader GetSecureXmlReader(string str)
        {
            return XmlReader.Create(new StringReader(str), GetSecureReaderSettings());
        }
    }
}
