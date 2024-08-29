using Sign.SystemItext.util;
using System.Globalization;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class SequenceList
    {
        protected const int COMMA = 1;

        protected const int MINUS = 2;

        protected const int NOT = 3;

        protected const int TEXT = 4;

        protected const int NUMBER = 5;

        protected const int END = 6;

        protected const char EOT = '\uffff';

        private const int FIRST = 0;

        private const int DIGIT = 1;

        private const int OTHER = 2;

        private const int DIGIT2 = 3;

        private const string NOT_OTHER = "-,!0123456789";

        protected char[] text;

        protected int ptr;

        protected int number;

        protected string other;

        protected int low;

        protected int high;

        protected bool odd;

        protected bool even;

        protected bool inverse;

        protected virtual int Type
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                int num = 0;
                while (true)
                {
                    char c = NextChar();
                    if (c == '\uffff')
                    {
                        break;
                    }

                    switch (num)
                    {
                        case 0:
                            switch (c)
                            {
                                case '!':
                                    return 3;
                                case '-':
                                    return 2;
                                case ',':
                                    return 1;
                                default:
                                    stringBuilder.Append(c);
                                    num = ((c >= '0' && c <= '9') ? 1 : 2);
                                    break;
                            }

                            break;
                        case 1:
                            if (c >= '0' && c <= '9')
                            {
                                stringBuilder.Append(c);
                                break;
                            }

                            PutBack();
                            number = int.Parse(other = stringBuilder.ToString());
                            return 5;
                        case 2:
                            if ("-,!0123456789".IndexOf(c) < 0)
                            {
                                stringBuilder.Append(c);
                                break;
                            }

                            PutBack();
                            other = stringBuilder.ToString().ToLower(CultureInfo.InvariantCulture);
                            return 4;
                    }
                }

                switch (num)
                {
                    case 1:
                        number = int.Parse(other = stringBuilder.ToString());
                        return 5;
                    case 2:
                        other = stringBuilder.ToString().ToLower(CultureInfo.InvariantCulture);
                        return 4;
                    default:
                        return 6;
                }
            }
        }

        protected SequenceList(string range)
        {
            ptr = 0;
            text = range.ToCharArray();
        }

        protected virtual char NextChar()
        {
            char c;
            do
            {
                if (ptr >= text.Length)
                {
                    return '\uffff';
                }

                c = text[ptr++];
            }
            while (c <= ' ');
            return c;
        }

        protected virtual void PutBack()
        {
            ptr--;
            if (ptr < 0)
            {
                ptr = 0;
            }
        }

        private void OtherProc()
        {
            if (other.Equals("odd") || other.Equals("o"))
            {
                odd = true;
                even = false;
            }
            else if (other.Equals("even") || other.Equals("e"))
            {
                odd = false;
                even = true;
            }
        }

        protected virtual bool GetAttributes()
        {
            low = -1;
            high = -1;
            odd = (even = (inverse = false));
            int num = 2;
            int type;
            while (true)
            {
                type = Type;
                if (type == 6 || type == 1)
                {
                    break;
                }

                switch (num)
                {
                    case 2:
                        switch (type)
                        {
                            case 3:
                                inverse = true;
                                break;
                            case 2:
                                num = 3;
                                break;
                            case 5:
                                low = number;
                                num = 1;
                                break;
                            default:
                                OtherProc();
                                break;
                        }

                        break;
                    case 1:
                        switch (type)
                        {
                            case 3:
                                inverse = true;
                                num = 2;
                                high = low;
                                break;
                            case 2:
                                num = 3;
                                break;
                            default:
                                high = low;
                                num = 2;
                                OtherProc();
                                break;
                        }

                        break;
                    case 3:
                        switch (type)
                        {
                            case 2:
                                break;
                            case 3:
                                inverse = true;
                                num = 2;
                                break;
                            case 5:
                                high = number;
                                num = 2;
                                break;
                            default:
                                num = 2;
                                OtherProc();
                                break;
                        }

                        break;
                }
            }

            if (num == 1)
            {
                high = low;
            }

            return type == 6;
        }

        public static ICollection<int> Expand(string ranges, int maxNumber)
        {
            SequenceList sequenceList = new SequenceList(ranges);
            List<int> list = new List<int>();
            bool flag = false;
            while (!flag)
            {
                flag = sequenceList.GetAttributes();
                if (sequenceList.low == -1 && sequenceList.high == -1 && !sequenceList.even && !sequenceList.odd)
                {
                    continue;
                }

                if (sequenceList.low < 1)
                {
                    sequenceList.low = 1;
                }

                if (sequenceList.high < 1 || sequenceList.high > maxNumber)
                {
                    sequenceList.high = maxNumber;
                }

                if (sequenceList.low > maxNumber)
                {
                    sequenceList.low = maxNumber;
                }

                int num = 1;
                if (sequenceList.inverse)
                {
                    if (sequenceList.low > sequenceList.high)
                    {
                        int num2 = sequenceList.low;
                        sequenceList.low = sequenceList.high;
                        sequenceList.high = num2;
                    }

                    ListIterator<int> listIterator = new ListIterator<int>(list);
                    while (listIterator.HasNext())
                    {
                        int num3 = listIterator.Next();
                        if ((!sequenceList.even || (num3 & 1) != 1) && (!sequenceList.odd || ((uint)num3 & (true ? 1u : 0u)) != 0) && num3 >= sequenceList.low && num3 <= sequenceList.high)
                        {
                            listIterator.Remove();
                        }
                    }

                    continue;
                }

                if (sequenceList.low > sequenceList.high)
                {
                    num = -1;
                    if (sequenceList.odd || sequenceList.even)
                    {
                        num--;
                        if (sequenceList.even)
                        {
                            sequenceList.low &= -2;
                        }
                        else
                        {
                            sequenceList.low -= (((sequenceList.low & 1) != 1) ? 1 : 0);
                        }
                    }

                    for (int i = sequenceList.low; i >= sequenceList.high; i += num)
                    {
                        list.Add(i);
                    }

                    continue;
                }

                if (sequenceList.odd || sequenceList.even)
                {
                    num++;
                    if (sequenceList.odd)
                    {
                        sequenceList.low |= 1;
                    }
                    else
                    {
                        sequenceList.low += (((sequenceList.low & 1) == 1) ? 1 : 0);
                    }
                }

                for (int j = sequenceList.low; j <= sequenceList.high; j += num)
                {
                    list.Add(j);
                }
            }

            return list;
        }
    }
}
