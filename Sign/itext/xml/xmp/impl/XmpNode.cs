using Sign.itext.xml.xmp.options;
using System.Collections;
using System.Text;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpNode : IComparable, ICloneable
    {
        private static readonly IList EmptyList = new ArrayList();

        private bool _alias;

        private IList _children;

        private bool _hasAliases;

        private bool _hasValueChild;

        private bool _implicit;

        private string _name;

        private PropertyOptions _options;

        private XmpNode _parent;

        private IList _qualifier;

        private string _value;

        public virtual XmpNode Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }

        public virtual int ChildrenLength
        {
            get
            {
                if (_children == null)
                {
                    return 0;
                }

                return _children.Count;
            }
        }

        public virtual int QualifierLength
        {
            get
            {
                if (_qualifier == null)
                {
                    return 0;
                }

                return _qualifier.Count;
            }
        }

        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public virtual string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public virtual PropertyOptions Options
        {
            get
            {
                _options = _options ?? new PropertyOptions();
                return _options;
            }
            set
            {
                _options = value;
            }
        }

        public virtual bool Implicit
        {
            get
            {
                return _implicit;
            }
            set
            {
                _implicit = value;
            }
        }

        public virtual bool HasAliases
        {
            get
            {
                return _hasAliases;
            }
            set
            {
                _hasAliases = value;
            }
        }

        public virtual bool Alias
        {
            get
            {
                return _alias;
            }
            set
            {
                _alias = value;
            }
        }

        public virtual bool HasValueChild
        {
            get
            {
                return _hasValueChild;
            }
            set
            {
                _hasValueChild = value;
            }
        }

        private bool LanguageNode => "xml:lang".Equals(_name);

        private bool TypeNode => "rdf:type".Equals(_name);

        internal IList Children
        {
            get
            {
                _children = _children ?? new ArrayList();
                return _children;
            }
        }

        public virtual IList UnmodifiableChildren => ArrayList.ReadOnly(new ArrayList(new ArrayList(Children)));

        private IList Qualifier
        {
            get
            {
                _qualifier = _qualifier ?? new ArrayList(0);
                return _qualifier;
            }
        }

        public XmpNode(string name, string value, PropertyOptions options)
        {
            _name = name;
            _value = value;
            _options = options;
        }

        public XmpNode(string name, PropertyOptions options)
            : this(name, null, options)
        {
        }

        public virtual object Clone()
        {
            PropertyOptions options;
            try
            {
                options = new PropertyOptions(Options.Options);
            }
            catch (XmpException)
            {
                options = new PropertyOptions();
            }

            XmpNode xmpNode = new XmpNode(_name, _value, options);
            CloneSubtree(xmpNode);
            return xmpNode;
        }

        public virtual int CompareTo(object xmpNode)
        {
            if (Options.SchemaNode)
            {
                return _value.CompareTo(((XmpNode)xmpNode).Value);
            }

            return _name.CompareTo(((XmpNode)xmpNode).Name);
        }

        public virtual void Clear()
        {
            _options = null;
            _name = null;
            _value = null;
            _children = null;
            _qualifier = null;
        }

        public virtual XmpNode GetChild(int index)
        {
            return (XmpNode)Children[index - 1];
        }

        public virtual void AddChild(XmpNode node)
        {
            AssertChildNotExisting(node.Name);
            node.Parent = this;
            Children.Add(node);
        }

        public virtual void AddChild(int index, XmpNode node)
        {
            AssertChildNotExisting(node.Name);
            node.Parent = this;
            Children.Insert(index - 1, node);
        }

        public virtual void ReplaceChild(int index, XmpNode node)
        {
            node.Parent = this;
            Children[index - 1] = node;
        }

        public virtual void RemoveChild(int itemIndex)
        {
            Children.RemoveAt(itemIndex - 1);
            CleanupChildren();
        }

        public virtual void RemoveChild(XmpNode node)
        {
            Children.Remove(node);
            CleanupChildren();
        }

        protected internal virtual void CleanupChildren()
        {
            if (_children.Count == 0)
            {
                _children = null;
            }
        }

        public virtual void RemoveChildren()
        {
            _children = null;
        }

        public virtual XmpNode FindChildByName(string expr)
        {
            return find(Children, expr);
        }

        public virtual XmpNode GetQualifier(int index)
        {
            return (XmpNode)Qualifier[index - 1];
        }

        public virtual void AddQualifier(XmpNode qualNode)
        {
            AssertQualifierNotExisting(qualNode.Name);
            qualNode.Parent = this;
            qualNode.Options.Qualifier = true;
            Options.HasQualifiers = true;
            if (qualNode.LanguageNode)
            {
                _options.HasLanguage = true;
                Qualifier.Insert(0, qualNode);
            }
            else if (qualNode.TypeNode)
            {
                _options.HasType = true;
                Qualifier.Insert(_options.HasLanguage ? 1 : 0, qualNode);
            }
            else
            {
                Qualifier.Add(qualNode);
            }
        }

        public virtual void RemoveQualifier(XmpNode qualNode)
        {
            PropertyOptions options = Options;
            if (qualNode.LanguageNode)
            {
                options.HasLanguage = false;
            }
            else if (qualNode.TypeNode)
            {
                options.HasType = false;
            }

            Qualifier.Remove(qualNode);
            if (_qualifier.Count == 0)
            {
                options.HasQualifiers = false;
                _qualifier = null;
            }
        }

        public virtual void RemoveQualifiers()
        {
            PropertyOptions options = Options;
            options.HasQualifiers = false;
            options.HasLanguage = false;
            options.HasType = false;
            _qualifier = null;
        }

        public virtual XmpNode FindQualifierByName(string expr)
        {
            return find(_qualifier, expr);
        }

        public virtual bool HasChildren()
        {
            if (_children != null)
            {
                return _children.Count > 0;
            }

            return false;
        }

        public virtual IEnumerator IterateChildren()
        {
            if (_children != null)
            {
                return Children.GetEnumerator();
            }

            return EmptyList.GetEnumerator();
        }

        public virtual bool HasQualifier()
        {
            if (_qualifier != null)
            {
                return _qualifier.Count > 0;
            }

            return false;
        }

        public virtual IEnumerator IterateQualifier()
        {
            if (_qualifier != null)
            {
                return Qualifier.GetEnumerator();
            }

            return EmptyList.GetEnumerator();
        }

        public virtual void CloneSubtree(XmpNode destination)
        {
            try
            {
                foreach (XmpNode child in Children)
                {
                    destination.AddChild((XmpNode)child.Clone());
                }

                foreach (XmpNode item in Qualifier)
                {
                    destination.AddQualifier((XmpNode)item.Clone());
                }
            }
            catch (XmpException)
            {
            }
        }

        public virtual string DumpNode(bool recursive)
        {
            StringBuilder stringBuilder = new StringBuilder(512);
            DumpNode(stringBuilder, recursive, 0, 0);
            return stringBuilder.ToString();
        }

        public virtual void Sort()
        {
            if (HasQualifier())
            {
                XmpNode[] array = new XmpNode[Qualifier.Count];
                Qualifier.CopyTo(array, 0);
                int i;
                for (i = 0; array.Length > i && ("xml:lang".Equals(array[i].Name) || "rdf:type".Equals(array[i].Name)); i++)
                {
                    array[i].Sort();
                }

                Array.Sort(array, i, array.Length - i);
                for (int j = 0; j < array.Length; j++)
                {
                    _qualifier[j] = array[j];
                    array[j].Sort();
                }
            }

            if (!HasChildren())
            {
                return;
            }

            if (!Options.Array)
            {
                ArrayList.Adapter(_children).Sort();
            }

            IEnumerator enumerator = IterateChildren();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null)
                {
                    ((XmpNode)enumerator.Current).Sort();
                }
            }
        }

        private void DumpNode(StringBuilder result, bool recursive, int indent, int index)
        {
            for (int i = 0; i < indent; i++)
            {
                result.Append('\t');
            }

            if (_parent != null)
            {
                if (Options.Qualifier)
                {
                    result.Append('?');
                    result.Append(_name);
                }
                else if (Parent.Options.Array)
                {
                    result.Append('[');
                    result.Append(index);
                    result.Append(']');
                }
                else
                {
                    result.Append(_name);
                }
            }
            else
            {
                result.Append("ROOT NODE");
                if (!string.IsNullOrEmpty(_name))
                {
                    result.Append(" (");
                    result.Append(_name);
                    result.Append(')');
                }
            }

            if (!string.IsNullOrEmpty(_value))
            {
                result.Append(" = \"");
                result.Append(_value);
                result.Append('"');
            }

            if (Options.ContainsOneOf(uint.MaxValue))
            {
                result.Append("\t(");
                result.Append(Options.ToString());
                result.Append(" : ");
                result.Append(Options.OptionsString);
                result.Append(')');
            }

            result.Append('\n');
            if (recursive && HasQualifier())
            {
                XmpNode[] array = new XmpNode[Qualifier.Count];
                Qualifier.CopyTo(array, 0);
                int j;
                for (j = 0; array.Length > j && ("xml:lang".Equals(array[j].Name) || "rdf:type".Equals(array[j].Name)); j++)
                {
                }

                Array.Sort(array, j, array.Length - j);
                for (j = 0; j < array.Length; j++)
                {
                    array[j].DumpNode(result, recursive, indent + 2, j + 1);
                }
            }

            if (recursive && HasChildren())
            {
                XmpNode[] array2 = new XmpNode[Children.Count];
                Children.CopyTo(array2, 0);
                if (!Options.Array)
                {
                    Array.Sort(array2);
                }

                for (int k = 0; k < array2.Length; k++)
                {
                    array2[k].DumpNode(result, recursive, indent + 1, k + 1);
                }
            }
        }

        private XmpNode find(IList list, string expr)
        {
            if (list != null)
            {
                foreach (XmpNode item in list)
                {
                    if (item.Name.Equals(expr))
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private void AssertChildNotExisting(string childName)
        {
            if (!"[]".Equals(childName) && FindChildByName(childName) != null)
            {
                throw new XmpException("Duplicate property or field node '" + childName + "'", 203);
            }
        }

        private void AssertQualifierNotExisting(string qualifierName)
        {
            if (!"[]".Equals(qualifierName) && FindQualifierByName(qualifierName) != null)
            {
                throw new XmpException("Duplicate '" + qualifierName + "' qualifier", 203);
            }
        }
    }
}
