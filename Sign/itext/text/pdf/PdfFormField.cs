using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfFormField : PdfAnnotation
    {
        public const int FF_READ_ONLY = 1;

        public const int FF_REQUIRED = 2;

        public const int FF_NO_EXPORT = 4;

        public const int FF_NO_TOGGLE_TO_OFF = 16384;

        public const int FF_RADIO = 32768;

        public const int FF_PUSHBUTTON = 65536;

        public const int FF_MULTILINE = 4096;

        public const int FF_PASSWORD = 8192;

        public const int FF_COMBO = 131072;

        public const int FF_EDIT = 262144;

        public const int FF_FILESELECT = 1048576;

        public const int FF_MULTISELECT = 2097152;

        public const int FF_DONOTSPELLCHECK = 4194304;

        public const int FF_DONOTSCROLL = 8388608;

        public const int FF_COMB = 16777216;

        public const int FF_RADIOSINUNISON = 33554432;

        public const int FF_RICHTEXT = 33554432;

        public const int Q_LEFT = 0;

        public const int Q_CENTER = 1;

        public const int Q_RIGHT = 2;

        public const int MK_NO_ICON = 0;

        public const int MK_NO_CAPTION = 1;

        public const int MK_CAPTION_BELOW = 2;

        public const int MK_CAPTION_ABOVE = 3;

        public const int MK_CAPTION_RIGHT = 4;

        public const int MK_CAPTION_LEFT = 5;

        public const int MK_CAPTION_OVERLAID = 6;

        public static readonly PdfName IF_SCALE_ALWAYS = PdfName.A;

        public static readonly PdfName IF_SCALE_BIGGER = PdfName.B;

        public static readonly PdfName IF_SCALE_SMALLER = PdfName.S;

        public static readonly PdfName IF_SCALE_NEVER = PdfName.N;

        public static readonly PdfName IF_SCALE_ANAMORPHIC = PdfName.A;

        public static readonly PdfName IF_SCALE_PROPORTIONAL = PdfName.P;

        public const bool MULTILINE = true;

        public const bool SINGLELINE = false;

        public const bool PLAINTEXT = false;

        public const bool PASSWORD = true;

        public static PdfName[] mergeTarget = new PdfName[4]
        {
            PdfName.FONT,
            PdfName.XOBJECT,
            PdfName.COLORSPACE,
            PdfName.PATTERN
        };

        internal PdfFormField parent;

        internal List<PdfFormField> kids;

        public virtual int Button
        {
            set
            {
                Put(PdfName.FT, PdfName.BTN);
                if (value != 0)
                {
                    Put(PdfName.FF, new PdfNumber(value));
                }
            }
        }

        public virtual PdfFormField Parent => parent;

        public virtual List<PdfFormField> Kids => kids;

        public virtual string ValueAsString
        {
            set
            {
                Put(PdfName.V, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual string ValueAsName
        {
            set
            {
                Put(PdfName.V, new PdfName(value));
            }
        }

        public virtual PdfSignature ValueAsSig
        {
            set
            {
                Put(PdfName.V, value);
            }
        }

        public virtual string RichValue
        {
            set
            {
                Put(PdfName.RV, new PdfString(value));
            }
        }

        public virtual string DefaultValueAsString
        {
            set
            {
                Put(PdfName.DV, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual string DefaultValueAsName
        {
            set
            {
                Put(PdfName.DV, new PdfName(value));
            }
        }

        public virtual string FieldName
        {
            set
            {
                if (value != null)
                {
                    Put(PdfName.T, new PdfString(value, "UnicodeBig"));
                }
            }
        }

        public virtual string UserName
        {
            set
            {
                Put(PdfName.TU, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual string MappingName
        {
            set
            {
                Put(PdfName.TM, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual int Quadding
        {
            set
            {
                Put(PdfName.Q, new PdfNumber(value));
            }
        }

        public PdfFormField(PdfWriter writer, float llx, float lly, float urx, float ury, PdfAction action)
            : base(writer, llx, lly, urx, ury, action)
        {
            Put(PdfName.TYPE, PdfName.ANNOT);
            Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annotation = true;
        }

        internal PdfFormField(PdfWriter writer)
            : base(writer, null)
        {
            form = true;
            annotation = false;
            role = PdfName.FORM;
        }

        public virtual void SetWidget(Rectangle rect, PdfName highlight)
        {
            Put(PdfName.TYPE, PdfName.ANNOT);
            Put(PdfName.SUBTYPE, PdfName.WIDGET);
            Put(PdfName.RECT, new PdfRectangle(rect));
            annotation = true;
            if (highlight != null && !highlight.Equals(PdfAnnotation.HIGHLIGHT_INVERT))
            {
                Put(PdfName.H, highlight);
            }
        }

        public static PdfFormField CreateEmpty(PdfWriter writer)
        {
            return new PdfFormField(writer);
        }

        protected static PdfFormField CreateButton(PdfWriter writer, int flags)
        {
            return new PdfFormField(writer)
            {
                Button = flags
            };
        }

        public static PdfFormField CreatePushButton(PdfWriter writer)
        {
            return CreateButton(writer, 65536);
        }

        public static PdfFormField CreateCheckBox(PdfWriter writer)
        {
            return CreateButton(writer, 0);
        }

        public static PdfFormField CreateRadioButton(PdfWriter writer, bool noToggleToOff)
        {
            return CreateButton(writer, 32768 + (noToggleToOff ? 16384 : 0));
        }

        public static PdfFormField CreateTextField(PdfWriter writer, bool multiline, bool password, int maxLen)
        {
            PdfFormField pdfFormField = new PdfFormField(writer);
            pdfFormField.Put(PdfName.FT, PdfName.TX);
            int num = (multiline ? 4096 : 0);
            num += (password ? 8192 : 0);
            pdfFormField.Put(PdfName.FF, new PdfNumber(num));
            if (maxLen > 0)
            {
                pdfFormField.Put(PdfName.MAXLEN, new PdfNumber(maxLen));
            }

            return pdfFormField;
        }

        protected static PdfFormField CreateChoice(PdfWriter writer, int flags, PdfArray options, int topIndex)
        {
            PdfFormField pdfFormField = new PdfFormField(writer);
            pdfFormField.Put(PdfName.FT, PdfName.CH);
            pdfFormField.Put(PdfName.FF, new PdfNumber(flags));
            pdfFormField.Put(PdfName.OPT, options);
            if (topIndex > 0)
            {
                pdfFormField.Put(PdfName.TI, new PdfNumber(topIndex));
            }

            return pdfFormField;
        }

        public static PdfFormField CreateList(PdfWriter writer, string[] options, int topIndex)
        {
            return CreateChoice(writer, 0, ProcessOptions(options), topIndex);
        }

        public static PdfFormField CreateList(PdfWriter writer, string[,] options, int topIndex)
        {
            return CreateChoice(writer, 0, ProcessOptions(options), topIndex);
        }

        public static PdfFormField CreateCombo(PdfWriter writer, bool edit, string[] options, int topIndex)
        {
            return CreateChoice(writer, 131072 + (edit ? 262144 : 0), ProcessOptions(options), topIndex);
        }

        public static PdfFormField CreateCombo(PdfWriter writer, bool edit, string[,] options, int topIndex)
        {
            return CreateChoice(writer, 131072 + (edit ? 262144 : 0), ProcessOptions(options), topIndex);
        }

        protected static PdfArray ProcessOptions(string[] options)
        {
            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < options.Length; i++)
            {
                pdfArray.Add(new PdfString(options[i], "UnicodeBig"));
            }

            return pdfArray;
        }

        protected static PdfArray ProcessOptions(string[,] options)
        {
            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < options.GetLength(0); i++)
            {
                PdfArray pdfArray2 = new PdfArray(new PdfString(options[i, 0], "UnicodeBig"));
                pdfArray2.Add(new PdfString(options[i, 1], "UnicodeBig"));
                pdfArray.Add(pdfArray2);
            }

            return pdfArray;
        }

        public static PdfFormField CreateSignature(PdfWriter writer)
        {
            PdfFormField pdfFormField = new PdfFormField(writer);
            pdfFormField.Put(PdfName.FT, PdfName.SIG);
            return pdfFormField;
        }

        public virtual void AddKid(PdfFormField field)
        {
            field.parent = this;
            if (kids == null)
            {
                kids = new List<PdfFormField>();
            }

            kids.Add(field);
        }

        public virtual int SetFieldFlags(int flags)
        {
            int num = ((PdfNumber)Get(PdfName.FF))?.IntValue ?? 0;
            int value = num | flags;
            Put(PdfName.FF, new PdfNumber(value));
            return num;
        }

        internal static void MergeResources(PdfDictionary result, PdfDictionary source, PdfStamperImp writer)
        {
            PdfDictionary pdfDictionary = null;
            PdfDictionary pdfDictionary2 = null;
            PdfName pdfName = null;
            for (int i = 0; i < mergeTarget.Length; i++)
            {
                pdfName = mergeTarget[i];
                if ((pdfDictionary = source.GetAsDict(pdfName)) != null)
                {
                    if ((pdfDictionary2 = (PdfDictionary)PdfReader.GetPdfObject(result.Get(pdfName), result)) == null)
                    {
                        pdfDictionary2 = new PdfDictionary();
                    }

                    pdfDictionary2.MergeDifferent(pdfDictionary);
                    result.Put(pdfName, pdfDictionary2);
                    writer?.MarkUsed(pdfDictionary2);
                }
            }
        }

        internal static void MergeResources(PdfDictionary result, PdfDictionary source)
        {
            MergeResources(result, source, null);
        }

        public override void SetUsed()
        {
            used = true;
            if (parent != null)
            {
                Put(PdfName.PARENT, parent.IndirectReference);
            }

            if (kids != null)
            {
                PdfArray pdfArray = new PdfArray();
                for (int i = 0; i < kids.Count; i++)
                {
                    pdfArray.Add(kids[i].IndirectReference);
                }

                Put(PdfName.KIDS, pdfArray);
            }

            if (templates == null)
            {
                return;
            }

            PdfDictionary pdfDictionary = new PdfDictionary();
            foreach (PdfTemplate template in templates)
            {
                MergeResources(pdfDictionary, (PdfDictionary)template.Resources);
            }

            Put(PdfName.DR, pdfDictionary);
        }
    }
}
