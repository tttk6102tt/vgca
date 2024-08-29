using Sign.itext.pdf;
using Sign.itext.text;

namespace Sign.itext
{
    public abstract class DocWriter : IDocListener, IElementListener, IDisposable
    {
        public const byte NEWLINE = 10;

        public const byte TAB = 9;

        public const byte LT = 60;

        public const byte SPACE = 32;

        public const byte EQUALS = 61;

        public const byte QUOTE = 34;

        public const byte GT = 62;

        public const byte FORWARD = 47;

        protected Rectangle pageSize;

        protected Document document;

        protected OutputStreamCounter os;

        protected bool open;

        protected bool pause;

        protected bool closeStream = true;

        public virtual int PageCount
        {
            set
            {
            }
        }

        public virtual bool CloseStream
        {
            get
            {
                return closeStream;
            }
            set
            {
                closeStream = value;
            }
        }

        protected DocWriter()
        {
        }

        protected DocWriter(Document document, Stream os)
        {
            this.document = document;
            this.os = new OutputStreamCounter(os);
        }

        public virtual bool Add(IElement element)
        {
            return false;
        }

        public virtual void Open()
        {
            open = true;
        }

        public virtual bool SetPageSize(Rectangle pageSize)
        {
            this.pageSize = pageSize;
            return true;
        }

        public virtual bool SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom)
        {
            return false;
        }

        public virtual bool NewPage()
        {
            if (!open)
            {
                return false;
            }

            return true;
        }

        public virtual void ResetPageCount()
        {
        }

        public virtual void Close()
        {
            open = false;
            os.Flush();
            if (closeStream)
            {
                os.Close();
            }
        }

        public static byte[] GetISOBytes(string text)
        {
            if (text == null)
            {
                return null;
            }

            int length = text.Length;
            byte[] array = new byte[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = (byte)text[i];
            }

            return array;
        }

        public virtual void Pause()
        {
            pause = true;
        }

        public virtual bool IsPaused()
        {
            return pause;
        }

        public virtual void Resume()
        {
            pause = false;
        }

        public virtual void Flush()
        {
            os.Flush();
        }

        protected virtual void Write(string str)
        {
            byte[] iSOBytes = GetISOBytes(str);
            os.Write(iSOBytes, 0, iSOBytes.Length);
        }

        protected virtual void AddTabs(int indent)
        {
            os.WriteByte(10);
            for (int i = 0; i < indent; i++)
            {
                os.WriteByte(9);
            }
        }

        protected virtual void Write(string key, string value)
        {
            os.WriteByte(32);
            Write(key);
            os.WriteByte(61);
            os.WriteByte(34);
            Write(value);
            os.WriteByte(34);
        }

        protected virtual void WriteStart(string tag)
        {
            os.WriteByte(60);
            Write(tag);
        }

        protected virtual void WriteEnd(string tag)
        {
            os.WriteByte(60);
            os.WriteByte(47);
            Write(tag);
            os.WriteByte(62);
        }

        protected virtual void WriteEnd()
        {
            os.WriteByte(32);
            os.WriteByte(47);
            os.WriteByte(62);
        }

        protected virtual bool WriteMarkupAttributes(Sign.SystemItext.util.Properties markup)
        {
            if (markup == null)
            {
                return false;
            }

            foreach (string key in markup.Keys)
            {
                Write(key, markup[key]);
            }

            markup.Clear();
            return true;
        }

        public virtual bool SetMarginMirroring(bool marginMirroring)
        {
            return false;
        }

        public virtual bool SetMarginMirroringTopBottom(bool MarginMirroring)
        {
            return false;
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
