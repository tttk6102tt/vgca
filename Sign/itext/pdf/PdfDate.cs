using System.Globalization;
using System.Text;

namespace Sign.itext.pdf
{
    public class PdfDate : PdfString
    {
        public PdfDate(DateTime d)
        {
            value = d.ToString("\\D\\:yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
            string text = d.ToString("zzz", DateTimeFormatInfo.InvariantInfo);
            text = text.Replace(":", "'");
            value = value + text + "'";
        }

        public PdfDate()
            : this(DateTime.Now)
        {
        }

        private static string SetLength(int i, int length)
        {
            return i.ToString().PadLeft(length, '0');
        }

        public virtual string GetW3CDate()
        {
            return GetW3CDate(value);
        }

        public static string GetW3CDate(string d)
        {
            if (d.StartsWith("D:"))
            {
                d = d.Substring(2);
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (d.Length < 4)
            {
                return "0000";
            }

            stringBuilder.Append(d.Substring(0, 4));
            d = d.Substring(4);
            if (d.Length < 2)
            {
                return stringBuilder.ToString();
            }

            stringBuilder.Append('-').Append(d.Substring(0, 2));
            d = d.Substring(2);
            if (d.Length < 2)
            {
                return stringBuilder.ToString();
            }

            stringBuilder.Append('-').Append(d.Substring(0, 2));
            d = d.Substring(2);
            if (d.Length < 2)
            {
                return stringBuilder.ToString();
            }

            stringBuilder.Append('T').Append(d.Substring(0, 2));
            d = d.Substring(2);
            if (d.Length < 2)
            {
                stringBuilder.Append(":00Z");
                return stringBuilder.ToString();
            }

            stringBuilder.Append(':').Append(d.Substring(0, 2));
            d = d.Substring(2);
            if (d.Length < 2)
            {
                stringBuilder.Append('Z');
                return stringBuilder.ToString();
            }

            stringBuilder.Append(':').Append(d.Substring(0, 2));
            d = d.Substring(2);
            if (d.StartsWith("-") || d.StartsWith("+"))
            {
                string text = d.Substring(0, 1);
                d = d.Substring(1);
                string text2 = "00";
                string text3 = "00";
                if (d.Length >= 2)
                {
                    text2 = d.Substring(0, 2);
                    if (d.Length > 2)
                    {
                        d = d.Substring(3);
                        if (d.Length >= 2)
                        {
                            text3 = d.Substring(0, 2);
                        }
                    }

                    stringBuilder.Append(text).Append(text2).Append(':')
                        .Append(text3);
                    return stringBuilder.ToString();
                }
            }

            stringBuilder.Append('Z');
            return stringBuilder.ToString();
        }

        public static DateTime Decode(string date)
        {
            if (date.StartsWith("D:"))
            {
                date = date.Substring(2);
            }

            int month = 1;
            int day = 1;
            int hour = 0;
            int minute = 0;
            int second = 0;
            int hours = 0;
            int minutes = 0;
            char c = '\0';
            int year = int.Parse(date.Substring(0, 4));
            if (date.Length >= 6)
            {
                month = int.Parse(date.Substring(4, 2));
                if (date.Length >= 8)
                {
                    day = int.Parse(date.Substring(6, 2));
                    if (date.Length >= 10)
                    {
                        hour = int.Parse(date.Substring(8, 2));
                        if (date.Length >= 12)
                        {
                            minute = int.Parse(date.Substring(10, 2));
                            if (date.Length >= 14)
                            {
                                second = int.Parse(date.Substring(12, 2));
                            }
                        }
                    }
                }
            }

            DateTime result = new DateTime(year, month, day, hour, minute, second);
            if (date.Length <= 14)
            {
                return result;
            }

            c = date[14];
            if (c == 'Z')
            {
                return result.ToLocalTime();
            }

            if (date.Length >= 17)
            {
                hours = int.Parse(date.Substring(15, 2));
                if (date.Length >= 20)
                {
                    minutes = int.Parse(date.Substring(18, 2));
                }
            }

            TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);
            if (c == '-')
            {
                result += timeSpan;
            }
            else
            {
                result -= timeSpan;
            }

            return result.ToLocalTime();
        }
    }
}
