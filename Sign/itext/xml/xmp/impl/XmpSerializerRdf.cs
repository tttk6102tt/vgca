using Sign.itext.xml.simpleparser;
using Sign.itext.xml.xmp.options;
using Sign.Org.BouncyCastle.Asn1.Utilities.Collections;
using System.Collections;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpSerializerRdf
    {
        private const int DEFAULT_PAD = 2048;

        private const string RDF_XMPMETA_END = "</x:xmpmeta>";

        private const string RDF_RDF_END = "</rdf:RDF>";

        private const string RDF_SCHEMA_START = "<rdf:Description rdf:about=";

        private const string RDF_SCHEMA_END = "</rdf:Description>";

        private const string RDF_STRUCT_START = "<rdf:Description";

        private const string RDF_STRUCT_END = "</rdf:Description>";

        private const string RDF_EMPTY_STRUCT = "<rdf:Description/>";

        private static readonly string PACKET_HEADER = "<?xpacket begin=\"\ufeff\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>";

        private static readonly string PACKET_TRAILER = "<?xpacket end=\"";

        private static readonly string PACKET_TRAILER2 = "\"?>";

        private static readonly string RDF_XMPMETA_START = "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\" x:xmptk=\"";

        private static readonly string RDF_RDF_START = "<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">";

        internal static readonly ISet RDF_ATTR_QUALIFIER = new HashSet(new string[5] { "xml:lang", "rdf:resource", "rdf:ID", "rdf:bagID", "rdf:nodeID" });

        private SerializeOptions _options;

        private CountOutputStream _outputStream;

        private int _padding;

        private int _unicodeSize = 1;

        private StreamWriter _writer;

        private XmpMetaImpl _xmp;

        public virtual void Serialize(IXmpMeta xmp, Stream @out, SerializeOptions options)
        {
            try
            {
                _outputStream = new CountOutputStream(@out);
                _xmp = (XmpMetaImpl)xmp;
                _options = options;
                _padding = options.Padding;
                _writer = new StreamWriter(_outputStream, new EncodingNoPreamble(IanaEncodings.GetEncodingEncoding(options.Encoding)));
                CheckOptionsConsistence();
                string text = SerializeAsRdf();
                _writer.Flush();
                AddPadding(text.Length);
                Write(text);
                _writer.Flush();
                _outputStream.Close();
            }
            catch (IOException)
            {
                throw new XmpException("Error writing to the OutputStream", 0);
            }
        }

        private void AddPadding(int tailLength)
        {
            if (_options.ExactPacketLength)
            {
                int num = _outputStream.BytesWritten + tailLength * _unicodeSize;
                if (num > _padding)
                {
                    throw new XmpException("Can't fit into specified packet size", 107);
                }

                _padding -= num;
            }

            _padding /= _unicodeSize;
            int length = _options.Newline.Length;
            if (_padding >= length)
            {
                _padding -= length;
                while (_padding >= 100 + length)
                {
                    WriteChars(100, ' ');
                    WriteNewline();
                    _padding -= 100 + length;
                }

                WriteChars(_padding, ' ');
                WriteNewline();
            }
            else
            {
                WriteChars(_padding, ' ');
            }
        }

        protected internal virtual void CheckOptionsConsistence()
        {
            if (_options.EncodeUtf16Be | _options.EncodeUtf16Le)
            {
                _unicodeSize = 2;
            }

            if (_options.ExactPacketLength)
            {
                if (_options.OmitPacketWrapper | _options.IncludeThumbnailPad)
                {
                    throw new XmpException("Inconsistent options for exact size Serialize", 103);
                }

                if ((_options.Padding & (_unicodeSize - 1)) != 0)
                {
                    throw new XmpException("Exact size must be a multiple of the Unicode element", 103);
                }
            }
            else if (_options.ReadOnlyPacket)
            {
                if (_options.OmitPacketWrapper | _options.IncludeThumbnailPad)
                {
                    throw new XmpException("Inconsistent options for read-only packet", 103);
                }

                _padding = 0;
            }
            else if (_options.OmitPacketWrapper)
            {
                if (_options.IncludeThumbnailPad)
                {
                    throw new XmpException("Inconsistent options for non-packet Serialize", 103);
                }

                _padding = 0;
            }
            else
            {
                if (_padding == 0)
                {
                    _padding = 2048 * _unicodeSize;
                }

                if (_options.IncludeThumbnailPad && !_xmp.DoesPropertyExist("http://ns.adobe.com/xap/1.0/", "Thumbnails"))
                {
                    _padding += 10000 * _unicodeSize;
                }
            }
        }

        private string SerializeAsRdf()
        {
            int num = 0;
            if (!_options.OmitPacketWrapper)
            {
                WriteIndent(num);
                Write(PACKET_HEADER);
                WriteNewline();
            }

            if (!_options.OmitXmpMetaElement)
            {
                WriteIndent(num);
                Write(RDF_XMPMETA_START);
                if (!_options.OmitVersionAttribute)
                {
                    Write(XmpMetaFactory.GetVersionInfo().Message);
                }

                Write("\">");
                WriteNewline();
                num++;
            }

            WriteIndent(num);
            Write(RDF_RDF_START);
            WriteNewline();
            if (_options.UseCanonicalFormat)
            {
                SerializeCanonicalRdfSchemas(num);
            }
            else
            {
                SerializeCompactRdfSchemas(num);
            }

            WriteIndent(num);
            Write("</rdf:RDF>");
            WriteNewline();
            if (!_options.OmitXmpMetaElement)
            {
                num--;
                WriteIndent(num);
                Write("</x:xmpmeta>");
                WriteNewline();
            }

            string text = "";
            if (!_options.OmitPacketWrapper)
            {
                for (num = _options.BaseIndent; num > 0; num--)
                {
                    text += _options.Indent;
                }

                text += PACKET_TRAILER;
                text += (_options.ReadOnlyPacket ? 'r' : 'w');
                text += PACKET_TRAILER2;
            }

            return text;
        }

        private void SerializeCanonicalRdfSchemas(int level)
        {
            if (_xmp.Root.ChildrenLength > 0)
            {
                StartOuterRdfDescription(_xmp.Root, level);
                IEnumerator enumerator = _xmp.Root.IterateChildren();
                while (enumerator.MoveNext())
                {
                    XmpNode schemaNode = (XmpNode)enumerator.Current;
                    SerializeCanonicalRdfSchema(schemaNode, level);
                }

                EndOuterRdfDescription(level);
            }
            else
            {
                WriteIndent(level + 1);
                Write("<rdf:Description rdf:about=");
                WriteTreeName();
                Write("/>");
                WriteNewline();
            }
        }

        private void WriteTreeName()
        {
            Write('"');
            string name = _xmp.Root.Name;
            if (name != null)
            {
                AppendNodeValue(name, forAttribute: true);
            }

            Write('"');
        }

        private void SerializeCompactRdfSchemas(int level)
        {
            WriteIndent(level + 1);
            Write("<rdf:Description rdf:about=");
            WriteTreeName();
            ISet set = new HashSet();
            set.Add("xml");
            set.Add("rdf");
            IEnumerator enumerator = _xmp.Root.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode node = (XmpNode)enumerator.Current;
                DeclareUsedNamespaces(node, set, level + 3);
            }

            bool flag = true;
            IEnumerator enumerator2 = _xmp.Root.IterateChildren();
            while (enumerator2.MoveNext())
            {
                XmpNode parentNode = (XmpNode)enumerator2.Current;
                flag &= SerializeCompactRdfAttrProps(parentNode, level + 2);
            }

            if (!flag)
            {
                Write('>');
                WriteNewline();
                IEnumerator enumerator3 = _xmp.Root.IterateChildren();
                while (enumerator3.MoveNext())
                {
                    XmpNode parentNode2 = (XmpNode)enumerator3.Current;
                    SerializeCompactRdfElementProps(parentNode2, level + 2);
                }

                WriteIndent(level + 1);
                Write("</rdf:Description>");
                WriteNewline();
            }
            else
            {
                Write("/>");
                WriteNewline();
            }
        }

        private bool SerializeCompactRdfAttrProps(XmpNode parentNode, int indent)
        {
            bool result = true;
            IEnumerator enumerator = parentNode.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode != null && canBeRDFAttrProp(xmpNode))
                {
                    WriteNewline();
                    WriteIndent(indent);
                    Write(xmpNode.Name);
                    Write("=\"");
                    AppendNodeValue(xmpNode.Value, forAttribute: true);
                    Write('"');
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        private void SerializeCompactRdfElementProps(XmpNode parentNode, int indent)
        {
            IEnumerator enumerator = parentNode.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode == null || canBeRDFAttrProp(xmpNode))
                {
                    continue;
                }

                bool flag = true;
                bool flag2 = true;
                string text = xmpNode.Name;
                if ("[]".Equals(text))
                {
                    text = "rdf:li";
                }

                WriteIndent(indent);
                Write('<');
                Write(text);
                bool flag3 = false;
                bool hasRdfResourceQual = false;
                IEnumerator enumerator2 = xmpNode.IterateQualifier();
                while (enumerator2.MoveNext())
                {
                    XmpNode xmpNode2 = (XmpNode)enumerator2.Current;
                    if (xmpNode2 != null)
                    {
                        if (!RDF_ATTR_QUALIFIER.Contains(xmpNode2.Name))
                        {
                            flag3 = true;
                            continue;
                        }

                        hasRdfResourceQual = "rdf:resource".Equals(xmpNode2.Name);
                        Write(' ');
                        Write(xmpNode2.Name);
                        Write("=\"");
                        AppendNodeValue(xmpNode2.Value, forAttribute: true);
                        Write('"');
                    }
                }

                if (flag3)
                {
                    SerializeCompactRdfGeneralQualifier(indent, xmpNode);
                }
                else if (!xmpNode.Options.CompositeProperty)
                {
                    object[] array = SerializeCompactRdfSimpleProp(xmpNode);
                    flag = ((bool?)array[0]).Value;
                    flag2 = ((bool?)array[1]).Value;
                }
                else if (xmpNode.Options.Array)
                {
                    SerializeCompactRdfArrayProp(xmpNode, indent);
                }
                else
                {
                    flag = SerializeCompactRdfStructProp(xmpNode, indent, hasRdfResourceQual);
                }

                if (flag)
                {
                    if (flag2)
                    {
                        WriteIndent(indent);
                    }

                    Write("</");
                    Write(text);
                    Write('>');
                    WriteNewline();
                }
            }
        }

        private object[] SerializeCompactRdfSimpleProp(XmpNode node)
        {
            bool? flag = true;
            bool? flag2 = true;
            if (node.Options.Uri)
            {
                Write(" rdf:resource=\"");
                AppendNodeValue(node.Value, forAttribute: true);
                Write("\"/>");
                WriteNewline();
                flag = false;
            }
            else if (string.IsNullOrEmpty(node.Value))
            {
                Write("/>");
                WriteNewline();
                flag = false;
            }
            else
            {
                Write('>');
                AppendNodeValue(node.Value, forAttribute: false);
                flag2 = false;
            }

            return new object[2] { flag, flag2 };
        }

        private void SerializeCompactRdfArrayProp(XmpNode node, int indent)
        {
            Write('>');
            WriteNewline();
            EmitRdfArrayTag(node, isStartTag: true, indent + 1);
            if (node.Options.ArrayAltText)
            {
                XmpNodeUtils.NormalizeLangArray(node);
            }

            SerializeCompactRdfElementProps(node, indent + 2);
            EmitRdfArrayTag(node, isStartTag: false, indent + 1);
        }

        private bool SerializeCompactRdfStructProp(XmpNode node, int indent, bool hasRdfResourceQual)
        {
            bool flag = false;
            bool flag2 = false;
            bool result = true;
            IEnumerator enumerator = node.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode != null)
                {
                    if (canBeRDFAttrProp(xmpNode))
                    {
                        flag = true;
                    }
                    else
                    {
                        flag2 = true;
                    }

                    if (flag && flag2)
                    {
                        break;
                    }
                }
            }

            if (hasRdfResourceQual && flag2)
            {
                throw new XmpException("Can't mix rdf:resource qualifier and element fields", 202);
            }

            if (!node.HasChildren())
            {
                Write(" rdf:parseType=\"Resource\"/>");
                WriteNewline();
                result = false;
            }
            else if (!flag2)
            {
                SerializeCompactRdfAttrProps(node, indent + 1);
                Write("/>");
                WriteNewline();
                result = false;
            }
            else if (!flag)
            {
                Write(" rdf:parseType=\"Resource\">");
                WriteNewline();
                SerializeCompactRdfElementProps(node, indent + 1);
            }
            else
            {
                Write('>');
                WriteNewline();
                WriteIndent(indent + 1);
                Write("<rdf:Description");
                SerializeCompactRdfAttrProps(node, indent + 2);
                Write(">");
                WriteNewline();
                SerializeCompactRdfElementProps(node, indent + 1);
                WriteIndent(indent + 1);
                Write("</rdf:Description>");
                WriteNewline();
            }

            return result;
        }

        private void SerializeCompactRdfGeneralQualifier(int indent, XmpNode node)
        {
            Write(" rdf:parseType=\"Resource\">");
            WriteNewline();
            SerializeCanonicalRdfProperty(node, useCanonicalRdf: false, emitAsRdfValue: true, indent + 1);
            IEnumerator enumerator = node.IterateQualifier();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode != null)
                {
                    SerializeCanonicalRdfProperty(xmpNode, useCanonicalRdf: false, emitAsRdfValue: false, indent + 1);
                }
            }
        }

        private void SerializeCanonicalRdfSchema(XmpNode schemaNode, int level)
        {
            IEnumerator enumerator = schemaNode.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode != null)
                {
                    SerializeCanonicalRdfProperty(xmpNode, _options.UseCanonicalFormat, emitAsRdfValue: false, level + 2);
                }
            }
        }

        private void DeclareUsedNamespaces(XmpNode node, ISet usedPrefixes, int indent)
        {
            if (node.Options.SchemaNode)
            {
                string prefix = node.Value.Substring(0, node.Value.Length - 1);
                DeclareNamespace(prefix, node.Name, usedPrefixes, indent);
            }
            else if (node.Options.Struct)
            {
                IEnumerator enumerator = node.IterateChildren();
                while (enumerator.MoveNext())
                {
                    XmpNode xmpNode = (XmpNode)enumerator.Current;
                    if (xmpNode != null)
                    {
                        DeclareNamespace(xmpNode.Name, null, usedPrefixes, indent);
                    }
                }
            }

            IEnumerator enumerator2 = node.IterateChildren();
            while (enumerator2.MoveNext())
            {
                XmpNode xmpNode2 = (XmpNode)enumerator2.Current;
                if (xmpNode2 != null)
                {
                    DeclareUsedNamespaces(xmpNode2, usedPrefixes, indent);
                }
            }

            IEnumerator enumerator3 = node.IterateQualifier();
            while (enumerator3.MoveNext())
            {
                XmpNode xmpNode3 = (XmpNode)enumerator3.Current;
                if (xmpNode3 != null)
                {
                    DeclareNamespace(xmpNode3.Name, null, usedPrefixes, indent);
                    DeclareUsedNamespaces(xmpNode3, usedPrefixes, indent);
                }
            }
        }

        private void DeclareNamespace(string prefix, string @namespace, ISet usedPrefixes, int indent)
        {
            if (@namespace == null)
            {
                QName qName = new QName(prefix);
                if (!qName.HasPrefix())
                {
                    return;
                }

                prefix = qName.Prefix;
                @namespace = XmpMetaFactory.SchemaRegistry.GetNamespaceUri(prefix + ":");
                DeclareNamespace(prefix, @namespace, usedPrefixes, indent);
            }

            if (!usedPrefixes.Contains(prefix))
            {
                WriteNewline();
                WriteIndent(indent);
                Write("xmlns:");
                Write(prefix);
                Write("=\"");
                Write(@namespace);
                Write('"');
                usedPrefixes.Add(prefix);
            }
        }

        private void StartOuterRdfDescription(XmpNode schemaNode, int level)
        {
            WriteIndent(level + 1);
            Write("<rdf:Description rdf:about=");
            WriteTreeName();
            ISet set = new HashSet();
            set.Add("xml");
            set.Add("rdf");
            DeclareUsedNamespaces(schemaNode, set, level + 3);
            Write('>');
            WriteNewline();
        }

        private void EndOuterRdfDescription(int level)
        {
            WriteIndent(level + 1);
            Write("</rdf:Description>");
            WriteNewline();
        }

        private void SerializeCanonicalRdfProperty(XmpNode node, bool useCanonicalRdf, bool emitAsRdfValue, int indent)
        {
            bool flag = true;
            bool flag2 = true;
            string text = node.Name;
            if (emitAsRdfValue)
            {
                text = "rdf:value";
            }
            else if ("[]".Equals(text))
            {
                text = "rdf:li";
            }

            WriteIndent(indent);
            Write('<');
            Write(text);
            bool flag3 = false;
            bool flag4 = false;
            IEnumerator enumerator = node.IterateQualifier();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode == null)
                {
                    continue;
                }

                if (!RDF_ATTR_QUALIFIER.Contains(xmpNode.Name))
                {
                    flag3 = true;
                    continue;
                }

                flag4 = "rdf:resource".Equals(xmpNode.Name);
                if (!emitAsRdfValue)
                {
                    Write(' ');
                    Write(xmpNode.Name);
                    Write("=\"");
                    AppendNodeValue(xmpNode.Value, forAttribute: true);
                    Write('"');
                }
            }

            if (flag3 && !emitAsRdfValue)
            {
                if (flag4)
                {
                    throw new XmpException("Can't mix rdf:resource and general qualifiers", 202);
                }

                if (useCanonicalRdf)
                {
                    Write(">");
                    WriteNewline();
                    indent++;
                    WriteIndent(indent);
                    Write("<rdf:Description");
                    Write(">");
                }
                else
                {
                    Write(" rdf:parseType=\"Resource\">");
                }

                WriteNewline();
                SerializeCanonicalRdfProperty(node, useCanonicalRdf, emitAsRdfValue: true, indent + 1);
                IEnumerator enumerator2 = node.IterateQualifier();
                while (enumerator2.MoveNext())
                {
                    XmpNode xmpNode2 = (XmpNode)enumerator2.Current;
                    if (xmpNode2 != null && !RDF_ATTR_QUALIFIER.Contains(xmpNode2.Name))
                    {
                        SerializeCanonicalRdfProperty(xmpNode2, useCanonicalRdf, emitAsRdfValue: false, indent + 1);
                    }
                }

                if (useCanonicalRdf)
                {
                    WriteIndent(indent);
                    Write("</rdf:Description>");
                    WriteNewline();
                    indent--;
                }
            }
            else if (!node.Options.CompositeProperty)
            {
                if (node.Options.Uri)
                {
                    Write(" rdf:resource=\"");
                    AppendNodeValue(node.Value, forAttribute: true);
                    Write("\"/>");
                    WriteNewline();
                    flag = false;
                }
                else if (node.Value == null || "".Equals(node.Value))
                {
                    Write("/>");
                    WriteNewline();
                    flag = false;
                }
                else
                {
                    Write('>');
                    AppendNodeValue(node.Value, forAttribute: false);
                    flag2 = false;
                }
            }
            else if (node.Options.Array)
            {
                Write('>');
                WriteNewline();
                EmitRdfArrayTag(node, isStartTag: true, indent + 1);
                if (node.Options.ArrayAltText)
                {
                    XmpNodeUtils.NormalizeLangArray(node);
                }

                IEnumerator enumerator3 = node.IterateChildren();
                while (enumerator3.MoveNext())
                {
                    XmpNode node2 = (XmpNode)enumerator3.Current;
                    SerializeCanonicalRdfProperty(node2, useCanonicalRdf, emitAsRdfValue: false, indent + 2);
                }

                EmitRdfArrayTag(node, isStartTag: false, indent + 1);
            }
            else if (!flag4)
            {
                if (!node.HasChildren())
                {
                    if (useCanonicalRdf)
                    {
                        Write(">");
                        WriteNewline();
                        WriteIndent(indent + 1);
                        Write("<rdf:Description/>");
                    }
                    else
                    {
                        Write(" rdf:parseType=\"Resource\"/>");
                        flag = false;
                    }

                    WriteNewline();
                }
                else
                {
                    if (useCanonicalRdf)
                    {
                        Write(">");
                        WriteNewline();
                        indent++;
                        WriteIndent(indent);
                        Write("<rdf:Description");
                        Write(">");
                    }
                    else
                    {
                        Write(" rdf:parseType=\"Resource\">");
                    }

                    WriteNewline();
                    IEnumerator enumerator4 = node.IterateChildren();
                    while (enumerator4.MoveNext())
                    {
                        XmpNode node3 = (XmpNode)enumerator4.Current;
                        SerializeCanonicalRdfProperty(node3, useCanonicalRdf, emitAsRdfValue: false, indent + 1);
                    }

                    if (useCanonicalRdf)
                    {
                        WriteIndent(indent);
                        Write("</rdf:Description>");
                        WriteNewline();
                        indent--;
                    }
                }
            }
            else
            {
                IEnumerator enumerator5 = node.IterateChildren();
                while (enumerator5.MoveNext())
                {
                    XmpNode xmpNode3 = (XmpNode)enumerator5.Current;
                    if (xmpNode3 != null)
                    {
                        if (!canBeRDFAttrProp(xmpNode3))
                        {
                            throw new XmpException("Can't mix rdf:resource and complex fields", 202);
                        }

                        WriteNewline();
                        WriteIndent(indent + 1);
                        Write(' ');
                        Write(xmpNode3.Name);
                        Write("=\"");
                        AppendNodeValue(xmpNode3.Value, forAttribute: true);
                        Write('"');
                    }
                }

                Write("/>");
                WriteNewline();
                flag = false;
            }

            if (flag)
            {
                if (flag2)
                {
                    WriteIndent(indent);
                }

                Write("</");
                Write(text);
                Write('>');
                WriteNewline();
            }
        }

        private void EmitRdfArrayTag(XmpNode arrayNode, bool isStartTag, int indent)
        {
            if (isStartTag || arrayNode.HasChildren())
            {
                WriteIndent(indent);
                Write(isStartTag ? "<rdf:" : "</rdf:");
                if (arrayNode.Options.ArrayAlternate)
                {
                    Write("Alt");
                }
                else if (arrayNode.Options.ArrayOrdered)
                {
                    Write("Seq");
                }
                else
                {
                    Write("Bag");
                }

                if (isStartTag && !arrayNode.HasChildren())
                {
                    Write("/>");
                }
                else
                {
                    Write(">");
                }

                WriteNewline();
            }
        }

        private void AppendNodeValue(string value, bool forAttribute)
        {
            if (value == null)
            {
                value = "";
            }

            Write(Utils.EscapeXml(value, forAttribute, escapeWhitespaces: true));
        }

        private bool canBeRDFAttrProp(XmpNode node)
        {
            if (!node.HasQualifier() && !node.Options.Uri && !node.Options.CompositeProperty && !node.Options.ContainsOneOf(1073741824u))
            {
                return !"[]".Equals(node.Name);
            }

            return false;
        }

        private void WriteIndent(int times)
        {
            for (int num = _options.BaseIndent + times; num > 0; num--)
            {
                _writer.Write(_options.Indent);
            }
        }

        private void Write(char c)
        {
            _writer.Write(c);
        }

        private void Write(string str)
        {
            _writer.Write(str);
        }

        private void WriteChars(int number, char c)
        {
            while (number > 0)
            {
                _writer.Write(c);
                number--;
            }
        }

        private void WriteNewline()
        {
            _writer.Write(_options.Newline);
        }
    }
}
