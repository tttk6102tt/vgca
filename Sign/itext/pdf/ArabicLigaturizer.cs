using Sign.itext.text.pdf;
using System.Text;

namespace Sign.itext.pdf
{
    public class ArabicLigaturizer
    {
        private class Charstruct
        {
            internal char basechar;

            internal char mark1;

            internal char vowel;

            internal int lignum;

            internal int numshapes = 1;
        }

        private static Dictionary<char, char[]> maptable;

        private const char ALEF = 'ا';

        private const char ALEFHAMZA = 'أ';

        private const char ALEFHAMZABELOW = 'إ';

        private const char ALEFMADDA = 'آ';

        private const char LAM = 'ل';

        private const char HAMZA = 'ء';

        private const char TATWEEL = 'ـ';

        private const char ZWJ = '\u200d';

        private const char HAMZAABOVE = '\u0654';

        private const char HAMZABELOW = '\u0655';

        private const char WAWHAMZA = 'ؤ';

        private const char YEHHAMZA = 'ئ';

        private const char WAW = 'و';

        private const char ALEFMAKSURA = 'ى';

        private const char YEH = 'ي';

        private const char FARSIYEH = 'ی';

        private const char SHADDA = '\u0651';

        private const char KASRA = '\u0650';

        private const char FATHA = '\u064e';

        private const char DAMMA = '\u064f';

        private const char MADDA = '\u0653';

        private const char LAM_ALEF = 'ﻻ';

        private const char LAM_ALEFHAMZA = 'ﻷ';

        private const char LAM_ALEFHAMZABELOW = 'ﻹ';

        private const char LAM_ALEFMADDA = 'ﻵ';

        private static char[][] chartable;

        public const int ar_nothing = 0;

        public const int ar_novowel = 1;

        public const int ar_composedtashkeel = 4;

        public const int ar_lig = 8;

        public const int DIGITS_EN2AN = 32;

        public const int DIGITS_AN2EN = 64;

        public const int DIGITS_EN2AN_INIT_LR = 96;

        public const int DIGITS_EN2AN_INIT_AL = 128;

        private const int DIGITS_RESERVED = 160;

        public const int DIGITS_MASK = 224;

        public const int DIGIT_TYPE_AN = 0;

        public const int DIGIT_TYPE_AN_EXTENDED = 256;

        public const int DIGIT_TYPE_MASK = 256;

        protected int options;

        protected int runDirection = 3;

        private static bool IsVowel(char s)
        {
            if (s < '\u064b' || s > '\u0655')
            {
                return s == '\u0670';
            }

            return true;
        }

        private static char Charshape(char s, int which)
        {
            if (s >= 'ء' && s <= 'ۓ')
            {
                if (maptable.TryGetValue(s, out var value))
                {
                    return value[which + 1];
                }
            }
            else if (s >= 'ﻵ' && s <= 'ﻻ')
            {
                return (char)(s + which);
            }

            return s;
        }

        private static int Shapecount(char s)
        {
            if (s >= 'ء' && s <= 'ۓ' && !IsVowel(s))
            {
                if (maptable.TryGetValue(s, out var value))
                {
                    return value.Length - 1;
                }
            }
            else if (s == '\u200d')
            {
                return 4;
            }

            return 1;
        }

