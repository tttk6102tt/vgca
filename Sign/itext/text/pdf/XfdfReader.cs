using Sign.itext.error_messages;
using Sign.itext.xml.simpleparser;

namespace Sign.itext.text.pdf
{
    public class XfdfReader : ISimpleXMLDocHandler
    {
        internal class Stackr : List<string>
        {
            internal void Push(string obj)
            {
                Add(obj);
            }

            internal string Pop()
            {
                if (base.Count == 0)
                {
                    throw new InvalidOperationException(MessageLocalization.GetComposedMessage("the.stack.is.empty"));
                }

                string result = base[base.Count - 1];
                RemoveAt(base.Count - 1);
                return result;
            }
        }

        private bool foundRoot;

        private readonly Stackr fieldNames = new Stackr();

        private readonly Stackr fieldValues = new Stackr();

        internal Dictionary<string, string> fields;

        protected Dictionary<string, List<string>> listFields;

        internal string fileSpec;

        public virtual Dictionary<string, string> Fields => fields;

        public virtual string FileSpec => fileSpec;

        public XfdfReader(string filename)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                SimpleXMLParser.Parse(this, fileStream);
            }
            finally
            {
                try
                {
                    fileStream?.Close();
                }
                catch
                {
                }
            }
        }

        public XfdfReader(byte[] xfdfIn)
            : this(new MemoryStream(xfdfIn))
        {
        }

        public XfdfReader(Stream isp)
        {
            SimpleXMLParser.Parse(this, isp);
        }

        public virtual string GetField(string name)
        {
            if (fields.ContainsKey(name))
            {
                return fields[name];
            }

            return null;
        }

        public virtual string GetFieldValue(string name)
        {
            return GetField(name);
        }

        public virtual List<string> GetListValues(string name)
        {
            if (listFields.ContainsKey(name))
            {
                return listFields[name];
            }

            return null;
        }

        public virtual void StartElement(string tag, IDictionary<string, string> h)
        {
            if (!foundRoot)
            {
                if (!tag.Equals("xfdf"))
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("root.element.is.not.xfdf.1", tag));
                }

                foundRoot = true;
            }

            if (!tag.Equals("xfdf"))
            {
                if (tag.Equals("f"))
                {
                    h.TryGetValue("href", out fileSpec);
                }
                else if (tag.Equals("fields"))
                {
                    fields = new Dictionary<string, string>();
                    listFields = new Dictionary<string, List<string>>();
                }
                else if (tag.Equals("field"))
                {
                    h.TryGetValue("name", out var value);
                    fieldNames.Push(value);
                }
                else if (tag.Equals("value"))
                {
                    fieldValues.Push("");
                }
            }
        }

        public virtual void EndElement(string tag)
        {
            if (tag.Equals("value"))
            {
                string text = "";
                for (int i = 0; i < fieldNames.Count; i++)
                {
                    text = text + "." + fieldNames[i];
                }

                if (text.StartsWith("."))
                {
                    text = text.Substring(1);
                }

                string text2 = fieldValues.Pop();
                fields.TryGetValue(text, out var value);
                fields[text] = text2;
                if (value != null)
                {
                    listFields.TryGetValue(text, out var value2);
                    if (value2 == null)
                    {
                        value2 = new List<string>();
                        value2.Add(value);
                    }

                    value2.Add(text2);
                    listFields[text] = value2;
                }
            }
            else if (tag.Equals("field") && fieldNames.Count != 0)
            {
                fieldNames.Pop();
            }
        }

        public virtual void StartDocument()
        {
            fileSpec = "";
        }

        public virtual void EndDocument()
        {
        }

        public virtual void Text(string str)
        {
            if (fieldNames.Count != 0 && fieldValues.Count != 0)
            {
                string text = fieldValues.Pop();
                text += str;
                fieldValues.Push(text);
            }
        }
    }
}
