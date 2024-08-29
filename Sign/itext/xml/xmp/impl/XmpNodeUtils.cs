using Sign.itext.xml.xmp.impl.xpath;
using Sign.itext.xml.xmp.options;
using System.Collections;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpNodeUtils : XmpConst
    {
        internal const int CLT_NO_VALUES = 0;

        internal const int CLT_SPECIFIC_MATCH = 1;

        internal const int CLT_SINGLE_GENERIC = 2;

        internal const int CLT_MULTIPLE_GENERIC = 3;

        internal const int CLT_XDEFAULT = 4;

        internal const int CLT_FIRST_ITEM = 5;

        private XmpNodeUtils()
        {
        }

        internal static XmpNode FindSchemaNode(XmpNode tree, string namespaceUri, bool createNodes)
        {
            return FindSchemaNode(tree, namespaceUri, null, createNodes);
        }

        internal static XmpNode FindSchemaNode(XmpNode tree, string namespaceUri, string suggestedPrefix, bool createNodes)
        {
            XmpNode xmpNode = tree.FindChildByName(namespaceUri);
            if (xmpNode == null && createNodes)
            {
                PropertyOptions propertyOptions = new PropertyOptions();
                propertyOptions.SchemaNode = true;
                xmpNode = new XmpNode(namespaceUri, propertyOptions);
                xmpNode.Implicit = true;
                string text = XmpMetaFactory.SchemaRegistry.GetNamespacePrefix(namespaceUri);
                if (text == null)
                {
                    if (string.IsNullOrEmpty(suggestedPrefix))
                    {
                        throw new XmpException("Unregistered schema namespace URI", 101);
                    }

                    text = XmpMetaFactory.SchemaRegistry.RegisterNamespace(namespaceUri, suggestedPrefix);
                }

                xmpNode.Value = text;
                tree.AddChild(xmpNode);
            }

            return xmpNode;
        }

        internal static XmpNode FindChildNode(XmpNode parent, string childName, bool createNodes)
        {
            if (!parent.Options.SchemaNode && !parent.Options.Struct)
            {
                if (!parent.Implicit)
                {
                    throw new XmpException("Named children only allowed for schemas and structs", 102);
                }

                if (parent.Options.Array)
                {
                    throw new XmpException("Named children not allowed for arrays", 102);
                }

                if (createNodes)
                {
                    parent.Options.Struct = true;
                }
            }

            XmpNode xmpNode = parent.FindChildByName(childName);
            if (xmpNode == null && createNodes)
            {
                PropertyOptions options = new PropertyOptions();
                xmpNode = new XmpNode(childName, options);
                xmpNode.Implicit = true;
                parent.AddChild(xmpNode);
            }

            return xmpNode;
        }

        internal static XmpNode FindNode(XmpNode xmpTree, XmpPath xpath, bool createNodes, PropertyOptions leafOptions)
        {
            if (xpath == null || xpath.Size() == 0)
            {
                throw new XmpException("Empty XmpPath", 102);
            }

            XmpNode xmpNode = null;
            XmpNode xmpNode2 = FindSchemaNode(xmpTree, xpath.GetSegment(0).Name, createNodes);
            if (xmpNode2 == null)
            {
                return null;
            }

            if (xmpNode2.Implicit)
            {
                xmpNode2.Implicit = false;
                xmpNode = xmpNode2;
            }

            try
            {
                for (int i = 1; i < xpath.Size(); i++)
                {
                    xmpNode2 = FollowXPathStep(xmpNode2, xpath.GetSegment(i), createNodes);
                    if (xmpNode2 == null)
                    {
                        if (createNodes)
                        {
                            DeleteNode(xmpNode);
                        }

                        return null;
                    }

                    if (xmpNode2.Implicit)
                    {
                        xmpNode2.Implicit = false;
                        if (i == 1 && xpath.GetSegment(i).Alias && xpath.GetSegment(i).AliasForm != 0)
                        {
                            xmpNode2.Options.SetOption(xpath.GetSegment(i).AliasForm, value: true);
                        }
                        else if (i < xpath.Size() - 1 && xpath.GetSegment(i).Kind == 1 && !xmpNode2.Options.CompositeProperty)
                        {
                            xmpNode2.Options.Struct = true;
                        }

                        if (xmpNode == null)
                        {
                            xmpNode = xmpNode2;
                        }
                    }
                }
            }
            catch (XmpException ex)
            {
                if (xmpNode != null)
                {
                    DeleteNode(xmpNode);
                }

                throw ex;
            }

            if (xmpNode != null)
            {
                xmpNode2.Options.MergeWith(leafOptions);
                xmpNode2.Options = xmpNode2.Options;
            }

            return xmpNode2;
        }

        internal static void DeleteNode(XmpNode node)
        {
            XmpNode parent = node.Parent;
            if (node.Options.Qualifier)
            {
                parent.RemoveQualifier(node);
            }
            else
            {
                parent.RemoveChild(node);
            }

            if (!parent.HasChildren() && parent.Options.SchemaNode)
            {
                parent.Parent.RemoveChild(parent);
            }
        }

        internal static void SetNodeValue(XmpNode node, object value)
        {
            string value2 = SerializeNodeValue(value);
            if (!node.Options.Qualifier || !"xml:lang".Equals(node.Name))
            {
                node.Value = value2;
            }
            else
            {
                node.Value = Utils.NormalizeLangValue(value2);
            }
        }

        internal static PropertyOptions VerifySetOptions(PropertyOptions options, object itemValue)
        {
            if (options == null)
            {
                options = new PropertyOptions();
            }

            if (options.ArrayAltText)
            {
                options.ArrayAlternate = true;
            }

            if (options.ArrayAlternate)
            {
                options.ArrayOrdered = true;
            }

            if (options.ArrayOrdered)
            {
                options.Array = true;
            }

            if (options.CompositeProperty && itemValue != null && itemValue.ToString()!.Length > 0)
            {
                throw new XmpException("Structs and arrays can't have values", 103);
            }

            options.AssertConsistency(options.Options);
            return options;
        }

        internal static string SerializeNodeValue(object value)
        {
            string text = ((value == null) ? null : ((value is bool?) ? XmpUtils.ConvertFromBoolean(((bool?)value).Value) : ((value is int?) ? XmpUtils.ConvertFromInteger(((int?)value).Value) : ((value is long?) ? XmpUtils.ConvertFromLong(((long?)value).Value) : ((value is double?) ? XmpUtils.ConvertFromDouble(((double?)value).Value) : ((value is IXmpDateTime) ? XmpUtils.ConvertFromDate((IXmpDateTime)value) : ((value is XmpCalendar) ? XmpUtils.ConvertFromDate(XmpDateTimeFactory.CreateFromCalendar((XmpCalendar)value)) : ((!(value is byte[])) ? value.ToString() : XmpUtils.EncodeBase64((byte[])value)))))))));
            if (text == null)
            {
                return null;
            }

            return Utils.RemoveControlChars(text);
        }

        private static XmpNode FollowXPathStep(XmpNode parentNode, XmpPathSegment nextStep, bool createNodes)
        {
            XmpNode result = null;
            uint kind = nextStep.Kind;
            switch (kind)
            {
                case 1u:
                    result = FindChildNode(parentNode, nextStep.Name, createNodes);
                    break;
                case 2u:
                    result = FindQualifierNode(parentNode, nextStep.Name.Substring(1), createNodes);
                    break;
                default:
                    {
                        if (!parentNode.Options.Array)
                        {
                            throw new XmpException("Indexing applied to non-array", 102);
                        }

                        int num;
                        switch (kind)
                        {
                            case 3u:
                                num = FindIndexedItem(parentNode, nextStep.Name, createNodes);
                                break;
                            case 4u:
                                num = parentNode.ChildrenLength;
                                break;
                            case 6u:
                                {
                                    string[] array2 = Utils.SplitNameAndValue(nextStep.Name);
                                    string fieldName = array2[0];
                                    string fieldValue = array2[1];
                                    num = LookupFieldSelector(parentNode, fieldName, fieldValue);
                                    break;
                                }
                            case 5u:
                                {
                                    string[] array = Utils.SplitNameAndValue(nextStep.Name);
                                    string qualName = array[0];
                                    string qualValue = array[1];
                                    num = LookupQualSelector(parentNode, qualName, qualValue, nextStep.AliasForm);
                                    break;
                                }
                            default:
                                throw new XmpException("Unknown array indexing step in FollowXPathStep", 9);
                        }

                        if (1 <= num && num <= parentNode.ChildrenLength)
                        {
                            result = parentNode.GetChild(num);
                        }

                        break;
                    }
            }

            return result;
        }

        private static XmpNode FindQualifierNode(XmpNode parent, string qualName, bool createNodes)
        {
            XmpNode xmpNode = parent.FindQualifierByName(qualName);
            if (xmpNode == null && createNodes)
            {
                xmpNode = new XmpNode(qualName, null);
                xmpNode.Implicit = true;
                parent.AddQualifier(xmpNode);
            }

            return xmpNode;
        }

        private static int FindIndexedItem(XmpNode arrayNode, string segment, bool createNodes)
        {
            int num;
            try
            {
                segment = segment.Substring(1, segment.Length - 1 - 1);
                num = Convert.ToInt32(segment);
                if (num < 1)
                {
                    throw new XmpException("Array index must be larger than zero", 102);
                }
            }
            catch (FormatException)
            {
                throw new XmpException("Array index not digits.", 102);
            }

            if (createNodes && num == arrayNode.ChildrenLength + 1)
            {
                XmpNode xmpNode = new XmpNode("[]", null);
                xmpNode.Implicit = true;
                arrayNode.AddChild(xmpNode);
            }

            return num;
        }

        private static int LookupFieldSelector(XmpNode arrayNode, string fieldName, string fieldValue)
        {
            int num = -1;
            for (int i = 1; i <= arrayNode.ChildrenLength; i++)
            {
                if (num >= 0)
                {
                    break;
                }

                XmpNode child = arrayNode.GetChild(i);
                if (!child.Options.Struct)
                {
                    throw new XmpException("Field selector must be used on array of struct", 102);
                }

                for (int j = 1; j <= child.ChildrenLength; j++)
                {
                    XmpNode child2 = child.GetChild(j);
                    if (fieldName.Equals(child2.Name) && fieldValue.Equals(child2.Value))
                    {
                        num = i;
                        break;
                    }
                }
            }

            return num;
        }

        private static int LookupQualSelector(XmpNode arrayNode, string qualName, string qualValue, uint aliasForm)
        {
            if ("xml:lang".Equals(qualName))
            {
                qualValue = Utils.NormalizeLangValue(qualValue);
                int num = LookupLanguageItem(arrayNode, qualValue);
                if (num < 0 && (aliasForm & 0x1000u) != 0)
                {
                    XmpNode xmpNode = new XmpNode("[]", null);
                    XmpNode qualNode = new XmpNode("xml:lang", "x-default", null);
                    xmpNode.AddQualifier(qualNode);
                    arrayNode.AddChild(1, xmpNode);
                    return 1;
                }

                return num;
            }

            for (int i = 1; i < arrayNode.ChildrenLength; i++)
            {
                IEnumerator enumerator = arrayNode.GetChild(i).IterateQualifier();
                while (enumerator.MoveNext())
                {
                    XmpNode xmpNode2 = (XmpNode)enumerator.Current;
                    if (xmpNode2 != null && qualName.Equals(xmpNode2.Name) && qualValue.Equals(xmpNode2.Value))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        internal static void NormalizeLangArray(XmpNode arrayNode)
        {
            if (!arrayNode.Options.ArrayAltText)
            {
                return;
            }

            for (int i = 2; i <= arrayNode.ChildrenLength; i++)
            {
                XmpNode child = arrayNode.GetChild(i);
                if (child.HasQualifier() && "x-default".Equals(child.GetQualifier(1).Value))
                {
                    try
                    {
                        arrayNode.RemoveChild(i);
                        arrayNode.AddChild(1, child);
                    }
                    catch (XmpException)
                    {
                    }

                    if (i == 2)
                    {
                        arrayNode.GetChild(2).Value = child.Value;
                    }

                    break;
                }
            }
        }

        internal static void DetectAltText(XmpNode arrayNode)
        {
            if (!arrayNode.Options.ArrayAlternate || !arrayNode.HasChildren())
            {
                return;
            }

            bool flag = false;
            IEnumerator enumerator = arrayNode.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode != null && xmpNode.Options != null && xmpNode.Options.HasLanguage)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                arrayNode.Options.ArrayAltText = true;
                NormalizeLangArray(arrayNode);
            }
        }

        internal static void AppendLangItem(XmpNode arrayNode, string itemLang, string itemValue)
        {
            XmpNode xmpNode = new XmpNode("[]", itemValue, null);
            XmpNode xmpNode2 = new XmpNode("xml:lang", itemLang, null);
            xmpNode.AddQualifier(xmpNode2);
            if (!"x-default".Equals(xmpNode2.Value))
            {
                arrayNode.AddChild(xmpNode);
            }
            else
            {
                arrayNode.AddChild(1, xmpNode);
            }
        }

        internal static object[] ChooseLocalizedText(XmpNode arrayNode, string genericLang, string specificLang)
        {
            if (!arrayNode.Options.ArrayAltText)
            {
                throw new XmpException("Localized text array is not alt-text", 102);
            }

            if (arrayNode.HasChildren())
            {
                int num = 0;
                XmpNode xmpNode = null;
                XmpNode xmpNode2 = null;
                IEnumerator enumerator = arrayNode.IterateChildren();
                while (enumerator.MoveNext())
                {
                    XmpNode xmpNode3 = (XmpNode)enumerator.Current;
                    if (xmpNode3 == null || xmpNode3.Options == null || xmpNode3.Options.CompositeProperty)
                    {
                        throw new XmpException("Alt-text array item is not simple", 102);
                    }

                    if (!xmpNode3.HasQualifier() || !"xml:lang".Equals(xmpNode3.GetQualifier(1).Name))
                    {
                        throw new XmpException("Alt-text array item has no language qualifier", 102);
                    }

                    string value = xmpNode3.GetQualifier(1).Value;
                    if (specificLang.Equals(value))
                    {
                        return new object[2] { 1, xmpNode3 };
                    }

                    if (genericLang != null && value.StartsWith(genericLang))
                    {
                        if (xmpNode == null)
                        {
                            xmpNode = xmpNode3;
                        }

                        num++;
                    }
                    else if ("x-default".Equals(value))
                    {
                        xmpNode2 = xmpNode3;
                    }
                }

                if (num != 1)
                {
                    if (num <= 1)
                    {
                        if (xmpNode2 == null)
                        {
                            return new object[2]
                            {
                                5,
                                arrayNode.GetChild(1)
                            };
                        }

                        return new object[2] { 4, xmpNode2 };
                    }

                    return new object[2] { 3, xmpNode };
                }

                return new object[2] { 2, xmpNode };
            }

            return new object[2] { 0, null };
        }

        internal static int LookupLanguageItem(XmpNode arrayNode, string language)
        {
            if (!arrayNode.Options.Array)
            {
                throw new XmpException("Language item must be used on array", 102);
            }

            for (int i = 1; i <= arrayNode.ChildrenLength; i++)
            {
                XmpNode child = arrayNode.GetChild(i);
                if (child.HasQualifier() && "xml:lang".Equals(child.GetQualifier(1).Name) && language.Equals(child.GetQualifier(1).Value))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
