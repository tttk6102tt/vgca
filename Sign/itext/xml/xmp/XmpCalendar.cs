﻿namespace Sign.itext.xml.xmp
{
    public class XmpCalendar
    {
        private DateTime _dateTime;

        private TimeZone _timeZone;

        public virtual DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
            }
        }

        public virtual TimeZone TimeZone
        {
            get
            {
                return _timeZone;
            }
            set
            {
                _timeZone = value;
            }
        }

        public virtual long TimeInMillis
        {
            get
            {
                return _dateTime.Ticks;
            }
            set
            {
                _dateTime = new DateTime(value);
            }
        }

        public XmpCalendar(DateTime dt, TimeZone tz)
        {
            DateTime = dt;
            TimeZone = tz;
        }

        public XmpCalendar(DateTime dt)
            : this(dt, TimeZone.CurrentTimeZone)
        {
        }

        public XmpCalendar(TimeZone tz)
            : this(DateTime.Now, tz)
        {
        }

        public XmpCalendar()
            : this(DateTime.Now, TimeZone.CurrentTimeZone)
        {
        }
    }
}