        private static int Ligature(char newchar, Charstruct oldchar)
        {
            int result = 0;
            if (oldchar.basechar == '\0')
            {
                return 0;
            }

            if (IsVowel(newchar))
            {
                result = 1;
                if (oldchar.vowel != 0 && newchar != '\u0651')
                {
                    result = 2;
                }

                switch (newchar)
                {
                    case '\u0651':
                        if (oldchar.mark1 == '\0')
                        {
                            oldchar.mark1 = '\u0651';
                            break;
                        }

                        return 0;
                    case '\u0655':
                        switch (oldchar.basechar)
                        {
                            case 'ا':
                                oldchar.basechar = 'إ';
                                result = 2;
                                break;
                            case 'ﻻ':
                                oldchar.basechar = 'ﻹ';
                                result = 2;
                                break;
                            default:
                                oldchar.mark1 = '\u0655';
                                break;
                        }

                        break;
                    case '\u0654':
                        switch (oldchar.basechar)
                        {
                            case 'ا':
                                oldchar.basechar = 'أ';
                                result = 2;
                                break;
                            case 'ﻻ':
                                oldchar.basechar = 'ﻷ';
                                result = 2;
                                break;
                            case 'و':
                                oldchar.basechar = 'ؤ';
                                result = 2;
                                break;
                            case 'ى':
                            case 'ي':
                            case 'ی':
                                oldchar.basechar = 'ئ';
                                result = 2;
                                break;
                            default:
                                oldchar.mark1 = '\u0654';
                                break;
                        }

                        break;
                    case '\u0653':
                        {
                            char basechar = oldchar.basechar;
                            if (basechar == 'ا')
                            {
                                oldchar.basechar = 'آ';
                                result = 2;
                            }

                            break;
                        }
                    default:
                        oldchar.vowel = newchar;
                        break;
                }

                if (result == 1)
                {
                    oldchar.lignum++;
                }

                return result;
            }

            if (oldchar.vowel != 0)
            {
                return 0;
            }

            switch (oldchar.basechar)
            {
                case 'ل':
                    switch (newchar)
                    {
                        case 'ا':
                            oldchar.basechar = 'ﻻ';
                            oldchar.numshapes = 2;
                            result = 3;
                            break;
                        case 'أ':
                            oldchar.basechar = 'ﻷ';
                            oldchar.numshapes = 2;
                            result = 3;
                            break;
                        case 'إ':
                            oldchar.basechar = 'ﻹ';
                            oldchar.numshapes = 2;
                            result = 3;
                            break;
                        case 'آ':
                            oldchar.basechar = 'ﻵ';
                            oldchar.numshapes = 2;
                            result = 3;
                            break;
                    }

                    break;
                case '\0':
                    oldchar.basechar = newchar;
                    oldchar.numshapes = Shapecount(newchar);
                    result = 1;
                    break;
            }

            return result;
        }

        private static void Copycstostring(StringBuilder str, Charstruct s, int level)
        {
            if (s.basechar == '\0')
            {
                return;
            }

            str.Append(s.basechar);
            s.lignum--;
            if (s.mark1 != 0)
            {
                if ((level & 1) == 0)
                {
                    str.Append(s.mark1);
                    s.lignum--;
                }
                else
                {
                    s.lignum--;
                }
            }

            if (s.vowel != 0)
            {
                if ((level & 1) == 0)
                {
                    str.Append(s.vowel);
                    s.lignum--;
                }
                else
                {
                    s.lignum--;
                }
            }
        }

