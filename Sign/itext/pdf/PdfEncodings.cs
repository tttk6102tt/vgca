using Sign.itext.xml.simpleparser;
using System.Globalization;
using System.Text;

namespace Sign.itext.pdf
{
    public class PdfEncodings
    {
        private class WingdingsConversion : IExtraEncoding
        {
            private static byte[] table = new byte[191]
            {
                0, 35, 34, 0, 0, 0, 41, 62, 81, 42,
                0, 0, 65, 63, 0, 0, 0, 0, 0, 252,
                0, 0, 0, 251, 0, 0, 0, 0, 0, 0,
                86, 0, 88, 89, 0, 0, 0, 0, 0, 0,
                0, 0, 181, 0, 0, 0, 0, 0, 182, 0,
                0, 0, 173, 175, 172, 0, 0, 0, 0, 0,
                0, 0, 0, 124, 123, 0, 0, 0, 84, 0,
                0, 0, 0, 0, 0, 0, 0, 166, 0, 0,
                0, 113, 114, 0, 0, 0, 117, 0, 0, 0,
                0, 0, 0, 125, 126, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 140, 141,
                142, 143, 144, 145, 146, 147, 148, 149, 129, 130,
                131, 132, 133, 134, 135, 136, 137, 138, 140, 141,
                142, 143, 144, 145, 146, 147, 148, 149, 232, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 232, 216, 0, 0, 196, 198, 0, 0, 240,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 220,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0
            };

            public virtual byte[] CharToByte(char char1, string encoding)
            {
                if (char1 == ' ')
                {
                    return new byte[1] { (byte)char1 };
                }

                if (char1 >= '✁' && char1 <= '➾')
                {
                    byte b = table[char1 - 9984];
                    if (b != 0)
                    {
                        return new byte[1] { b };
                    }
                }

                return new byte[0];
            }

            public virtual byte[] CharToByte(string text, string encoding)
            {
                char[] array = text.ToCharArray();
                byte[] array2 = new byte[array.Length];
                int num = 0;
                int num2 = array.Length;
                for (int i = 0; i < num2; i++)
                {
                    char c = array[i];
                    if (c == ' ')
                    {
                        array2[num++] = (byte)c;
                    }
                    else if (c >= '✁' && c <= '➾')
                    {
                        byte b = table[c - 9984];
                        if (b != 0)
                        {
                            array2[num++] = b;
                        }
                    }
                }

                if (num == num2)
                {
                    return array2;
                }

                byte[] array3 = new byte[num];
                Array.Copy(array2, 0, array3, 0, num);
                return array3;
            }

            public virtual string ByteToChar(byte[] b, string encoding)
            {
                return null;
            }
        }

        private class Cp437Conversion : IExtraEncoding
        {
            private static IntHashtable c2b;

            private static char[] table;

            public virtual byte[] CharToByte(string text, string encoding)
            {
                char[] array = text.ToCharArray();
                byte[] array2 = new byte[array.Length];
                int num = 0;
                int num2 = array.Length;
                for (int i = 0; i < num2; i++)
                {
                    char c = array[i];
                    if (c < '\u0080')
                    {
                        array2[num++] = (byte)c;
                        continue;
                    }

                    byte b = (byte)c2b[c];
                    if (b != 0)
                    {
                        array2[num++] = b;
                    }
                }

                if (num == num2)
                {
                    return array2;
                }

                byte[] array3 = new byte[num];
                Array.Copy(array2, 0, array3, 0, num);
                return array3;
            }

            public virtual byte[] CharToByte(char char1, string encoding)
            {
                if (char1 < '\u0080')
                {
                    return new byte[1] { (byte)char1 };
                }

                byte b = (byte)c2b[char1];
                if (b != 0)
                {
                    return new byte[1] { b };
                }

                return new byte[0];
            }

            public virtual string ByteToChar(byte[] b, string encoding)
            {
                int num = b.Length;
                char[] array = new char[num];
                int length = 0;
                for (int i = 0; i < num; i++)
                {
                    int num2 = b[i] & 0xFF;
                    if (num2 >= 32)
                    {
                        if (num2 < 128)
                        {
                            array[length++] = (char)num2;
                            continue;
                        }

                        char c = table[num2 - 128];
                        array[length++] = c;
                    }
                }

                return new string(array, 0, length);
            }

