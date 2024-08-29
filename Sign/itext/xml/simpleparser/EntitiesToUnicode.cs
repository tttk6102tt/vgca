using System.Globalization;
using System.Text;

namespace Sign.itext.xml.simpleparser
{
    public class EntitiesToUnicode
    {
        private static readonly Dictionary<string, char> map;

        static EntitiesToUnicode()
        {
            map = new Dictionary<string, char>();
            map["nbsp"] = '\u00a0';
            map["iexcl"] = '¡';
            map["cent"] = '¢';
            map["pound"] = '£';
            map["curren"] = '¤';
            map["yen"] = '¥';
            map["brvbar"] = '¦';
            map["sect"] = '§';
            map["uml"] = '\u00a8';
            map["copy"] = '©';
            map["ordf"] = 'ª';
            map["laquo"] = '«';
            map["not"] = '¬';
            map["shy"] = '­';
            map["reg"] = '®';
            map["macr"] = '\u00af';
            map["deg"] = '°';
            map["plusmn"] = '±';
            map["sup2"] = '²';
            map["sup3"] = '³';
            map["acute"] = '\u00b4';
            map["micro"] = 'µ';
            map["para"] = '¶';
            map["middot"] = '·';
            map["cedil"] = '\u00b8';
            map["sup1"] = '¹';
            map["ordm"] = 'º';
            map["raquo"] = '»';
            map["frac14"] = '¼';
            map["frac12"] = '½';
            map["frac34"] = '¾';
            map["iquest"] = '¿';
            map["Agrave"] = 'À';
            map["Aacute"] = 'Á';
            map["Acirc"] = 'Â';
            map["Atilde"] = 'Ã';
            map["Auml"] = 'Ä';
            map["Aring"] = 'Å';
            map["AElig"] = 'Æ';
            map["Ccedil"] = 'Ç';
            map["Egrave"] = 'È';
            map["Eacute"] = 'É';
            map["Ecirc"] = 'Ê';
            map["Euml"] = 'Ë';
            map["Igrave"] = 'Ì';
            map["Iacute"] = 'Í';
            map["Icirc"] = 'Î';
            map["Iuml"] = 'Ï';
            map["ETH"] = 'Ð';
            map["Ntilde"] = 'Ñ';
            map["Ograve"] = 'Ò';
            map["Oacute"] = 'Ó';
            map["Ocirc"] = 'Ô';
            map["Otilde"] = 'Õ';
            map["Ouml"] = 'Ö';
            map["times"] = '×';
            map["Oslash"] = 'Ø';
            map["Ugrave"] = 'Ù';
            map["Uacute"] = 'Ú';
            map["Ucirc"] = 'Û';
            map["Uuml"] = 'Ü';
            map["Yacute"] = 'Ý';
            map["THORN"] = 'Þ';
            map["szlig"] = 'ß';
            map["agrave"] = 'à';
            map["aacute"] = 'á';
            map["acirc"] = 'â';
            map["atilde"] = 'ã';
            map["auml"] = 'ä';
            map["aring"] = 'å';
            map["aelig"] = 'æ';
            map["ccedil"] = 'ç';
            map["egrave"] = 'è';
            map["eacute"] = 'é';
            map["ecirc"] = 'ê';
            map["euml"] = 'ë';
            map["igrave"] = 'ì';
            map["iacute"] = 'í';
            map["icirc"] = 'î';
            map["iuml"] = 'ï';
            map["eth"] = 'ð';
            map["ntilde"] = 'ñ';
            map["ograve"] = 'ò';
            map["oacute"] = 'ó';
            map["ocirc"] = 'ô';
            map["otilde"] = 'õ';
            map["ouml"] = 'ö';
            map["divide"] = '÷';
            map["oslash"] = 'ø';
            map["ugrave"] = 'ù';
            map["uacute"] = 'ú';
            map["ucirc"] = 'û';
            map["uuml"] = 'ü';
            map["yacute"] = 'ý';
            map["thorn"] = 'þ';
            map["yuml"] = 'ÿ';
            map["fnof"] = 'ƒ';
            map["Alpha"] = 'Α';
            map["Beta"] = 'Β';
            map["Gamma"] = 'Γ';
            map["Delta"] = 'Δ';
            map["Epsilon"] = 'Ε';
            map["Zeta"] = 'Ζ';
            map["Eta"] = 'Η';
            map["Theta"] = 'Θ';
            map["Iota"] = 'Ι';
            map["Kappa"] = 'Κ';
            map["Lambda"] = 'Λ';
            map["Mu"] = 'Μ';
            map["Nu"] = 'Ν';
            map["Xi"] = 'Ξ';
            map["Omicron"] = 'Ο';
            map["Pi"] = 'Π';
            map["Rho"] = 'Ρ';
            map["Sigma"] = 'Σ';
            map["Tau"] = 'Τ';
            map["Upsilon"] = 'Υ';
            map["Phi"] = 'Φ';
            map["Chi"] = 'Χ';
            map["Psi"] = 'Ψ';
            map["Omega"] = 'Ω';
            map["alpha"] = 'α';
            map["beta"] = 'β';
            map["gamma"] = 'γ';
            map["delta"] = 'δ';
            map["epsilon"] = 'ε';
            map["zeta"] = 'ζ';
            map["eta"] = 'η';
            map["theta"] = 'θ';
            map["iota"] = 'ι';
            map["kappa"] = 'κ';
            map["lambda"] = 'λ';
            map["mu"] = 'μ';
            map["nu"] = 'ν';
            map["xi"] = 'ξ';
            map["omicron"] = 'ο';
            map["pi"] = 'π';
            map["rho"] = 'ρ';
            map["sigmaf"] = 'ς';
            map["sigma"] = 'σ';
            map["tau"] = 'τ';
            map["upsilon"] = 'υ';
            map["phi"] = 'φ';
            map["chi"] = 'χ';
            map["psi"] = 'ψ';
            map["omega"] = 'ω';
            map["thetasym"] = 'ϑ';
            map["upsih"] = 'ϒ';
            map["piv"] = 'ϖ';
            map["bull"] = '•';
            map["hellip"] = '…';
            map["prime"] = '′';
            map["Prime"] = '″';
            map["oline"] = '‾';
            map["frasl"] = '⁄';
            map["weierp"] = '℘';
            map["image"] = 'ℑ';
            map["real"] = 'ℜ';
            map["trade"] = '™';
            map["alefsym"] = 'ℵ';
            map["larr"] = '←';
            map["uarr"] = '↑';
            map["rarr"] = '→';
            map["darr"] = '↓';
            map["harr"] = '↔';
            map["crarr"] = '↵';
            map["lArr"] = '⇐';
            map["uArr"] = '⇑';
            map["rArr"] = '⇒';
            map["dArr"] = '⇓';
            map["hArr"] = '⇔';
            map["forall"] = '∀';
            map["part"] = '∂';
            map["exist"] = '∃';
            map["empty"] = '∅';
            map["nabla"] = '∇';
            map["isin"] = '∈';
            map["notin"] = '∉';
            map["ni"] = '∋';
            map["prod"] = '∏';
            map["sum"] = '∑';
            map["minus"] = '−';
            map["lowast"] = '∗';
            map["radic"] = '√';
            map["prop"] = '∝';
            map["infin"] = '∞';
            map["ang"] = '∠';
            map["and"] = '∧';
            map["or"] = '∨';
            map["cap"] = '∩';
            map["cup"] = '∪';
            map["int"] = '∫';
            map["there4"] = '∴';
            map["sim"] = '∼';
            map["cong"] = '≅';
            map["asymp"] = '≈';
            map["ne"] = '≠';
            map["equiv"] = '≡';
            map["le"] = '≤';
            map["ge"] = '≥';
            map["sub"] = '⊂';
            map["sup"] = '⊃';
            map["nsub"] = '⊄';
            map["sube"] = '⊆';
            map["supe"] = '⊇';
            map["oplus"] = '⊕';
            map["otimes"] = '⊗';
            map["perp"] = '⊥';
            map["sdot"] = '⋅';
            map["lceil"] = '⌈';
            map["rceil"] = '⌉';
            map["lfloor"] = '⌊';
            map["rfloor"] = '⌋';
            map["lang"] = '〈';
            map["rang"] = '〉';
            map["loz"] = '◊';
            map["spades"] = '♠';
            map["clubs"] = '♣';
            map["hearts"] = '♥';
            map["diams"] = '♦';
            map["quot"] = '"';
            map["amp"] = '&';
            map["apos"] = '\'';
            map["lt"] = '<';
            map["gt"] = '>';
            map["OElig"] = 'Œ';
            map["oelig"] = 'œ';
            map["Scaron"] = 'Š';
            map["scaron"] = 'š';
            map["Yuml"] = 'Ÿ';
            map["circ"] = 'ˆ';
            map["tilde"] = '\u02dc';
            map["ensp"] = '\u2002';
            map["emsp"] = '\u2003';
            map["thinsp"] = '\u2009';
            map["zwnj"] = '\u200c';
            map["zwj"] = '\u200d';
            map["lrm"] = '\u200e';
            map["rlm"] = '\u200f';
            map["ndash"] = '–';
            map["mdash"] = '—';
            map["lsquo"] = '‘';
            map["rsquo"] = '’';
            map["sbquo"] = '‚';
            map["ldquo"] = '“';
            map["rdquo"] = '”';
            map["bdquo"] = '„';
            map["dagger"] = '†';
            map["Dagger"] = '‡';
            map["permil"] = '‰';
            map["lsaquo"] = '‹';
            map["rsaquo"] = '›';
            map["euro"] = '€';
        }

