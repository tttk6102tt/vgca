using Sign.itext.xml.xmp.options;
using Sign.itext.xml.xmp.properties;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Sign.itext.xml.xmp.impl
{
    public sealed class XmpSchemaRegistryImpl : XmpConst, IXmpSchemaRegistry
    {
        private class XmpAliasInfoImpl : IXmpAliasInfo
        {
            private readonly AliasOptions _aliasForm;

            private readonly string _namespace;

            private readonly string _prefix;

            private readonly string _propName;

            public string Namespace => _namespace;

            public string Prefix => _prefix;

            public string PropName => _propName;

            public AliasOptions AliasForm => _aliasForm;

            public XmpAliasInfoImpl(string @namespace, string prefix, string propName, AliasOptions aliasForm)
            {
                _namespace = @namespace;
                _prefix = prefix;
                _propName = propName;
                _aliasForm = aliasForm;
            }

            public override string ToString()
            {
                return string.Concat(Prefix, PropName, " NS(", Namespace, "), FORM (", AliasForm, ")");
            }
        }

        private readonly IDictionary _aliasMap = new Hashtable();

        private readonly IDictionary _namespaceToPrefixMap = new Hashtable();

        private readonly IDictionary _prefixToNamespaceMap = new Hashtable();

        private readonly Regex _regex = new Regex("[/*?\\[\\]]");

        public XmpSchemaRegistryImpl()
        {
            try
            {
                RegisterStandardNamespaces();
                RegisterStandardAliases();
            }
            catch (XmpException)
            {
                throw new Exception("The XMPSchemaRegistry cannot be initialized!");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string RegisterNamespace(string namespaceUri, string suggestedPrefix)
        {
            ParameterAsserts.AssertSchemaNs(namespaceUri);
            ParameterAsserts.AssertPrefix(suggestedPrefix);
            if (suggestedPrefix[suggestedPrefix.Length - 1] != ':')
            {
                suggestedPrefix += ":";
            }

            if (!Utils.IsXmlNameNs(suggestedPrefix.Substring(0, suggestedPrefix.Length - 1)))
            {
                throw new XmpException("The prefix is a bad XML name", 201);
            }

            string text = (string)_namespaceToPrefixMap[namespaceUri];
            string text2 = (string)_prefixToNamespaceMap[suggestedPrefix];
            if (text != null)
            {
                return text;
            }

            if (text2 != null)
            {
                string text3 = suggestedPrefix;
                int num = 1;
                while (_prefixToNamespaceMap.Contains(text3))
                {
                    text3 = suggestedPrefix.Substring(0, suggestedPrefix.Length - 1) + "_" + num + "_:";
                    num++;
                }

                suggestedPrefix = text3;
            }

            _prefixToNamespaceMap[suggestedPrefix] = namespaceUri;
            _namespaceToPrefixMap[namespaceUri] = suggestedPrefix;
            return suggestedPrefix;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DeleteNamespace(string namespaceUri)
        {
            string namespacePrefix = GetNamespacePrefix(namespaceUri);
            if (namespacePrefix != null)
            {
                _namespaceToPrefixMap.Remove(namespaceUri);
                _prefixToNamespaceMap.Remove(namespacePrefix);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GetNamespacePrefix(string namespaceUri)
        {
            return (string)_namespaceToPrefixMap[namespaceUri];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GetNamespaceUri(string namespacePrefix)
        {
            if (namespacePrefix != null && !namespacePrefix.EndsWith(":"))
            {
                namespacePrefix += ":";
            }

            return (string)_prefixToNamespaceMap[namespacePrefix];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IXmpAliasInfo ResolveAlias(string aliasNs, string aliasProp)
        {
            string namespacePrefix = GetNamespacePrefix(aliasNs);
            if (namespacePrefix == null)
            {
                return null;
            }

            return (IXmpAliasInfo)_aliasMap[namespacePrefix + aliasProp];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IXmpAliasInfo FindAlias(string qname)
        {
            return (IXmpAliasInfo)_aliasMap[qname];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IXmpAliasInfo[] FindAliases(string aliasNs)
        {
            string namespacePrefix = GetNamespacePrefix(aliasNs);
            IList list = new ArrayList();
            if (namespacePrefix != null)
            {
                IEnumerator enumerator = _aliasMap.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string text = (string)enumerator.Current;
                    if (text != null && text.StartsWith(namespacePrefix))
                    {
                        list.Add(FindAlias(text));
                    }
                }
            }

            IXmpAliasInfo[] array = new IXmpAliasInfo[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDictionary GetNamespaces()
        {
            return ReadOnlyDictionary.ReadOnly(new Hashtable(_namespaceToPrefixMap));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDictionary GetPrefixes()
        {
            return ReadOnlyDictionary.ReadOnly(new Hashtable(_prefixToNamespaceMap));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDictionary GetAliases()
        {
            return ReadOnlyDictionary.ReadOnly(new Hashtable(_aliasMap));
        }

        private void RegisterStandardNamespaces()
        {
            RegisterNamespace("http://www.w3.org/XML/1998/namespace", "xml");
            RegisterNamespace("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "rdf");
            RegisterNamespace("http://purl.org/dc/elements/1.1/", "dc");
            RegisterNamespace("http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/", "Iptc4xmpCore");
            RegisterNamespace("http://iptc.org/std/Iptc4xmpExt/2008-02-29/", "Iptc4xmpExt");
            RegisterNamespace("http://ns.adobe.com/DICOM/", "DICOM");
            RegisterNamespace("http://ns.useplus.org/ldf/xmp/1.0/", "plus");
            RegisterNamespace("adobe:ns:meta/", "x");
            RegisterNamespace("http://ns.adobe.com/iX/1.0/", "iX");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/", "xmp");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/rights/", "xmpRights");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/mm/", "xmpMM");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/bj/", "xmpBJ");
            RegisterNamespace("http://ns.adobe.com/xmp/note/", "xmpNote");
            RegisterNamespace("http://ns.adobe.com/pdf/1.3/", "pdf");
            RegisterNamespace("http://ns.adobe.com/pdfx/1.3/", "pdfx");
            RegisterNamespace("http://www.npes.org/pdfx/ns/id/", "pdfxid");
            RegisterNamespace("http://www.aiim.org/pdfa/ns/schema#", "pdfaSchema");
            RegisterNamespace("http://www.aiim.org/pdfa/ns/property#", "pdfaProperty");
            RegisterNamespace("http://www.aiim.org/pdfa/ns/type#", "pdfaType");
            RegisterNamespace("http://www.aiim.org/pdfa/ns/field#", "pdfaField");
            RegisterNamespace("http://www.aiim.org/pdfa/ns/id/", "pdfaid");
            RegisterNamespace("http://www.aiim.org/pdfua/ns/id/", "pdfuaid");
            RegisterNamespace("http://www.aiim.org/pdfa/ns/extension/", "pdfaExtension");
            RegisterNamespace("http://ns.adobe.com/photoshop/1.0/", "photoshop");
            RegisterNamespace("http://ns.adobe.com/album/1.0/", "album");
            RegisterNamespace("http://ns.adobe.com/exif/1.0/", "exif");
            RegisterNamespace("http://cipa.jp/exif/1.0/", "exifEX");
            RegisterNamespace("http://ns.adobe.com/exif/1.0/aux/", "aux");
            RegisterNamespace("http://ns.adobe.com/tiff/1.0/", "tiff");
            RegisterNamespace("http://ns.adobe.com/png/1.0/", "png");
            RegisterNamespace("http://ns.adobe.com/jpeg/1.0/", "jpeg");
            RegisterNamespace("http://ns.adobe.com/jp2k/1.0/", "jp2k");
            RegisterNamespace("http://ns.adobe.com/camera-raw-settings/1.0/", "crs");
            RegisterNamespace("http://ns.adobe.com/StockPhoto/1.0/", "bmsp");
            RegisterNamespace("http://ns.adobe.com/creatorAtom/1.0/", "creatorAtom");
            RegisterNamespace("http://ns.adobe.com/asf/1.0/", "asf");
            RegisterNamespace("http://ns.adobe.com/xmp/wav/1.0/", "wav");
            RegisterNamespace("http://ns.adobe.com/bwf/bext/1.0/", "bext");
            RegisterNamespace("http://ns.adobe.com/riff/info/", "riffinfo");
            RegisterNamespace("http://ns.adobe.com/xmp/1.0/Script/", "xmpScript");
            RegisterNamespace("http://ns.adobe.com/TransformXMP/", "txmp");
            RegisterNamespace("http://ns.adobe.com/swf/1.0/", "swf");
            RegisterNamespace("http://ns.adobe.com/xmp/1.0/DynamicMedia/", "xmpDM");
            RegisterNamespace("http://ns.adobe.com/xmp/transient/1.0/", "xmpx");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/t/", "xmpT");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/t/pg/", "xmpTPg");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/g/", "xmpG");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/g/img/", "xmpGImg");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/Font#", "stFnt");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/Dimensions#", "stDim");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/ResourceEvent#", "stEvt");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/ResourceRef#", "stRef");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/Version#", "stVer");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/Job#", "stJob");
            RegisterNamespace("http://ns.adobe.com/xap/1.0/sType/ManifestItem#", "stMfs");
            RegisterNamespace("http://ns.adobe.com/xmp/Identifier/qual/1.0/", "xmpidq");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void RegisterAlias(string aliasNs, string aliasProp, string actualNs, string actualProp, AliasOptions aliasForm)
        {
            ParameterAsserts.AssertSchemaNs(aliasNs);
            ParameterAsserts.AssertPropName(aliasProp);
            ParameterAsserts.AssertSchemaNs(actualNs);
            ParameterAsserts.AssertPropName(actualProp);
            AliasOptions aliasForm2 = ((aliasForm != null) ? new AliasOptions(XmpNodeUtils.VerifySetOptions(aliasForm.ToPropertyOptions(), null).Options) : new AliasOptions());
            if (_regex.IsMatch(aliasProp) || _regex.IsMatch(actualProp))
            {
                throw new XmpException("Alias and actual property names must be simple", 102);
            }

            string namespacePrefix = GetNamespacePrefix(aliasNs);
            string namespacePrefix2 = GetNamespacePrefix(actualNs);
            if (namespacePrefix == null)
            {
                throw new XmpException("Alias namespace is not registered", 101);
            }

            if (namespacePrefix2 == null)
            {
                throw new XmpException("Actual namespace is not registered", 101);
            }

            string key = namespacePrefix + aliasProp;
            if (_aliasMap.Contains(key))
            {
                throw new XmpException("Alias is already existing", 4);
            }

            if (_aliasMap.Contains(namespacePrefix2 + actualProp))
            {
                throw new XmpException("Actual property is already an alias, use the base property", 4);
            }

            IXmpAliasInfo value = new XmpAliasInfoImpl(actualNs, namespacePrefix2, actualProp, aliasForm2);
            _aliasMap[key] = value;
        }

        private void RegisterStandardAliases()
        {
            AliasOptions aliasOptions = new AliasOptions();
            aliasOptions.ArrayOrdered = true;
            AliasOptions aliasOptions2 = new AliasOptions();
            aliasOptions2.ArrayAltText = true;
            RegisterAlias("http://ns.adobe.com/xap/1.0/", "Author", "http://purl.org/dc/elements/1.1/", "creator", aliasOptions);
            RegisterAlias("http://ns.adobe.com/xap/1.0/", "Authors", "http://purl.org/dc/elements/1.1/", "creator", null);
            RegisterAlias("http://ns.adobe.com/xap/1.0/", "Description", "http://purl.org/dc/elements/1.1/", "description", null);
            RegisterAlias("http://ns.adobe.com/xap/1.0/", "Format", "http://purl.org/dc/elements/1.1/", "format", null);
            RegisterAlias("http://ns.adobe.com/xap/1.0/", "Keywords", "http://purl.org/dc/elements/1.1/", "subject", null);
            RegisterAlias("http://ns.adobe.com/xap/1.0/", "Locale", "http://purl.org/dc/elements/1.1/", "language", null);
            RegisterAlias("http://ns.adobe.com/xap/1.0/", "Title", "http://purl.org/dc/elements/1.1/", "title", null);
            RegisterAlias("http://ns.adobe.com/xap/1.0/rights/", "Copyright", "http://purl.org/dc/elements/1.1/", "rights", null);
            RegisterAlias("http://ns.adobe.com/pdf/1.3/", "Author", "http://purl.org/dc/elements/1.1/", "creator", aliasOptions);
            RegisterAlias("http://ns.adobe.com/pdf/1.3/", "BaseURL", "http://ns.adobe.com/xap/1.0/", "BaseURL", null);
            RegisterAlias("http://ns.adobe.com/pdf/1.3/", "CreationDate", "http://ns.adobe.com/xap/1.0/", "CreateDate", null);
            RegisterAlias("http://ns.adobe.com/pdf/1.3/", "Creator", "http://ns.adobe.com/xap/1.0/", "CreatorTool", null);
            RegisterAlias("http://ns.adobe.com/pdf/1.3/", "ModDate", "http://ns.adobe.com/xap/1.0/", "ModifyDate", null);
            RegisterAlias("http://ns.adobe.com/pdf/1.3/", "Subject", "http://purl.org/dc/elements/1.1/", "description", aliasOptions2);
            RegisterAlias("http://ns.adobe.com/pdf/1.3/", "Title", "http://purl.org/dc/elements/1.1/", "title", aliasOptions2);
            RegisterAlias("http://ns.adobe.com/photoshop/1.0/", "Author", "http://purl.org/dc/elements/1.1/", "creator", aliasOptions);
            RegisterAlias("http://ns.adobe.com/photoshop/1.0/", "Caption", "http://purl.org/dc/elements/1.1/", "description", aliasOptions2);
            RegisterAlias("http://ns.adobe.com/photoshop/1.0/", "Copyright", "http://purl.org/dc/elements/1.1/", "rights", aliasOptions2);
            RegisterAlias("http://ns.adobe.com/photoshop/1.0/", "Keywords", "http://purl.org/dc/elements/1.1/", "subject", null);
            RegisterAlias("http://ns.adobe.com/photoshop/1.0/", "Marked", "http://ns.adobe.com/xap/1.0/rights/", "Marked", null);
            RegisterAlias("http://ns.adobe.com/photoshop/1.0/", "Title", "http://purl.org/dc/elements/1.1/", "title", aliasOptions2);
            RegisterAlias("http://ns.adobe.com/photoshop/1.0/", "WebStatement", "http://ns.adobe.com/xap/1.0/rights/", "WebStatement", null);
            RegisterAlias("http://ns.adobe.com/tiff/1.0/", "Artist", "http://purl.org/dc/elements/1.1/", "creator", aliasOptions);
            RegisterAlias("http://ns.adobe.com/tiff/1.0/", "Copyright", "http://purl.org/dc/elements/1.1/", "rights", null);
            RegisterAlias("http://ns.adobe.com/tiff/1.0/", "DateTime", "http://ns.adobe.com/xap/1.0/", "ModifyDate", null);
            RegisterAlias("http://ns.adobe.com/tiff/1.0/", "ImageDescription", "http://purl.org/dc/elements/1.1/", "description", null);
            RegisterAlias("http://ns.adobe.com/tiff/1.0/", "Software", "http://ns.adobe.com/xap/1.0/", "CreatorTool", null);
            RegisterAlias("http://ns.adobe.com/png/1.0/", "Author", "http://purl.org/dc/elements/1.1/", "creator", aliasOptions);
            RegisterAlias("http://ns.adobe.com/png/1.0/", "Copyright", "http://purl.org/dc/elements/1.1/", "rights", aliasOptions2);
            RegisterAlias("http://ns.adobe.com/png/1.0/", "CreationTime", "http://ns.adobe.com/xap/1.0/", "CreateDate", null);
            RegisterAlias("http://ns.adobe.com/png/1.0/", "Description", "http://purl.org/dc/elements/1.1/", "description", aliasOptions2);
            RegisterAlias("http://ns.adobe.com/png/1.0/", "ModificationTime", "http://ns.adobe.com/xap/1.0/", "ModifyDate", null);
            RegisterAlias("http://ns.adobe.com/png/1.0/", "Software", "http://ns.adobe.com/xap/1.0/", "CreatorTool", null);
            RegisterAlias("http://ns.adobe.com/png/1.0/", "Title", "http://purl.org/dc/elements/1.1/", "title", aliasOptions2);
        }
    }
}
