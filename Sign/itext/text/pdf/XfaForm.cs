using Sign.itext.pdf;
using Sign.itext.xml;
using System.Text;
using System.Xml;

namespace Sign.itext.text.pdf
{
    public class XfaForm
    {
        public class InverseStore
        {
            protected internal List<string> part = new List<string>();

            protected internal List<object> follow = new List<object>();

            public virtual string DefaultName
            {
                get
                {
                    InverseStore inverseStore = this;
                    object obj;
                    while (true)
                    {
                        obj = inverseStore.follow[0];
                        if (obj is string)
                        {
                            break;
                        }

                        inverseStore = (InverseStore)obj;
                    }

                    return (string)obj;
                }
            }

            public virtual bool IsSimilar(string name)
            {
                int num = name.IndexOf('[');
                name = name.Substring(0, num + 1);
                foreach (string item in part)
                {
                    if (item.StartsWith(name))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public class Stack2<T> : List<T>
        {
            public virtual T Peek()
            {
                if (base.Count == 0)
                {
                    throw new InvalidOperationException();
                }

                return base[base.Count - 1];
            }

            public virtual T Pop()
            {
                if (base.Count == 0)
                {
                    throw new InvalidOperationException();
                }

                T result = base[base.Count - 1];
                RemoveAt(base.Count - 1);
                return result;
            }

            public virtual T Push(T item)
            {
                Add(item);
                return item;
            }

            public virtual bool Empty()
            {
                return base.Count == 0;
            }
        }

        public class Xml2Som
        {
            protected List<string> order;

            protected Dictionary<string, XmlNode> name2Node;

            protected Dictionary<string, InverseStore> inverseSearch;

            protected Stack2<string> stack;

            protected int anform;

            public virtual List<string> Order
            {
                get
                {
                    return order;
                }
                set
                {
                    order = value;
                }
            }

            public virtual Dictionary<string, XmlNode> Name2Node
            {
                get
                {
                    return name2Node;
                }
                set
                {
                    name2Node = value;
                }
            }

            public virtual Dictionary<string, InverseStore> InverseSearch
            {
                get
                {
                    return inverseSearch;
                }
                set
                {
                    inverseSearch = value;
                }
            }

            public static string EscapeSom(string s)
            {
                if (s == null)
                {
                    return "";
                }

                int num = s.IndexOf('.');
                if (num < 0)
                {
                    return s;
                }

                StringBuilder stringBuilder = new StringBuilder();
                int num2 = 0;
                while (num >= 0)
                {
                    stringBuilder.Append(s.Substring(num2, num - num2));
                    stringBuilder.Append('\\');
                    num2 = num;
                    num = s.IndexOf('.', num + 1);
                }

                stringBuilder.Append(s.Substring(num2));
                return stringBuilder.ToString();
            }

            public static string UnescapeSom(string s)
            {
                int num = s.IndexOf('\\');
                if (num < 0)
                {
                    return s;
                }

                StringBuilder stringBuilder = new StringBuilder();
                int num2 = 0;
                while (num >= 0)
                {
                    stringBuilder.Append(s.Substring(num2, num - num2));
                    num2 = num + 1;
                    num = s.IndexOf('\\', num + 1);
                }

                stringBuilder.Append(s.Substring(num2));
                return stringBuilder.ToString();
            }

            protected virtual string PrintStack()
            {
                if (stack.Empty())
                {
                    return "";
                }

                StringBuilder stringBuilder = new StringBuilder();
                foreach (string item in stack)
                {
                    stringBuilder.Append('.').Append(item);
                }

                return stringBuilder.ToString(1, stringBuilder.Length - 1);
            }

            public static string GetShortName(string s)
            {
                int num = s.IndexOf(".#subform[");
                if (num < 0)
                {
                    return s;
                }

                int num2 = 0;
                StringBuilder stringBuilder = new StringBuilder();
                while (num >= 0)
                {
                    stringBuilder.Append(s.Substring(num2, num - num2));
                    num = s.IndexOf("]", num + 10);
                    if (num < 0)
                    {
                        return stringBuilder.ToString();
                    }

                    num2 = num + 1;
                    num = s.IndexOf(".#subform[", num2);
                }

                stringBuilder.Append(s.Substring(num2));
                return stringBuilder.ToString();
            }

            public virtual void InverseSearchAdd(string unstack)
            {
                InverseSearchAdd(inverseSearch, stack, unstack);
            }

            public static void InverseSearchAdd(Dictionary<string, InverseStore> inverseSearch, Stack2<string> stack, string unstack)
            {
                string key = stack.Peek();
                inverseSearch.TryGetValue(key, out var value);
                if (value == null)
                {
                    value = (inverseSearch[key] = new InverseStore());
                }

                for (int num = stack.Count - 2; num >= 0; num--)
                {
                    key = stack[num];
                    int num2 = value.part.IndexOf(key);
                    InverseStore inverseStore2;
                    if (num2 < 0)
                    {
                        value.part.Add(key);
                        inverseStore2 = new InverseStore();
                        value.follow.Add(inverseStore2);
                    }
                    else
                    {
                        inverseStore2 = (InverseStore)value.follow[num2];
                    }

                    value = inverseStore2;
                }

                value.part.Add("");
                value.follow.Add(unstack);
            }

            public virtual string InverseSearchGlobal(List<string> parts)
            {
                if (parts.Count == 0)
                {
                    return null;
                }

                inverseSearch.TryGetValue(parts[parts.Count - 1], out var value);
                if (value == null)
                {
                    return null;
                }

                for (int num = parts.Count - 2; num >= 0; num--)
                {
                    string text = parts[num];
                    int num2 = value.part.IndexOf(text);
                    if (num2 < 0)
                    {
                        if (value.IsSimilar(text))
                        {
                            return null;
                        }

                        return value.DefaultName;
                    }

                    value = (InverseStore)value.follow[num2];
                }

                return value.DefaultName;
            }

            public static Stack2<string> SplitParts(string name)
            {
                while (name.StartsWith("."))
                {
                    name = name.Substring(1);
                }

                Stack2<string> stack = new Stack2<string>();
                int num = 0;
                int num2 = 0;
                string text;
                while (true)
                {
                    num2 = num;
                    while (true)
                    {
                        num2 = name.IndexOf('.', num2);
                        if (num2 < 0 || name[num2 - 1] != '\\')
                        {
                            break;
                        }

                        num2++;
                    }

                    if (num2 < 0)
                    {
                        break;
                    }

                    text = name.Substring(num, num2 - num);
                    if (!text.EndsWith("]"))
                    {
                        text += "[0]";
                    }

                    stack.Add(text);
                    num = num2 + 1;
                }

                text = name.Substring(num);
                if (!text.EndsWith("]"))
                {
                    text += "[0]";
                }

                stack.Add(text);
                return stack;
            }
        }

        public class Xml2SomDatasets : Xml2Som
        {
            public Xml2SomDatasets(XmlNode n)
            {
                order = new List<string>();
                name2Node = new Dictionary<string, XmlNode>();
                stack = new Stack2<string>();
                anform = 0;
                inverseSearch = new Dictionary<string, InverseStore>();
                ProcessDatasetsInternal(n);
            }

            public virtual XmlNode InsertNode(XmlNode n, string shortName)
            {
                Stack2<string> stack = Xml2Som.SplitParts(shortName);
                XmlDocument ownerDocument = n.OwnerDocument;
                XmlNode xmlNode = null;
                n = n.FirstChild;
                while (n.NodeType != XmlNodeType.Element)
                {
                    n = n.NextSibling;
                }

                for (int i = 0; i < stack.Count; i++)
                {
                    string text = stack[i];
                    int num = text.LastIndexOf('[');
                    string text2 = text.Substring(0, num);
                    num = int.Parse(text.Substring(num + 1, text.Length - num - 2));
                    int j = -1;
                    for (xmlNode = n.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                    {
                        if (xmlNode.NodeType == XmlNodeType.Element && Xml2Som.EscapeSom(xmlNode.LocalName).Equals(text2))
                        {
                            j++;
                            if (j == num)
                            {
                                break;
                            }
                        }
                    }

                    for (; j < num; j++)
                    {
                        xmlNode = ownerDocument.CreateElement(text2);
                        xmlNode = n.AppendChild(xmlNode);
                        XmlNode xmlNode2 = ownerDocument.CreateNode(XmlNodeType.Attribute, "dataNode", "http://www.xfa.org/schema/xfa-data/1.0/");
                        xmlNode2.Value = "dataGroup";
                        xmlNode.Attributes!.SetNamedItem(xmlNode2);
                    }

                    n = xmlNode;
                }

                Xml2Som.InverseSearchAdd(inverseSearch, stack, shortName);
                name2Node[shortName] = xmlNode;
                order.Add(shortName);
                return xmlNode;
            }

            private static bool HasChildren(XmlNode n)
            {
                XmlNode namedItem = n.Attributes!.GetNamedItem("dataNode", "http://www.xfa.org/schema/xfa-data/1.0/");
                if (namedItem != null)
                {
                    string value = namedItem.Value;
                    if ("dataGroup".Equals(value))
                    {
                        return true;
                    }

                    if ("dataValue".Equals(value))
                    {
                        return false;
                    }
                }

                if (!n.HasChildNodes)
                {
                    return false;
                }

                for (XmlNode xmlNode = n.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                {
                    if (xmlNode.NodeType == XmlNodeType.Element)
                    {
                        return true;
                    }
                }

                return false;
            }

            private void ProcessDatasetsInternal(XmlNode n)
            {
                if (n == null)
                {
                    return;
                }

                Dictionary<string, int> dictionary = new Dictionary<string, int>();
                for (XmlNode xmlNode = n.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                {
                    if (xmlNode.NodeType == XmlNodeType.Element)
                    {
                        string text = Xml2Som.EscapeSom(xmlNode.LocalName);
                        int num2 = (dictionary[text] = (dictionary.ContainsKey(text) ? (dictionary[text] + 1) : 0));
                        if (HasChildren(xmlNode))
                        {
                            stack.Push(text + "[" + num2 + "]");
                            ProcessDatasetsInternal(xmlNode);
                            stack.Pop();
                        }
                        else
                        {
                            stack.Push(text + "[" + num2 + "]");
                            string text2 = PrintStack();
                            order.Add(text2);
                            InverseSearchAdd(text2);
                            name2Node[text2] = xmlNode;
                            stack.Pop();
                        }
                    }
                }
            }
        }

        public class AcroFieldsSearch : Xml2Som
        {
            private Dictionary<string, string> acroShort2LongName;

            public virtual Dictionary<string, string> AcroShort2LongName
            {
                get
                {
                    return acroShort2LongName;
                }
                set
                {
                    acroShort2LongName = value;
                }
            }

            public AcroFieldsSearch(ICollection<string> items)
            {
                inverseSearch = new Dictionary<string, InverseStore>();
                acroShort2LongName = new Dictionary<string, string>();
                foreach (string item in items)
                {
                    string shortName = Xml2Som.GetShortName(item);
                    acroShort2LongName[shortName] = item;
                    Xml2Som.InverseSearchAdd(inverseSearch, Xml2Som.SplitParts(shortName), item);
                }
            }
        }

        public class Xml2SomTemplate : Xml2Som
        {
            private bool dynamicForm;

            private int templateLevel;

            public virtual bool DynamicForm
            {
                get
                {
                    return dynamicForm;
                }
                set
                {
                    dynamicForm = value;
                }
            }

            public Xml2SomTemplate(XmlNode n)
            {
                order = new List<string>();
                name2Node = new Dictionary<string, XmlNode>();
                stack = new Stack2<string>();
                anform = 0;
                templateLevel = 0;
                inverseSearch = new Dictionary<string, InverseStore>();
                ProcessTemplate(n, null);
            }

            public virtual string GetFieldType(string s)
            {
                name2Node.TryGetValue(s, out var value);
                if (value == null)
                {
                    return null;
                }

                if ("exclGroup".Equals(value.LocalName))
                {
                    return "exclGroup";
                }

                XmlNode xmlNode = value.FirstChild;
                while (xmlNode != null && (xmlNode.NodeType != XmlNodeType.Element || !"ui".Equals(xmlNode.LocalName)))
                {
                    xmlNode = xmlNode.NextSibling;
                }

                if (xmlNode == null)
                {
                    return null;
                }

                for (XmlNode xmlNode2 = xmlNode.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                {
                    if (xmlNode2.NodeType == XmlNodeType.Element && (!"extras".Equals(xmlNode2.LocalName) || !"picture".Equals(xmlNode2.LocalName)))
                    {
                        return xmlNode2.LocalName;
                    }
                }

                return null;
            }

            private void ProcessTemplate(XmlNode n, Dictionary<string, int> ff)
            {
                if (ff == null)
                {
                    ff = new Dictionary<string, int>();
                }

                Dictionary<string, int> dictionary = new Dictionary<string, int>();
                for (XmlNode xmlNode = n.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                {
                    if (xmlNode.NodeType == XmlNodeType.Element)
                    {
                        string localName = xmlNode.LocalName;
                        if ("subform".Equals(localName))
                        {
                            XmlNode namedItem = xmlNode.Attributes!.GetNamedItem("name");
                            string text = "#subform";
                            bool flag = true;
                            if (namedItem != null)
                            {
                                text = Xml2Som.EscapeSom(namedItem.Value);
                                flag = false;
                            }

                            int num2;
                            if (!flag)
                            {
                                num2 = (dictionary[text] = (dictionary.ContainsKey(text) ? (dictionary[text] + 1) : 0));
                            }
                            else
                            {
                                num2 = anform;
                                anform++;
                            }

                            stack.Push(text + "[" + num2 + "]");
                            templateLevel++;
                            if (flag)
                            {
                                ProcessTemplate(xmlNode, ff);
                            }
                            else
                            {
                                ProcessTemplate(xmlNode, null);
                            }

                            templateLevel--;
                            stack.Pop();
                        }
                        else if ("field".Equals(localName) || "exclGroup".Equals(localName))
                        {
                            XmlNode namedItem2 = xmlNode.Attributes!.GetNamedItem("name");
                            if (namedItem2 != null)
                            {
                                string text2 = Xml2Som.EscapeSom(namedItem2.Value);
                                int num4 = (ff[text2] = (ff.ContainsKey(text2) ? (ff[text2] + 1) : 0));
                                stack.Push(text2 + "[" + num4 + "]");
                                string text3 = PrintStack();
                                order.Add(text3);
                                InverseSearchAdd(text3);
                                name2Node[text3] = xmlNode;
                                stack.Pop();
                            }
                        }
                        else if (!dynamicForm && templateLevel > 0 && "occur".Equals(localName))
                        {
                            int num5 = 1;
                            int num6 = 1;
                            int num7 = 1;
                            XmlNode namedItem3 = xmlNode.Attributes!.GetNamedItem("initial");
                            if (namedItem3 != null)
                            {
                                try
                                {
                                    num5 = int.Parse(namedItem3.Value!.Trim());
                                }
                                catch
                                {
                                }
                            }

                            namedItem3 = xmlNode.Attributes!.GetNamedItem("min");
                            if (namedItem3 != null)
                            {
                                try
                                {
                                    num6 = int.Parse(namedItem3.Value!.Trim());
                                }
                                catch
                                {
                                }
                            }

                            namedItem3 = xmlNode.Attributes!.GetNamedItem("max");
                            if (namedItem3 != null)
                            {
                                try
                                {
                                    num7 = int.Parse(namedItem3.Value!.Trim());
                                }
                                catch
                                {
                                }
                            }

                            if (num5 != num6 || num6 != num7)
                            {
                                dynamicForm = true;
                            }
                        }
                    }
                }
            }
        }

        private Xml2SomTemplate templateSom;

        private XmlNode templateNode;

        private Xml2SomDatasets datasetsSom;

        private AcroFieldsSearch acroFieldsSom;

        private PdfReader reader;

        private bool xfaPresent;

        private XmlDocument domDocument;

        private bool changed;

        private XmlNode datasetsNode;

        public const string XFA_DATA_SCHEMA = "http://www.xfa.org/schema/xfa-data/1.0/";

        public virtual bool XfaPresent
        {
            get
            {
                return xfaPresent;
            }
            set
            {
                xfaPresent = value;
            }
        }

        public virtual XmlDocument DomDocument
        {
            get
            {
                return domDocument;
            }
            set
            {
                domDocument = value;
                ExtractNodes();
            }
        }

        public virtual PdfReader Reader
        {
            get
            {
                return reader;
            }
            set
            {
                reader = value;
            }
        }

        public virtual bool Changed
        {
            get
            {
                return changed;
            }
            set
            {
                changed = value;
            }
        }

        public virtual Xml2SomTemplate TemplateSom
        {
            get
            {
                return templateSom;
            }
            set
            {
                templateSom = value;
            }
        }

        public virtual Xml2SomDatasets DatasetsSom
        {
            get
            {
                return datasetsSom;
            }
            set
            {
                datasetsSom = value;
            }
        }

        public virtual AcroFieldsSearch AcroFieldsSom
        {
            get
            {
                return acroFieldsSom;
            }
            set
            {
                acroFieldsSom = value;
            }
        }

        public virtual XmlNode DatasetsNode => datasetsNode;

        public XfaForm()
        {
        }

        public static PdfObject GetXfaObject(PdfReader reader)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.ACROFORM));
            if (pdfDictionary == null)
            {
                return null;
            }

            return PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.XFA));
        }

        public XfaForm(PdfReader reader)
        {
            this.reader = reader;
            PdfObject xfaObject = GetXfaObject(reader);
            if (xfaObject == null)
            {
                xfaPresent = false;
                return;
            }

            xfaPresent = true;
            MemoryStream memoryStream = new MemoryStream();
            if (xfaObject.IsArray())
            {
                PdfArray pdfArray = (PdfArray)xfaObject;
                for (int i = 1; i < pdfArray.Size; i += 2)
                {
                    PdfObject directObject = pdfArray.GetDirectObject(i);
                    if (directObject is PRStream)
                    {
                        byte[] streamBytes = PdfReader.GetStreamBytes((PRStream)directObject);
                        memoryStream.Write(streamBytes, 0, streamBytes.Length);
                    }
                }
            }
            else if (xfaObject is PRStream)
            {
                byte[] streamBytes2 = PdfReader.GetStreamBytes((PRStream)xfaObject);
                memoryStream.Write(streamBytes2, 0, streamBytes2.Length);
            }

            memoryStream.Seek(0L, SeekOrigin.Begin);
            XmlTextReader xmlTextReader = new XmlTextReader(memoryStream);
            domDocument = new XmlDocument();
            domDocument.PreserveWhitespace = true;
            domDocument.Load(xmlTextReader);
            ExtractNodes();
        }

        private void ExtractNodes()
        {
            Dictionary<string, XmlNode> dictionary = ExtractXFANodes(domDocument);
            if (dictionary.ContainsKey("template"))
            {
                templateNode = dictionary["template"];
                templateSom = new Xml2SomTemplate(templateNode);
            }

            if (dictionary.ContainsKey("datasets"))
            {
                datasetsNode = dictionary["datasets"];
                datasetsSom = new Xml2SomDatasets(datasetsNode.FirstChild);
            }

            if (datasetsNode == null)
            {
                CreateDatasetsNode(domDocument.FirstChild);
            }
        }

        public static Dictionary<string, XmlNode> ExtractXFANodes(XmlDocument domDocument)
        {
            Dictionary<string, XmlNode> dictionary = new Dictionary<string, XmlNode>();
            XmlNode xmlNode = domDocument.FirstChild;
            while (xmlNode.NodeType != XmlNodeType.Element || xmlNode.ChildNodes.Count == 0)
            {
                xmlNode = xmlNode.NextSibling;
            }

            for (xmlNode = xmlNode.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
            {
                if (xmlNode.NodeType == XmlNodeType.Element)
                {
                    string localName = xmlNode.LocalName;
                    dictionary[localName] = xmlNode;
                }
            }

            return dictionary;
        }

        private void CreateDatasetsNode(XmlNode n)
        {
            while (n.ChildNodes.Count == 0)
            {
                n = n.NextSibling;
            }

            if (n != null)
            {
                XmlElement xmlElement = n.OwnerDocument!.CreateElement("xfa", "datasets", "http://www.xfa.org/schema/xfa-data/1.0/");
                xmlElement.SetAttribute("xmlns:xfa", "http://www.xfa.org/schema/xfa-data/1.0/");
                datasetsNode = xmlElement;
                n.AppendChild(datasetsNode);
            }
        }

        public static void SetXfa(XfaForm form, PdfReader reader, PdfWriter writer)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.ACROFORM));
            if (pdfDictionary == null)
            {
                return;
            }

            PdfObject xfaObject = GetXfaObject(reader);
            if (xfaObject.IsArray())
            {
                PdfArray pdfArray = (PdfArray)xfaObject;
                int num = -1;
                int num2 = -1;
                for (int i = 0; i < pdfArray.Size; i += 2)
                {
                    PdfString asString = pdfArray.GetAsString(i);
                    if ("template".Equals(asString.ToString()))
                    {
                        num = i + 1;
                    }

                    if ("datasets".Equals(asString.ToString()))
                    {
                        num2 = i + 1;
                    }
                }

                if (num > -1 && num2 > -1)
                {
                    reader.KillXref(pdfArray.GetAsIndirectObject(num));
                    reader.KillXref(pdfArray.GetAsIndirectObject(num2));
                    PdfStream pdfStream = new PdfStream(SerializeDoc(form.templateNode));
                    pdfStream.FlateCompress(writer.CompressionLevel);
                    pdfArray[num] = writer.AddToBody(pdfStream).IndirectReference;
                    PdfStream pdfStream2 = new PdfStream(SerializeDoc(form.datasetsNode));
                    pdfStream2.FlateCompress(writer.CompressionLevel);
                    pdfArray[num2] = writer.AddToBody(pdfStream2).IndirectReference;
                    pdfDictionary.Put(PdfName.XFA, new PdfArray(pdfArray));
                    return;
                }
            }

            reader.KillXref(pdfDictionary.Get(PdfName.XFA));
            PdfStream pdfStream3 = new PdfStream(SerializeDoc(form.domDocument));
            pdfStream3.FlateCompress(writer.CompressionLevel);
            PdfIndirectReference indirectReference = writer.AddToBody(pdfStream3).IndirectReference;
            pdfDictionary.Put(PdfName.XFA, indirectReference);
        }