        public static char DecodeEntity(string name)
        {
            if (name.StartsWith("#x"))
            {
                try
                {
                    return (char)int.Parse(name.Substring(2), NumberStyles.AllowHexSpecifier);
                }
                catch
                {
                    return '\0';
                }
            }

            if (name.StartsWith("#"))
            {
                try
                {
                    return (char)int.Parse(name.Substring(1));
                }
                catch
                {
                    return '\0';
                }
            }

            if (map.ContainsKey(name))
            {
                return map[name];
            }

            return '\0';
        }

        public static string DecodeString(string s)
        {
            int num = s.IndexOf('&');
            if (num == -1)
            {
                return s;
            }

            StringBuilder stringBuilder = new StringBuilder(s.Substring(0, num));
            int num2;
            while (true)
            {
                num2 = s.IndexOf(';', num);
                if (num2 == -1)
                {
                    stringBuilder.Append(s.Substring(num));
                    return stringBuilder.ToString();
                }

                int num3 = s.IndexOf('&', num + 1);
                while (num3 != -1 && num3 < num2)
                {
                    stringBuilder.Append(s.Substring(num, num3 - num));
                    num = num3;
                    num3 = s.IndexOf('&', num + 1);
                }

                char c = DecodeEntity(s.Substring(num + 1, num2 - (num + 1)));
                if (s.Length < num2 + 1)
                {
                    return stringBuilder.ToString();
                }

                if (c == '\0')
                {
                    stringBuilder.Append(s.Substring(num, num2 + 1 - num));
                }
                else
                {
                    stringBuilder.Append(c);
                }

                num = s.IndexOf('&', num2);
                if (num == -1)
                {
                    break;
                }

                stringBuilder.Append(s.Substring(num2 + 1, num - (num2 + 1)));
            }

            stringBuilder.Append(s.Substring(num2 + 1));
            return stringBuilder.ToString();
        }
    }
}
