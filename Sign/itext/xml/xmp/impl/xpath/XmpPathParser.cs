using Sign.itext.xml.xmp.properties;

namespace Sign.itext.xml.xmp.impl.xpath
{
    public sealed class XmpPathParser
    {
        internal XmpPathParser()
        {
        }

        public static XmpPath ExpandXPath(string schemaNs, string path)
        {
            if (schemaNs == null || path == null)
            {
                throw new XmpException("Parameter must not be null", 4);
            }

            XmpPath xmpPath = new XmpPath();
            PathPosition pathPosition = new PathPosition();
            pathPosition.Path = path;
            ParseRootNode(schemaNs, pathPosition, xmpPath);
            while (pathPosition.StepEnd < path.Length)
            {
                pathPosition.StepBegin = pathPosition.StepEnd;
                SkipPathDelimiter(path, pathPosition);
                pathPosition.StepEnd = pathPosition.StepBegin;
                XmpPathSegment xmpPathSegment = ((path[pathPosition.StepBegin] != '[') ? ParseStructSegment(pathPosition) : ParseIndexSegment(pathPosition));
                if (xmpPathSegment.Kind == 1)
                {
                    if (xmpPathSegment.Name[0] == '@')
                    {
                        xmpPathSegment.Name = "?" + xmpPathSegment.Name.Substring(1);
                        if (!"?xml:lang".Equals(xmpPathSegment.Name))
                        {
                            throw new XmpException("Only xml:lang allowed with '@'", 102);
                        }
                    }

                    if (xmpPathSegment.Name[0] == '?')
                    {
                        pathPosition.NameStart++;
                        xmpPathSegment.Kind = 2u;
                    }

                    VerifyQualName(pathPosition.Path.Substring(pathPosition.NameStart, pathPosition.NameEnd - pathPosition.NameStart));
                }
                else if (xmpPathSegment.Kind == 6)
                {
                    if (xmpPathSegment.Name[1] == '@')
                    {
                        xmpPathSegment.Name = "[?" + xmpPathSegment.Name.Substring(2);
                        if (!xmpPathSegment.Name.StartsWith("[?xml:lang="))
                        {
                            throw new XmpException("Only xml:lang allowed with '@'", 102);
                        }
                    }

                    if (xmpPathSegment.Name[1] == '?')
                    {
                        pathPosition.NameStart++;
                        xmpPathSegment.Kind = 5u;
                        VerifyQualName(pathPosition.Path.Substring(pathPosition.NameStart, pathPosition.NameEnd - pathPosition.NameStart));
                    }
                }

                xmpPath.Add(xmpPathSegment);
            }

            return xmpPath;
        }

        internal static void SkipPathDelimiter(string path, PathPosition pos)
        {
            if (path[pos.StepBegin] == '/')
            {
                pos.StepBegin++;
                if (pos.StepBegin >= path.Length)
                {
                    throw new XmpException("Empty XmpPath segment", 102);
                }
            }

            if (path[pos.StepBegin] == '*')
            {
                pos.StepBegin++;
                if (pos.StepBegin >= path.Length || path[pos.StepBegin] != '[')
                {
                    throw new XmpException("Missing '[' after '*'", 102);
                }
            }
        }

        internal static XmpPathSegment ParseStructSegment(PathPosition pos)
        {
            pos.NameStart = pos.StepBegin;
            while (pos.StepEnd < pos.Path.Length && "/[*".IndexOf(pos.Path[pos.StepEnd]) < 0)
            {
                pos.StepEnd++;
            }

            pos.NameEnd = pos.StepEnd;
            if (pos.StepEnd == pos.StepBegin)
            {
                throw new XmpException("Empty XmpPath segment", 102);
            }

            return new XmpPathSegment(pos.Path.Substring(pos.StepBegin, pos.StepEnd - pos.StepBegin), 1u);
        }

        internal static XmpPathSegment ParseIndexSegment(PathPosition pos)
        {
            pos.StepEnd++;
            XmpPathSegment xmpPathSegment;
            if ('0' <= pos.Path[pos.StepEnd] && pos.Path[pos.StepEnd] <= '9')
            {
                while (pos.StepEnd < pos.Path.Length && '0' <= pos.Path[pos.StepEnd] && pos.Path[pos.StepEnd] <= '9')
                {
                    pos.StepEnd++;
                }

                xmpPathSegment = new XmpPathSegment(null, 3u);
            }
            else
            {
                while (pos.StepEnd < pos.Path.Length && pos.Path[pos.StepEnd] != ']' && pos.Path[pos.StepEnd] != '=')
                {
                    pos.StepEnd++;
                }

                if (pos.StepEnd >= pos.Path.Length)
                {
                    throw new XmpException("Missing ']' or '=' for array index", 102);
                }

                if (pos.Path[pos.StepEnd] == ']')
                {
                    if (!"[last()".Equals(pos.Path.Substring(pos.StepBegin, pos.StepEnd - pos.StepBegin)))
                    {
                        throw new XmpException("Invalid non-numeric array index", 102);
                    }

                    xmpPathSegment = new XmpPathSegment(null, 4u);
                }
                else
                {
                    pos.NameStart = pos.StepBegin + 1;
                    pos.NameEnd = pos.StepEnd;
                    pos.StepEnd++;
                    char c = pos.Path[pos.StepEnd];
                    if (c != '\'' && c != '"')
                    {
                        throw new XmpException("Invalid quote in array selector", 102);
                    }

                    pos.StepEnd++;
                    while (pos.StepEnd < pos.Path.Length)
                    {
                        if (pos.Path[pos.StepEnd] == c)
                        {
                            if (pos.StepEnd + 1 >= pos.Path.Length || pos.Path[pos.StepEnd + 1] != c)
                            {
                                break;
                            }

                            pos.StepEnd++;
                        }

                        pos.StepEnd++;
                    }

                    if (pos.StepEnd >= pos.Path.Length)
                    {
                        throw new XmpException("No terminating quote for array selector", 102);
                    }

                    pos.StepEnd++;
                    xmpPathSegment = new XmpPathSegment(null, 6u);
                }
            }

            if (pos.StepEnd >= pos.Path.Length || pos.Path[pos.StepEnd] != ']')
            {
                throw new XmpException("Missing ']' for array index", 102);
            }

            pos.StepEnd++;
            xmpPathSegment.Name = pos.Path.Substring(pos.StepBegin, pos.StepEnd - pos.StepBegin);
            return xmpPathSegment;
        }