        internal static void Doublelig(StringBuilder str, int level)
        {
            int num;
            int num2 = (num = str.Length);
            int num3 = 0;
            int num4 = 1;
            while (num4 < num2)
            {
                char c = '\0';
                if (((uint)level & 4u) != 0)
                {
                    switch (str[num3])
                    {
                        case '\u0651':
                            switch (str[num4])
                            {
                                case '\u0650':
                                    c = 'ﱢ';
                                    break;
                                case '\u064e':
                                    c = 'ﱠ';
                                    break;
                                case '\u064f':
                                    c = 'ﱡ';
                                    break;
                                case '\u064c':
                                    c = 'ﱞ';
                                    break;
                                case '\u064d':
                                    c = 'ﱟ';
                                    break;
                            }

                            break;
                        case '\u0650':
                            if (str[num4] == '\u0651')
                            {
                                c = 'ﱢ';
                            }

                            break;
                        case '\u064e':
                            if (str[num4] == '\u0651')
                            {
                                c = 'ﱠ';
                            }

                            break;
                        case '\u064f':
                            if (str[num4] == '\u0651')
                            {
                                c = 'ﱡ';
                            }

                            break;
                    }
                }

                if (((uint)level & 8u) != 0)
                {
                    switch (str[num3])
                    {
                        case 'ﻟ':
                            switch (str[num4])
                            {
                                case 'ﺞ':
                                    c = 'ﰿ';
                                    break;
                                case 'ﺠ':
                                    c = 'ﳉ';
                                    break;
                                case 'ﺢ':
                                    c = 'ﱀ';
                                    break;
                                case 'ﺤ':
                                    c = 'ﳊ';
                                    break;
                                case 'ﺦ':
                                    c = 'ﱁ';
                                    break;
                                case 'ﺨ':
                                    c = 'ﳋ';
                                    break;
                                case 'ﻢ':
                                    c = 'ﱂ';
                                    break;
                                case 'ﻤ':
                                    c = 'ﳌ';
                                    break;
                            }

                            break;
                        case 'ﺗ':
                            switch (str[num4])
                            {
                                case 'ﺠ':
                                    c = 'ﲡ';
                                    break;
                                case 'ﺤ':
                                    c = 'ﲢ';
                                    break;
                                case 'ﺨ':
                                    c = 'ﲣ';
                                    break;
                            }

                            break;
                        case 'ﺑ':
                            switch (str[num4])
                            {
                                case 'ﺠ':
                                    c = 'ﲜ';
                                    break;
                                case 'ﺤ':
                                    c = 'ﲝ';
                                    break;
                                case 'ﺨ':
                                    c = 'ﲞ';
                                    break;
                            }

                            break;
                        case 'ﻧ':
                            switch (str[num4])
                            {
                                case 'ﺠ':
                                    c = 'ﳒ';
                                    break;
                                case 'ﺤ':
                                    c = 'ﳓ';
                                    break;
                                case 'ﺨ':
                                    c = 'ﳔ';
                                    break;
                            }

                            break;
                        case 'ﻨ':
                            switch (str[num4])
                            {
                                case 'ﺮ':
                                    c = 'ﲊ';
                                    break;
                                case 'ﺰ':
                                    c = 'ﲋ';
                                    break;
                            }

                            break;
                        case 'ﻣ':
                            switch (str[num4])
                            {
                                case 'ﺠ':
                                    c = 'ﳎ';
                                    break;
                                case 'ﺤ':
                                    c = 'ﳏ';
                                    break;
                                case 'ﺨ':
                                    c = 'ﳐ';
                                    break;
                                case 'ﻤ':
                                    c = 'ﳑ';
                                    break;
                            }

                            break;
                        case 'ﻓ':
                            {
                                char c2 = str[num4];
                                if (c2 == 'ﻲ')
                                {
                                    c = 'ﰲ';
                                }

                                break;
                            }
                    }
                }

                if (c != 0)
                {
                    str[num3] = c;
                    num--;
                    num4++;
                }
                else
                {
                    num3++;
                    str[num3] = str[num4];
                    num4++;
                }
            }

            str.Length = num;
        }

        private static bool Connects_to_left(Charstruct a)
        {
            return a.numshapes > 2;
        }

        internal static void Shape(char[] text, StringBuilder str, int level)
        {
            int num = 0;
            Charstruct charstruct = new Charstruct();
            Charstruct charstruct2 = new Charstruct();
            int num3;
            while (num < text.Length)
            {
                char c = text[num++];
                if (Ligature(c, charstruct2) == 0)
                {
                    int num2 = Shapecount(c);
                    num3 = ((num2 != 1) ? 2 : 0);
                    if (Connects_to_left(charstruct))
                    {
                        num3++;
                    }

                    num3 %= charstruct2.numshapes;
                    charstruct2.basechar = Charshape(charstruct2.basechar, num3);
                    Copycstostring(str, charstruct, level);
                    charstruct = charstruct2;
                    charstruct2 = new Charstruct();
                    charstruct2.basechar = c;
                    charstruct2.numshapes = num2;
                    charstruct2.lignum++;
                }
                else
                {
                    _ = 1;
                }
            }

            num3 = (Connects_to_left(charstruct) ? 1 : 0);
            num3 %= charstruct2.numshapes;
            charstruct2.basechar = Charshape(charstruct2.basechar, num3);
            Copycstostring(str, charstruct, level);
            Copycstostring(str, charstruct2, level);
        }

