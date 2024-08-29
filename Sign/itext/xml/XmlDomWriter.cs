using System.Text;
using System.Xml;

namespace Sign.itext.xml
{
    public class XmlDomWriter
    {
        protected TextWriter fOut;

        protected bool fCanonical;

        protected bool fXML11;

        public XmlDomWriter()
        {
        }

        public XmlDomWriter(bool canonical)
        {
            fCanonical = canonical;
        }

        public virtual void SetCanonical(bool canonical)
        {
            fCanonical = canonical;
        }

        public virtual void SetOutput(Stream stream, string encoding)
        {
            Encoding encoding2 = ((encoding != null) ? Encoding.GetEncoding(encoding) : new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            fOut = new StreamWriter(stream, encoding2);
        }

        public virtual void SetOutput(TextWriter writer)
        {
            fOut = writer;
        }

        public virtual void Write(XmlNode node)
        {
            if (node == null)
            {
                return;
            }

            XmlNodeType nodeType = node.NodeType;
            switch (nodeType)
            {
                case XmlNodeType.Document:
                    {
                        XmlDocument xmlDocument = (XmlDocument)node;
                        fXML11 = false;
                        if (!fCanonical)
                        {
                            if (fXML11)
                            {
                                fOut.WriteLine("<?xml version=\"1.1\" encoding=\"UTF-8\"?>");
                            }
                            else
                            {
                                fOut.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                            }

                            fOut.Flush();
                            Write(xmlDocument.DocumentType);
                        }

                        Write(xmlDocument.DocumentElement);
                        break;
                    }
                case XmlNodeType.DocumentType:
                    {
                        XmlDocumentType xmlDocumentType = (XmlDocumentType)node;
                        fOut.Write("<!DOCTYPE ");
                        fOut.Write(xmlDocumentType.Name);
                        string publicId = xmlDocumentType.PublicId;
                        string systemId = xmlDocumentType.SystemId;
                        if (publicId != null)
                        {
                            fOut.Write(" PUBLIC '");
                            fOut.Write(publicId);
                            fOut.Write("' '");
                            fOut.Write(systemId);
                            fOut.Write('\'');
                        }
                        else if (systemId != null)
                        {
                            fOut.Write(" SYSTEM '");
                            fOut.Write(systemId);
                            fOut.Write('\'');
                        }

                        string internalSubset = xmlDocumentType.InternalSubset;
                        if (internalSubset != null)
                        {
                            fOut.WriteLine(" [");
                            fOut.Write(internalSubset);
                            fOut.Write(']');
                        }

                        fOut.WriteLine('>');
                        break;
                    }
                case XmlNodeType.Element:
                    {
                        fOut.Write('<');
                        fOut.Write(node.Name);
                        XmlAttribute[] array = SortAttributes(node.Attributes);
                        foreach (XmlAttribute xmlAttribute in array)
                        {
                            fOut.Write(' ');
                            fOut.Write(xmlAttribute.Name);
                            fOut.Write("=\"");
                            NormalizeAndPrint(xmlAttribute.Value, isAttValue: true);
                            fOut.Write('"');
                        }

                        fOut.Write('>');
                        fOut.Flush();
                        for (XmlNode xmlNode = node.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                        {
                            Write(xmlNode);
                        }

                        break;
                    }
                case XmlNodeType.EntityReference:
                    if (fCanonical)
                    {
                        for (XmlNode xmlNode2 = node.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                        {
                            Write(xmlNode2);
                        }
                    }
                    else
                    {
                        fOut.Write('&');
                        fOut.Write(node.Name);
                        fOut.Write(';');
                        fOut.Flush();
                    }

                    break;
                case XmlNodeType.CDATA:
                    if (fCanonical)
                    {
                        NormalizeAndPrint(node.Value, isAttValue: false);
                    }
                    else
                    {
                        fOut.Write("<![CDATA[");
                        fOut.Write(node.Value);
                        fOut.Write("]]>");
                    }

                    fOut.Flush();
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    NormalizeAndPrint(node.Value, isAttValue: false);
                    fOut.Flush();
                    break;
                case XmlNodeType.ProcessingInstruction:
                    {
                        fOut.Write("<?");
                        fOut.Write(node.Name);
                        string value2 = node.Value;
                        if (value2 != null && value2.Length > 0)
                        {
                            fOut.Write(' ');
                            fOut.Write(value2);
                        }

                        fOut.Write("?>");
                        fOut.Flush();
                        break;
                    }
                case XmlNodeType.Comment:
                    if (!fCanonical)
                    {
                        fOut.Write("<!--");
                        string value = node.Value;
                        if (value != null && value.Length > 0)
                        {
                            fOut.Write(value);
                        }

                        fOut.Write("-->");
                        fOut.Flush();
                    }

                    break;
            }

            if (nodeType == XmlNodeType.Element)
            {
                fOut.Write("</");
                fOut.Write(node.Name);
                fOut.Write('>');
                fOut.Flush();
            }
        }

        protected virtual XmlAttribute[] SortAttributes(XmlAttributeCollection attrs)
        {
            int num = attrs?.Count ?? 0;
            XmlAttribute[] array = new XmlAttribute[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = attrs[i];
            }

            for (int j = 0; j < num - 1; j++)
            {
                string strB = array[j].Name;
                int num2 = j;
                for (int k = j + 1; k < num; k++)
                {
                    string name = array[k].Name;
                    if (name.CompareTo(strB) < 0)
                    {
                        strB = name;
                        num2 = k;
                    }
                }

                if (num2 != j)
                {
                    XmlAttribute xmlAttribute = array[j];
                    array[j] = array[num2];
                    array[num2] = xmlAttribute;
                }
            }

            return array;
        }

        protected virtual void NormalizeAndPrint(string s, bool isAttValue)
        {
            int num = s?.Length ?? 0;
            for (int i = 0; i < num; i++)
            {
                char c = s[i];
                NormalizeAndPrint(c, isAttValue);
            }
        }

        protected virtual void NormalizeAndPrint(char c, bool isAttValue)
        {
            switch (c)
            {
                case '<':
                    fOut.Write("&lt;");
                    return;
                case '>':
                    fOut.Write("&gt;");
                    return;
                case '&':
                    fOut.Write("&amp;");
                    return;
                case '"':
                    if (isAttValue)
                    {
                        fOut.Write("&quot;");
                    }
                    else
                    {
                        fOut.Write("\"");
                    }

                    return;
                case '\r':
                    fOut.Write("&#xD;");
                    return;
                case '\n':
                    if (fCanonical)
                    {
                        fOut.Write("&#xA;");
                        return;
                    }

                    break;
            }

            if ((fXML11 && ((c >= '\u0001' && c <= '\u001f' && c != '\t' && c != '\n') || (c >= '\u007f' && c <= '\u009f') || c == '\u2028')) || (isAttValue && (c == '\t' || c == '\n')))
            {
                fOut.Write("&#x");
                int num = c;
                fOut.Write(num.ToString("X"));
                fOut.Write(";");
            }
            else
            {
                fOut.Write(c);
            }
        }
    }
}
