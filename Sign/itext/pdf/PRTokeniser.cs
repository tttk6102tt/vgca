using Sign.itext.error_messages;
using Sign.itext.io;
using Sign.itext.text.exceptions;
using System.Text;

namespace Sign.itext.pdf
{
    public class PRTokeniser
    {
        public enum TokType
        {
            NUMBER = 1,
            STRING,
            NAME,
            COMMENT,
            START_ARRAY,
            END_ARRAY,
            START_DIC,
            END_DIC,
            REF,
            OTHER,
            ENDOFFILE
        }

        internal const string EMPTY = "";

        private readonly RandomAccessFileOrArray file;

        protected TokType type;

        protected string stringValue;

        protected int reference;

        protected int generation;

        protected bool hexString;

        public virtual long FilePointer => file.FilePointer;

        public virtual long Length => file.Length;

        public virtual RandomAccessFileOrArray SafeFile => new RandomAccessFileOrArray(file);

        public virtual RandomAccessFileOrArray File => file;

        public virtual TokType TokenType => type;

        public virtual string StringValue => stringValue;

        public virtual int Reference => reference;

        public virtual int Generation => generation;

        public virtual long LongValue => long.Parse(stringValue);

        public virtual int IntValue => int.Parse(stringValue);

        public PRTokeniser(RandomAccessFileOrArray file)
        {
            this.file = file;
        }

        public virtual void Seek(long pos)
        {
            file.Seek(pos);
        }

        public virtual void Close()
        {
            file.Close();
        }

        public virtual int Read()
        {
            return file.Read();
        }

        public virtual string ReadString(int size)
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (size-- > 0)
            {
                int num = Read();
                if (num == -1)
                {
                    break;
                }

                stringBuilder.Append((char)num);
            }

            return stringBuilder.ToString();
        }

        public static bool IsWhitespace(int ch)
        {
            return IsWhitespace(ch, isWhitespace: true);
        }

        public static bool IsWhitespace(int ch, bool isWhitespace)
        {
            if ((!isWhitespace || ch != 0) && ch != 9 && ch != 10 && ch != 12 && ch != 13)
            {
                return ch == 32;
            }

            return true;
        }

        public static bool IsDelimiter(int ch)
        {
            if (ch != 40 && ch != 41 && ch != 60 && ch != 62 && ch != 91 && ch != 93 && ch != 47)
            {
                return ch == 37;
            }

            return true;
        }

        public virtual void BackOnePosition(int ch)
        {
            if (ch != -1)
            {
                file.PushBack((byte)ch);
            }
        }

        public virtual void ThrowError(string error)
        {
            throw new InvalidPdfException(MessageLocalization.GetComposedMessage("1.at.file.pointer.2", error, file.FilePointer));
        }

        public virtual int GetHeaderOffset()
        {
            string text = ReadString(1024);
            int num = text.IndexOf("%PDF-");
            if (num < 0)
            {
                num = text.IndexOf("%FDF-");
                if (num < 0)
                {
                    throw new InvalidPdfException(MessageLocalization.GetComposedMessage("pdf.header.not.found"));
                }
            }

            return num;
        }

        public virtual char CheckPdfHeader()
        {
            file.Seek(0);
            string text = ReadString(1024);
            if (text.IndexOf("%PDF-") != 0)
            {
                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("pdf.header.not.found"));
            }

