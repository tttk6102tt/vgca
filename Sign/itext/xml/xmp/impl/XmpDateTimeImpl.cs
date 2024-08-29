namespace Sign.itext.xml.xmp.impl
{
    public class XmpDateTimeImpl : IXmpDateTime, IComparable
    {
        private int _day;

        private bool _hasDate;

        private bool _hasTime;

        private bool _hasTimeZone;

        private int _hour;

        private int _minute;

        private int _month;

        private int _nanoSeconds;

        private int _second;

        private TimeZone _timeZone;

        private int _year;

        public virtual int Year
        {
            get
            {
                return _year;
            }
            set
            {
                _year = Math.Min(Math.Abs(value), 9999);
                _hasDate = true;
            }
        }

        public virtual int Month
        {
            get
            {
                return _month;
            }
            set
            {
                if (value < 1)
                {
                    _month = 1;
                }
                else if (value > 12)
                {
                    _month = 12;
                }
                else
                {
                    _month = value;
                }

                _hasDate = true;
            }
        }

        public virtual int Day
        {
            get
            {
                return _day;
            }
            set
            {
                if (value < 1)
                {
                    _day = 1;
                }
                else if (value > 31)
                {
                    _day = 31;
                }
                else
                {
                    _day = value;
                }

                _hasDate = true;
            }
        }

        public virtual int Hour
        {
            get
            {
                return _hour;
            }
            set
            {
                _hour = Math.Min(Math.Abs(value), 23);
                _hasTime = true;
            }
        }

        public virtual int Minute
        {
            get
            {
                return _minute;
            }
            set
            {
                _minute = Math.Min(Math.Abs(value), 59);
                _hasTime = true;
            }
        }

        public virtual int Second
        {
            get
            {
                return _second;
            }
            set
            {
                _second = Math.Min(Math.Abs(value), 59);
                _hasTime = true;
            }
        }

        public virtual int NanoSecond
        {
            get
            {
                return _nanoSeconds;
            }
            set
            {
                _nanoSeconds = value;
                _hasTime = true;
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
                _hasTime = true;
                _hasTimeZone = true;
            }
        }

        public virtual XmpCalendar Calendar
        {
            get
            {
                XmpCalendar xmpCalendar = new XmpCalendar();
                if (_hasTimeZone)
                {
                    xmpCalendar.TimeZone = _timeZone;
                }

                xmpCalendar.DateTime = new DateTime(_year, _month - 1, _day, _hour, _minute, _second, _nanoSeconds / 1000000);
                return xmpCalendar;
            }
        }

        public virtual string Iso8601String => Iso8601Converter.Render(this);

        public XmpDateTimeImpl()
        {
        }

        public XmpDateTimeImpl(XmpCalendar calendar)
        {
            DateTime dateTime = calendar.DateTime;
            TimeZone timeZone = calendar.TimeZone;
            _year = dateTime.Year;
            _month = dateTime.Month + 1;
            _day = dateTime.Day;
            _hour = dateTime.Hour;
            _minute = dateTime.Minute;
            _second = dateTime.Second;
            _nanoSeconds = dateTime.Millisecond * 1000000;
            _timeZone = timeZone;
            _hasDate = (_hasTime = (_hasTimeZone = true));
        }

        public XmpDateTimeImpl(DateTime date, TimeZone timeZone)
        {
            _year = date.Year;
            _month = date.Month + 1;
            _day = date.Day;
            _hour = date.Hour;
            _minute = date.Minute;
            _second = date.Second;
            _nanoSeconds = date.Millisecond * 1000000;
            _timeZone = timeZone;
            _hasDate = (_hasTime = (_hasTimeZone = true));
        }

        public XmpDateTimeImpl(string strValue)
        {
            Iso8601Converter.Parse(strValue, this);
        }

        public virtual int CompareTo(object dt)
        {
            long num = Calendar.TimeInMillis - ((IXmpDateTime)dt).Calendar.TimeInMillis;
            if (num != 0L)
            {
                return Math.Sign(num);
            }

            num = _nanoSeconds - ((IXmpDateTime)dt).NanoSecond;
            return Math.Sign(num);
        }

        public virtual bool HasDate()
        {
            return _hasDate;
        }

        public virtual bool HasTime()
        {
            return _hasTime;
        }

        public virtual bool HasTimeZone()
        {
            return _hasTimeZone;
        }

        public override string ToString()
        {
            return Iso8601String;
        }
    }
}