            static Cp437Conversion()
            {
                c2b = new IntHashtable();
                table = new char[128]
                {
                    'Ç', 'ü', 'é', 'â', 'ä', 'à', 'å', 'ç', 'ê', 'ë',
                    'è', 'ï', 'î', 'ì', 'Ä', 'Å', 'É', 'æ', 'Æ', 'ô',
                    'ö', 'ò', 'û', 'ù', 'ÿ', 'Ö', 'Ü', '¢', '£', '¥',
                    '₧', 'ƒ', 'á', 'í', 'ó', 'ú', 'ñ', 'Ñ', 'ª', 'º',
                    '¿', '⌐', '¬', '½', '¼', '¡', '«', '»', '░', '▒',
                    '▓', '│', '┤', '╡', '╢', '╖', '╕', '╣', '║', '╗',
                    '╝', '╜', '╛', '┐', '└', '┴', '┬', '├', '─', '┼',
                    '╞', '╟', '╚', '╔', '╩', '╦', '╠', '═', '╬', '╧',
                    '╨', '╤', '╥', '╙', '╘', '╒', '╓', '╫', '╪', '┘',
                    '┌', '█', '▄', '▌', '▐', '▀', 'α', 'ß', 'Γ', 'π',
                    'Σ', 'σ', 'µ', 'τ', 'Φ', 'Θ', 'Ω', 'δ', '∞', 'φ',
                    'ε', '∩', '≡', '±', '≥', '≤', '⌠', '⌡', '÷', '≈',
                    '°', '∙', '·', '√', 'ⁿ', '²', '■', '\u00a0'
                };
                for (int i = 0; i < table.Length; i++)
                {
                    c2b[table[i]] = i + 128;
                }
            }
        }

        private class SymbolConversion : IExtraEncoding
        {
            private static IntHashtable t1;

            private static IntHashtable t2;

            private IntHashtable translation;

            private readonly char[] byteToChar;

            private static char[] table1;

            private static char[] table2;

            internal SymbolConversion(bool symbol)
            {
                if (symbol)
                {
                    translation = t1;
                    byteToChar = table1;
                }
                else
                {
                    translation = t2;
                    byteToChar = table2;
                }
            }

            public virtual byte[] CharToByte(string text, string encoding)
            {
                char[] array = text.ToCharArray();
                byte[] array2 = new byte[array.Length];
                int num = 0;
                int num2 = array.Length;
                for (int i = 0; i < num2; i++)
                {
                    char key = array[i];
                    byte b = (byte)translation[key];
                    if (b != 0)
                    {
                        array2[num++] = b;
                    }
                }

                if (num == num2)
                {
                    return array2;
                }

                byte[] array3 = new byte[num];
                Array.Copy(array2, 0, array3, 0, num);
                return array3;
            }

            public virtual byte[] CharToByte(char char1, string encoding)
            {
                byte b = (byte)translation[char1];
                if (b != 0)
                {
                    return new byte[1] { b };
                }

                return new byte[0];
            }

            public virtual string ByteToChar(byte[] b, string encoding)
            {
                int num = b.Length;
                char[] array = new char[num];
                int length = 0;
                for (int i = 0; i < num; i++)
                {
                    int num2 = b[i] & 0xFF;
                    char c = byteToChar[num2];
                    array[length++] = c;
                }

                return new string(array, 0, length);
            }

