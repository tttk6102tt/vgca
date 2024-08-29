using Sign.itext.error_messages;
using Sign.itext.xml.simpleparser.handler;
using System.Globalization;
using System.Text;

namespace Sign.itext.xml.simpleparser
{
    public sealed class SimpleXMLParser
    {
        private const int UNKNOWN = 0;

        private const int TEXT = 1;

        private const int TAG_ENCOUNTERED = 2;

        private const int EXAMIN_TAG = 3;

        private const int TAG_EXAMINED = 4;

        private const int IN_CLOSETAG = 5;

        private const int SINGLE_TAG = 6;

        private const int CDATA = 7;

        private const int COMMENT = 8;

        private const int PI = 9;

        private const int ENTITY = 10;

        private const int QUOTE = 11;

        private const int ATTRIBUTE_KEY = 12;

        private const int ATTRIBUTE_EQUAL = 13;

        private const int ATTRIBUTE_VALUE = 14;

        private Stack<int> stack;

        private int character;

        private int previousCharacter = -1;

        private int lines = 1;

        private int columns;

        private bool eol;

        private bool nowhite;

        private int state;

        private bool html;

        private StringBuilder text = new StringBuilder();

        private StringBuilder entity = new StringBuilder();

        private string tag;

        private Dictionary<string, string> attributes;

        private ISimpleXMLDocHandler doc;

        private ISimpleXMLDocHandlerComment comment;

        private int nested;

        private int quoteCharacter = 34;

        private string attributekey;

        private string attributevalue;

        private INewLineHandler newLineHandler;

        private SimpleXMLParser(ISimpleXMLDocHandler doc, ISimpleXMLDocHandlerComment comment, bool html)
        {
            this.doc = doc;
            this.comment = comment;
            this.html = html;
            if (html)
            {
                newLineHandler = new HTMLNewLineHandler();
            }
            else
            {
                newLineHandler = new NeverNewLineHandler();
            }

            stack = new Stack<int>();
            state = (html ? 1 : 0);
        }

