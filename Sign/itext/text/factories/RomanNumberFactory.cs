using System.Globalization;
using System.Text;

namespace Sign.itext.text.factories
{
    public class RomanNumberFactory
    {
        internal class RomanDigit
        {
            public char digit;

            public int value;

            public bool pre;

            internal RomanDigit(char digit, int value, bool pre)
            {
                this.digit = digit;
                this.value = value;
                this.pre = pre;
            }
        }

        private static RomanDigit[] roman = new RomanDigit[7]
        {
            new RomanDigit('m', 1000, pre: false),
            new RomanDigit('d', 500, pre: false),
            new RomanDigit('c', 100, pre: true),
            new RomanDigit('l', 50, pre: false),
            new RomanDigit('x', 10, pre: true),
            new RomanDigit('v', 5, pre: false),
            new RomanDigit('i', 1, pre: true)
        };

        public static string GetString(int index)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (index < 0)
            {
                stringBuilder.Append('-');
                index = -index;
            }

            if (index > 3000)
            {
                stringBuilder.Append('|');
                stringBuilder.Append(GetString(index / 1000));
                stringBuilder.Append('|');
                index -= index / 1000 * 1000;
            }

            int num = 0;
            while (true)
            {
                RomanDigit romanDigit = roman[num];
                while (index >= romanDigit.value)
                {
                    stringBuilder.Append(romanDigit.digit);
                    index -= romanDigit.value;
                }

                if (index <= 0)
                {
                    break;
                }

                int num2 = num;
                while (!roman[++num2].pre)
                {
                }

                if (index + roman[num2].value >= romanDigit.value)
                {
                    stringBuilder.Append(roman[num2].digit).Append(romanDigit.digit);
                    index -= romanDigit.value - roman[num2].value;
                }

                num++;
            }

            return stringBuilder.ToString();
        }

        public static string GetLowerCaseString(int index)
        {
            return GetString(index);
        }

        public static string GetUpperCaseString(int index)
        {
            return GetString(index).ToUpper(CultureInfo.InvariantCulture);
        }

        public static string GetString(int index, bool lowercase)
        {
            if (lowercase)
            {
                return GetLowerCaseString(index);
            }

            return GetUpperCaseString(index);
        }
    }
}
