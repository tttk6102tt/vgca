namespace Sign.itext.text
{
    public class Annotation : IElement
    {
        public const int TEXT = 0;

        public const int URL_NET = 1;

        public const int URL_AS_STRING = 2;

        public const int FILE_DEST = 3;

        public const int FILE_PAGE = 4;

        public const int NAMED_DEST = 5;

        public const int LAUNCH = 6;

        public const int SCREEN = 7;

        public const string TITLE = "title";

        public const string CONTENT = "content";

        public const string URL = "url";

        public const string FILE = "file";

        public const string DESTINATION = "destination";

        public const string PAGE = "page";

        public const string NAMED = "named";

        public const string APPLICATION = "application";

        public const string PARAMETERS = "parameters";

        public const string OPERATION = "operation";

        public const string DEFAULTDIR = "defaultdir";

        public const string LLX = "llx";

        public const string LLY = "lly";

        public const string URX = "urx";

        public const string URY = "ury";

        public const string MIMETYPE = "mime";

        protected int annotationtype;

        protected Dictionary<string, object> annotationAttributes = new Dictionary<string, object>();

        private float llx = float.NaN;

        private float lly = float.NaN;

        private float urx = float.NaN;

        private float ury = float.NaN;

        public virtual int Type => 29;

        public virtual IList<Chunk> Chunks => new List<Chunk>();

        public virtual int AnnotationType => annotationtype;

        public virtual string Title
        {
            get
            {
                if (annotationAttributes.ContainsKey("title"))
                {
                    return (string)annotationAttributes["title"];
                }

                return "";
            }
        }

        public virtual string Content
        {
            get
            {
                if (annotationAttributes.ContainsKey("content"))
                {
                    return (string)annotationAttributes["content"];
                }

                return "";
            }
        }

        public virtual Dictionary<string, object> Attributes => annotationAttributes;

        private Annotation(float llx, float lly, float urx, float ury)
        {
            this.llx = llx;
            this.lly = lly;
            this.urx = urx;
            this.ury = ury;
        }

        public Annotation(Annotation an)
        {
            annotationtype = an.annotationtype;
            annotationAttributes = an.annotationAttributes;
            llx = an.llx;
            lly = an.lly;
            urx = an.urx;
            ury = an.ury;
        }

        public Annotation(string title, string text)
        {
            annotationtype = 0;
            annotationAttributes["title"] = title;
            annotationAttributes["content"] = text;
        }

        public Annotation(string title, string text, float llx, float lly, float urx, float ury)
            : this(llx, lly, urx, ury)
        {
            annotationtype = 0;
            annotationAttributes["title"] = title;
            annotationAttributes["content"] = text;
        }

        public Annotation(float llx, float lly, float urx, float ury, Uri url)
            : this(llx, lly, urx, ury)
        {
            annotationtype = 1;
            annotationAttributes["url"] = url;
        }

        public Annotation(float llx, float lly, float urx, float ury, string url)
            : this(llx, lly, urx, ury)
        {
            annotationtype = 2;
            annotationAttributes["file"] = url;
        }

        public Annotation(float llx, float lly, float urx, float ury, string file, string dest)
            : this(llx, lly, urx, ury)
        {
            annotationtype = 3;
            annotationAttributes["file"] = file;
            annotationAttributes["destination"] = dest;
        }

        public Annotation(float llx, float lly, float urx, float ury, string moviePath, string mimeType, bool showOnDisplay)
            : this(llx, lly, urx, ury)
        {
            annotationtype = 7;
            annotationAttributes["file"] = moviePath;
            annotationAttributes["mime"] = mimeType;
            annotationAttributes["parameters"] = new bool[2] { false, showOnDisplay };
        }

        public Annotation(float llx, float lly, float urx, float ury, string file, int page)
            : this(llx, lly, urx, ury)
        {
            annotationtype = 4;
            annotationAttributes["file"] = file;
            annotationAttributes["page"] = page;
        }

        public Annotation(float llx, float lly, float urx, float ury, int named)
            : this(llx, lly, urx, ury)
        {
            annotationtype = 5;
            annotationAttributes["named"] = named;
        }

        public Annotation(float llx, float lly, float urx, float ury, string application, string parameters, string operation, string defaultdir)
            : this(llx, lly, urx, ury)
        {
            annotationtype = 6;
            annotationAttributes["application"] = application;
            annotationAttributes["parameters"] = parameters;
            annotationAttributes["operation"] = operation;
            annotationAttributes["defaultdir"] = defaultdir;
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                return listener.Add(this);
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        public virtual void SetDimensions(float llx, float lly, float urx, float ury)
        {
            this.llx = llx;
            this.lly = lly;
            this.urx = urx;
            this.ury = ury;
        }

        public virtual float GetLlx()
        {
            return llx;
        }

        public virtual float GetLly()
        {
            return lly;
        }

        public virtual float GetUrx()
        {
            return urx;
        }

        public virtual float GetUry()
        {
            return ury;
        }

        public virtual float GetLlx(float def)
        {
            if (float.IsNaN(llx))
            {
                return def;
            }

            return llx;
        }

        public virtual float GetLly(float def)
        {
            if (float.IsNaN(lly))
            {
                return def;
            }

            return lly;
        }

        public virtual float GetUrx(float def)
        {
            if (float.IsNaN(urx))
            {
                return def;
            }

            return urx;
        }

        public virtual float GetUry(float def)
        {
            if (float.IsNaN(ury))
            {
                return def;
            }

            return ury;
        }

        public virtual bool IsContent()
        {
            return true;
        }

        public virtual bool IsNestable()
        {
            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
