using System.Collections;
using System.Text;

namespace Sign.itext.xml.xmp.options
{
    public abstract class XmpOptions
    {
        private IDictionary _optionNames;

        private uint _options;

        public virtual uint Options
        {
            get
            {
                return _options;
            }
            set
            {
                AssertOptionsValid(value);
                _options = value;
            }
        }

        public virtual string OptionsString
        {
            get
            {
                if (_options != 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    uint num = _options;
                    while (num != 0)
                    {
                        uint num2 = num & (num - 1);
                        uint option = num ^ num2;
                        string optionName = GetOptionName(option);
                        stringBuilder.Append(optionName);
                        if (num2 != 0)
                        {
                            stringBuilder.Append(" | ");
                        }

                        num = num2;
                    }

                    return stringBuilder.ToString();
                }

                return "<none>";
            }
        }

        protected internal abstract uint ValidOptions { get; }

        protected XmpOptions()
        {
        }

        public XmpOptions(uint options)
        {
            AssertOptionsValid(options);
            Options = options;
        }

        public virtual void Clear()
        {
            _options = 0u;
        }

        public virtual bool IsExactly(uint optionBits)
        {
            return Options == optionBits;
        }

        public virtual bool ContainsAllOptions(uint optionBits)
        {
            return (Options & optionBits) == optionBits;
        }

        public virtual bool ContainsOneOf(uint optionBits)
        {
            return (Options & optionBits) != 0;
        }

        protected internal virtual bool GetOption(uint optionBit)
        {
            return (_options & optionBit) != 0;
        }

        public virtual void SetOption(uint optionBits, bool value)
        {
            _options = (value ? (_options | optionBits) : (_options & ~optionBits));
        }

        public override bool Equals(object obj)
        {
            return Options == ((XmpOptions)obj).Options;
        }

        public override int GetHashCode()
        {
            return (int)Options;
        }

        public override string ToString()
        {
            return "0x" + _options.ToString("X");
        }

        protected internal abstract string DefineOptionName(uint option);

        protected internal virtual void AssertConsistency(uint options)
        {
        }

        private void AssertOptionsValid(uint options)
        {
            uint num = options & ~ValidOptions;
            if (num == 0)
            {
                AssertConsistency(options);
                return;
            }

            throw new XmpException("The option bit(s) 0x" + num.ToString("X") + " are invalid!", 103);
        }

        private string GetOptionName(uint option)
        {
            IDictionary dictionary = ProcureOptionNames();
            uint? num = option;
            string text = (string)dictionary[num];
            if (text == null)
            {
                text = DefineOptionName(option);
                if (text != null)
                {
                    dictionary[num] = text;
                }
                else
                {
                    text = "<option name not defined>";
                }
            }

            return text;
        }

        private IDictionary ProcureOptionNames()
        {
            _optionNames = _optionNames ?? new Hashtable();
            return _optionNames;
        }
    }
}
