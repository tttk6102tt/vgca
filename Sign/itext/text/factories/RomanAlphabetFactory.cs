using Sign.itext.error_messages;
using System.Globalization;

namespace Sign.itext.text.factories
{
    public class RomanAlphabetFactory
    {
        public static string GetString(int index)
        {
            if (index < 1)
            {
                throw new FormatException(MessageLocalization.GetComposedMessage("you.can.t.translate.a.negative.number.into.an.alphabetical.value"));
            }

            index--;
            int num = 1;
            int num2 = 0;
            int num3 = 26;
            while (index >= num3 + num2)
            {
                num++;
                num2 += num3;
                num3 *= 26;
            }

            int num4 = index - num2;
            char[] array = new char[num];
            while (num > 0)
            {
                array[--num] = (char)(97 + num4 % 26);
                num4 /= 26;
            }

            return new string(array);
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
