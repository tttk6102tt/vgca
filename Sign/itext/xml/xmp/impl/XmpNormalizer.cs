using Sign.itext.xml.xmp.impl.xpath;
using Sign.itext.xml.xmp.options;
using Sign.itext.xml.xmp.properties;
using System.Collections;

namespace Sign.itext.xml.xmp.impl
{
    public class XmpNormalizer
    {
        private static IDictionary _dcArrayForms;

        static XmpNormalizer()
        {
            InitDcArrays();
        }

        private XmpNormalizer()
        {
        }

        internal static IXmpMeta Process(XmpMetaImpl xmp, ParseOptions options)
        {
            XmpNode root = xmp.Root;
            TouchUpDataModel(xmp);
            MoveExplicitAliases(root, options);
            TweakOldXmp(root);
            DeleteEmptySchemas(root);
            return xmp;
        }

        private static void TweakOldXmp(XmpNode tree)
        {
            if (tree.Name == null || tree.Name.Length < Utils.UUID_LENGTH)
            {
                return;
            }

            string text = tree.Name.ToLower();
            if (text.StartsWith("uuid:"))
            {
                text = text.Substring(5);
            }

            if (Utils.CheckUuidFormat(text))
            {
                XmpPath xpath = XmpPathParser.ExpandXPath("http://ns.adobe.com/xap/1.0/mm/", "InstanceID");
                XmpNode xmpNode = XmpNodeUtils.FindNode(tree, xpath, createNodes: true, null);
                if (xmpNode == null)
                {
                    throw new XmpException("Failure creating xmpMM:InstanceID", 9);
                }

                xmpNode.Options = null;
                xmpNode.Value = "uuid:" + text;
                xmpNode.RemoveChildren();
                xmpNode.RemoveQualifiers();
                tree.Name = null;
            }
        }

