using Sign.itext.xml.xmp.options;
using System.Collections;
using System.Xml;

namespace Sign.itext.xml.xmp.impl
{
    public class ParseRdf : XmpConst
    {
        public const int RDFTERM_OTHER = 0;

        public const int RDFTERM_RDF = 1;

        public const int RDFTERM_ID = 2;

        public const int RDFTERM_ABOUT = 3;

        public const int RDFTERM_PARSE_TYPE = 4;

        public const int RDFTERM_RESOURCE = 5;

        public const int RDFTERM_NODE_ID = 6;

        public const int RDFTERM_DATATYPE = 7;

        public const int RDFTERM_DESCRIPTION = 8;

        public const int RDFTERM_LI = 9;

        public const int RDFTERM_ABOUT_EACH = 10;

        public const int RDFTERM_ABOUT_EACH_PREFIX = 11;

        public const int RDFTERM_BAG_ID = 12;

        public const int RDFTERM_FIRST_CORE = 1;

        public const int RDFTERM_LAST_CORE = 7;

        public const int RDFTERM_FIRST_SYNTAX = 1;

        public const int RDFTERM_LAST_SYNTAX = 9;

        public const int RDFTERM_FIRST_OLD = 10;

        public const int RDFTERM_LAST_OLD = 12;

        public const string DEFAULT_PREFIX = "_dflt";

        internal static XmpMetaImpl Parse(XmlNode xmlRoot)
        {
            XmpMetaImpl xmpMetaImpl = new XmpMetaImpl();
            RdfRdf(xmpMetaImpl, xmlRoot);
            return xmpMetaImpl;
        }

        internal static void RdfRdf(XmpMetaImpl xmp, XmlNode rdfRdfNode)
        {
            if (rdfRdfNode.Attributes != null && rdfRdfNode.Attributes!.Count > 0)
            {
                RdfNodeElementList(xmp, xmp.Root, rdfRdfNode);
                return;
            }

            throw new XmpException("Invalid attributes of rdf:RDF element", 202);
        }

        private static void RdfNodeElementList(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode rdfRdfNode)
        {
            for (int i = 0; i < rdfRdfNode.ChildNodes.Count; i++)
            {
                XmlNode xmlNode = rdfRdfNode.ChildNodes[i];
                if (!IsWhitespaceNode(xmlNode))
                {
                    RdfNodeElement(xmp, xmpParent, xmlNode, isTopLevel: true);
                }
            }
        }

        private static void RdfNodeElement(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlNode, bool isTopLevel)
        {
            int rdfTermKind = GetRdfTermKind(xmlNode);
            if (rdfTermKind != 8 && rdfTermKind != 0)
            {
                throw new XmpException("Node element must be rdf:Description or typed node", 202);
            }

            if (isTopLevel && rdfTermKind == 0)
            {
                throw new XmpException("Top level typed node not allowed", 203);
            }

            RdfNodeElementAttrs(xmp, xmpParent, xmlNode, isTopLevel);
            RdfPropertyElementList(xmp, xmpParent, xmlNode, isTopLevel);
        }

        private static void RdfNodeElementAttrs(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlNode, bool isTopLevel)
        {
            int num = 0;
            if (xmlNode == null || xmlNode.Attributes == null)
            {
                return;
            }

            for (int i = 0; i < xmlNode.Attributes!.Count; i++)
            {
                XmlNode xmlNode2 = xmlNode.Attributes![i];
                if ("xmlns".Equals(xmlNode2.Prefix) || (xmlNode2.Prefix == null && "xmlns".Equals(xmlNode2.Name)))
                {
                    continue;
                }

                int rdfTermKind = GetRdfTermKind(xmlNode2);
                switch (rdfTermKind)
                {
                    case 2:
                    case 3:
                    case 6:
                        if (num > 0)
                        {
                            throw new XmpException("Mutally exclusive about, ID, nodeID attributes", 202);
                        }

                        num++;
                        if (!isTopLevel || rdfTermKind != 3)
                        {
                            break;
                        }

                        if (!string.IsNullOrEmpty(xmpParent.Name))
                        {
                            if (!xmpParent.Name.Equals(xmlNode2.Value))
                            {
                                throw new XmpException("Mismatched top level rdf:about values", 203);
                            }
                        }
                        else
                        {
                            xmpParent.Name = xmlNode2.Value;
                        }

                        break;
                    case 0:
                        AddChildNode(xmp, xmpParent, xmlNode2, xmlNode2.Value, isTopLevel);
                        break;
                    default:
                        throw new XmpException("Invalid nodeElement attribute", 202);
                }
            }
        }