        internal static void ParseRootNode(string schemaNs, PathPosition pos, XmpPath expandedXPath)
        {
            while (pos.StepEnd < pos.Path.Length && "/[*".IndexOf(pos.Path[pos.StepEnd]) < 0)
            {
                pos.StepEnd++;
            }

            if (pos.StepEnd == pos.StepBegin)
            {
                throw new XmpException("Empty initial XmpPath step", 102);
            }

            string text = VerifyXPathRoot(schemaNs, pos.Path.Substring(pos.StepBegin, pos.StepEnd - pos.StepBegin));
            IXmpAliasInfo xmpAliasInfo = XmpMetaFactory.SchemaRegistry.FindAlias(text);
            if (xmpAliasInfo == null)
            {
                expandedXPath.Add(new XmpPathSegment(schemaNs, 2147483648u));
                XmpPathSegment segment = new XmpPathSegment(text, 1u);
                expandedXPath.Add(segment);
                return;
            }

            expandedXPath.Add(new XmpPathSegment(xmpAliasInfo.Namespace, 2147483648u));
            XmpPathSegment xmpPathSegment = new XmpPathSegment(VerifyXPathRoot(xmpAliasInfo.Namespace, xmpAliasInfo.PropName), 1u);
            xmpPathSegment.Alias = true;
            xmpPathSegment.AliasForm = xmpAliasInfo.AliasForm.Options;
            expandedXPath.Add(xmpPathSegment);
            if (xmpAliasInfo.AliasForm.ArrayAltText)
            {
                XmpPathSegment xmpPathSegment2 = new XmpPathSegment("[?xml:lang='x-default']", 5u);
                xmpPathSegment2.Alias = true;
                xmpPathSegment2.AliasForm = xmpAliasInfo.AliasForm.Options;
                expandedXPath.Add(xmpPathSegment2);
            }
            else if (xmpAliasInfo.AliasForm.Array)
            {
                XmpPathSegment xmpPathSegment3 = new XmpPathSegment("[1]", 3u);
                xmpPathSegment3.Alias = true;
                xmpPathSegment3.AliasForm = xmpAliasInfo.AliasForm.Options;
                expandedXPath.Add(xmpPathSegment3);
            }
        }

        internal static void VerifyQualName(string qualName)
        {
            int num = qualName.IndexOf(':');
            if (num > 0)
            {
                string text = qualName.Substring(0, num);
                if (Utils.IsXmlNameNs(text))
                {
                    if (XmpMetaFactory.SchemaRegistry.GetNamespaceUri(text) != null)
                    {
                        return;
                    }

                    throw new XmpException("Unknown namespace prefix for qualified name", 102);
                }
            }

            throw new XmpException("Ill-formed qualified name", 102);
        }

        internal static void VerifySimpleXmlName(string name)
        {
            if (!Utils.IsXmlName(name))
            {
                throw new XmpException("Bad XML name", 102);
            }
        }

        internal static string VerifyXPathRoot(string schemaNs, string rootProp)
        {
            if (string.IsNullOrEmpty(schemaNs))
            {
                throw new XmpException("Schema namespace URI is required", 101);
            }

            if (rootProp[0] == '?' || rootProp[0] == '@')
            {
                throw new XmpException("Top level name must not be a qualifier", 102);
            }

            if (rootProp.IndexOf('/') >= 0 || rootProp.IndexOf('[') >= 0)
            {
                throw new XmpException("Top level name must be simple", 102);
            }

            string namespacePrefix = XmpMetaFactory.SchemaRegistry.GetNamespacePrefix(schemaNs);
            if (namespacePrefix == null)
            {
                throw new XmpException("Unregistered schema namespace URI", 101);
            }

            int num = rootProp.IndexOf(':');
            if (num < 0)
            {
                VerifySimpleXmlName(rootProp);
                return namespacePrefix + rootProp;
            }

            VerifySimpleXmlName(rootProp.Substring(0, num));
            VerifySimpleXmlName(rootProp.Substring(num));
            namespacePrefix = rootProp.Substring(0, num + 1);
            string namespacePrefix2 = XmpMetaFactory.SchemaRegistry.GetNamespacePrefix(schemaNs);
            if (namespacePrefix2 == null)
            {
                throw new XmpException("Unknown schema namespace prefix", 101);
            }

            if (!namespacePrefix.Equals(namespacePrefix2))
            {
                throw new XmpException("Schema namespace URI and prefix mismatch", 101);
            }

            return rootProp;
        }
    }
}
