using Sign.itext.xml.xmp.impl.xpath;
using Sign.itext.xml.xmp.options;
using Sign.itext.xml.xmp.properties;
using System.Collections;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpIteratorImpl : IXmpIterator, IEnumerator
    {
        private class NodeIterator : IEnumerator
        {
            private class XmpPropertyInfoImpl : IXmpPropertyInfo, IXmpProperty
            {
                private readonly string _baseNs;

                private readonly XmpNode _node;

                private readonly string _path;

                private readonly string _value;

                public virtual string Namespace
                {
                    get
                    {
                        if (!_node.Options.SchemaNode)
                        {
                            QName qName = new QName(_node.Name);
                            return XmpMetaFactory.SchemaRegistry.GetNamespaceUri(qName.Prefix);
                        }

                        return _baseNs;
                    }
                }

                public virtual string Path => _path;

                public virtual string Value => _value;

                public virtual PropertyOptions Options => _node.Options;

                public virtual string Language => null;

                public XmpPropertyInfoImpl(XmpNode node, string baseNs, string path, string value)
                {
                    _node = node;
                    _baseNs = baseNs;
                    _path = path;
                    _value = value;
                }
            }

            private const int ITERATE_NODE = 0;

            private const int ITERATE_CHILDREN = 1;

            private const int ITERATE_QUALIFIER = 2;

            private static readonly IList EmptyList = new ArrayList();

            private readonly XmpIteratorImpl _outerInstance;

            private readonly string _path;

            private readonly XmpNode _visitedNode;

            private IEnumerator _childrenIterator;

            private int _index;

            private IXmpPropertyInfo _returnProperty;

            private int _state;

            private IEnumerator _subIterator = EmptyList.GetEnumerator();

            protected internal virtual IEnumerator ChildrenIterator
            {
                get
                {
                    return _childrenIterator;
                }
                set
                {
                    _childrenIterator = value;
                }
            }

            protected internal virtual IXmpPropertyInfo ReturnProperty
            {
                get
                {
                    return _returnProperty;
                }
                set
                {
                    _returnProperty = value;
                }
            }

            public virtual object Current => _returnProperty;

            public NodeIterator(XmpIteratorImpl outerInstance, XmpNode visitedNode, string parentPath, int index)
            {
                _outerInstance = outerInstance;
                _visitedNode = visitedNode;
                _state = 0;
                if (visitedNode.Options.SchemaNode)
                {
                    outerInstance.BaseNs = visitedNode.Name;
                }

                _path = AccumulatePath(visitedNode, parentPath, index);
            }

            public virtual bool MoveNext()
            {
                if (_state == 0)
                {
                    return ReportNode();
                }

                if (_state == 1)
                {
                    if (_childrenIterator == null)
                    {
                        _childrenIterator = _visitedNode.IterateChildren();
                    }

                    bool flag = IterateChildren(_childrenIterator);
                    if (!flag && _visitedNode.HasQualifier() && !_outerInstance.Options.OmitQualifiers)
                    {
                        _state = 2;
                        _childrenIterator = null;
                        flag = MoveNext();
                    }

                    return flag;
                }

                if (_childrenIterator == null)
                {
                    _childrenIterator = _visitedNode.IterateQualifier();
                }

                return IterateChildren(_childrenIterator);
            }

            public virtual void Reset()
            {
                throw new NotSupportedException();
            }

            protected internal virtual bool ReportNode()
            {
                _state = 1;
                if (_visitedNode.Parent != null && (!_outerInstance.Options.JustLeafnodes || !_visitedNode.HasChildren()))
                {
                    _returnProperty = CreatePropertyInfo(_visitedNode, _outerInstance.BaseNs, _path);
                    return true;
                }

                return MoveNext();
            }

            private bool IterateChildren(IEnumerator iterator)
            {
                if (_outerInstance._skipSiblings)
                {
                    _outerInstance._skipSiblings = false;
                    _subIterator = EmptyList.GetEnumerator();
                }

                bool num = _subIterator.MoveNext();
                if (!num && iterator.MoveNext())
                {
                    XmpNode visitedNode = (XmpNode)iterator.Current;
                    _index++;
                    _subIterator = new NodeIterator(_outerInstance, visitedNode, _path, _index);
                }

                if (num)
                {
                    _returnProperty = (IXmpPropertyInfo)_subIterator.Current;
                    return true;
                }

                return false;
            }

            protected internal virtual string AccumulatePath(XmpNode currNode, string parentPath, int currentIndex)
            {
                if (currNode.Parent == null || currNode.Options.SchemaNode)
                {
                    return null;
                }

                string text;
                string text2;
                if (currNode.Parent.Options.Array)
                {
                    text = "";
                    text2 = "[" + Convert.ToString(currentIndex) + "]";
                }
                else
                {
                    text = "/";
                    text2 = currNode.Name;
                }

                if (string.IsNullOrEmpty(parentPath))
                {
                    return text2;
                }

                if (_outerInstance.Options.JustLeafname)
                {
                    if (text2.StartsWith("?"))
                    {
                        return text2.Substring(1);
                    }

                    return text2;
                }

                return parentPath + text + text2;
            }

            protected internal virtual IXmpPropertyInfo CreatePropertyInfo(XmpNode node, string baseNs, string path)
            {
                string value = (node.Options.SchemaNode ? null : node.Value);
                return new XmpPropertyInfoImpl(node, baseNs, path, value);
            }
        }

        private class NodeIteratorChildren : NodeIterator
        {
            private readonly IEnumerator _childrenIterator;

            private readonly XmpIteratorImpl _outerInstance;

            private readonly string _parentPath;

            private int _index;

            public NodeIteratorChildren(XmpIteratorImpl outerInstance, XmpNode parentNode, string parentPath)
                : base(outerInstance, parentNode, parentPath, 0)
            {
                _outerInstance = outerInstance;
                if (parentNode.Options.SchemaNode)
                {
                    outerInstance.BaseNs = parentNode.Name;
                }

                _parentPath = AccumulatePath(parentNode, parentPath, 1);
                _childrenIterator = parentNode.IterateChildren();
            }

            public override bool MoveNext()
            {
                if (_outerInstance._skipSiblings)
                {
                    return false;
                }

                if (_childrenIterator.MoveNext())
                {
                    XmpNode xmpNode = (XmpNode)_childrenIterator.Current;
                    if (xmpNode != null)
                    {
                        _index++;
                        string path = null;
                        if (xmpNode.Options.SchemaNode)
                        {
                            _outerInstance.BaseNs = xmpNode.Name;
                        }
                        else if (xmpNode.Parent != null)
                        {
                            path = AccumulatePath(xmpNode, _parentPath, _index);
                        }

                        if (!_outerInstance.Options.JustLeafnodes || !xmpNode.HasChildren())
                        {
                            ReturnProperty = CreatePropertyInfo(xmpNode, _outerInstance.BaseNs, path);
                            return true;
                        }
                    }

                    return MoveNext();
                }

                return false;
            }
        }

        private static readonly IList EmptyList = new ArrayList();

        private readonly IEnumerator _nodeIterator;

        private readonly IteratorOptions _options;

        private string _baseNs;

        private bool _skipSiblings;

        protected internal bool skipSubtree;

        protected internal virtual IteratorOptions Options => _options;

        protected internal virtual string BaseNs
        {
            get
            {
                return _baseNs;
            }
            set
            {
                _baseNs = value;
            }
        }

        public virtual object Current => _nodeIterator.Current;

        public XmpIteratorImpl(XmpMetaImpl xmp, string schemaNs, string propPath, IteratorOptions options)
        {
            _options = options ?? new IteratorOptions();
            string parentPath = null;
            bool flag = !string.IsNullOrEmpty(schemaNs);
            bool flag2 = !string.IsNullOrEmpty(propPath);
            XmpNode xmpNode;
            if (!flag && !flag2)
            {
                xmpNode = xmp.Root;
            }
            else if (flag && flag2)
            {
                XmpPath xmpPath = XmpPathParser.ExpandXPath(schemaNs, propPath);
                XmpPath xmpPath2 = new XmpPath();
                for (int i = 0; i < xmpPath.Size() - 1; i++)
                {
                    xmpPath2.Add(xmpPath.GetSegment(i));
                }

                xmpNode = XmpNodeUtils.FindNode(xmp.Root, xmpPath, createNodes: false, null);
                _baseNs = schemaNs;
                parentPath = xmpPath2.ToString();
            }
            else
            {
                if (!flag || flag2)
                {
                    throw new XmpException("Schema namespace URI is required", 101);
                }

                xmpNode = XmpNodeUtils.FindSchemaNode(xmp.Root, schemaNs, createNodes: false);
            }

            if (xmpNode != null)
            {
                _nodeIterator = ((!_options.JustChildren) ? new NodeIterator(this, xmpNode, parentPath, 1) : new NodeIteratorChildren(this, xmpNode, parentPath));
            }
            else
            {
                _nodeIterator = EmptyList.GetEnumerator();
            }
        }

        public virtual void SkipSubtree()
        {
            skipSubtree = true;
        }

        public virtual void SkipSiblings()
        {
            SkipSubtree();
            _skipSiblings = true;
        }

        public virtual bool MoveNext()
        {
            return _nodeIterator.MoveNext();
        }

        public virtual void Reset()
        {
            _nodeIterator.Reset();
        }
    }
}