        internal static int Arabic_shape(char[] src, int srcoffset, int srclength, char[] dest, int destoffset, int destlength, int level)
        {
            char[] array = new char[srclength];
            for (int num = srclength + srcoffset - 1; num >= srcoffset; num--)
            {
                array[num - srcoffset] = src[num];
            }

            StringBuilder stringBuilder = new StringBuilder(srclength);
            Shape(array, stringBuilder, level);
            if (((uint)level & 0xCu) != 0)
            {
                Doublelig(stringBuilder, level);
            }

            Array.Copy(stringBuilder.ToString().ToCharArray(), 0, dest, destoffset, stringBuilder.Length);
            return stringBuilder.Length;
        }

        internal static void ProcessNumbers(char[] text, int offset, int length, int options)
        {
            int num = offset + length;
            if ((options & 0xE0) == 0)
            {
                return;
            }

            char c = '0';
            switch (options & 0x100)
            {
                case 0:
                    c = '٠';
                    break;
                case 256:
                    c = '۰';
                    break;
            }

            switch (options & 0xE0)
            {
                case 32:
                    {
                        int num3 = c - 48;
                        for (int j = offset; j < num; j++)
                        {
                            char c4 = text[j];
                            if (c4 <= '9' && c4 >= '0')
                            {
                                text[j] += (char)(ushort)num3;
                            }
                        }

                        break;
                    }
                case 64:
                    {
                        char c2 = (char)(c + 9);
                        int num2 = 48 - c;
                        for (int i = offset; i < num; i++)
                        {
                            char c3 = text[i];
                            if (c3 <= c2 && c3 >= c)
                            {
                                text[i] += (char)(ushort)num2;
                            }
                        }

                        break;
                    }
                case 96:
                    ShapeToArabicDigitsWithContext(text, 0, length, c, lastStrongWasAL: false);
                    break;
                case 128:
                    ShapeToArabicDigitsWithContext(text, 0, length, c, lastStrongWasAL: true);
                    break;
            }
        }

        internal static void ShapeToArabicDigitsWithContext(char[] dest, int start, int length, char digitBase, bool lastStrongWasAL)
        {
            digitBase = (char)(digitBase - 48);
            int num = start + length;
            for (int i = start; i < num; i++)
            {
                char c = dest[i];
                switch (BidiOrder.GetDirection(c))
                {
                    case 0:
                    case 3:
                        lastStrongWasAL = false;
                        break;
                    case 4:
                        lastStrongWasAL = true;
                        break;
                    case 8:
                        if (lastStrongWasAL && c <= '9')
                        {
                            dest[i] = (char)(c + digitBase);
                        }

                        break;
                }
            }
        }

