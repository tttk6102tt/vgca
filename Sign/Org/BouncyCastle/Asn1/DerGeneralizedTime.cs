﻿using Sign.Org.BouncyCastle.Utilities;
using System.Globalization;
using System.Text;

namespace Sign.Org.BouncyCastle.Asn1
{
    public class DerGeneralizedTime : Asn1Object
    {
        private readonly string time;

        public string TimeString => time;

        private bool HasFractionalSeconds => time.IndexOf('.') == 14;

        public static DerGeneralizedTime GetInstance(object obj)
        {
            if (obj == null || obj is DerGeneralizedTime)
            {
                return (DerGeneralizedTime)obj;
            }

            throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name, "obj");
        }

        public static DerGeneralizedTime GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            Asn1Object @object = obj.GetObject();
            if (isExplicit || @object is DerGeneralizedTime)
            {
                return GetInstance(@object);
            }

            return new DerGeneralizedTime(((Asn1OctetString)@object).GetOctets());
        }

        public DerGeneralizedTime(string time)
        {
            this.time = time;
            try
            {
                ToDateTime();
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("invalid date string: " + ex.Message);
            }
        }

        public DerGeneralizedTime(DateTime time)
        {
            this.time = time.ToString("yyyyMMddHHmmss\\Z");
        }

        internal DerGeneralizedTime(byte[] bytes)
        {
            time = Strings.FromAsciiByteArray(bytes);
        }

        public string GetTime()
        {
            if (time[time.Length - 1] == 'Z')
            {
                return time.Substring(0, time.Length - 1) + "GMT+00:00";
            }

            int num = time.Length - 5;
            char c = time[num];
            if (c == '-' || c == '+')
            {
                return time.Substring(0, num) + "GMT" + time.Substring(num, 3) + ":" + time.Substring(num + 3);
            }

            num = time.Length - 3;
            c = time[num];
            if (c == '-' || c == '+')
            {
                return time.Substring(0, num) + "GMT" + time.Substring(num) + ":00";
            }

            return time + CalculateGmtOffset();
        }

        private string CalculateGmtOffset()
        {
            char c = '+';
            DateTime dateTime = ToDateTime();
            TimeSpan timeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            if (timeSpan.CompareTo(TimeSpan.Zero) < 0)
            {
                c = '-';
                timeSpan = timeSpan.Duration();
            }

            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            return "GMT" + c + Convert(hours) + ":" + Convert(minutes);
        }

        private static string Convert(int time)
        {
            if (time < 10)
            {
                return "0" + time;
            }

            return time.ToString();
        }

        public DateTime ToDateTime()
        {
            string text = time;
            bool makeUniversal = false;
            string formatStr;
            if (text.EndsWith("Z"))
            {
                if (HasFractionalSeconds)
                {
                    int count = text.Length - text.IndexOf('.') - 2;
                    formatStr = "yyyyMMddHHmmss." + FString(count) + "\\Z";
                }
                else
                {
                    formatStr = "yyyyMMddHHmmss\\Z";
                }
            }
            else if (time.IndexOf('-') > 0 || time.IndexOf('+') > 0)
            {
                text = GetTime();
                makeUniversal = true;
                if (HasFractionalSeconds)
                {
                    int count2 = text.IndexOf("GMT") - 1 - text.IndexOf('.');
                    formatStr = "yyyyMMddHHmmss." + FString(count2) + "'GMT'zzz";
                }
                else
                {
                    formatStr = "yyyyMMddHHmmss'GMT'zzz";
                }
            }
            else if (HasFractionalSeconds)
            {
                int count3 = text.Length - 1 - text.IndexOf('.');
                formatStr = "yyyyMMddHHmmss." + FString(count3);
            }
            else
            {
                formatStr = "yyyyMMddHHmmss";
            }

            return ParseDateString(text, formatStr, makeUniversal);
        }

        private string FString(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                stringBuilder.Append('f');
            }

            return stringBuilder.ToString();
        }

        private DateTime ParseDateString(string dateStr, string formatStr, bool makeUniversal)
        {
            DateTime result = DateTime.ParseExact(dateStr, formatStr, DateTimeFormatInfo.InvariantInfo);
            if (!makeUniversal)
            {
                return result;
            }

            return result.ToUniversalTime();
        }

        private byte[] GetOctets()
        {
            return Strings.ToAsciiByteArray(time);
        }

        internal override void Encode(DerOutputStream derOut)
        {
            derOut.WriteEncoded(24, GetOctets());
        }

        protected override bool Asn1Equals(Asn1Object asn1Object)
        {
            DerGeneralizedTime derGeneralizedTime = asn1Object as DerGeneralizedTime;
            if (derGeneralizedTime == null)
            {
                return false;
            }

            return time.Equals(derGeneralizedTime.time);
        }

        protected override int Asn1GetHashCode()
        {
            return time.GetHashCode();
        }
    }
}