        private void Go(TextReader reader)
        {
            doc.StartDocument();
            while (true)
            {
                if (previousCharacter == -1)
                {
                    character = reader.Read();
                }
                else
                {
                    character = previousCharacter;
                    previousCharacter = -1;
                }

                if (character == -1)
                {
                    if (html)
                    {
                        if (html && state == 1)
                        {
                            Flush();
                        }

                        doc.EndDocument();
                    }
                    else
                    {
                        ThrowException(MessageLocalization.GetComposedMessage("missing.end.tag"));
                    }

                    break;
                }

                if (character == 10 && eol)
                {
                    eol = false;
                    continue;
                }

                if (eol)
                {
                    eol = false;
                }
                else if (character == 10)
                {
                    lines++;
                    columns = 0;
                }
                else if (character == 13)
                {
                    eol = true;
                    character = 10;
                    lines++;
                    columns = 0;
                }
                else
                {
                    columns++;
                }

                switch (state)
                {
                    case 0:
                        if (character == 60)
                        {
                            SaveState(1);
                            state = 2;
                        }

                        break;
                    case 1:
                        if (character == 60)
                        {
                            Flush();
                            SaveState(state);
                            state = 2;
                        }
                        else if (character == 38)
                        {
                            SaveState(state);
                            entity.Length = 0;
                            state = 10;
                            nowhite = true;
                        }
                        else if (character == 32)
                        {
                            if (html && nowhite)
                            {
                                this.text.Append(' ');
                                nowhite = false;
                                break;
                            }

                            if (nowhite)
                            {
                                this.text.Append((char)character);
                            }

                            nowhite = false;
                        }
                        else if (char.IsWhiteSpace((char)character))
                        {
                            if (!html)
                            {
                                if (nowhite)
                                {
                                    this.text.Append((char)character);
                                }

                                nowhite = false;
                            }
                        }
                        else
                        {
                            this.text.Append((char)character);
                            nowhite = true;
                        }

                        break;
                    case 2:
                        InitTag();
                        if (character == 47)
                        {
                            state = 5;
                        }
                        else if (character == 63)
                        {
                            RestoreState();
                            state = 9;
                        }
                        else
                        {
                            this.text.Append((char)character);
                            state = 3;
                        }

                        break;
                    case 3:
                        if (character == 62)
                        {
                            DoTag();
                            ProcessTag(start: true);
                            InitTag();
                            state = RestoreState();
                        }
                        else if (character == 47)
                        {
                            state = 6;
                        }
                        else if (character == 45 && this.text.ToString().Equals("!-"))
                        {
                            Flush();
                            state = 8;
                        }
                        else if (character == 91 && this.text.ToString().Equals("![CDATA"))
                        {
                            Flush();
                            state = 7;
                        }
                        else if (character == 69 && this.text.ToString().Equals("!DOCTYP"))
                        {
                            Flush();
                            state = 9;
                        }
                        else if (char.IsWhiteSpace((char)character))
                        {
                            DoTag();
                            state = 4;
                        }
                        else
                        {
                            this.text.Append((char)character);
                        }

                        break;
                    case 4:
                        if (character == 62)
                        {
                            ProcessTag(start: true);
                            InitTag();
                            state = RestoreState();
                        }
                        else if (character == 47)
                        {
                            state = 6;
                        }
                        else if (!char.IsWhiteSpace((char)character))
                        {
                            this.text.Append((char)character);
                            state = 12;
                        }

                        break;
                    case 5:
                        if (character == 62)
                        {
                            DoTag();
                            ProcessTag(start: false);
                            if (!html && nested == 0)
                            {
                                return;
                            }

                            state = RestoreState();
                        }
                        else if (!char.IsWhiteSpace((char)character))
                        {
                            this.text.Append((char)character);
                        }

                        break;
                    case 6:
                        if (character != 62)
                        {
                            ThrowException(MessageLocalization.GetComposedMessage("expected.gt.for.tag.lt.1.gt", tag));
                        }

                        DoTag();
                        ProcessTag(start: true);
                        ProcessTag(start: false);
                        InitTag();
                        if (!html && nested == 0)
                        {
                            doc.EndDocument();
                            return;
                        }

                        state = RestoreState();
                        break;
                    case 7:
                        if (character == 62 && this.text.ToString().EndsWith("]]"))
                        {
                            this.text.Length = this.text.Length - 2;
                            Flush();
                            state = RestoreState();
                        }
                        else
                        {
                            this.text.Append((char)character);
                        }

                        break;
                    case 8:
                        if (character == 62 && this.text.ToString().EndsWith("--"))
                        {
                            this.text.Length = this.text.Length - 2;
                            Flush();
                            state = RestoreState();
                        }
                        else
                        {
                            this.text.Append((char)character);
                        }

                        break;
                    case 9:
                        if (character == 62)
                        {
                            state = RestoreState();
                            if (state == 1)
                            {
                                state = 0;
                            }
                        }

                        break;
                    case 10:
                        if (character == 59)
                        {
                            state = RestoreState();
                            string text = entity.ToString();
                            entity.Length = 0;
                            char c = EntitiesToUnicode.DecodeEntity(text);
                            if (c == '\0')
                            {
                                this.text.Append('&').Append(text).Append(';');
                            }
                            else
                            {
                                this.text.Append(c);
                            }
                        }
                        else if ((character != 35 && (character < 48 || character > 57) && (character < 97 || character > 122) && (character < 65 || character > 90)) || entity.Length >= 7)
                        {
                            state = RestoreState();
                            previousCharacter = character;
                            this.text.Append('&').Append(entity.ToString());
                            entity.Length = 0;
                        }
                        else
                        {
                            entity.Append((char)character);
                        }

                        break;
                    case 11:
                        if (html && quoteCharacter == 32 && character == 62)
                        {
                            Flush();
                            ProcessTag(start: true);
                            InitTag();
                            state = RestoreState();
                        }
                        else if (html && quoteCharacter == 32 && char.IsWhiteSpace((char)character))
                        {
                            Flush();
                            state = 4;
                        }
                        else if (html && quoteCharacter == 32)
                        {
                            this.text.Append((char)character);
                        }
                        else if (character == quoteCharacter)
                        {
                            Flush();
                            state = 4;
                        }
                        else if (" \r\n\t".IndexOf((char)character) >= 0)
                        {
                            this.text.Append(' ');
                        }
                        else if (character == 38)
                        {
                            SaveState(state);
                            state = 10;
                            entity.Length = 0;
                        }
                        else
                        {
                            this.text.Append((char)character);
                        }

                        break;
                    case 12:
                        if (char.IsWhiteSpace((char)character))
                        {
                            Flush();
                            state = 13;
                        }
                        else if (character == 61)
                        {
                            Flush();
                            state = 14;
                        }
                        else if (html && character == 62)
                        {
                            this.text.Length = 0;
                            ProcessTag(start: true);
                            InitTag();
                            state = RestoreState();
                        }
                        else
                        {
                            this.text.Append((char)character);
                        }

                        break;
                    case 13:
                        if (character == 61)
                        {
                            state = 14;
                        }
                        else if (!char.IsWhiteSpace((char)character))
                        {
                            if (html && character == 62)
                            {
                                this.text.Length = 0;
                                ProcessTag(start: true);
                                InitTag();
                                state = RestoreState();
                            }
                            else if (html && character == 47)
                            {
                                Flush();
                                state = 6;
                            }
                            else if (html)
                            {
                                Flush();
                                this.text.Append((char)character);
                                state = 12;
                            }
                            else
                            {
                                ThrowException(MessageLocalization.GetComposedMessage("error.in.attribute.processing"));
                            }
                        }

                        break;
                    case 14:
                        if (character == 34 || character == 39)
                        {
                            quoteCharacter = character;
                            state = 11;
                        }
                        else if (!char.IsWhiteSpace((char)character))
                        {
                            if (html && character == 62)
                            {
                                Flush();
                                ProcessTag(start: true);
                                InitTag();
                                state = RestoreState();
                            }
                            else if (html)
                            {
                                this.text.Append((char)character);
                                quoteCharacter = 32;
                                state = 11;
                            }
                            else
                            {
                                ThrowException(MessageLocalization.GetComposedMessage("error.in.attribute.processing"));
                            }
                        }

                        break;
                }
            }
        }