        public virtual void SetXfa(PdfWriter writer)
        {
            SetXfa(this, reader, writer);
        }

        public static byte[] SerializeDoc(XmlNode n)
        {
            XmlDomWriter xmlDomWriter = new XmlDomWriter();
            MemoryStream memoryStream = new MemoryStream();
            xmlDomWriter.SetOutput(memoryStream, null);
            xmlDomWriter.SetCanonical(canonical: false);
            xmlDomWriter.Write(n);
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        public virtual string FindFieldName(string name, AcroFields af)
        {
            IDictionary<string, AcroFields.Item> fields = af.Fields;
            if (fields.ContainsKey(name))
            {
                return name;
            }

            if (acroFieldsSom == null)
            {
                if (fields.Count == 0 && xfaPresent)
                {
                    acroFieldsSom = new AcroFieldsSearch(datasetsSom.Name2Node.Keys);
                }
                else
                {
                    acroFieldsSom = new AcroFieldsSearch(fields.Keys);
                }
            }

            if (acroFieldsSom.AcroShort2LongName.ContainsKey(name))
            {
                return acroFieldsSom.AcroShort2LongName[name];
            }

            return acroFieldsSom.InverseSearchGlobal(Xml2Som.SplitParts(name));
        }

        public virtual string FindDatasetsName(string name)
        {
            if (datasetsSom.Name2Node.ContainsKey(name))
            {
                return name;
            }

            return datasetsSom.InverseSearchGlobal(Xml2Som.SplitParts(name));
        }

        public virtual XmlNode FindDatasetsNode(string name)
        {
            if (name == null)
            {
                return null;
            }

            name = FindDatasetsName(name);
            if (name == null)
            {
                return null;
            }

            return datasetsSom.Name2Node[name];
        }

        public static string GetNodeText(XmlNode n)
        {
            if (n == null)
            {
                return "";
            }

            return GetNodeText(n, "");
        }

        private static string GetNodeText(XmlNode n, string name)
        {
            for (XmlNode xmlNode = n.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
            {
                if (xmlNode.NodeType == XmlNodeType.Element)
                {
                    name = GetNodeText(xmlNode, name);
                }
                else if (xmlNode.NodeType == XmlNodeType.Text)
                {
                    name += xmlNode.Value;
                }
            }

            return name;
        }

        public virtual void SetNodeText(XmlNode n, string text)
        {
            if (n != null)
            {
                XmlNode xmlNode = null;
                while ((xmlNode = n.FirstChild) != null)
                {
                    n.RemoveChild(xmlNode);
                }

                n.Attributes!.RemoveNamedItem("dataNode", "http://www.xfa.org/schema/xfa-data/1.0/");
                n.AppendChild(domDocument.CreateTextNode(text));
                changed = true;
            }
        }

        public virtual void FillXfaForm(string file)
        {
            using FileStream stream = new FileStream(file, FileMode.Open);
            FillXfaForm(stream, readOnly: false);
        }

        public virtual void FillXfaForm(Stream stream)
        {
            FillXfaForm(stream, readOnly: false);
        }

        public virtual void FillXfaForm(Stream stream, bool readOnly)
        {
            FillXfaForm(new XmlTextReader(stream), readOnly);
        }

        public virtual void FillXfaForm(XmlReader reader)
        {
            FillXfaForm(reader, readOnly: false);
        }

        public virtual void FillXfaForm(XmlReader reader, bool readOnly)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(reader);
            FillXfaForm(xmlDocument.DocumentElement);
        }

        public virtual void FillXfaForm(XmlNode node)
        {
            FillXfaForm(node, readOnly: false);
        }

        public virtual void FillXfaForm(XmlNode node, bool readOnly)
        {
            if (readOnly)
            {
                XmlNodeList elementsByTagName = domDocument.GetElementsByTagName("field");
                for (int i = 0; i < elementsByTagName.Count; i++)
                {
                    ((XmlElement)elementsByTagName.Item(i)).SetAttribute("access", "readOnly");
                }
            }

            XmlNodeList childNodes = datasetsNode.ChildNodes;
            XmlNode xmlNode = null;
            foreach (XmlNode item in childNodes)
            {
                if (item.NodeType == XmlNodeType.Element && item.LocalName.Equals("data") && "http://www.xfa.org/schema/xfa-data/1.0/".Equals(item.NamespaceURI))
                {
                    xmlNode = item;
                    break;
                }
            }

            if (xmlNode == null)
            {
                xmlNode = datasetsNode.OwnerDocument!.CreateElement("xfa:data", "http://www.xfa.org/schema/xfa-data/1.0/");
                datasetsNode.AppendChild(xmlNode);
            }

            if (xmlNode.ChildNodes.Count == 0)
            {
                xmlNode.AppendChild(domDocument.ImportNode(node, deep: true));
            }
            else
            {
                XmlNode firstElementNode = GetFirstElementNode(xmlNode);
                if (firstElementNode != null)
                {
                    xmlNode.ReplaceChild(domDocument.ImportNode(node, deep: true), firstElementNode);
                }
            }

            ExtractNodes();
            Changed = true;
        }

        private XmlNode GetFirstElementNode(XmlNode src)
        {
            XmlNode result = null;
            XmlNodeList childNodes = src.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                if (childNodes[i]!.NodeType == XmlNodeType.Element)
                {
                    result = childNodes[i];
                    break;
                }
            }

            return result;
        }
    }
}