        private static void RdfPropertyElementList(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlParent, bool isTopLevel)
        {
            for (int i = 0; i < xmlParent.ChildNodes.Count; i++)
            {
                XmlNode xmlNode = xmlParent.ChildNodes[i];
                if (!IsWhitespaceNode(xmlNode))
                {
                    if (xmlNode.NodeType != XmlNodeType.Element)
                    {
                        throw new XmpException("Expected property element node not found", 202);
                    }

                    RdfPropertyElement(xmp, xmpParent, xmlNode, isTopLevel);
                }
            }
        }

        private static void RdfPropertyElement(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlNode, bool isTopLevel)
        {
            if (!IsPropertyElementName(GetRdfTermKind(xmlNode)))
            {
                throw new XmpException("Invalid property element name", 202);
            }

            XmlAttributeCollection attributes = xmlNode.Attributes;
            if (attributes == null)
            {
                return;
            }

            IList list = null;
            for (int i = 0; i < attributes.Count; i++)
            {
                XmlNode xmlNode2 = attributes[i];
                if ("xmlns".Equals(xmlNode2.Prefix) || (xmlNode2.Prefix == null && "xmlns".Equals(xmlNode2.Name)))
                {
                    if (list == null)
                    {
                        list = new ArrayList();
                    }

                    list.Add(xmlNode2.Name);
                }
            }

            if (list != null)
            {
                IEnumerator enumerator = list.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string name = (string)enumerator.Current;
                    attributes.RemoveNamedItem(name);
                }
            }

            if (attributes.Count > 3)
            {
                RdfEmptyPropertyElement(xmp, xmpParent, xmlNode, isTopLevel);
                return;
            }

