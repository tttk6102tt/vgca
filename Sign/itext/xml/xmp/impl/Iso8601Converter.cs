namespace Sign.itext.xml.xmp.impl
{
    public static class Iso8601Converter
    {
        public static IXmpDateTime Parse(string iso8601String)
        {
            return Parse(iso8601String, new XmpDateTimeImpl());
        }

        public static IXmpDateTime Parse(string iso8601String, IXmpDateTime binValue)
        {
            if (iso8601String == null)
            {
                throw new XmpException("Parameter must not be null", 4);
            }

            if (iso8601String.Length == 0)
            {
                return binValue;
            }

            ParseState parseState = new ParseState(iso8601String);
            if (parseState.Ch(0) == '-')
            {
                parseState.Skip();
            }

            int num = parseState.GatherInt("Invalid year in date string", 9999);
            if (parseState.HasNext() && parseState.Ch() != '-')
            {
                throw new XmpException("Invalid date string, after year", 5);
            }

            if (parseState.Ch(0) == '-')
            {
                num = -num;
            }

            binValue.Year = num;
            if (!parseState.HasNext())
            {
                return binValue;
            }

            parseState.Skip();
            num = parseState.GatherInt("Invalid month in date string", 12);
            if (parseState.HasNext() && parseState.Ch() != '-')
            {
                throw new XmpException("Invalid date string, after month", 5);
            }

            binValue.Month = num;
            if (!parseState.HasNext())
            {
                return binValue;
            }

            parseState.Skip();
            num = parseState.GatherInt("Invalid day in date string", 31);
            if (parseState.HasNext() && parseState.Ch() != 'T')
            {
                throw new XmpException("Invalid date string, after day", 5);
            }

            binValue.Day = num;
            if (!parseState.HasNext())
            {
                return binValue;
            }

            parseState.Skip();
            num = (binValue.Hour = parseState.GatherInt("Invalid hour in date string", 23));
            if (!parseState.HasNext())
            {
                return binValue;
            }

            if (parseState.Ch() == ':')
            {
                parseState.Skip();
                num = parseState.GatherInt("Invalid minute in date string", 59);
                if (parseState.HasNext() && parseState.Ch() != ':' && parseState.Ch() != 'Z' && parseState.Ch() != '+' && parseState.Ch() != '-')
                {
                    throw new XmpException("Invalid date string, after minute", 5);
                }

                binValue.Minute = num;
            }

            if (!parseState.HasNext())
            {
                return binValue;
            }

            if (parseState.HasNext() && parseState.Ch() == ':')
            {
                parseState.Skip();
                num = parseState.GatherInt("Invalid whole seconds in date string", 59);
                if (parseState.HasNext() && parseState.Ch() != '.' && parseState.Ch() != 'Z' && parseState.Ch() != '+' && parseState.Ch() != '-')
                {
                    throw new XmpException("Invalid date string, after whole seconds", 5);
                }

                binValue.Second = num;
                if (parseState.Ch() == '.')
                {
                    parseState.Skip();
                    int num3 = parseState.Pos();
                    num = parseState.GatherInt("Invalid fractional seconds in date string", 999999999);
                    if (parseState.HasNext() && parseState.Ch() != 'Z' && parseState.Ch() != '+' && parseState.Ch() != '-')
                    {
                        throw new XmpException("Invalid date string, after fractional second", 5);
                    }

                    for (num3 = parseState.Pos() - num3; num3 > 9; num3--)
                    {
                        num /= 10;
                    }

                    for (; num3 < 9; num3++)
                    {
                        num *= 10;
                    }

                    binValue.NanoSecond = num;
                }
            }
            else if (parseState.Ch() != 'Z' && parseState.Ch() != '+' && parseState.Ch() != '-')
            {
                throw new XmpException("Invalid date string, after time", 5);
            }

            if (!parseState.HasNext())
            {
                return binValue;
            }

            if (parseState.Ch() == 'Z')
            {
                parseState.Skip();
            }
            else if (parseState.HasNext())
            {
                if (parseState.Ch() != '+' && parseState.Ch() != '-')
                {
                    throw new XmpException("Time zone must begin with 'Z', '+', or '-'", 5);
                }

                parseState.Skip();
                if (parseState.HasNext())
                {
                    if (parseState.Ch() != ':')
                    {
                        throw new XmpException("Invalid date string, after time zone hour", 5);
                    }

                    parseState.Skip();
                }
            }

            binValue.TimeZone = TimeZone.CurrentTimeZone;
            if (parseState.HasNext())
            {
                throw new XmpException("Invalid date string, extra chars at end", 5);
            }

            return binValue;
        }

        public static string Render(IXmpDateTime dateTime)
        {
            return dateTime.Calendar.DateTime.ToString("s");
        }
    }
}
