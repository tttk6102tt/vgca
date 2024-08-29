using Sign.itext.xml.xmp.impl.xpath;
using Sign.itext.xml.xmp.options;
using Sign.itext.xml.xmp.properties;
using System.Collections;
using System.Text;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpUtilsImpl : XmpConst
    {
        private const int UCK_NORMAL = 0;

        private const int UCK_SPACE = 1;

        private const int UCK_COMMA = 2;

        private const int UCK_SEMICOLON = 3;

        private const int UCK_QUOTE = 4;

        private const int UCK_CONTROL = 5;

        private const string SPACES = " \u3000〿";

        private const string COMMAS = ",，､﹐﹑、،՝";

        private const string SEMICOLA = ";；﹔؛;";

        private const string CONTROLS = "\u2028\u2029";

        private const string QUOTES = "\"«»〝〞〟―‹›";

        private XmpUtilsImpl()
        {
        }

        public static string CatenateArrayItems(IXmpMeta xmp, string schemaNs, string arrayName, string separator, string quotes, bool allowCommas)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            ParameterAsserts.AssertImplementation(xmp);
            if (string.IsNullOrEmpty(separator))
            {
                separator = "; ";
            }

            if (string.IsNullOrEmpty(quotes))
            {
                quotes = "\"";
            }

            XmpMetaImpl obj = (XmpMetaImpl)xmp;
            XmpNode xmpNode = XmpNodeUtils.FindNode(xpath: XmpPathParser.ExpandXPath(schemaNs, arrayName), xmpTree: obj.Root, createNodes: false, leafOptions: null);
            if (xmpNode == null)
            {
                return "";
            }

            if (!xmpNode.Options.Array || xmpNode.Options.ArrayAlternate)
            {
                throw new XmpException("Named property must be non-alternate array", 4);
            }

            CheckSeparator(separator);
            char openQuote = quotes[0];
            char closeQuote = CheckQuotes(quotes, openQuote);
            StringBuilder stringBuilder = new StringBuilder();
            IEnumerator enumerator = xmpNode.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode2 = (XmpNode)enumerator.Current;
                if (xmpNode2 != null)
                {
                    if (xmpNode2.Options.CompositeProperty)
                    {
                        throw new XmpException("Array items must be simple", 4);
                    }

                    string value = ApplyQuotes(xmpNode2.Value, openQuote, closeQuote, allowCommas);
                    stringBuilder.Append(value);
                    if (enumerator.MoveNext())
                    {
                        stringBuilder.Append(separator);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public static void SeparateArrayItems(IXmpMeta xmp, string schemaNs, string arrayName, string catedStr, PropertyOptions arrayOptions, bool preserveCommas)
        {
            ParameterAsserts.AssertSchemaNs(schemaNs);
            ParameterAsserts.AssertArrayName(arrayName);
            if (catedStr == null)
            {
                throw new XmpException("Parameter must not be null", 4);
            }

            ParameterAsserts.AssertImplementation(xmp);
            XmpMetaImpl xmp2 = (XmpMetaImpl)xmp;
            XmpNode xmpNode = SeparateFindCreateArray(schemaNs, arrayName, arrayOptions, xmp2);
            int num = 0;
            char c = '\0';
            int i = 0;
            int length = catedStr.Length;
            while (i < length)
            {
                int j;
                for (j = i; j < length; j++)
                {
                    c = catedStr[j];
                    num = ClassifyCharacter(c);
                    if (num == 0 || num == 4)
                    {
                        break;
                    }
                }

                if (j >= length)
                {
                    break;
                }

                string text;
                if (num != 4)
                {
                    for (i = j; i < length; i++)
                    {
                        c = catedStr[i];
                        num = ClassifyCharacter(c);
                        if (num != 0 && num != 4 && !(num == 2 && preserveCommas))
                        {
                            if (num != 1 || i + 1 >= length)
                            {
                                break;
                            }

                            c = catedStr[i + 1];
                            int num2 = ClassifyCharacter(c);
                            if (num2 != 0 && num2 != 4 && !(num2 == 2 && preserveCommas))
                            {
                                break;
                            }
                        }
                    }

                    text = catedStr.Substring(j, i - j);
                }
                else
                {
                    char openQuote = c;
                    char closingQuote = GetClosingQuote(openQuote);
                    j++;
                    text = "";
                    for (i = j; i < length; i++)
                    {
                        c = catedStr[i];
                        num = ClassifyCharacter(c);
                        if (num != 4 || !IsSurroundingQuote(c, openQuote, closingQuote))
                        {
                            text += c;
                            continue;
                        }

                        char c2;
                        if (i + 1 < length)
                        {
                            c2 = catedStr[i + 1];
                            int num2 = ClassifyCharacter(c2);
                        }
                        else
                        {
                            int num2 = 3;
                            c2 = ';';
                        }

                        if (c == c2)
                        {
                            text += c;
                            i++;
                            continue;
                        }

                        if (!IsClosingingQuote(c, openQuote, closingQuote))
                        {
                            text += c;
                            continue;
                        }

                        i++;
                        break;
                    }
                }

                int num3 = -1;
                for (int k = 1; k <= xmpNode.ChildrenLength; k++)
                {
                    if (text.Equals(xmpNode.GetChild(k).Value))
                    {
                        num3 = k;
                        break;
                    }
                }

                if (num3 < 0)
                {
                    XmpNode node = new XmpNode("[]", text, null);
                    xmpNode.AddChild(node);
                }
            }
        }

        private static XmpNode SeparateFindCreateArray(string schemaNs, string arrayName, PropertyOptions arrayOptions, XmpMetaImpl xmp)
        {
            arrayOptions = XmpNodeUtils.VerifySetOptions(arrayOptions, null);
            if (!arrayOptions.OnlyArrayOptions)
            {
                throw new XmpException("Options can only provide array form", 103);
            }

            XmpPath xpath = XmpPathParser.ExpandXPath(schemaNs, arrayName);
            XmpNode xmpNode = XmpNodeUtils.FindNode(xmp.Root, xpath, createNodes: false, null);
            if (xmpNode != null)
            {
                PropertyOptions options = xmpNode.Options;
                if (!options.Array || options.ArrayAlternate)
                {
                    throw new XmpException("Named property must be non-alternate array", 102);
                }

                if (arrayOptions.EqualArrayTypes(options))
                {
                    throw new XmpException("Mismatch of specified and existing array form", 102);
                }
            }
            else
            {
                arrayOptions.Array = true;
                xmpNode = XmpNodeUtils.FindNode(xmp.Root, xpath, createNodes: true, arrayOptions);
                if (xmpNode == null)
                {
                    throw new XmpException("Failed to create named array", 102);
                }
            }

            return xmpNode;
        }

        public static void RemoveProperties(IXmpMeta xmp, string schemaNs, string propName, bool doAllProperties, bool includeAliases)
        {
            ParameterAsserts.AssertImplementation(xmp);
            XmpMetaImpl xmpMetaImpl = (XmpMetaImpl)xmp;
            if (!string.IsNullOrEmpty(propName))
            {
                if (string.IsNullOrEmpty(schemaNs))
                {
                    throw new XmpException("Property name requires schema namespace", 4);
                }

                XmpPath xmpPath = XmpPathParser.ExpandXPath(schemaNs, propName);
                XmpNode xmpNode = XmpNodeUtils.FindNode(xmpMetaImpl.Root, xmpPath, createNodes: false, null);
                if (xmpNode != null && (doAllProperties || !Utils.IsInternalProperty(xmpPath.GetSegment(0).Name, xmpPath.GetSegment(1).Name)))
                {
                    XmpNode parent = xmpNode.Parent;
                    parent.RemoveChild(xmpNode);
                    if (parent.Options.SchemaNode && !parent.HasChildren())
                    {
                        parent.Parent.RemoveChild(parent);
                    }
                }

                return;
            }

            if (!string.IsNullOrEmpty(schemaNs))
            {
                XmpNode xmpNode2 = XmpNodeUtils.FindSchemaNode(xmpMetaImpl.Root, schemaNs, createNodes: false);
                if (xmpNode2 != null && RemoveSchemaChildren(xmpNode2, doAllProperties))
                {
                    xmpMetaImpl.Root.RemoveChild(xmpNode2);
                }

                if (includeAliases)
                {
                    IXmpAliasInfo[] array = XmpMetaFactory.SchemaRegistry.FindAliases(schemaNs);
                    foreach (IXmpAliasInfo xmpAliasInfo in array)
                    {
                        XmpPath xpath = XmpPathParser.ExpandXPath(xmpAliasInfo.Namespace, xmpAliasInfo.PropName);
                        XmpNode xmpNode3 = XmpNodeUtils.FindNode(xmpMetaImpl.Root, xpath, createNodes: false, null);
                        xmpNode3?.Parent.RemoveChild(xmpNode3);
                    }
                }

                return;
            }

            ArrayList arrayList = new ArrayList();
            IEnumerator enumerator = xmpMetaImpl.Root.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode4 = (XmpNode)enumerator.Current;
                if (xmpNode4 != null && RemoveSchemaChildren(xmpNode4, doAllProperties))
                {
                    arrayList.Add(xmpNode4);
                }
            }

            foreach (XmpNode item in arrayList)
            {
                xmpMetaImpl.Root.Children.Remove(item);
            }

            arrayList.Clear();
        }

        public static void AppendProperties(IXmpMeta source, IXmpMeta destination, bool doAllProperties, bool replaceOldValues, bool deleteEmptyValues)
        {
            ParameterAsserts.AssertImplementation(source);
            ParameterAsserts.AssertImplementation(destination);
            XmpMetaImpl obj = (XmpMetaImpl)source;
            XmpMetaImpl xmpMetaImpl = (XmpMetaImpl)destination;
            IEnumerator enumerator = obj.Root.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode == null)
                {
                    continue;
                }

                XmpNode xmpNode2 = XmpNodeUtils.FindSchemaNode(xmpMetaImpl.Root, xmpNode.Name, createNodes: false);
                bool flag = false;
                if (xmpNode2 == null)
                {
                    PropertyOptions propertyOptions = new PropertyOptions();
                    propertyOptions.SchemaNode = true;
                    xmpNode2 = new XmpNode(xmpNode.Name, xmpNode.Value, propertyOptions);
                    xmpMetaImpl.Root.AddChild(xmpNode2);
                    flag = true;
                }

                IEnumerator enumerator2 = xmpNode.IterateChildren();
                while (enumerator2.MoveNext())
                {
                    XmpNode xmpNode3 = (XmpNode)enumerator2.Current;
                    if (xmpNode3 != null && (doAllProperties || !Utils.IsInternalProperty(xmpNode.Name, xmpNode3.Name)))
                    {
                        AppendSubtree(xmpMetaImpl, xmpNode3, xmpNode2, replaceOldValues, deleteEmptyValues);
                    }
                }

                if (!xmpNode2.HasChildren() && (flag || deleteEmptyValues))
                {
                    xmpMetaImpl.Root.RemoveChild(xmpNode2);
                }
            }
        }

        private static bool RemoveSchemaChildren(XmpNode schemaNode, bool doAllProperties)
        {
            ArrayList arrayList = new ArrayList();
            IEnumerator enumerator = schemaNode.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode != null && (doAllProperties || !Utils.IsInternalProperty(schemaNode.Name, xmpNode.Name)))
                {
                    arrayList.Add(xmpNode);
                }
            }

            foreach (XmpNode item in arrayList)
            {
                schemaNode.Children.Remove(item);
            }

            arrayList.Clear();
            return !schemaNode.HasChildren();
        }

        private static void AppendSubtree(XmpMetaImpl destXmp, XmpNode sourceNode, XmpNode destParent, bool replaceOldValues, bool deleteEmptyValues)
        {
            XmpNode xmpNode = XmpNodeUtils.FindChildNode(destParent, sourceNode.Name, createNodes: false);
            bool flag = false;
            if (deleteEmptyValues)
            {
                flag = (sourceNode.Options.Simple ? string.IsNullOrEmpty(sourceNode.Value) : (!sourceNode.HasChildren()));
            }

            if (deleteEmptyValues && flag)
            {
                if (xmpNode != null)
                {
                    destParent.RemoveChild(xmpNode);
                }

                return;
            }

            if (xmpNode == null)
            {
                destParent.AddChild((XmpNode)sourceNode.Clone());
                return;
            }

            if (replaceOldValues)
            {
                destXmp.SetNode(xmpNode, sourceNode.Value, sourceNode.Options, deleteExisting: true);
                destParent.RemoveChild(xmpNode);
                xmpNode = (XmpNode)sourceNode.Clone();
                destParent.AddChild(xmpNode);
                return;
            }

            PropertyOptions options = sourceNode.Options;
            PropertyOptions options2 = xmpNode.Options;
            if (options != options2)
            {
                return;
            }

            if (options.Struct)
            {
                IEnumerator enumerator = sourceNode.IterateChildren();
                while (enumerator.MoveNext())
                {
                    XmpNode xmpNode2 = (XmpNode)enumerator.Current;
                    if (xmpNode2 != null)
                    {
                        AppendSubtree(destXmp, xmpNode2, xmpNode, replaceOldValues, deleteEmptyValues);
                        if (deleteEmptyValues && !xmpNode.HasChildren())
                        {
                            destParent.RemoveChild(xmpNode);
                        }
                    }
                }
            }
            else if (options.ArrayAltText)
            {
                IEnumerator enumerator2 = sourceNode.IterateChildren();
                while (enumerator2.MoveNext())
                {
                    XmpNode xmpNode3 = (XmpNode)enumerator2.Current;
                    if (xmpNode3 == null || !xmpNode3.HasQualifier() || !"xml:lang".Equals(xmpNode3.GetQualifier(1).Name))
                    {
                        continue;
                    }

                    int num = XmpNodeUtils.LookupLanguageItem(xmpNode, xmpNode3.GetQualifier(1).Value);
                    if (deleteEmptyValues && string.IsNullOrEmpty(xmpNode3.Value))
                    {
                        if (num != -1)
                        {
                            xmpNode.RemoveChild(num);
                            if (!xmpNode.HasChildren())
                            {
                                destParent.RemoveChild(xmpNode);
                            }
                        }
                    }
                    else if (num == -1)
                    {
                        if (!"x-default".Equals(xmpNode3.GetQualifier(1).Value) || !xmpNode.HasChildren())
                        {
                            xmpNode3.CloneSubtree(xmpNode);
                            continue;
                        }

                        XmpNode xmpNode4 = new XmpNode(xmpNode3.Name, xmpNode3.Value, xmpNode3.Options);
                        xmpNode3.CloneSubtree(xmpNode4);
                        xmpNode.AddChild(1, xmpNode4);
                    }
                }
            }
            else
            {
                if (!options.Array)
                {
                    return;
                }

                IEnumerator enumerator3 = sourceNode.IterateChildren();
                while (enumerator3.MoveNext())
                {
                    XmpNode xmpNode5 = (XmpNode)enumerator3.Current;
                    if (xmpNode5 == null)
                    {
                        continue;
                    }

                    bool flag2 = false;
                    IEnumerator enumerator4 = xmpNode.IterateChildren();
                    while (enumerator4.MoveNext())
                    {
                        XmpNode xmpNode6 = (XmpNode)enumerator4.Current;
                        if (xmpNode6 != null && ItemValuesMatch(xmpNode5, xmpNode6))
                        {
                            flag2 = true;
                        }
                    }

                    if (!flag2)
                    {
                        xmpNode = (XmpNode)xmpNode5.Clone();
                        destParent.AddChild(xmpNode);
                    }
                }
            }
        }

        private static bool ItemValuesMatch(XmpNode leftNode, XmpNode rightNode)
        {
            PropertyOptions options = leftNode.Options;
            PropertyOptions options2 = rightNode.Options;
            if (options.Equals(options2))
            {
                return false;
            }

            if (options.Options == 0)
            {
                if (!leftNode.Value.Equals(rightNode.Value))
                {
                    return false;
                }

                if (leftNode.Options.HasLanguage != rightNode.Options.HasLanguage)
                {
                    return false;
                }

                if (leftNode.Options.HasLanguage && !leftNode.GetQualifier(1).Value.Equals(rightNode.GetQualifier(1).Value))
                {
                    return false;
                }
            }
            else if (options.Struct)
            {
                if (leftNode.ChildrenLength != rightNode.ChildrenLength)
                {
                    return false;
                }

                IEnumerator enumerator = leftNode.IterateChildren();
                while (enumerator.MoveNext())
                {
                    XmpNode xmpNode = (XmpNode)enumerator.Current;
                    if (xmpNode != null)
                    {
                        XmpNode xmpNode2 = XmpNodeUtils.FindChildNode(rightNode, xmpNode.Name, createNodes: false);
                        if (xmpNode2 == null || !ItemValuesMatch(xmpNode, xmpNode2))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                IEnumerator enumerator2 = leftNode.IterateChildren();
                while (enumerator2.MoveNext())
                {
                    XmpNode xmpNode3 = (XmpNode)enumerator2.Current;
                    if (xmpNode3 == null)
                    {
                        continue;
                    }

                    bool flag = false;
                    IEnumerator enumerator3 = rightNode.IterateChildren();
                    while (enumerator3.MoveNext())
                    {
                        XmpNode xmpNode4 = (XmpNode)enumerator3.Current;
                        if (xmpNode4 != null && ItemValuesMatch(xmpNode3, xmpNode4))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void CheckSeparator(string separator)
        {
            bool flag = false;
            for (int i = 0; i < separator.Length; i++)
            {
                switch (ClassifyCharacter(separator[i]))
                {
                    case 3:
                        if (flag)
                        {
                            throw new XmpException("Separator can have only one semicolon", 4);
                        }

                        flag = true;
                        break;
                    default:
                        throw new XmpException("Separator can have only spaces and one semicolon", 4);
                    case 1:
                        break;
                }
            }

            if (!flag)
            {
                throw new XmpException("Separator must have one semicolon", 4);
            }
        }

        private static char CheckQuotes(string quotes, char openQuote)
        {
            if (ClassifyCharacter(openQuote) != 4)
            {
                throw new XmpException("Invalid quoting character", 4);
            }

            char c;
            if (quotes.Length == 1)
            {
                c = openQuote;
            }
            else
            {
                c = quotes[1];
                if (ClassifyCharacter(c) != 4)
                {
                    throw new XmpException("Invalid quoting character", 4);
                }
            }

            if (c != GetClosingQuote(openQuote))
            {
                throw new XmpException("Mismatched quote pair", 4);
            }

            return c;
        }

        private static int ClassifyCharacter(char ch)
        {
            if (" \u3000〿".IndexOf(ch) >= 0 || ('\u2000' <= ch && ch <= '\u200b'))
            {
                return 1;
            }

            if (",，､﹐﹑、،՝".IndexOf(ch) >= 0)
            {
                return 2;
            }

            if (";；﹔؛;".IndexOf(ch) >= 0)
            {
                return 3;
            }

            if ("\"«»〝〞〟―‹›".IndexOf(ch) >= 0 || ('〈' <= ch && ch <= '』') || ('‘' <= ch && ch <= '‟'))
            {
                return 4;
            }

            if (ch < ' ' || "\u2028\u2029".IndexOf(ch) >= 0)
            {
                return 5;
            }

            return 0;
        }

        private static char GetClosingQuote(char openQuote)
        {
            return openQuote switch
            {
                '"' => '"',
                '«' => '»',
                '»' => '«',
                '―' => '―',
                '‘' => '’',
                '‚' => '‛',
                '“' => '”',
                '„' => '‟',
                '‹' => '›',
                '›' => '‹',
                '〈' => '〉',
                '《' => '》',
                '「' => '」',
                '『' => '』',
                '〝' => '〟',
                _ => '\0',
            };
        }

        private static string ApplyQuotes(string item, char openQuote, char closeQuote, bool allowCommas)
        {
            if (item == null)
            {
                item = "";
            }

            bool flag = false;
            int i;
            for (i = 0; i < item.Length; i++)
            {
                int num = ClassifyCharacter(item[i]);
                if (i == 0 && num == 4)
                {
                    break;
                }

                if (num == 1)
                {
                    if (flag)
                    {
                        break;
                    }

                    flag = true;
                    continue;
                }

                flag = false;
                switch (num)
                {
                    case 2:
                        if (allowCommas)
                        {
                            continue;
                        }

                        break;
                    default:
                        continue;
                    case 3:
                    case 5:
                        break;
                }

                break;
            }

            if (i < item.Length)
            {
                StringBuilder stringBuilder = new StringBuilder(item.Length + 2);
                int j;
                for (j = 0; j <= i; j++)
                {
                    if (ClassifyCharacter(item[i]) == 4)
                    {
                        break;
                    }
                }

                stringBuilder.Append(openQuote).Append(item.Substring(0, j));
                for (int k = j; k < item.Length; k++)
                {
                    stringBuilder.Append(item[k]);
                    if (ClassifyCharacter(item[k]) == 4 && IsSurroundingQuote(item[k], openQuote, closeQuote))
                    {
                        stringBuilder.Append(item[k]);
                    }
                }

                stringBuilder.Append(closeQuote);
                item = stringBuilder.ToString();
            }

            return item;
        }

        private static bool IsSurroundingQuote(char ch, char openQuote, char closeQuote)
        {
            if (ch != openQuote)
            {
                return IsClosingingQuote(ch, openQuote, closeQuote);
            }

            return true;
        }

        private static bool IsClosingingQuote(char ch, char openQuote, char closeQuote)
        {
            if (ch != closeQuote)
            {
                if (openQuote != '〝' || ch != '〞')
                {
                    return ch == '〟';
                }

                return true;
            }

            return true;
        }
    }
}