            return text[7];
        }

        public virtual void CheckFdfHeader()
        {
            file.Seek(0);
            if (ReadString(1024).IndexOf("%FDF-") != 0)
            {
                throw new InvalidPdfException(MessageLocalization.GetComposedMessage("fdf.header.not.found"));
            }
        }

        public virtual long GetStartxref()
        {
            int num = 1024;
            long num2 = file.Length - num;
            if (num2 < 1)
            {
                num2 = 1L;
            }

            while (num2 > 0)
            {
                file.Seek(num2);
                int num3 = ReadString(num).LastIndexOf("startxref");
                if (num3 >= 0)
                {
                    return num2 + num3;
                }

                num2 = num2 - num + 9;
            }

            throw new InvalidPdfException(MessageLocalization.GetComposedMessage("pdf.startxref.not.found"));
        }

        public static int GetHex(int v)
        {
            if (v >= 48 && v <= 57)
            {
                return v - 48;
            }

            if (v >= 65 && v <= 70)
            {
                return v - 65 + 10;
            }

            if (v >= 97 && v <= 102)
            {
                return v - 97 + 10;
            }

            return -1;
        }

        public virtual void NextValidToken()
        {
            int num = 0;
            string s = null;
            string s2 = null;
            long pos = 0L;
            while (NextToken())
            {
                if (type == TokType.COMMENT)
                {
                    continue;
                }

                switch (num)
                {
                    case 0:
                        if (type != TokType.NUMBER)
                        {
                            return;
                        }

                        pos = file.FilePointer;
                        s = stringValue;
                        num++;
                        break;
                    case 1:
                        if (type != TokType.NUMBER)
                        {
                            file.Seek(pos);
                            type = TokType.NUMBER;
                            stringValue = s;
                            return;
                        }

                        s2 = stringValue;
                        num++;
                        break;
                    default:
                        if (type != TokType.OTHER || !stringValue.Equals("R"))
                        {
                            file.Seek(pos);
                            type = TokType.NUMBER;
                            stringValue = s;
                        }
                        else
                        {
                            type = TokType.REF;
                            reference = int.Parse(s);
                            generation = int.Parse(s2);
                        }

                        return;
                }
            }

            if (num == 1)
            {
                type = TokType.NUMBER;
            }
        }

        public virtual bool NextToken()
        {
            int num = 0;
            do
            {
                num = file.Read();
            }
            while (num != -1 && IsWhitespace(num));
            if (num == -1)
            {
                type = TokType.ENDOFFILE;
                return false;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringValue = "";
            /*
            switch (num)
            {
                case 91:
                    type = TokType.START_ARRAY;
                    break;
                case 93:
                    type = TokType.END_ARRAY;
                    break;
                case 47:
                    stringBuilder.Length = 0;
                    type = TokType.NAME;
                    while (true)
                    {
                        num = file.Read();
                        if (num == -1 || IsDelimiter(num) || IsWhitespace(num))
                        {
                            break;
                        }

                        if (num == 35)
                        {
                            num = (GetHex(file.Read()) << 4) + GetHex(file.Read());
                        }

                        stringBuilder.Append((char)num);
                    }

                    BackOnePosition(num);
                    break;
                case 62:
                    num = file.Read();
                    if (num != 62)
                    {
                        ThrowError(MessageLocalization.GetComposedMessage("greaterthan.not.expected"));
                    }

                    type = TokType.END_DIC;
                    break;
                case 60:
                    {
                        int num2 = file.Read();
                        if (num2 == 60)
                        {
                            type = TokType.START_DIC;
                            break;
                        }

                        stringBuilder.Length = 0;
                        type = TokType.STRING;
                        hexString = true;
                        int num3 = 0;
                        while (true)
                        {
                            if (IsWhitespace(num2))
                            {
                                num2 = file.Read();
                                continue;
                            }

                            if (num2 == 62)
                            {
                                break;
                            }

                            num2 = GetHex(num2);
                            if (num2 < 0)
                            {
                                break;
                            }

                            num3 = file.Read();
                            while (IsWhitespace(num3))
                            {
                                num3 = file.Read();
                            }

                            if (num3 == 62)
                            {
                                num = num2 << 4;
                                stringBuilder.Append((char)num);
                                break;
                            }

                            num3 = GetHex(num3);
                            if (num3 < 0)
                            {
                                break;
                            }

                            num = (num2 << 4) + num3;
                            stringBuilder.Append((char)num);
                            num2 = file.Read();
                        }

                        if (num2 < 0 || num3 < 0)
                        {
                            ThrowError(MessageLocalization.GetComposedMessage("error.reading.string"));
                        }

                        break;
                    }
                case 37:
                    type = TokType.COMMENT;
                    do
                    {
                        num = file.Read();
                    }
                    while (num != -1 && num != 13 && num != 10);
                    break;
                case 40:
                    {
                        stringBuilder.Length = 0;
                        type = TokType.STRING;
                        hexString = false;
                        int num4 = 0;
                        while (true)
                        {
                            num = file.Read();
                            if (num == -1)
                            {
                                break;
                            }

                            if (num == 40)
                            {
                                num4++;
                            }
                            else if (num == 41)
                            {
                                num4--;
                            }
                            else if (num == 92)
                            {
                                bool flag2 = false;
                                num = file.Read();
                                switch (num)
                                {
                                    case 110:
                                        num = 10;
                                        break;
                                    case 114:
                                        num = 13;
                                        break;
                                    case 116:
                                        num = 9;
                                        break;
                                    case 98:
                                        num = 8;
                                        break;
                                    case 102:
                                        num = 12;
                                        break;
                                    case 13:
                                        flag2 = true;
                                        num = file.Read();
                                        if (num != 10)
                                        {
                                            BackOnePosition(num);
                                        }

                                        break;
                                    case 10:
                                        flag2 = true;
                                        break;
                                    default:
                                        {
                                            if (num < 48 || num > 55)
                                            {
                                                break;
                                            }

                                            int num5 = num - 48;
                                            num = file.Read();
                                            if (num < 48 || num > 55)
                                            {
                                                BackOnePosition(num);
                                                num = num5;
                                                break;
                                            }

                                            num5 = (num5 << 3) + num - 48;
                                            num = file.Read();
                                            if (num < 48 || num > 55)
                                            {
                                                BackOnePosition(num);
                                                num = num5;
                                            }
                                            else
                                            {
                                                num5 = (num5 << 3) + num - 48;
                                                num = num5 & 0xFF;
                                            }

                                            break;
                                        }
                                    case 40:
                                    case 41:
                                    case 92:
                                        break;
                                }

                                if (flag2)
                                {
                                    continue;
                                }

                                if (num < 0)
                                {
                                    break;
                                }
                            }
                            else if (num == 13)
                            {
                                num = file.Read();
                                if (num < 0)
                                {
                                    break;
                                }

                                if (num != 10)
                                {
                                    BackOnePosition(num);
                                    num = 10;
                                }
                            }

                            if (num4 == -1)
                            {
                                break;
                            }

                            stringBuilder.Append((char)num);
                        }

                        if (num == -1)
                        {
                            ThrowError(MessageLocalization.GetComposedMessage("error.reading.string"));
                        }

                        break;
                    }
                default:
                    stringBuilder.Length = 0;
                    switch (num)
                    {
                        case 43:
                        case 45:
                        case 46:
                        case 48:
                        case 49:
                        case 50:
                        case 51:
                        case 52:
                        case 53:
                        case 54:
                        case 55:
                        case 56:
                        case 57:
                            type = TokType.NUMBER;
                            if (num == 45)
                            {
                                bool flag = false;
                                do
                                {
                                    flag = !flag;
                                    num = file.Read();
                                }
                                while (num == 45);
                                if (flag)
                                {
                                    stringBuilder.Append('-');
                                }
                            }
                            else
                            {
                                stringBuilder.Append((char)num);
                                num = file.Read();
                            }

                            while (true)
                            {
                                if (num >= 48 && num <= 57) // Check for numbers 0-9
                                {
                                    stringBuilder.Append((char)num);
                                    num = file.Read();
                                    continue; // Continue to the next iteration of the loop
                                }
                                else if (num == 46) // Check for decimal point
                                {
                                    stringBuilder.Append((char)num);
                                    num = file.Read();
                                    continue; // Continue to the next iteration of the loop
                                }
                                else if (num == -1) // Check for end of file
                                {
                                    break; // Exit the loop
                                }
                                else
                                {
                                    num = file.Read(); // Read the next character
                                }
                            }

                            break;
                        default:
                            type = TokType.OTHER;
                            do
                            {
                                stringBuilder.Append((char)num);
                                num = file.Read();
                            }
                            while (num != -1 && !IsDelimiter(num) && !IsWhitespace(num));
                            break;
                    }

                    if (num != -1)
                    {
                        BackOnePosition(num);
                    }

                    break;
            }
            */
            switch (num)
            {
                case 91:
                    type = TokType.START_ARRAY;
                    break;
                case 93:
                    type = TokType.END_ARRAY;
                    break;
                case 47:
                    stringBuilder.Length = 0;
                    type = TokType.NAME;
                    while (true)
                    {
                        num = file.Read();
                        if (num == -1 || IsDelimiter(num) || IsWhitespace(num))
                        {
                            break;
                        }

                        if (num == 35)
                        {
                            num = (GetHex(file.Read()) << 4) + GetHex(file.Read());
                        }

                        stringBuilder.Append((char)num);
                    }

                    BackOnePosition(num);
                    break;
                case 62:
                    num = file.Read();
                    if (num != 62)
                    {
                        ThrowError(MessageLocalization.GetComposedMessage("greaterthan.not.expected"));
                    }

                    type = TokType.END_DIC;
                    break;
                case 60:
                    {
                        int num2 = file.Read();
                        if (num2 == 60)
                        {
                            type = TokType.START_DIC;
                            break;
                        }

                        stringBuilder.Length = 0;
                        type = TokType.STRING;
                        hexString = true;
                        int num3 = 0;
                        while (true)
                        {
                            if (IsWhitespace(num2))
                            {
                                num2 = file.Read();
                                continue;
                            }

                            if (num2 == 62)
                            {
                                break;
                            }

                            num2 = GetHex(num2);
                            if (num2 < 0)
                            {
                                break;
                            }

                            num3 = file.Read();
                            while (IsWhitespace(num3))
                            {
                                num3 = file.Read();
                            }

                            if (num3 == 62)
                            {
                                num = num2 << 4;
                                stringBuilder.Append((char)num);
                                break;
                            }

                            num3 = GetHex(num3);
                            if (num3 < 0)
                            {
                                break;
                            }

                            num = (num2 << 4) + num3;
                            stringBuilder.Append((char)num);
                            num2 = file.Read();
                        }

                        if (num2 < 0 || num3 < 0)
                        {
                            ThrowError(MessageLocalization.GetComposedMessage("error.reading.string"));
                        }

                        break;
                    }
                case 37:
                    type = TokType.COMMENT;
                    do
                    {
                        num = file.Read();
                    }
                    while (num != -1 && num != 13 && num != 10);
                    break;
                case 40:
                    {
                        stringBuilder.Length = 0;
                        type = TokType.STRING;
                        hexString = false;
                        int num4 = 0;
                        while (true)
                        {
                            num = file.Read();
                            if (num == -1)
                            {
                                break;
                            }

                            if (num == 40)
                            {
                                num4++;
                            }
                            else if (num == 41)
                            {
                                num4--;
                            }
                            else if (num == 92)
                            {
                                bool flag2 = false;
                                num = file.Read();
                                switch (num)
                                {
                                    case 110:
                                        num = 10;
                                        break;
                                    case 114:
                                        num = 13;
                                        break;
                                    case 116:
                                        num = 9;
                                        break;
                                    case 98:
                                        num = 8;
                                        break;
                                    case 102:
                                        num = 12;
                                        break;
                                    case 13:
                                        flag2 = true;
                                        num = file.Read();
                                        if (num != 10)
                                        {
                                            BackOnePosition(num);
                                        }

                                        break;
                                    case 10:
                                        flag2 = true;
                                        break;
                                    default:
                                        {
                                            if (num < 48 || num > 55)
                                            {
                                                break;
                                            }

                                            int num5 = num - 48;
                                            num = file.Read();
                                            if (num < 48 || num > 55)
                                            {
                                                BackOnePosition(num);
                                                num = num5;
                                                break;
                                            }

                                            num5 = (num5 << 3) + num - 48;
                                            num = file.Read();
                                            if (num < 48 || num > 55)
                                            {
                                                BackOnePosition(num);
                                                num = num5;
                                            }
                                            else
                                            {
                                                num5 = (num5 << 3) + num - 48;
                                                num = num5 & 0xFF;
                                            }

                                            break;
                                        }
                                    case 40:
                                    case 41:
                                    case 92:
                                        break;
                                }

                                if (flag2)
                                {
                                    continue;
                                }

                                if (num < 0)
                                {
                                    break;
                                }
                            }
                            else if (num == 13)
                            {
                                num = file.Read();
                                if (num < 0)
                                {
                                    break;
                                }

                                if (num != 10)
                                {
                                    BackOnePosition(num);
                                    num = 10;
                                }
                            }

                            if (num4 == -1)
                            {
                                break;
                            }

                            stringBuilder.Append((char)num);
                        }

                        if (num == -1)
                        {
                            ThrowError(MessageLocalization.GetComposedMessage("error.reading.string"));
                        }

                        break;
                    }
                default:
                    stringBuilder.Length = 0;
                    switch (num)
                    {
                        case 43:
                        case 45:
                        case 46:
                        case 48:
                        case 49:
                        case 50:
                        case 51:
                        case 52:
                        case 53:
                        case 54:
                        case 55:
                        case 56:
                        case 57:
                            type = TokType.NUMBER;
                            if (num == 45)
                            {
                                bool flag = false;
                                do
                                {
                                    flag = !flag;
                                    num = file.Read();
                                }
                                while (num == 45);
                                if (flag)
                                {
                                    stringBuilder.Append('-');
                                }
                            }
                            else
                            {
                                stringBuilder.Append((char)num);
                                num = file.Read();
                            }

                            while (true)
                            {
                                switch (num)
                                {
                                    case 48:
                                    case 49:
                                    case 50:
                                    case 51:
                                    case 52:
                                    case 53:
                                    case 54:
                                    case 55:
                                    case 56:
                                    case 57:
                                        goto IL_0490;
                                    default:
                                        if (num == 46)
                                        {
                                            goto IL_0490;
                                        }

                                        break;
                                    case -1:
                                        break;
                                }

                                break;
                                IL_0490:
                                stringBuilder.Append((char)num);
                                num = file.Read();
                            }

                            break;
                        default:
                            type = TokType.OTHER;
                            do
                            {
                                stringBuilder.Append((char)num);
                                num = file.Read();
                            }
                            while (num != -1 && !IsDelimiter(num) && !IsWhitespace(num));
                            break;
                    }

                    if (num != -1)
                    {
                        BackOnePosition(num);
                    }

                    break;
            }

            if (stringBuilder != null)
            {
                stringValue = stringBuilder.ToString();
            }

            return true;
        }

        public virtual bool ReadLineSegment(byte[] input)
        {
            return ReadLineSegment(input, isNullWhitespace: true);
        }

        public virtual bool ReadLineSegment(byte[] input, bool isNullWhitespace)
        {
            int num = -1;
            bool flag = false;
            int num2 = 0;
            int num3 = input.Length;
            if (num2 < num3)
            {
                while (IsWhitespace(num = Read(), isNullWhitespace))
                {
                }
            }

            while (!flag && num2 < num3)
            {
                switch (num)
                {
                    case -1:
                    case 10:
                        flag = true;
                        break;
                    case 13:
                        {
                            flag = true;
                            long filePointer = FilePointer;
                            if (Read() != 10)
                            {
                                Seek(filePointer);
                            }

                            break;
                        }
                    default:
                        input[num2++] = (byte)num;
                        break;
                }

                if (flag || num3 <= num2)
                {
                    break;
                }

                num = Read();
            }

            if (num2 >= num3)
            {
                flag = false;
                while (!flag)
                {
                    switch (num = Read())
                    {
                        case -1:
                        case 10:
                            flag = true;
                            break;
                        case 13:
                            {
                                flag = true;
                                long filePointer2 = FilePointer;
                                if (Read() != 10)
                                {
                                    Seek(filePointer2);
                                }

                                break;
                            }
                    }
                }
            }

            if (num == -1 && num2 == 0)
            {
                return false;
            }

            if (num2 + 2 <= num3)
            {
                input[num2++] = 32;
                input[num2] = 88;
            }

            return true;
        }

        public static long[] CheckObjectStart(byte[] line)
        {
            try
            {
                PRTokeniser pRTokeniser = new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(line)));
                int num = 0;
                int num2 = 0;
                if (!pRTokeniser.NextToken() || pRTokeniser.TokenType != TokType.NUMBER)
                {
                    return null;
                }

                num = pRTokeniser.IntValue;
                if (!pRTokeniser.NextToken() || pRTokeniser.TokenType != TokType.NUMBER)
                {
                    return null;
                }

                num2 = pRTokeniser.IntValue;
                if (!pRTokeniser.NextToken())
                {
                    return null;
                }

                if (!pRTokeniser.StringValue.Equals("obj"))
                {
                    return null;
                }

                return new long[2] { num, num2 };
            }
            catch
            {
            }

            return null;
        }

        public virtual bool IsHexString()
        {
            return hexString;
        }
    }
}