            static SymbolConversion()
            {
                t1 = new IntHashtable();
                t2 = new IntHashtable();
                table1 = new char[256]
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', ' ', '!', '∀', '#', '∃', '%', '&', '∋',
                    '(', ')', '*', '+', ',', '-', '.', '/', '0', '1',
                    '2', '3', '4', '5', '6', '7', '8', '9', ':', ';',
                    '<', '=', '>', '?', '≅', 'Α', 'Β', 'Χ', 'Δ', 'Ε',
                    'Φ', 'Γ', 'Η', 'Ι', 'ϑ', 'Κ', 'Λ', 'Μ', 'Ν', 'Ο',
                    'Π', 'Θ', 'Ρ', 'Σ', 'Τ', 'Υ', 'ς', 'Ω', 'Ξ', 'Ψ',
                    'Ζ', '[', '∴', ']', '⊥', '_', '\u0305', 'α', 'β', 'χ',
                    'δ', 'ε', 'ϕ', 'γ', 'η', 'ι', 'φ', 'κ', 'λ', 'μ',
                    'ν', 'ο', 'π', 'θ', 'ρ', 'σ', 'τ', 'υ', 'ϖ', 'ω',
                    'ξ', 'ψ', 'ζ', '{', '|', '}', '~', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '€', 'ϒ', '′', '≤', '⁄', '∞', 'ƒ', '♣', '♦', '♥',
                    '♠', '↔', '←', '↑', '→', '↓', '°', '±', '″', '≥',
                    '×', '∝', '∂', '•', '÷', '≠', '≡', '≈', '…', '│',
                    '─', '↵', 'ℵ', 'ℑ', 'ℜ', '℘', '⊗', '⊕', '∅', '∩',
                    '∪', '⊃', '⊇', '⊄', '⊂', '⊆', '∈', '∉', '∠', '∇',
                    '®', '©', '™', '∏', '√', '⋅', '¬', '∧', '∨', '⇔',
                    '⇐', '⇑', '⇒', '⇓', '◊', '〈', '\0', '\0', '\0', '∑',
                    '⎛', '⎜', '⎝', '⎡', '⎢', '⎣', '⎧', '⎨', '⎩', '⎪',
                    '\0', '〉', '∫', '⌠', '⎮', '⌡', '⎞', '⎟', '⎠', '⎤',
                    '⎥', '⎦', '⎫', '⎬', '⎭', '\0'
                };
                table2 = new char[256]
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', ' ', '✁', '✂', '✃', '✄', '☎', '✆', '✇',
                    '✈', '✉', '☛', '☞', '✌', '✍', '✎', '✏', '✐', '✑',
                    '✒', '✓', '✔', '✕', '✖', '✗', '✘', '✙', '✚', '✛',
                    '✜', '✝', '✞', '✟', '✠', '✡', '✢', '✣', '✤', '✥',
                    '✦', '✧', '★', '✩', '✪', '✫', '✬', '✭', '✮', '✯',
                    '✰', '✱', '✲', '✳', '✴', '✵', '✶', '✷', '✸', '✹',
                    '✺', '✻', '✼', '✽', '✾', '✿', '❀', '❁', '❂', '❃',
                    '❄', '❅', '❆', '❇', '❈', '❉', '❊', '❋', '●', '❍',
                    '■', '❏', '❐', '❑', '❒', '▲', '▼', '◆', '❖', '◗',
                    '❘', '❙', '❚', '❛', '❜', '❝', '❞', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '❡', '❢', '❣', '❤', '❥', '❦', '❧', '♣', '♦',
                    '♥', '♠', '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧',
                    '⑨', '⑩', '❶', '❷', '❸', '❹', '❺', '❻', '❼', '❽',
                    '❾', '❿', '➀', '➁', '➂', '➃', '➄', '➅', '➆', '➇',
                    '➈', '➉', '➊', '➋', '➌', '➍', '➎', '➏', '➐', '➑',
                    '➒', '➓', '➔', '→', '↔', '↕', '➘', '➙', '➚', '➛',
                    '➜', '➝', '➞', '➟', '➠', '➡', '➢', '➣', '➤', '➥',
                    '➦', '➧', '➨', '➩', '➪', '➫', '➬', '➭', '➮', '➯',
                    '\0', '➱', '➲', '➳', '➴', '➵', '➶', '➷', '➸', '➹',
                    '➺', '➻', '➼', '➽', '➾', '\0'
                };
                for (int i = 0; i < 256; i++)
                {
                    int num = table1[i];
                    if (num != 0)
                    {
                        t1[num] = i;
                    }
                }

