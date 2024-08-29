using Sign.itext.xml.xmp.impl;

namespace Sign.itext.xml.xmp
{
    public static class XmpDateTimeFactory
    {
        public static IXmpDateTime CurrentDateTime => new XmpDateTimeImpl(new XmpCalendar());

        public static IXmpDateTime CreateFromCalendar(XmpCalendar calendar)
        {
            return new XmpDateTimeImpl(calendar);
        }

        public static IXmpDateTime Create()
        {
            return new XmpDateTimeImpl();
        }

        public static IXmpDateTime Create(int year, int month, int day)
        {
            return new XmpDateTimeImpl
            {
                Year = year,
                Month = month,
                Day = day
            };
        }

        public static IXmpDateTime Create(int year, int month, int day, int hour, int minute, int second, int nanoSecond)
        {
            return new XmpDateTimeImpl
            {
                Year = year,
                Month = month,
                Day = day,
                Hour = hour,
                Minute = minute,
                Second = second,
                NanoSecond = nanoSecond
            };
        }

        public static IXmpDateTime CreateFromIso8601(string strValue)
        {
            return new XmpDateTimeImpl(strValue);
        }

        public static IXmpDateTime SetLocalTimeZone(IXmpDateTime dateTime)
        {
            XmpCalendar calendar = dateTime.Calendar;
            calendar.TimeZone = TimeZone.CurrentTimeZone;
            return new XmpDateTimeImpl(calendar);
        }

        public static IXmpDateTime ConvertToUtcTime(IXmpDateTime dateTime)
        {
            long timeInMillis = dateTime.Calendar.TimeInMillis;
            return new XmpDateTimeImpl(new XmpCalendar
            {
                TimeInMillis = timeInMillis
            });
        }

        public static IXmpDateTime ConvertToLocalTime(IXmpDateTime dateTime)
        {
            long timeInMillis = dateTime.Calendar.TimeInMillis;
            return new XmpDateTimeImpl(new XmpCalendar
            {
                TimeInMillis = timeInMillis
            });
        }
    }
}
