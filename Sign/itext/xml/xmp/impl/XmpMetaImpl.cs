using Sign.itext.xml.xmp.impl.xpath;
using Sign.itext.xml.xmp.options;
using Sign.itext.xml.xmp.properties;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpMetaImpl : XmpConst, IXmpMeta, ICloneable
    {
        private class XmpPropertyImpl1 : IXmpProperty
        {
            private readonly XmpNode _itemNode;

            public virtual string Value => _itemNode.Value;

            public virtual PropertyOptions Options => _itemNode.Options;

            public virtual string Language => _itemNode.GetQualifier(1).Value;

            public XmpPropertyImpl1(XmpNode itemNode)
            {
                _itemNode = itemNode;
            }

            public override string ToString()
            {
                return _itemNode.Value;
            }
        }

        private class XmpPropertyImpl2 : IXmpProperty
        {
            private readonly XmpNode _propNode;

            private readonly object _value;

            public virtual string Value
            {
                get
                {
                    if (_value == null)
                    {
                        return null;
                    }

                    return _value.ToString();
                }
            }

            public virtual PropertyOptions Options => _propNode.Options;

            public virtual string Language => null;

            public XmpPropertyImpl2(XmpNode propNode, object value)
            {
                _value = value;
                _propNode = propNode;
            }

            public override string ToString()
            {
                return _value.ToString();
            }
        }

        private const int VALUE_STRING = 0;

        private const int VALUE_BOOLEAN = 1;

        private const int VALUE_INTEGER = 2;

        private const int VALUE_LONG = 3;

        private const int VALUE_DOUBLE = 4;

        private const int VALUE_DATE = 5;

        private const int VALUE_CALENDAR = 6;

        private const int VALUE_BASE64 = 7;

        private readonly XmpNode _tree;

        private string _packetHeader;

        public virtual XmpNode Root => _tree;

        public virtual string ObjectName
        {
            get
            {
                return _tree.Name ?? "";
            }
            set
            {
                _tree.Name = value;
            }
        }

        public virtual string PacketHeader
        {
            get
            {
                return _packetHeader;
            }
            set
            {
                _packetHeader = value;
            }
        }

        public XmpMetaImpl()
        {
            _tree = new XmpNode(null, null, null);
        }

        public XmpMetaImpl(XmpNode tree)
        {
            _tree = tree;
        }

        public virtual void AppendArrayItem(string schemaNs, string arrayName, PropertyOptions arrayOptions, string itemValue, PropertyOptions itemOptions)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            if (arrayOptions == null)
            {
                arrayOptions = new PropertyOptions();
            }

            if (!arrayOptions.OnlyArrayOptions)
            {
                throw new XmpException("Only array form flags allowed for arrayOptions", 103);
            }

            arrayOptions = XmpNodeUtils.VerifySetOptions(arrayOptions, null);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null);
            if (xmpNode != null)
            {
                if (!xmpNode.Options.Array)
                {
                    throw new XmpException("The named property is not an array", 102);
                }
            }
            else
            {
                if (!arrayOptions.Array)
                {
                    throw new XmpException("Explicit arrayOptions required to create new array", 103);
                }

                xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: true, arrayOptions);
                if (xmpNode == null)
                {
                    throw new XmpException("Failure creating array node", 102);
                }
            }

            DoSetArrayItem(xmpNode, -1, itemValue, itemOptions, insert: true);
        }

        public virtual void AppendArrayItem(string schemaNs, string arrayName, string itemValue)
        {
            AppendArrayItem(schemaNs, arrayName, null, itemValue, null);
        }

        public virtual int CountArrayItems(string schemaNs, string arrayName)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null);
            if (xmpNode == null)
            {
                return 0;
            }

            if (xmpNode.Options.Array)
            {
                return xmpNode.ChildrenLength;
            }

            throw new XmpException("The named property is not an array", 102);
        }

        public virtual void DeleteArrayItem(string schemaNs, string arrayName, int itemIndex)
        {
            try
            {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertArrayName(arrayName);
                string propName = XmpPathFactory.ComposeArrayItemPath(arrayName, itemIndex);
                DeleteProperty(schemaNs, propName);
            }
            catch (XmpException)
            {
            }
        }

        public virtual void DeleteProperty(string schemaNs, string propName)
        {
            try
            {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertPropName(propName);
                XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, propName);
                XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null);
                if (xmpNode != null)
                {
                    XmpNodeUtils.DeleteNode(xmpNode);
                }
            }
            catch (XmpException)
            {
            }
        }

        public virtual void DeleteQualifier(string schemaNs, string propName, string qualNs, string qualName)
        {
            try
            {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertPropName(propName);
                string propName2 = propName + XmpPathFactory.ComposeQualifierPath(qualNs, qualName);
                DeleteProperty(schemaNs, propName2);
            }
            catch (XmpException)
            {
            }
        }

        public virtual void DeleteStructField(string schemaNs, string structName, string fieldNs, string fieldName)
        {
            try
            {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertStructName(structName);
                string propName = structName + XmpPathFactory.ComposeStructFieldPath(fieldNs, fieldName);
                DeleteProperty(schemaNs, propName);
            }
            catch (XmpException)
            {
            }
        }

        public virtual bool DoesPropertyExist(string schemaNs, string propName)
        {
            try
            {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertPropName(propName);
                XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, propName);
                return XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null) != null;
            }
            catch (XmpException)
            {
                return false;
            }
        }

        public virtual bool DoesArrayItemExist(string schemaNs, string arrayName, int itemIndex)
        {
            try
            {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertArrayName(arrayName);
                string propName = XmpPathFactory.ComposeArrayItemPath(arrayName, itemIndex);
                return DoesPropertyExist(schemaNs, propName);
            }
            catch (XmpException)
            {
                return false;
            }
        }

        public virtual bool DoesStructFieldExist(string schemaNs, string structName, string fieldNs, string fieldName)
        {
            try
            {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertStructName(structName);
                string text = XmpPathFactory.ComposeStructFieldPath(fieldNs, fieldName);
                return DoesPropertyExist(schemaNs, structName + text);
            }
            catch (XmpException)
            {
                return false;
            }
        }

        public virtual bool DoesQualifierExist(string schemaNs, string propName, string qualNs, string qualName)
        {
            try
            {
                ParameterAsserts.AssertSchemaNs(schemaNs);
                ParameterAsserts.AssertPropName(propName);
                string text = XmpPathFactory.ComposeQualifierPath(qualNs, qualName);
                return DoesPropertyExist(schemaNs, propName + text);
            }
            catch (XmpException)
            {
                return false;
            }
        }

        public virtual IXmpProperty GetArrayItem(string schemaNs, string arrayName, int itemIndex)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            string propName = XmpPathFactory.ComposeArrayItemPath(arrayName, itemIndex);
            return GetProperty(schemaNs, propName);
        }

        public virtual IXmpProperty GetLocalizedText(string schemaNs, string altTextName, string genericLang, string specificLang)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(altTextName);
            ParameterAsserts.AssertSpecificLang(specificLang);
            genericLang = ((genericLang != null) ? Utils.NormalizeLangValue(genericLang) : null);
            specificLang = Utils.NormalizeLangValue(specificLang);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, altTextName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null);
            if (xmpNode == null)
            {
                return null;
            }

            object[] array = XmpNodeUtils.ChooseLocalizedText(xmpNode, genericLang, specificLang);
            int value = ((int?)array[0]).Value;
            XmpNode itemNode = (XmpNode)array[1];
            if (value != 0)
            {
                return new XmpPropertyImpl1(itemNode);
            }

            return null;
        }

        public virtual void SetLocalizedText(string schemaNs, string altTextName, string genericLang, string specificLang, string itemValue, PropertyOptions options)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(altTextName);
            ParameterAsserts.AssertSpecificLang(specificLang);
            genericLang = ((genericLang != null) ? Utils.NormalizeLangValue(genericLang) : null);
            specificLang = Utils.NormalizeLangValue(specificLang);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, altTextName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: true, new PropertyOptions(7680u));
            if (xmpNode == null)
            {
                throw new XmpException("Failed to find or create array node", 102);
            }

            if (!xmpNode.Options.ArrayAltText)
            {
                if (xmpNode.HasChildren() || !xmpNode.Options.ArrayAlternate)
                {
                    throw new XmpException("Specified property is no alt-text array", 102);
                }

                xmpNode.Options.ArrayAltText = true;
            }

            bool flag = false;
            XmpNode xmpNode2 = null;
            foreach (XmpNode child in xmpNode.Children)
            {
                if (!child.HasQualifier() || !"xml:lang".Equals(child.GetQualifier(1).Name))
                {
                    throw new XmpException("Language qualifier must be first", 102);
                }

                if ("x-default".Equals(child.GetQualifier(1).Value))
                {
                    xmpNode2 = child;
                    flag = true;
                    break;
                }
            }

            if (xmpNode2 != null && xmpNode.ChildrenLength > 1)
            {
                xmpNode.RemoveChild(xmpNode2);
                xmpNode.AddChild(1, xmpNode2);
            }

            object[] array = XmpNodeUtils.ChooseLocalizedText(xmpNode, genericLang, specificLang);
            int value = ((int?)array[0]).Value;
            XmpNode xmpNode4 = (XmpNode)array[1];
            bool flag2 = "x-default".Equals(specificLang);
            switch (value)
            {
                case 0:
                    XmpNodeUtils.AppendLangItem(xmpNode, "x-default", itemValue);
                    flag = true;
                    if (!flag2)
                    {
                        XmpNodeUtils.AppendLangItem(xmpNode, specificLang, itemValue);
                    }

                    break;
                case 1:
                    if (!flag2)
                    {
                        if (flag && xmpNode2 != xmpNode4 && xmpNode2 != null && xmpNode2.Value.Equals(xmpNode4.Value))
                        {
                            xmpNode2.Value = itemValue;
                        }

                        xmpNode4.Value = itemValue;
                        break;
                    }

                    foreach (XmpNode child2 in xmpNode.Children)
                    {
                        if (child2 != xmpNode2 && child2.Value.Equals(xmpNode2?.Value))
                        {
                            child2.Value = itemValue;
                        }
                    }

                    if (xmpNode2 != null)
                    {
                        xmpNode2.Value = itemValue;
                    }

                    break;
                case 2:
                    if (flag && xmpNode2 != xmpNode4 && xmpNode2 != null && xmpNode2.Value.Equals(xmpNode4.Value))
                    {
                        xmpNode2.Value = itemValue;
                    }

                    xmpNode4.Value = itemValue;
                    break;
                case 3:
                    XmpNodeUtils.AppendLangItem(xmpNode, specificLang, itemValue);
                    if (flag2)
                    {
                        flag = true;
                    }

                    break;
                case 4:
                    if (xmpNode2 != null && xmpNode.ChildrenLength == 1)
                    {
                        xmpNode2.Value = itemValue;
                    }

                    XmpNodeUtils.AppendLangItem(xmpNode, specificLang, itemValue);
                    break;
                case 5:
                    XmpNodeUtils.AppendLangItem(xmpNode, specificLang, itemValue);
                    if (flag2)
                    {
                        flag = true;
                    }

                    break;
                default:
                    throw new XmpException("Unexpected result from ChooseLocalizedText", 9);
            }

            if (!flag && xmpNode.ChildrenLength == 1)
            {
                XmpNodeUtils.AppendLangItem(xmpNode, "x-default", itemValue);
            }
        }

        public virtual void SetLocalizedText(string schemaNs, string altTextName, string genericLang, string specificLang, string itemValue)
        {
            SetLocalizedText(schemaNs, altTextName, genericLang, specificLang, itemValue, null);
        }

        public virtual IXmpProperty GetProperty(string schemaNs, string propName)
        {
            return GetProperty(schemaNs, propName, 0);
        }

        public virtual bool? GetPropertyBoolean(string schemaNs, string propName)
        {
            return (bool?)GetPropertyObject(schemaNs, propName, 1);
        }

        public virtual void SetPropertyBoolean(string schemaNs, string propName, bool propValue, PropertyOptions options)
        {
            SetProperty(schemaNs, propName, propValue ? "True" : "False", options);
        }

        public virtual void SetPropertyBoolean(string schemaNs, string propName, bool propValue)
        {
            SetProperty(schemaNs, propName, propValue ? "True" : "False", null);
        }

        public virtual int? GetPropertyInteger(string schemaNs, string propName)
        {
            return (int?)GetPropertyObject(schemaNs, propName, 2);
        }

        public virtual void SetPropertyInteger(string schemaNs, string propName, int propValue, PropertyOptions options)
        {
            SetProperty(schemaNs, propName, propValue, options);
        }

        public virtual void SetPropertyInteger(string schemaNs, string propName, int propValue)
        {
            SetProperty(schemaNs, propName, propValue, null);
        }

        public virtual long? GetPropertyLong(string schemaNs, string propName)
        {
            return (long?)GetPropertyObject(schemaNs, propName, 3);
        }

        public virtual void SetPropertyLong(string schemaNs, string propName, long propValue, PropertyOptions options)
        {
            SetProperty(schemaNs, propName, propValue, options);
        }

        public virtual void SetPropertyLong(string schemaNs, string propName, long propValue)
        {
            SetProperty(schemaNs, propName, propValue, null);
        }

        public virtual double? GetPropertyDouble(string schemaNs, string propName)
        {
            return (double?)GetPropertyObject(schemaNs, propName, 4);
        }

        public virtual void SetPropertyDouble(string schemaNs, string propName, double propValue, PropertyOptions options)
        {
            SetProperty(schemaNs, propName, propValue, options);
        }

        public virtual void SetPropertyDouble(string schemaNs, string propName, double propValue)
        {
            SetProperty(schemaNs, propName, propValue, null);
        }

        public virtual IXmpDateTime GetPropertyDate(string schemaNs, string propName)
        {
            return (IXmpDateTime)GetPropertyObject(schemaNs, propName, 5);
        }

        public virtual void SetPropertyDate(string schemaNs, string propName, IXmpDateTime propValue, PropertyOptions options)
        {
            SetProperty(schemaNs, propName, propValue, options);
        }

        public virtual void SetPropertyDate(string schemaNs, string propName, IXmpDateTime propValue)
        {
            SetProperty(schemaNs, propName, propValue, null);
        }

        public virtual DateTime GetPropertyCalendar(string schemaNs, string propName)
        {
            return (DateTime)GetPropertyObject(schemaNs, propName, 6);
        }

        public virtual void SetPropertyCalendar(string schemaNs, string propName, DateTime propValue, PropertyOptions options)
        {
            SetProperty(schemaNs, propName, propValue, options);
        }

        public virtual void SetPropertyCalendar(string schemaNs, string propName, DateTime propValue)
        {
            SetProperty(schemaNs, propName, propValue, null);
        }

        public virtual sbyte[] GetPropertyBase64(string schemaNs, string propName)
        {
            return (sbyte[])GetPropertyObject(schemaNs, propName, 7);
        }

        public virtual string GetPropertyString(string schemaNs, string propName)
        {
            return (string)GetPropertyObject(schemaNs, propName, 0);
        }

        public virtual void SetPropertyBase64(string schemaNs, string propName, sbyte[] propValue, PropertyOptions options)
        {
            SetProperty(schemaNs, propName, propValue, options);
        }

        public virtual void SetPropertyBase64(string schemaNs, string propName, sbyte[] propValue)
        {
            SetProperty(schemaNs, propName, propValue, null);
        }

        public virtual IXmpProperty GetQualifier(string schemaNs, string propName, string qualNs, string qualName)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);
            string propName2 = propName + XmpPathFactory.ComposeQualifierPath(qualNs, qualName);
            return GetProperty(schemaNs, propName2);
        }

        public virtual IXmpProperty GetStructField(string schemaNs, string structName, string fieldNs, string fieldName)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertStructName(structName);
            string propName = structName + XmpPathFactory.ComposeStructFieldPath(fieldNs, fieldName);
            return GetProperty(schemaNs, propName);
        }

        public virtual IXmpIterator Iterator()
        {
            return Iterator(null, null, null);
        }

        public virtual IXmpIterator Iterator(IteratorOptions options)
        {
            return Iterator(null, null, options);
        }

        public virtual IXmpIterator Iterator(string schemaNs, string propName, IteratorOptions options)
        {
            return new XmpIteratorImpl(this, schemaNs, propName, options);
        }

        public virtual void SetArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue, PropertyOptions options)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null);
            if (xmpNode != null)
            {
                DoSetArrayItem(xmpNode, itemIndex, itemValue, options, insert: false);
                return;
            }

            throw new XmpException("Specified array does not exist", 102);
        }

        public virtual void SetArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue)
        {
            SetArrayItem(schemaNs, arrayName, itemIndex, itemValue, null);
        }

        public virtual void InsertArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue, PropertyOptions options)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null);
            if (xmpNode != null)
            {
                DoSetArrayItem(xmpNode, itemIndex, itemValue, options, insert: true);
                return;
            }

            throw new XmpException("Specified array does not exist", 102);
        }

        public virtual void InsertArrayItem(string schemaNs, string arrayName, int itemIndex, string itemValue)
        {
            InsertArrayItem(schemaNs, arrayName, itemIndex, itemValue, null);
        }

        public virtual void SetProperty(string schemaNs, string propName, object propValue, PropertyOptions options)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);
            options = XmpNodeUtils.VerifySetOptions(options, propValue);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, propName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: true, options);
            if (xmpNode != null)
            {
                SetNode(xmpNode, propValue, options, deleteExisting: false);
                return;
            }

            throw new XmpException("Specified property does not exist", 102);
        }

        public virtual void SetProperty(string schemaNs, string propName, object propValue)
        {
            SetProperty(schemaNs, propName, propValue, null);
        }

        public virtual void SetQualifier(string schemaNs, string propName, string qualNs, string qualName, string qualValue, PropertyOptions options)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);
            if (!DoesPropertyExist(schemaNs, propName))
            {
                throw new XmpException("Specified property does not exist!", 102);
            }

            string propName2 = propName + XmpPathFactory.ComposeQualifierPath(qualNs, qualName);
            SetProperty(schemaNs, propName2, qualValue, options);
        }

        public virtual void SetQualifier(string schemaNs, string propName, string qualNs, string qualName, string qualValue)
        {
            SetQualifier(schemaNs, propName, qualNs, qualName, qualValue, null);
        }

        public virtual void SetStructField(string schemaNs, string structName, string fieldNs, string fieldName, string fieldValue, PropertyOptions options)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertStructName(structName);
            string propName = structName + XmpPathFactory.ComposeStructFieldPath(fieldNs, fieldName);
            SetProperty(schemaNs, propName, fieldValue, options);
        }

        public virtual void SetStructField(string schemaNs, string structName, string fieldNs, string fieldName, string fieldValue)
        {
            SetStructField(schemaNs, structName, fieldNs, fieldName, fieldValue, null);
        }

        public virtual object Clone()
        {
            return new XmpMetaImpl((XmpNode)_tree.Clone());
        }

        public virtual string DumpObject()
        {
            return Root.DumpNode(recursive: true);
        }

        public virtual void Sort()
        {
            _tree.Sort();
        }

        public virtual void Normalize(ParseOptions options)
        {
            if (options == null)
            {
                options = new ParseOptions();
            }

            XmpNormalizer.Process(this, options);
        }

        protected internal virtual IXmpProperty GetProperty(string schemaNs, string propName, int valueType)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, propName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null);
            if (xmpNode != null)
            {
                if (valueType != 0 && xmpNode.Options.CompositeProperty)
                {
                    throw new XmpException("Property must be simple when a value type is requested", 102);
                }

                object value = evaluateNodeValue(valueType, xmpNode);
                return new XmpPropertyImpl2(xmpNode, value);
            }

            return null;
        }

        protected internal virtual object GetPropertyObject(string schemaNs, string propName, int valueType)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertPropName(propName);
            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, propName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(_tree, xpath, createNodes: false, null);
            if (xmpNode != null)
            {
                if (valueType != 0 && xmpNode.Options.CompositeProperty)
                {
                    throw new XmpException("Property must be simple when a value type is requested", 102);
                }

                return evaluateNodeValue(valueType, xmpNode);
            }

            return null;
        }

        private void DoSetArrayItem(XmpNode arrayNode, int itemIndex, string itemValue, PropertyOptions itemOptions, bool insert)
        {
            XmpNode node = new XmpNode("[]", null);
            itemOptions = XmpNodeUtils.VerifySetOptions(itemOptions, itemValue);
            int num = (insert ? (arrayNode.ChildrenLength + 1) : arrayNode.ChildrenLength);
            if (itemIndex == -1)
            {
                itemIndex = num;
            }

            if (1 <= itemIndex && itemIndex <= num)
            {
                if (!insert)
                {
                    arrayNode.RemoveChild(itemIndex);
                }

                arrayNode.AddChild(itemIndex, node);
                SetNode(node, itemValue, itemOptions, deleteExisting: false);
                return;
            }

            throw new XmpException("Array index out of bounds", 104);
        }

        internal virtual void SetNode(XmpNode node, object value, PropertyOptions newOptions, bool deleteExisting)
        {
            if (deleteExisting)
            {
                node.Clear();
            }

            node.Options.MergeWith(newOptions);
            if (!node.Options.CompositeProperty)
            {
                XmpNodeUtils.SetNodeValue(node, value);
                return;
            }

            if (value != null && value.ToString()!.Length > 0)
            {
                throw new XmpException("Composite nodes can't have values", 102);
            }

            node.RemoveChildren();
        }

        private object evaluateNodeValue(int valueType, XmpNode propNode)
        {
            string value = propNode.Value;
            return valueType switch
            {
                1 => XmpUtils.ConvertToBoolean(value),
                2 => XmpUtils.ConvertToInteger(value),
                3 => XmpUtils.ConvertToLong(value),
                4 => XmpUtils.ConvertToDouble(value),
                5 => XmpUtils.ConvertToDate(value),
                6 => XmpUtils.ConvertToDate(value).Calendar,
                7 => XmpUtils.DecodeBase64(value),
                _ => (value != null || propNode.Options.CompositeProperty) ? value : "",
            };
        }
    }
}
