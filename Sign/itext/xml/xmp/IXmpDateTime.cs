namespace Sign.itext.xml.xmp
{
    public interface IXmpDateTime : IComparable
    {
        int Year { get; set; }

        int Month { get; set; }

        int Day { get; set; }

        int Hour { get; set; }

        int Minute { get; set; }

        int Second { get; set; }

        int NanoSecond { get; set; }

        TimeZone TimeZone { get; set; }

        XmpCalendar Calendar { get; }

        string Iso8601String { get; }

        bool HasDate();

        bool HasTime();

        bool HasTimeZone();
    }
}