        private static void TouchUpDataModel(XmpMetaImpl xmp)
        {
            XmpNodeUtils.FindSchemaNode(xmp.Root, "http://purl.org/dc/elements/1.1/", createNodes: true);
            IEnumerator enumerator = xmp.Root.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode != null && "http://purl.org/dc/elements/1.1/".Equals(xmpNode.Name))
                {
                    NormalizeDcArrays(xmpNode);
                }
                else if (xmpNode != null && "http://ns.adobe.com/exif/1.0/".Equals(xmpNode.Name))
                {
                    FixGpsTimeStamp(xmpNode);
                    XmpNode xmpNode2 = XmpNodeUtils.FindChildNode(xmpNode, "exif:UserComment", createNodes: false);
                    if (xmpNode2 != null)
                    {
                        RepairAltText(xmpNode2);
                    }
                }
                else if (xmpNode != null && "http://ns.adobe.com/xmp/1.0/DynamicMedia/".Equals(xmpNode.Name))
                {
                    XmpNode xmpNode3 = XmpNodeUtils.FindChildNode(xmpNode, "xmpDM:copyright", createNodes: false);
                    if (xmpNode3 != null)
                    {
                        MigrateAudioCopyright(xmp, xmpNode3);
                    }
                }
                else if (xmpNode != null && "http://ns.adobe.com/xap/1.0/rights/".Equals(xmpNode.Name))
                {
                    XmpNode xmpNode4 = XmpNodeUtils.FindChildNode(xmpNode, "xmpRights:UsageTerms", createNodes: false);
                    if (xmpNode4 != null)
                    {
                        RepairAltText(xmpNode4);
                    }
                }
            }
        }

        private static void NormalizeDcArrays(XmpNode dcSchema)
        {
            for (int i = 1; i <= dcSchema.ChildrenLength; i++)
            {
                XmpNode child = dcSchema.GetChild(i);
                PropertyOptions propertyOptions = (PropertyOptions)_dcArrayForms[child.Name];
                if (propertyOptions == null)
                {
                    continue;
                }

                if (child.Options.Simple)
                {
                    XmpNode xmpNode = new XmpNode(child.Name, propertyOptions);
                    child.Name = "[]";
                    xmpNode.AddChild(child);
                    dcSchema.ReplaceChild(i, xmpNode);
                    if (propertyOptions.ArrayAltText && !child.Options.HasLanguage)
                    {
                        XmpNode qualNode = new XmpNode("xml:lang", "x-default", null);
                        child.AddQualifier(qualNode);
                    }
                }
                else
                {
                    child.Options.SetOption(7680u, value: false);
                    child.Options.MergeWith(propertyOptions);
                    if (propertyOptions.ArrayAltText)
                    {
                        RepairAltText(child);
                    }
                }
            }
        }

        private static void RepairAltText(XmpNode arrayNode)
        {
            if (arrayNode == null || !arrayNode.Options.Array)
            {
                return;
            }

            arrayNode.Options.ArrayOrdered = true;
            arrayNode.Options.ArrayAlternate = true;
            arrayNode.Options.ArrayAltText = true;
            ArrayList arrayList = new ArrayList();
            IEnumerator enumerator = arrayNode.IterateChildren();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode == null)
                {
                    continue;
                }

                if (xmpNode.Options.CompositeProperty)
                {
                    arrayList.Add(xmpNode);
                }
                else if (!xmpNode.Options.HasLanguage)
                {
                    if (string.IsNullOrEmpty(xmpNode.Value))
                    {
                        arrayList.Add(xmpNode);
                        continue;
                    }

                    XmpNode qualNode = new XmpNode("xml:lang", "x-repair", null);
                    xmpNode.AddQualifier(qualNode);
                }
            }

            foreach (object item in arrayList)
            {
                arrayNode.Children.Remove(item);
            }
        }

        private static void MoveExplicitAliases(XmpNode tree, ParseOptions options)
        {
            if (!tree.HasAliases)
            {
                return;
            }

            tree.HasAliases = false;
            bool strictAliasing = options.StrictAliasing;
            IEnumerator enumerator = tree.UnmodifiableChildren.GetEnumerator();
            while (enumerator.MoveNext())
            {
                XmpNode xmpNode = (XmpNode)enumerator.Current;
                if (xmpNode == null || !xmpNode.HasAliases)
                {
                    continue;
                }

                ArrayList arrayList = new ArrayList();
                IEnumerator enumerator2 = xmpNode.IterateChildren();
                while (enumerator2.MoveNext())
                {
                    XmpNode xmpNode2 = (XmpNode)enumerator2.Current;
                    if (xmpNode2 == null || !xmpNode2.Alias)
                    {
                        continue;
                    }

                    xmpNode2.Alias = false;
                    IXmpAliasInfo xmpAliasInfo = XmpMetaFactory.SchemaRegistry.FindAlias(xmpNode2.Name);
                    if (xmpAliasInfo == null)
                    {
                        continue;
                    }

                    XmpNode xmpNode3 = XmpNodeUtils.FindSchemaNode(tree, xmpAliasInfo.Namespace, null, createNodes: true);
                    xmpNode3.Implicit = false;
                    XmpNode xmpNode4 = XmpNodeUtils.FindChildNode(xmpNode3, xmpAliasInfo.Prefix + xmpAliasInfo.PropName, createNodes: false);
                    if (xmpNode4 == null)
                    {
                        if (xmpAliasInfo.AliasForm.Simple)
                        {
                            string text2 = (xmpNode2.Name = xmpAliasInfo.Prefix + xmpAliasInfo.PropName);
                            xmpNode3.AddChild(xmpNode2);
                        }
                        else
                        {
                            xmpNode4 = new XmpNode(xmpAliasInfo.Prefix + xmpAliasInfo.PropName, xmpAliasInfo.AliasForm.ToPropertyOptions());
                            xmpNode3.AddChild(xmpNode4);
                            TransplantArrayItemAlias(xmpNode2, xmpNode4);
                        }

                        arrayList.Add(xmpNode2);
                        continue;
                    }

                    if (xmpAliasInfo.AliasForm.Simple)
                    {
                        if (strictAliasing)
                        {
                            CompareAliasedSubtrees(xmpNode2, xmpNode4, outerCall: true);
                        }

                        arrayList.Add(xmpNode2);
                        continue;
                    }

                    XmpNode xmpNode5 = null;
                    if (xmpAliasInfo.AliasForm.ArrayAltText)
                    {
                        int num = XmpNodeUtils.LookupLanguageItem(xmpNode4, "x-default");
                        if (num != -1)
                        {
                            xmpNode5 = xmpNode4.GetChild(num);
                        }
                    }
                    else if (xmpNode4.HasChildren())
                    {
                        xmpNode5 = xmpNode4.GetChild(1);
                    }

                    if (xmpNode5 == null)
                    {
                        TransplantArrayItemAlias(xmpNode2, xmpNode4);
                    }
                    else if (strictAliasing)
                    {
                        CompareAliasedSubtrees(xmpNode2, xmpNode5, outerCall: true);
                    }

                    arrayList.Add(xmpNode2);
                }

                foreach (object item in arrayList)
                {
                    xmpNode.Children.Remove(item);
                }

                arrayList.Clear();
                xmpNode.HasAliases = false;
            }
        }

        private static void TransplantArrayItemAlias(XmpNode childNode, XmpNode baseArray)
        {
            if (baseArray.Options.ArrayAltText)
            {
                if (childNode.Options.HasLanguage)
                {
                    throw new XmpException("Alias to x-default already has a language qualifier", 203);
                }

                XmpNode qualNode = new XmpNode("xml:lang", "x-default", null);
                childNode.AddQualifier(qualNode);
            }

            childNode.Name = "[]";
            baseArray.AddChild(childNode);
        }

        private static void FixGpsTimeStamp(XmpNode exifSchema)
        {
            XmpNode xmpNode = XmpNodeUtils.FindChildNode(exifSchema, "exif:GPSTimeStamp", createNodes: false);
            if (xmpNode == null)
            {
                return;
            }

            try
            {
                IXmpDateTime xmpDateTime = XmpUtils.ConvertToDate(xmpNode.Value);
                if (xmpDateTime.Year == 0 && xmpDateTime.Month == 0 && xmpDateTime.Day == 0)
                {
                    IXmpDateTime xmpDateTime2 = XmpUtils.ConvertToDate((XmpNodeUtils.FindChildNode(exifSchema, "exif:DateTimeOriginal", createNodes: false) ?? XmpNodeUtils.FindChildNode(exifSchema, "exif:DateTimeDigitized", createNodes: false)).Value);
                    XmpCalendar calendar = xmpDateTime.Calendar;
                    DateTime dateTime2 = (calendar.DateTime = new DateTime(xmpDateTime2.Year, xmpDateTime2.Month, xmpDateTime2.Day, calendar.DateTime.Hour, calendar.DateTime.Minute, calendar.DateTime.Second, calendar.DateTime.Millisecond));
                    xmpDateTime = new XmpDateTimeImpl(calendar);
                    xmpNode.Value = XmpUtils.ConvertFromDate(xmpDateTime);
                }
            }
            catch (XmpException)
            {
            }
        }

        private static void DeleteEmptySchemas(XmpNode tree)
        {
            ArrayList arrayList = new ArrayList();
            foreach (XmpNode child in tree.Children)
            {
                if (!child.HasChildren())
                {
                    arrayList.Add(child);
                }
            }

            foreach (XmpNode item in arrayList)
            {
                tree.Children.Remove(item);
            }
        }

        private static void CompareAliasedSubtrees(XmpNode aliasNode, XmpNode baseNode, bool outerCall)
        {
            if (!aliasNode.Value.Equals(baseNode.Value) || aliasNode.ChildrenLength != baseNode.ChildrenLength)
            {
                throw new XmpException("Mismatch between alias and base nodes", 203);
            }

            if (!outerCall && (!aliasNode.Name.Equals(baseNode.Name) || !aliasNode.Options.Equals(baseNode.Options) || aliasNode.QualifierLength != baseNode.QualifierLength))
            {
                throw new XmpException("Mismatch between alias and base nodes", 203);
            }

            IEnumerator enumerator = aliasNode.IterateChildren();
            IEnumerator enumerator2 = baseNode.IterateChildren();
            while (enumerator.MoveNext() && enumerator2.MoveNext())
            {
                XmpNode aliasNode2 = (XmpNode)enumerator.Current;
                XmpNode baseNode2 = (XmpNode)enumerator2.Current;
                CompareAliasedSubtrees(aliasNode2, baseNode2, outerCall: false);
            }

            IEnumerator enumerator3 = aliasNode.IterateQualifier();
            IEnumerator enumerator4 = baseNode.IterateQualifier();
            while (enumerator3.MoveNext() && enumerator4.MoveNext())
            {
                XmpNode aliasNode3 = (XmpNode)enumerator3.Current;
                XmpNode baseNode3 = (XmpNode)enumerator4.Current;
                CompareAliasedSubtrees(aliasNode3, baseNode3, outerCall: false);
            }
        }

        private static void MigrateAudioCopyright(IXmpMeta xmp, XmpNode dmCopyright)
        {
            try
            {
                XmpNode parent = XmpNodeUtils.FindSchemaNode(((XmpMetaImpl)xmp).Root, "http://purl.org/dc/elements/1.1/", createNodes: true);
                string value = dmCopyright.Value;
                XmpNode xmpNode = XmpNodeUtils.FindChildNode(parent, "dc:rights", createNodes: false);
                if (xmpNode == null || !xmpNode.HasChildren())
                {
                    value = "\n\n" + value;
                    xmp.SetLocalizedText("http://purl.org/dc/elements/1.1/", "rights", "", "x-default", value, null);
                }
                else
                {
                    int num = XmpNodeUtils.LookupLanguageItem(xmpNode, "x-default");
                    if (num < 0)
                    {
                        string value2 = xmpNode.GetChild(1).Value;
                        xmp.SetLocalizedText("http://purl.org/dc/elements/1.1/", "rights", "", "x-default", value2, null);
                        num = XmpNodeUtils.LookupLanguageItem(xmpNode, "x-default");
                    }

                    XmpNode child = xmpNode.GetChild(num);
                    string value3 = child.Value;
                    int num2 = value3.IndexOf("\n\n");
                    if (num2 < 0)
                    {
                        if (!value.Equals(value3))
                        {
                            child.Value = value3 + "\n\n" + value;
                        }
                    }
                    else if (!value3.Substring(num2 + 2).Equals(value))
                    {
                        child.Value = value3.Substring(0, num2 + 2) + value;
                    }
                }

                dmCopyright.Parent.RemoveChild(dmCopyright);
            }
            catch (XmpException)
            {
            }
        }

        private static void InitDcArrays()
        {
            _dcArrayForms = new Hashtable();
            PropertyOptions propertyOptions = new PropertyOptions();
            propertyOptions.Array = true;
            _dcArrayForms["dc:contributor"] = propertyOptions;
            _dcArrayForms["dc:language"] = propertyOptions;
            _dcArrayForms["dc:publisher"] = propertyOptions;
            _dcArrayForms["dc:relation"] = propertyOptions;
            _dcArrayForms["dc:subject"] = propertyOptions;
            _dcArrayForms["dc:type"] = propertyOptions;
            PropertyOptions propertyOptions2 = new PropertyOptions();
            propertyOptions2.Array = true;
            propertyOptions2.ArrayOrdered = true;
            _dcArrayForms["dc:creator"] = propertyOptions2;
            _dcArrayForms["dc:date"] = propertyOptions2;
            PropertyOptions propertyOptions3 = new PropertyOptions();
            propertyOptions3.Array = true;
            propertyOptions3.ArrayOrdered = true;
            propertyOptions3.ArrayAlternate = true;
            propertyOptions3.ArrayAltText = true;
            _dcArrayForms["dc:description"] = propertyOptions3;
            _dcArrayForms["dc:rights"] = propertyOptions3;
            _dcArrayForms["dc:title"] = propertyOptions3;
        }
    }
}