        static ArabicLigaturizer()
        {
            maptable = new Dictionary<char, char[]>();
            chartable = new char[76][]
            {
                new char[2] { 'ء', 'ﺀ' },
                new char[3] { 'آ', 'ﺁ', 'ﺂ' },
                new char[3] { 'أ', 'ﺃ', 'ﺄ' },
                new char[3] { 'ؤ', 'ﺅ', 'ﺆ' },
                new char[3] { 'إ', 'ﺇ', 'ﺈ' },
                new char[5] { 'ئ', 'ﺉ', 'ﺊ', 'ﺋ', 'ﺌ' },
                new char[3] { 'ا', 'ﺍ', 'ﺎ' },
                new char[5] { 'ب', 'ﺏ', 'ﺐ', 'ﺑ', 'ﺒ' },
                new char[3] { 'ة', 'ﺓ', 'ﺔ' },
                new char[5] { 'ت', 'ﺕ', 'ﺖ', 'ﺗ', 'ﺘ' },
                new char[5] { 'ث', 'ﺙ', 'ﺚ', 'ﺛ', 'ﺜ' },
                new char[5] { 'ج', 'ﺝ', 'ﺞ', 'ﺟ', 'ﺠ' },
                new char[5] { 'ح', 'ﺡ', 'ﺢ', 'ﺣ', 'ﺤ' },
                new char[5] { 'خ', 'ﺥ', 'ﺦ', 'ﺧ', 'ﺨ' },
                new char[3] { 'د', 'ﺩ', 'ﺪ' },
                new char[3] { 'ذ', 'ﺫ', 'ﺬ' },
                new char[3] { 'ر', 'ﺭ', 'ﺮ' },
                new char[3] { 'ز', 'ﺯ', 'ﺰ' },
                new char[5] { 'س', 'ﺱ', 'ﺲ', 'ﺳ', 'ﺴ' },
                new char[5] { 'ش', 'ﺵ', 'ﺶ', 'ﺷ', 'ﺸ' },
                new char[5] { 'ص', 'ﺹ', 'ﺺ', 'ﺻ', 'ﺼ' },
                new char[5] { 'ض', 'ﺽ', 'ﺾ', 'ﺿ', 'ﻀ' },
                new char[5] { 'ط', 'ﻁ', 'ﻂ', 'ﻃ', 'ﻄ' },
                new char[5] { 'ظ', 'ﻅ', 'ﻆ', 'ﻇ', 'ﻈ' },
                new char[5] { 'ع', 'ﻉ', 'ﻊ', 'ﻋ', 'ﻌ' },
                new char[5] { 'غ', 'ﻍ', 'ﻎ', 'ﻏ', 'ﻐ' },
                new char[5] { 'ـ', 'ـ', 'ـ', 'ـ', 'ـ' },
                new char[5] { 'ف', 'ﻑ', 'ﻒ', 'ﻓ', 'ﻔ' },
                new char[5] { 'ق', 'ﻕ', 'ﻖ', 'ﻗ', 'ﻘ' },
                new char[5] { 'ك', 'ﻙ', 'ﻚ', 'ﻛ', 'ﻜ' },
                new char[5] { 'ل', 'ﻝ', 'ﻞ', 'ﻟ', 'ﻠ' },
                new char[5] { 'م', 'ﻡ', 'ﻢ', 'ﻣ', 'ﻤ' },
                new char[5] { 'ن', 'ﻥ', 'ﻦ', 'ﻧ', 'ﻨ' },
                new char[5] { 'ه', 'ﻩ', 'ﻪ', 'ﻫ', 'ﻬ' },
                new char[3] { 'و', 'ﻭ', 'ﻮ' },
                new char[5] { 'ى', 'ﻯ', 'ﻰ', 'ﯨ', 'ﯩ' },
                new char[5] { 'ي', 'ﻱ', 'ﻲ', 'ﻳ', 'ﻴ' },
                new char[3] { 'ٱ', 'ﭐ', 'ﭑ' },
                new char[5] { 'ٹ', 'ﭦ', 'ﭧ', 'ﭨ', 'ﭩ' },
                new char[5] { 'ٺ', 'ﭞ', 'ﭟ', 'ﭠ', 'ﭡ' },
                new char[5] { 'ٻ', 'ﭒ', 'ﭓ', 'ﭔ', 'ﭕ' },
                new char[5] { 'پ', 'ﭖ', 'ﭗ', 'ﭘ', 'ﭙ' },
                new char[5] { 'ٿ', 'ﭢ', 'ﭣ', 'ﭤ', 'ﭥ' },
                new char[5] { 'ڀ', 'ﭚ', 'ﭛ', 'ﭜ', 'ﭝ' },
                new char[5] { 'ڃ', 'ﭶ', 'ﭷ', 'ﭸ', 'ﭹ' },
                new char[5] { 'ڄ', 'ﭲ', 'ﭳ', 'ﭴ', 'ﭵ' },
                new char[5] { 'چ', 'ﭺ', 'ﭻ', 'ﭼ', 'ﭽ' },
                new char[5] { 'ڇ', 'ﭾ', 'ﭿ', 'ﮀ', 'ﮁ' },
                new char[3] { 'ڈ', 'ﮈ', 'ﮉ' },
                new char[3] { 'ڌ', 'ﮄ', 'ﮅ' },
                new char[3] { 'ڍ', 'ﮂ', 'ﮃ' },
                new char[3] { 'ڎ', 'ﮆ', 'ﮇ' },
                new char[3] { 'ڑ', 'ﮌ', 'ﮍ' },
                new char[3] { 'ژ', 'ﮊ', 'ﮋ' },
                new char[5] { 'ڤ', 'ﭪ', 'ﭫ', 'ﭬ', 'ﭭ' },
                new char[5] { 'ڦ', 'ﭮ', 'ﭯ', 'ﭰ', 'ﭱ' },
                new char[5] { 'ک', 'ﮎ', 'ﮏ', 'ﮐ', 'ﮑ' },
                new char[5] { 'ڭ', 'ﯓ', 'ﯔ', 'ﯕ', 'ﯖ' },
                new char[5] { 'گ', 'ﮒ', 'ﮓ', 'ﮔ', 'ﮕ' },
                new char[5] { 'ڱ', 'ﮚ', 'ﮛ', 'ﮜ', 'ﮝ' },
                new char[5] { 'ڳ', 'ﮖ', 'ﮗ', 'ﮘ', 'ﮙ' },
                new char[3] { 'ں', 'ﮞ', 'ﮟ' },
                new char[5] { 'ڻ', 'ﮠ', 'ﮡ', 'ﮢ', 'ﮣ' },
                new char[5] { 'ھ', 'ﮪ', 'ﮫ', 'ﮬ', 'ﮭ' },
                new char[3] { 'ۀ', 'ﮤ', 'ﮥ' },
                new char[5] { 'ہ', 'ﮦ', 'ﮧ', 'ﮨ', 'ﮩ' },
                new char[3] { 'ۅ', 'ﯠ', 'ﯡ' },
                new char[3] { 'ۆ', 'ﯙ', 'ﯚ' },
                new char[3] { 'ۇ', 'ﯗ', 'ﯘ' },
                new char[3] { 'ۈ', 'ﯛ', 'ﯜ' },
                new char[3] { 'ۉ', 'ﯢ', 'ﯣ' },
                new char[3] { 'ۋ', 'ﯞ', 'ﯟ' },
                new char[5] { 'ی', 'ﯼ', 'ﯽ', 'ﯾ', 'ﯿ' },
                new char[5] { 'ې', 'ﯤ', 'ﯥ', 'ﯦ', 'ﯧ' },
                new char[3] { 'ے', 'ﮮ', 'ﮯ' },
                new char[3] { 'ۓ', 'ﮰ', 'ﮱ' }
            };
            char[][] array = chartable;
            foreach (char[] array2 in array)
            {
                maptable[array2[0]] = array2;
            }
        }

        public ArabicLigaturizer()
        {
        }

        public ArabicLigaturizer(int runDirection, int options)
        {
            this.runDirection = runDirection;
            this.options = options;
        }

        public virtual string Process(string s)
        {
            return BidiLine.ProcessLTR(s, runDirection, options);
        }

        public virtual bool IsRTL()
        {
            return true;
        }
    }
}