            for (int j = 0; j < attributes.Count; j++)
            {
                XmlNode xmlNode3 = attributes[j];
                string localName = xmlNode3.LocalName;
                string namespaceURI = xmlNode3.NamespaceURI;
                string value = xmlNode3.Value;
                if (!"xml:lang".Equals(xmlNode3.Name) || ("ID".Equals(localName) && "http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(namespaceURI)))
                {
                    if ("datatype".Equals(localName) && "http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(namespaceURI))
                    {
                        RdfLiteralPropertyElement(xmp, xmpParent, xmlNode, isTopLevel);
                    }
                    else if (!"parseType".Equals(localName) || !"http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(namespaceURI))
                    {
                        RdfEmptyPropertyElement(xmp, xmpParent, xmlNode, isTopLevel);
                    }
                    else if ("Literal".Equals(value))
                    {
                        RdfParseTypeLiteralPropertyElement();
                    }
                    else if ("Resource".Equals(value))
                    {
                        RdfParseTypeResourcePropertyElement(xmp, xmpParent, xmlNode, isTopLevel);
                    }
                    else if ("Collection".Equals(value))
                    {
                        RdfParseTypeCollectionPropertyElement();
                    }
                    else
                    {
                        RdfParseTypeOtherPropertyElement();
                    }

                    return;
                }
            }

            if (xmlNode.HasChildNodes)
            {
                for (int k = 0; k < xmlNode.ChildNodes.Count; k++)
                {
                    if (xmlNode.ChildNodes[k]!.NodeType != XmlNodeType.Text)
                    {
                        RdfResourcePropertyElement(xmp, xmpParent, xmlNode, isTopLevel);
                        return;
                    }
                }

                RdfLiteralPropertyElement(xmp, xmpParent, xmlNode, isTopLevel);
            }
            else
            {
                RdfEmptyPropertyElement(xmp, xmpParent, xmlNode, isTopLevel);
            }
        }

        private static void RdfResourcePropertyElement(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlNode, bool isTopLevel)
        {
            if (isTopLevel && "iX:changes".Equals(xmlNode.Name))
            {
                return;
            }

            XmpNode xmpNode = AddChildNode(xmp, xmpParent, xmlNode, "", isTopLevel);
            if (xmlNode.Attributes != null)
            {
                for (int i = 0; i < xmlNode.Attributes!.Count; i++)
                {
                    XmlNode xmlNode2 = xmlNode.Attributes![i];
                    if (!"xmlns".Equals(xmlNode2.Prefix) && (xmlNode2.Prefix != null || !"xmlns".Equals(xmlNode2.Name)))
                    {
                        string localName = xmlNode2.LocalName;
                        string namespaceURI = xmlNode2.NamespaceURI;
                        if ("xml:lang".Equals(xmlNode2.Name))
                        {
                            AddQualifierNode(xmpNode, "xml:lang", xmlNode2.Value);
                        }
                        else if ("ID".Equals(localName) && "http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(namespaceURI))
                        {
                            continue;
                        }

                        throw new XmpException("Invalid attribute for resource property element", 202);
                    }
                }
            }

            bool flag = false;
            for (int j = 0; j < xmlNode.ChildNodes.Count; j++)
            {
                XmlNode xmlNode3 = xmlNode.ChildNodes[j];
                if (IsWhitespaceNode(xmlNode3))
                {
                    continue;
                }

                if (xmlNode3.NodeType == XmlNodeType.Element && !flag)
                {
                    bool flag2 = "http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(xmlNode3.NamespaceURI);
                    string localName2 = xmlNode3.LocalName;
                    if (flag2 && "Bag".Equals(localName2))
                    {
                        xmpNode.Options.Array = true;
                    }
                    else if (flag2 && "Seq".Equals(localName2))
                    {
                        xmpNode.Options.Array = true;
                        xmpNode.Options.ArrayOrdered = true;
                    }
                    else if (flag2 && "Alt".Equals(localName2))
                    {
                        xmpNode.Options.Array = true;
                        xmpNode.Options.ArrayOrdered = true;
                        xmpNode.Options.ArrayAlternate = true;
                    }
                    else
                    {
                        xmpNode.Options.Struct = true;
                        if (!flag2 && !"Description".Equals(localName2))
                        {
                            string namespaceURI2 = xmlNode3.NamespaceURI;
                            if (namespaceURI2 == null)
                            {
                                throw new XmpException("All XML elements must be in a namespace", 203);
                            }

                            namespaceURI2 = namespaceURI2 + ":" + localName2;
                            AddQualifierNode(xmpNode, "rdf:type", namespaceURI2);
                        }
                    }

                    RdfNodeElement(xmp, xmpNode, xmlNode3, isTopLevel: false);
                    if (xmpNode.HasValueChild)
                    {
                        FixupQualifiedNode(xmpNode);
                    }
                    else if (xmpNode.Options.ArrayAlternate)
                    {
                        XmpNodeUtils.DetectAltText(xmpNode);
                    }

                    flag = true;
                    continue;
                }

                if (flag)
                {
                    throw new XmpException("Invalid child of resource property element", 202);
                }

                throw new XmpException("Children of resource property element must be XML elements", 202);
            }

            if (!flag)
            {
                throw new XmpException("Missing child of resource property element", 202);
            }
        }

        private static void RdfLiteralPropertyElement(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlNode, bool isTopLevel)
        {
            XmpNode xmpNode = AddChildNode(xmp, xmpParent, xmlNode, null, isTopLevel);
            if (xmlNode.Attributes != null)
            {
                for (int i = 0; i < xmlNode.Attributes!.Count; i++)
                {
                    XmlNode xmlNode2 = xmlNode.Attributes![i];
                    if (!"xmlns".Equals(xmlNode2.Prefix) && (xmlNode2.Prefix != null || !"xmlns".Equals(xmlNode2.Name)))
                    {
                        string namespaceURI = xmlNode2.NamespaceURI;
                        string localName = xmlNode2.LocalName;
                        if ("xml:lang".Equals(xmlNode2.Name))
                        {
                            AddQualifierNode(xmpNode, "xml:lang", xmlNode2.Value);
                        }
                        else if (!"http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(namespaceURI) || (!"ID".Equals(localName) && !"datatype".Equals(localName)))
                        {
                            throw new XmpException("Invalid attribute for literal property element", 202);
                        }
                    }
                }
            }

            string text = "";
            for (int j = 0; j < xmlNode.ChildNodes.Count; j++)
            {
                XmlNode xmlNode3 = xmlNode.ChildNodes[j];
                if (xmlNode3.NodeType == XmlNodeType.Text)
                {
                    text += xmlNode3.Value;
                    continue;
                }

                throw new XmpException("Invalid child of literal property element", 202);
            }

            xmpNode.Value = text;
        }

        private static void RdfParseTypeLiteralPropertyElement()
        {
            throw new XmpException("ParseTypeLiteral property element not allowed", 203);
        }

        private static void RdfParseTypeResourcePropertyElement(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlNode, bool isTopLevel)
        {
            XmpNode xmpNode = AddChildNode(xmp, xmpParent, xmlNode, "", isTopLevel);
            xmpNode.Options.Struct = true;
            if (xmlNode.Attributes != null)
            {
                for (int i = 0; i < xmlNode.Attributes!.Count; i++)
                {
                    XmlNode xmlNode2 = xmlNode.Attributes![i];
                    if (!"xmlns".Equals(xmlNode2.Prefix) && (xmlNode2.Prefix != null || !"xmlns".Equals(xmlNode2.Name)))
                    {
                        string localName = xmlNode2.LocalName;
                        string namespaceURI = xmlNode2.NamespaceURI;
                        if ("xml:lang".Equals(xmlNode2.Name))
                        {
                            AddQualifierNode(xmpNode, "xml:lang", xmlNode2.Value);
                        }
                        else if ("http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(namespaceURI) && ("ID".Equals(localName) || "parseType".Equals(localName)))
                        {
                            continue;
                        }

                        throw new XmpException("Invalid attribute for ParseTypeResource property element", 202);
                    }
                }
            }

            RdfPropertyElementList(xmp, xmpNode, xmlNode, isTopLevel: false);
            if (xmpNode.HasValueChild)
            {
                FixupQualifiedNode(xmpNode);
            }
        }

        private static void RdfParseTypeCollectionPropertyElement()
        {
            throw new XmpException("ParseTypeCollection property element not allowed", 203);
        }

        private static void RdfParseTypeOtherPropertyElement()
        {
            throw new XmpException("ParseTypeOther property element not allowed", 203);
        }

        private static void RdfEmptyPropertyElement(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlNode, bool isTopLevel)
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            XmlNode xmlNode2 = null;
            if (xmlNode.HasChildNodes)
            {
                throw new XmpException("Nested content not allowed with rdf:resource or property attributes", 202);
            }

            if (xmlNode.Attributes != null)
            {
                for (int i = 0; i < xmlNode.Attributes!.Count; i++)
                {
                    XmlNode xmlNode3 = xmlNode.Attributes![i];
                    if ("xmlns".Equals(xmlNode3.Prefix) || (xmlNode3.Prefix == null && "xmlns".Equals(xmlNode3.Name)))
                    {
                        continue;
                    }

                    switch (GetRdfTermKind(xmlNode3))
                    {
                        case 5:
                            if (flag3)
                            {
                                throw new XmpException("Empty property element can't have both rdf:resource and rdf:nodeID", 202);
                            }

                            if (flag4)
                            {
                                throw new XmpException("Empty property element can't have both rdf:value and rdf:resource", 203);
                            }

                            flag2 = true;
                            if (!flag4)
                            {
                                xmlNode2 = xmlNode3;
                            }

                            break;
                        case 6:
                            if (flag2)
                            {
                                throw new XmpException("Empty property element can't have both rdf:resource and rdf:nodeID", 202);
                            }

                            flag3 = true;
                            break;
                        case 0:
                            if ("value".Equals(xmlNode3.LocalName) && "http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(xmlNode3.NamespaceURI))
                            {
                                if (flag2)
                                {
                                    throw new XmpException("Empty property element can't have both rdf:value and rdf:resource", 203);
                                }

                                flag4 = true;
                                xmlNode2 = xmlNode3;
                            }
                            else if (!"xml:lang".Equals(xmlNode3.Name))
                            {
                                flag = true;
                            }

                            break;
                        default:
                            throw new XmpException("Unrecognized attribute of empty property element", 202);
                        case 2:
                            break;
                    }
                }
            }

            XmpNode xmpNode = AddChildNode(xmp, xmpParent, xmlNode, "", isTopLevel);
            bool flag5 = false;
            if (flag4 || flag2)
            {
                xmpNode.Value = ((xmlNode2 != null) ? xmlNode2.Value : "");
                if (!flag4)
                {
                    xmpNode.Options.Uri = true;
                }
            }
            else if (flag)
            {
                xmpNode.Options.Struct = true;
                flag5 = true;
            }

            if (xmlNode.Attributes == null)
            {
                return;
            }

            for (int j = 0; j < xmlNode.Attributes!.Count; j++)
            {
                XmlNode xmlNode4 = xmlNode.Attributes![j];
                if (xmlNode4 == xmlNode2 || "xmlns".Equals(xmlNode4.Prefix) || (xmlNode4.Prefix == null && "xmlns".Equals(xmlNode4.Name)))
                {
                    continue;
                }

                switch (GetRdfTermKind(xmlNode4))
                {
                    case 5:
                        AddQualifierNode(xmpNode, "rdf:resource", xmlNode4.Value);
                        break;
                    case 0:
                        if (!flag5)
                        {
                            AddQualifierNode(xmpNode, xmlNode4.Name, xmlNode4.Value);
                        }
                        else if ("xml:lang".Equals(xmlNode4.Name))
                        {
                            AddQualifierNode(xmpNode, "xml:lang", xmlNode4.Value);
                        }
                        else
                        {
                            AddChildNode(xmp, xmpNode, xmlNode4, xmlNode4.Value, isTopLevel: false);
                        }

                        break;
                    default:
                        throw new XmpException("Unrecognized attribute of empty property element", 202);
                    case 2:
                    case 6:
                        break;
                }
            }
        }

        private static XmpNode AddChildNode(XmpMetaImpl xmp, XmpNode xmpParent, XmlNode xmlNode, string value, bool isTopLevel)
        {
            IXmpSchemaRegistry schemaRegistry = XmpMetaFactory.SchemaRegistry;
            string text = xmlNode.NamespaceURI;
            if (text != null)
            {
                if ("http://purl.org/dc/1.1/".Equals(text))
                {
                    text = "http://purl.org/dc/elements/1.1/";
                }

                string text2 = schemaRegistry.GetNamespacePrefix(text);
                if (text2 == null)
                {
                    text2 = xmlNode.Prefix ?? "_dflt";
                    text2 = schemaRegistry.RegisterNamespace(text, text2);
                }

                string text3 = text2 + xmlNode.LocalName;
                PropertyOptions options = new PropertyOptions();
                bool alias = false;
                if (isTopLevel)
                {
                    XmpNode xmpNode = XmpNodeUtils.FindSchemaNode(xmp.Root, text, "_dflt", createNodes: true);
                    xmpNode.Implicit = false;
                    xmpParent = xmpNode;
                    if (schemaRegistry.FindAlias(text3) != null)
                    {
                        alias = true;
                        xmp.Root.HasAliases = true;
                        xmpNode.HasAliases = true;
                    }
                }

                bool num = "rdf:li".Equals(text3);
                bool num2 = "rdf:value".Equals(text3);
                XmpNode xmpNode2 = new XmpNode(text3, value, options)
                {
                    Alias = alias
                };
                if (!num2)
                {
                    xmpParent.AddChild(xmpNode2);
                }
                else
                {
                    xmpParent.AddChild(1, xmpNode2);
                }

                if (num2)
                {
                    if (isTopLevel || !xmpParent.Options.Struct)
                    {
                        throw new XmpException("Misplaced rdf:value element", 202);
                    }

                    xmpParent.HasValueChild = true;
                }

                if (num)
                {
                    if (!xmpParent.Options.Array)
                    {
                        throw new XmpException("Misplaced rdf:li element", 202);
                    }

                    xmpNode2.Name = "[]";
                }

                return xmpNode2;
            }

            throw new XmpException("XML namespace required for all elements and attributes", 202);
        }

        private static XmpNode AddQualifierNode(XmpNode xmpParent, string name, string value)
        {
            bool flag = "xml:lang".Equals(name);
            XmpNode xmpNode = new XmpNode(name, flag ? Utils.NormalizeLangValue(value) : value, null);
            xmpParent.AddQualifier(xmpNode);
            return xmpNode;
        }

        private static void FixupQualifiedNode(XmpNode xmpParent)
        {
            XmpNode child = xmpParent.GetChild(1);
            if (child.Options.HasLanguage)
            {
                if (xmpParent.Options.HasLanguage)
                {
                    throw new XmpException("Redundant xml:lang for rdf:value element", 203);
                }

                XmpNode qualifier = child.GetQualifier(1);
                child.RemoveQualifier(qualifier);
                xmpParent.AddQualifier(qualifier);
            }

            for (int i = 1; i <= child.QualifierLength; i++)
            {
                XmpNode qualifier2 = child.GetQualifier(i);
                xmpParent.AddQualifier(qualifier2);
            }

            for (int j = 2; j <= xmpParent.ChildrenLength; j++)
            {
                XmpNode child2 = xmpParent.GetChild(j);
                xmpParent.AddQualifier(child2);
            }

            xmpParent.HasValueChild = false;
            xmpParent.Options.Struct = false;
            xmpParent.Options.MergeWith(child.Options);
            xmpParent.Value = child.Value;
            xmpParent.RemoveChildren();
            IEnumerator enumerator = child.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode node = (XmpNode)enumerator.Current;
                xmpParent.AddChild(node);
            }
        }

        private static bool IsWhitespaceNode(XmlNode node)
        {
            if (node.NodeType != XmlNodeType.Text)
            {
                return false;
            }

            string value = node.Value;
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsPropertyElementName(int term)
        {
            if (term == 8 || IsOldTerm(term))
            {
                return false;
            }

            return !IsCoreSyntaxTerm(term);
        }

        private static bool IsOldTerm(int term)
        {
            if (10 <= term)
            {
                return term <= 12;
            }

            return false;
        }

        private static bool IsCoreSyntaxTerm(int term)
        {
            if (1 <= term)
            {
                return term <= 7;
            }

            return false;
        }

        private static int GetRdfTermKind(XmlNode node)
        {
            string localName = node.LocalName;
            string text = node.NamespaceURI;
            if (text == null && ("about".Equals(localName) || "ID".Equals(localName)) && node is XmlAttribute && "http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(((XmlAttribute)node).OwnerElement!.NamespaceURI))
            {
                text = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
            }

            if ("http://www.w3.org/1999/02/22-rdf-syntax-ns#".Equals(text))
            {
                if ("li".Equals(localName))
                {
                    return 9;
                }

                if ("parseType".Equals(localName))
                {
                    return 4;
                }

                if ("Description".Equals(localName))
                {
                    return 8;
                }

                if ("about".Equals(localName))
                {
                    return 3;
                }

                if ("resource".Equals(localName))
                {
                    return 5;
                }

                if ("RDF".Equals(localName))
                {
                    return 1;
                }

                if ("ID".Equals(localName))
                {
                    return 2;
                }

                if ("nodeID".Equals(localName))
                {
                    return 6;
                }

                if ("datatype".Equals(localName))
                {
                    return 7;
                }

                if ("aboutEach".Equals(localName))
                {
                    return 10;
                }

                if ("aboutEachPrefix".Equals(localName))
                {
                    return 11;
                }

                if ("bagID".Equals(localName))
                {
                    return 12;
                }
            }

            return 0;
        }
    }
}
