using System.Globalization;

namespace Sign.itext.text.factories
{
    public class GreekAlphabetFactory
    {
        public static string GetString(int index) => GreekAlphabetFactory.GetString(index, true);

        public static string GetLowerCaseString(int index) => GreekAlphabetFactory.GetString(index);

        public static string GetUpperCaseString(int index) => GreekAlphabetFactory.GetString(index).ToUpper(CultureInfo.InvariantCulture);

        public static string GetString(int index, bool lowercase)
        {
            if (index < 1)
                return "";
            --index;
            int length = 1;
            int num1 = 0;
            for (int index1 = 24; index >= index1 + num1; index1 *= 24)
            {
                ++length;
                num1 += index1;
            }
            int num2 = index - num1;
            char[] chArray = new char[length];
            while (length > 0)
            {
                --length;
                chArray[length] = (char)(num2 % 24);
                if (chArray[length] > '\u0010')
                    ++chArray[length];
                chArray[length] += lowercase ? 'α' : 'Α';
                chArray[length] = SpecialSymbol.GetCorrespondingSymbol(chArray[length]);
                num2 /= 24;
            }
            return new string(chArray);
        }
    }
}