                for (int j = 0; j < 256; j++)
                {
                    int num2 = table2[j];
                    if (num2 != 0)
                    {
                        t2[num2] = j;
                    }
                }
            }
        }

        private class SymbolTTConversion : IExtraEncoding
        {
            public virtual byte[] CharToByte(char char1, string encoding)
            {
                if ((char1 & 0xFF00) == 0 || (char1 & 0xFF00) == 61440)
                {
                    return new byte[1] { (byte)char1 };
                }

                return new byte[0];
            }

            public virtual byte[] CharToByte(string text, string encoding)
            {
                char[] array = text.ToCharArray();
                byte[] array2 = new byte[array.Length];
                int num = 0;
                int num2 = array.Length;
                for (int i = 0; i < num2; i++)
                {
                    char c = array[i];
                    if ((c & 0xFF00) == 0 || (c & 0xFF00) == 61440)
                    {
                        array2[num++] = (byte)c;
                    }
                }

                if (num == num2)
                {
                    return array2;
                }

                byte[] array3 = new byte[num];
                Array.Copy(array2, 0, array3, 0, num);
                return array3;
            }

            public virtual string ByteToChar(byte[] b, string encoding)
            {
                return null;
            }
        }

        internal static char[] winansiByteToChar;

        internal static char[] pdfEncodingByteToChar;

        internal static IntHashtable winansi;

        internal static IntHashtable pdfEncoding;

        internal static Dictionary<string, IExtraEncoding> extraEncodings;

        static PdfEncodings()
        {
            winansiByteToChar = new char[256]
            {
                '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t',
                '\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013',
                '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d',
                '\u001e', '\u001f', ' ', '!', '"', '#', '$', '%', '&', '\'',
                '(', ')', '*', '+', ',', '-', '.', '/', '0', '1',
                '2', '3', '4', '5', '6', '7', '8', '9', ':', ';',
                '<', '=', '>', '?', '@', 'A', 'B', 'C', 'D', 'E',
                'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
                'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y',
                'Z', '[', '\\', ']', '^', '_', '`', 'a', 'b', 'c',
                'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
                'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
                'x', 'y', 'z', '{', '|', '}', '~', '\u007f', '€', '\ufffd',
                '‚', 'ƒ', '„', '…', '†', '‡', 'ˆ', '‰', 'Š', '‹',
                'Œ', '\ufffd', 'Ž', '\ufffd', '\ufffd', '‘', '’', '“', '”', '•',
                '–', '—', '\u02dc', '™', 'š', '›', 'œ', '\ufffd', 'ž', 'Ÿ',
                '\u00a0', '¡', '¢', '£', '¤', '¥', '¦', '§', '\u00a8', '©',
                'ª', '«', '¬', '­', '®', '\u00af', '°', '±', '²', '³',
                '\u00b4', 'µ', '¶', '·', '\u00b8', '¹', 'º', '»', '¼', '½',
                '¾', '¿', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ', 'Ç',
                'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï', 'Ð', 'Ñ',
                'Ò', 'Ó', 'Ô', 'Õ', 'Ö', '×', 'Ø', 'Ù', 'Ú', 'Û',
                'Ü', 'Ý', 'Þ', 'ß', 'à', 'á', 'â', 'ã', 'ä', 'å',
                'æ', 'ç', 'è', 'é', 'ê', 'ë', 'ì', 'í', 'î', 'ï',
                'ð', 'ñ', 'ò', 'ó', 'ô', 'õ', 'ö', '÷', 'ø', 'ù',
                'ú', 'û', 'ü', 'ý', 'þ', 'ÿ'
            };
            pdfEncodingByteToChar = new char[256]
            {
                '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t',
                '\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013',
                '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d',
                '\u001e', '\u001f', ' ', '!', '"', '#', '$', '%', '&', '\'',
                '(', ')', '*', '+', ',', '-', '.', '/', '0', '1',
                '2', '3', '4', '5', '6', '7', '8', '9', ':', ';',
                '<', '=', '>', '?', '@', 'A', 'B', 'C', 'D', 'E',
                'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
                'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y',
                'Z', '[', '\\', ']', '^', '_', '`', 'a', 'b', 'c',
                'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
                'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
                'x', 'y', 'z', '{', '|', '}', '~', '\u007f', '•', '†',
                '‡', '…', '—', '–', 'ƒ', '⁄', '‹', '›', '−', '‰',
                '„', '“', '”', '‘', '’', '‚', '™', 'ﬁ', 'ﬂ', 'Ł',
                'Œ', 'Š', 'Ÿ', 'Ž', 'ı', 'ł', 'œ', 'š', 'ž', '\ufffd',
                '€', '¡', '¢', '£', '¤', '¥', '¦', '§', '\u00a8', '©',
                'ª', '«', '¬', '­', '®', '\u00af', '°', '±', '²', '³',
                '\u00b4', 'µ', '¶', '·', '\u00b8', '¹', 'º', '»', '¼', '½',
                '¾', '¿', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ', 'Ç',
                'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï', 'Ð', 'Ñ',
                'Ò', 'Ó', 'Ô', 'Õ', 'Ö', '×', 'Ø', 'Ù', 'Ú', 'Û',
                'Ü', 'Ý', 'Þ', 'ß', 'à', 'á', 'â', 'ã', 'ä', 'å',
                'æ', 'ç', 'è', 'é', 'ê', 'ë', 'ì', 'í', 'î', 'ï',
                'ð', 'ñ', 'ò', 'ó', 'ô', 'õ', 'ö', '÷', 'ø', 'ù',
                'ú', 'û', 'ü', 'ý', 'þ', 'ÿ'
            };
            winansi = new IntHashtable();
            pdfEncoding = new IntHashtable();
            extraEncodings = new Dictionary<string, IExtraEncoding>();
            for (int i = 128; i < 161; i++)
            {
                char c = winansiByteToChar[i];
                if (c != '\ufffd')
                {
                    winansi[c] = i;
                }
            }

            for (int j = 128; j < 161; j++)
            {
                char c2 = pdfEncodingByteToChar[j];
                if (c2 != '\ufffd')
                {
                    pdfEncoding[c2] = j;
                }
            }

            AddExtraEncoding("Wingdings", new WingdingsConversion());
            AddExtraEncoding("Symbol", new SymbolConversion(symbol: true));
            AddExtraEncoding("ZapfDingbats", new SymbolConversion(symbol: false));
            AddExtraEncoding("SymbolTT", new SymbolTTConversion());
            AddExtraEncoding("Cp437", new Cp437Conversion());
        }

        public static byte[] ConvertToBytes(string text, string encoding)
        {
            if (text == null)
            {
                return new byte[0];
            }

            if (encoding == null || encoding.Length == 0)
            {
                int length = text.Length;
                byte[] array = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    array[i] = (byte)text[i];
                }

                return array;
            }

            extraEncodings.TryGetValue(encoding.ToLower(CultureInfo.InvariantCulture), out var value);
            if (value != null)
            {
                byte[] array2 = value.CharToByte(text, encoding);
                if (array2 != null)
                {
                    return array2;
                }
            }

            IntHashtable intHashtable = null;
            if (encoding.Equals("Cp1252"))
            {
                intHashtable = winansi;
            }
            else if (encoding.Equals("PDF"))
            {
                intHashtable = pdfEncoding;
            }

            if (intHashtable != null)
            {
                char[] array3 = text.ToCharArray();
                int num = array3.Length;
                int num2 = 0;
                byte[] array4 = new byte[num];
                int num3 = 0;
                for (int j = 0; j < num; j++)
                {
                    char c = array3[j];
                    num3 = ((c >= '\u0080' && (c <= '\u00a0' || c > 'ÿ')) ? intHashtable[c] : c);
                    if (num3 != 0)
                    {
                        array4[num2++] = (byte)num3;
                    }
                }

                if (num2 == num)
                {
                    return array4;
                }

                byte[] array5 = new byte[num2];
                Array.Copy(array4, 0, array5, 0, num2);
                return array5;
            }

            Encoding encodingEncoding = IanaEncodings.GetEncodingEncoding(encoding);
            byte[] preamble = encodingEncoding.GetPreamble();
            if (preamble.Length == 0)
            {
                return encodingEncoding.GetBytes(text);
            }

            byte[] bytes = encodingEncoding.GetBytes(text);
            byte[] array6 = new byte[bytes.Length + preamble.Length];
            Array.Copy(preamble, 0, array6, 0, preamble.Length);
            Array.Copy(bytes, 0, array6, preamble.Length, bytes.Length);
            return array6;
        }

        public static byte[] ConvertToBytes(char char1, string encoding)
        {
            if (encoding == null || encoding.Length == 0)
            {
                return new byte[1] { (byte)char1 };
            }

            extraEncodings.TryGetValue(encoding.ToLower(CultureInfo.InvariantCulture), out var value);
            if (value != null)
            {
                byte[] array = value.CharToByte(char1, encoding);
                if (array != null)
                {
                    return array;
                }
            }

            IntHashtable intHashtable = null;
            if (encoding.Equals("Cp1252"))
            {
                intHashtable = winansi;
            }
            else if (encoding.Equals("PDF"))
            {
                intHashtable = pdfEncoding;
            }

            if (intHashtable != null)
            {
                int num = 0;
                num = ((char1 >= '\u0080' && (char1 <= '\u00a0' || char1 > 'ÿ')) ? intHashtable[char1] : char1);
                if (num != 0)
                {
                    return new byte[1] { (byte)num };
                }

                return new byte[0];
            }

            Encoding encodingEncoding = IanaEncodings.GetEncodingEncoding(encoding);
            byte[] preamble = encodingEncoding.GetPreamble();
            char[] chars = new char[1] { char1 };
            if (preamble.Length == 0)
            {
                return encodingEncoding.GetBytes(chars);
            }

            byte[] bytes = encodingEncoding.GetBytes(chars);
            byte[] array2 = new byte[bytes.Length + preamble.Length];
            Array.Copy(preamble, 0, array2, 0, preamble.Length);
            Array.Copy(bytes, 0, array2, preamble.Length, bytes.Length);
            return array2;
        }

        public static string ConvertToString(byte[] bytes, string encoding)
        {
            if (bytes == null)
            {
                return "";
            }

            if (encoding == null || encoding.Length == 0)
            {
                char[] array = new char[bytes.Length];
                for (int i = 0; i < bytes.Length; i++)
                {
                    array[i] = (char)(bytes[i] & 0xFFu);
                }

                return new string(array);
            }

            extraEncodings.TryGetValue(encoding.ToLower(CultureInfo.InvariantCulture), out var value);
            if (value != null)
            {
                string text = value.ByteToChar(bytes, encoding);
                if (text != null)
                {
                    return text;
                }
            }

            char[] array2 = null;
            if (encoding.Equals("Cp1252"))
            {
                array2 = winansiByteToChar;
            }
            else if (encoding.Equals("PDF"))
            {
                array2 = pdfEncodingByteToChar;
            }

            if (array2 != null)
            {
                int num = bytes.Length;
                char[] array3 = new char[num];
                for (int j = 0; j < num; j++)
                {
                    array3[j] = array2[bytes[j] & 0xFF];
                }

                return new string(array3);
            }

            string text2 = encoding.ToUpper(CultureInfo.InvariantCulture);
            Encoding encoding2 = null;
            if (text2.Equals("UNICODEBIGUNMARKED"))
            {
                encoding2 = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);
            }
            else if (text2.Equals("UNICODELITTLEUNMARKED"))
            {
                encoding2 = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
            }

            if (encoding2 != null)
            {
                return encoding2.GetString(bytes);
            }

            bool flag = false;
            bool flag2 = false;
            int num2 = 0;
            if (bytes.Length >= 2)
            {
                if (bytes[0] == 254 && bytes[1] == byte.MaxValue)
                {
                    flag = true;
                    flag2 = true;
                    num2 = 2;
                }
                else if (bytes[0] == byte.MaxValue && bytes[1] == 254)
                {
                    flag = true;
                    flag2 = false;
                    num2 = 2;
                }
            }

            if (text2.Equals("UNICODEBIG"))
            {
                encoding2 = new UnicodeEncoding(!flag || flag2, byteOrderMark: false);
            }
            else if (text2.Equals("UNICODELITTLE"))
            {
                encoding2 = new UnicodeEncoding(flag && flag2, byteOrderMark: false);
            }

            if (encoding2 != null)
            {
                return encoding2.GetString(bytes, num2, bytes.Length - num2);
            }

            return IanaEncodings.GetEncodingEncoding(encoding).GetString(bytes);
        }

        public static bool IsPdfDocEncoding(string text)
        {
            if (text == null)
            {
                return true;
            }

            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                char c = text[i];
                if (c >= '\u0080' && (c <= '\u00a0' || c > 'ÿ') && !pdfEncoding.ContainsKey(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static void AddExtraEncoding(string name, IExtraEncoding enc)
        {
            lock (extraEncodings)
            {
                extraEncodings = new Dictionary<string, IExtraEncoding>(extraEncodings) { [name.ToLower(CultureInfo.InvariantCulture)] = enc };
            }
        }
    }
}