        private int RestoreState()
        {
            if (stack.Count != 0)
            {
                return stack.Pop();
            }

            return 0;
        }

        private void SaveState(int s)
        {
            stack.Push(s);
        }

        private void Flush()
        {
            switch (state)
            {
                case 1:
                case 7:
                    if (text.Length > 0)
                    {
                        doc.Text(text.ToString());
                    }

                    break;
                case 8:
                    if (comment != null)
                    {
                        comment.Comment(text.ToString());
                    }

                    break;
                case 12:
                    attributekey = text.ToString();
                    if (html)
                    {
                        attributekey = attributekey.ToLower(CultureInfo.InvariantCulture);
                    }

                    break;
                case 11:
                case 14:
                    attributevalue = text.ToString();
                    attributes[attributekey] = attributevalue;
                    break;
            }

            text.Length = 0;
        }

        private void InitTag()
        {
            tag = null;
            attributes = new Dictionary<string, string>();
        }

        private void DoTag()
        {
            if (tag == null)
            {
                tag = text.ToString();
            }

            if (html)
            {
                tag = tag.ToLower(CultureInfo.InvariantCulture);
            }

            text.Length = 0;
        }

        private void ProcessTag(bool start)
        {
            if (start)
            {
                nested++;
                doc.StartElement(tag, attributes);
                return;
            }

            if (newLineHandler.IsNewLineTag(tag))
            {
                nowhite = false;
            }

            nested--;
            doc.EndElement(tag);
        }

        private void ThrowException(string s)
        {
            throw new IOException(MessageLocalization.GetComposedMessage("1.near.line.2.column.3", s, lines, columns));
        }

        public static void Parse(ISimpleXMLDocHandler doc, ISimpleXMLDocHandlerComment comment, TextReader r, bool html)
        {
            new SimpleXMLParser(doc, comment, html).Go(r);
        }

        public static void Parse(ISimpleXMLDocHandler doc, Stream inp)
        {
            byte[] array = new byte[4];
            if (inp.Read(array, 0, array.Length) != 4)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("insufficient.length"));
            }

            string text = XMLUtil.GetEncodingName(array);
            string text2 = null;
            if (text.Equals("UTF-8"))
            {
                StringBuilder stringBuilder = new StringBuilder();
                int num;
                while ((num = inp.ReadByte()) != -1 && num != 62)
                {
                    stringBuilder.Append((char)num);
                }

                text2 = stringBuilder.ToString();
            }
            else if (text.Equals("CP037"))
            {
                MemoryStream memoryStream = new MemoryStream();
                int num2;
                while ((num2 = inp.ReadByte()) != -1 && num2 != 110)
                {
                    memoryStream.WriteByte((byte)num2);
                }

                text2 = Encoding.GetEncoding(37).GetString(memoryStream.ToArray());
            }

            if (text2 != null)
            {
                text2 = GetDeclaredEncoding(text2);
                if (text2 != null)
                {
                    text = text2;
                }
            }

            Parse(doc, new StreamReader(inp, IanaEncodings.GetEncodingEncoding(text)));
        }

        private static string GetDeclaredEncoding(string decl)
        {
            if (decl == null)
            {
                return null;
            }

            int num = decl.IndexOf("encoding");
            if (num < 0)
            {
                return null;
            }

            int num2 = decl.IndexOf('"', num);
            int num3 = decl.IndexOf('\'', num);
            if (num2 == num3)
            {
                return null;
            }

            if ((num2 < 0 && num3 > 0) || (num3 > 0 && num3 < num2))
            {
                int num4 = decl.IndexOf('\'', num3 + 1);
                if (num4 < 0)
                {
                    return null;
                }

                return decl.Substring(num3 + 1, num4 - (num3 + 1));
            }

            if ((num3 < 0 && num2 > 0) || (num2 > 0 && num2 < num3))
            {
                int num5 = decl.IndexOf('"', num2 + 1);
                if (num5 < 0)
                {
                    return null;
                }

                return decl.Substring(num2 + 1, num5 - (num2 + 1));
            }

            return null;
        }

        public static void Parse(ISimpleXMLDocHandler doc, TextReader r)
        {
            Parse(doc, null, r, html: false);
        }

        public static string EscapeXML(string s, bool onlyASCII)
        {
            return XMLUtil.EscapeXML(s, onlyASCII);
        }
    }
}
