using System.Text;

namespace Sign.SystemItext.util
{
    public class Properties
    {
        private Dictionary<string, string> _col;

        private const string whiteSpaceChars = " \t\r\n\f";

        private const string keyValueSeparators = "=: \t\r\n\f";

        private const string strictKeyValueSeparators = "=:";

        public virtual int Count => _col.Count;

        public virtual string this[string key]
        {
            get
            {
                _col.TryGetValue(key, out var value);
                return value;
            }
            set
            {
                _col[key] = value;
            }
        }

        public virtual Dictionary<string, string>.KeyCollection Keys => _col.Keys;

        public Properties()
        {
            _col = new Dictionary<string, string>();
        }

        public virtual string Remove(string key)
        {
            _col.TryGetValue(key, out var value);
            _col.Remove(key);
            return value;
        }

        public virtual Dictionary<string, string>.Enumerator GetEnumerator()
        {
            return _col.GetEnumerator();
        }

        public virtual bool ContainsKey(string key)
        {
            return _col.ContainsKey(key);
        }

        public virtual void Add(string key, string value)
        {
            _col[key] = value;
        }

        public virtual void AddAll(Properties col)
        {
            foreach (string key in col.Keys)
            {
                _col[key] = col[key];
            }
        }

        public virtual void Clear()
        {
            _col.Clear();
        }

        public virtual void Load(Stream inStream)
        {
            StreamReader streamReader = new StreamReader(inStream, Encoding.GetEncoding(1252));
            while (true)
            {
                string text = streamReader.ReadLine();
                if (text == null)
                {
                    break;
                }

                if (text.Length <= 0)
                {
                    continue;
                }

                int length = text.Length;
                int i;
                for (i = 0; i < length && " \t\r\n\f".IndexOf(text[i]) != -1; i++)
                {
                }

                if (i == length)
                {
                    continue;
                }

                char c = text[i];
                if (c == '#' || c == '!')
                {
                    continue;
                }

                while (ContinueLine(text))
                {
                    string text2 = streamReader.ReadLine();
                    if (text2 == null)
                    {
                        text2 = "";
                    }

                    string text3 = text.Substring(0, length - 1);
                    int j;
                    for (j = 0; j < text2.Length && " \t\r\n\f".IndexOf(text2[j]) != -1; j++)
                    {
                    }

                    text2 = text2.Substring(j, text2.Length - j);
                    text = text3 + text2;
                    length = text.Length;
                }

                int k;
                for (k = i; k < length; k++)
                {
                    char c2 = text[k];
                    if (c2 == '\\')
                    {
                        k++;
                    }
                    else if ("=: \t\r\n\f".IndexOf(c2) != -1)
                    {
                        break;
                    }
                }

                int l;
                for (l = k; l < length && " \t\r\n\f".IndexOf(text[l]) != -1; l++)
                {
                }

                if (l < length && "=:".IndexOf(text[l]) != -1)
                {
                    l++;
                }

                for (; l < length && " \t\r\n\f".IndexOf(text[l]) != -1; l++)
                {
                }

                string theString = text.Substring(i, k - i);
                string theString2 = ((k < length) ? text.Substring(l, length - l) : "");
                theString = LoadConvert(theString);
                theString2 = LoadConvert(theString2);
                Add(theString, theString2);
            }
        }

        private string LoadConvert(string theString)
        {
            int length = theString.Length;
            StringBuilder stringBuilder = new StringBuilder(length);
            int num = 0;
            while (num < length)
            {
                char c = theString[num++];
                if (c == '\\')
                {
                    c = theString[num++];
                    switch (c)
                    {
                        case 'u':
                            {
                                int num2 = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    c = theString[num++];
                                    switch (c)
                                    {
                                        case '0':
                                        case '1':
                                        case '2':
                                        case '3':
                                        case '4':
                                        case '5':
                                        case '6':
                                        case '7':
                                        case '8':
                                        case '9':
                                            num2 = (num2 << 4) + c - 48;
                                            break;
                                        case 'a':
                                        case 'b':
                                        case 'c':
                                        case 'd':
                                        case 'e':
                                        case 'f':
                                            num2 = (num2 << 4) + 10 + c - 97;
                                            break;
                                        case 'A':
                                        case 'B':
                                        case 'C':
                                        case 'D':
                                        case 'E':
                                        case 'F':
                                            num2 = (num2 << 4) + 10 + c - 65;
                                            break;
                                        default:
                                            throw new ArgumentException("Malformed \\uxxxx encoding.");
                                    }
                                }

                                stringBuilder.Append((char)num2);
                                continue;
                            }
                        case 't':
                            c = '\t';
                            break;
                        case 'r':
                            c = '\r';
                            break;
                        case 'n':
                            c = '\n';
                            break;
                        case 'f':
                            c = '\f';
                            break;
                    }

                    stringBuilder.Append(c);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        private bool ContinueLine(string line)
        {
            int num = 0;
            int num2 = line.Length - 1;
            while (num2 >= 0 && line[num2--] == '\\')
            {
                num++;
            }

            return num % 2 == 1;
        }
    }
}
