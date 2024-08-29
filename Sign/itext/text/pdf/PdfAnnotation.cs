using Sign.itext.awt.geom;
using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.SystemItext.util.collections;
using System.Drawing.Drawing2D;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfAnnotation : PdfDictionary, IAccessibleElement
    {
        public class PdfImportedLink
        {
            private float llx;

            private float lly;

            private float urx;

            private float ury;

            private Dictionary<PdfName, PdfObject> parameters;

            private PdfArray destination;

            private int newPage;

            private PdfArray rect;

            internal PdfImportedLink(PdfDictionary annotation)
            {
                parameters = new Dictionary<PdfName, PdfObject>(annotation.hashMap);
                try
                {
                    if (parameters.ContainsKey(PdfName.DEST))
                    {
                        destination = (PdfArray)parameters[PdfName.DEST];
                    }

                    parameters.Remove(PdfName.DEST);
                }
                catch (Exception)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("you.have.to.consolidate.the.named.destinations.of.your.reader"));
                }

                if (destination != null)
                {
                    destination = new PdfArray(destination);
                }

                PdfArray pdfArray = (PdfArray)parameters[PdfName.RECT];
                parameters.Remove(PdfName.RECT);
                llx = pdfArray.GetAsNumber(0).FloatValue;
                lly = pdfArray.GetAsNumber(1).FloatValue;
                urx = pdfArray.GetAsNumber(2).FloatValue;
                ury = pdfArray.GetAsNumber(3).FloatValue;
                rect = new PdfArray(pdfArray);
            }

            public virtual IDictionary<PdfName, PdfObject> GetParameters()
            {
                return new Dictionary<PdfName, PdfObject>(parameters);
            }

            public virtual PdfArray GetRect()
            {
                return new PdfArray(rect);
            }

            public virtual bool IsInternal()
            {
                return destination != null;
            }

            public virtual int GetDestinationPage()
            {
                if (!IsInternal())
                {
                    return 0;
                }

                PRIndirectReference pRIndirectReference = (PRIndirectReference)destination.GetAsIndirectObject(0);
                PdfReader reader = pRIndirectReference.Reader;
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PRIndirectReference pageOrigRef = reader.GetPageOrigRef(i);
                    if (pageOrigRef.Generation == pRIndirectReference.Generation && pageOrigRef.Number == pRIndirectReference.Number)
                    {
                        return i;
                    }
                }

                throw new ArgumentException(MessageLocalization.GetComposedMessage("page.not.found"));
            }

            public virtual void SetDestinationPage(int newPage)
            {
                if (!IsInternal())
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("cannot.change.destination.of.external.link"));
                }

                this.newPage = newPage;
            }

            public virtual void TransformDestination(float a, float b, float c, float d, float e, float f)
            {
                if (!IsInternal())
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("cannot.change.destination.of.external.link"));
                }

                if (destination.GetAsName(1).Equals(PdfName.XYZ))
                {
                    float floatValue = destination.GetAsNumber(2).FloatValue;
                    float floatValue2 = destination.GetAsNumber(3).FloatValue;
                    float value = floatValue * a + floatValue2 * c + e;
                    float value2 = floatValue * b + floatValue2 * d + f;
                    destination.ArrayList[2] = new PdfNumber(value);
                    destination.ArrayList[3] = new PdfNumber(value2);
                }
            }

            public virtual void TransformRect(float a, float b, float c, float d, float e, float f)
            {
                float num = llx * a + lly * c + e;
                float num2 = llx * b + lly * d + f;
                llx = num;
                lly = num2;
                num = urx * a + ury * c + e;
                num2 = urx * b + ury * d + f;
                urx = num;
                ury = num2;
            }

            public virtual PdfAnnotation CreateAnnotation(PdfWriter writer)
            {
                PdfAnnotation pdfAnnotation = writer.CreateAnnotation(new Rectangle(llx, lly, urx, ury), null);
                if (newPage != 0)
                {
                    PdfIndirectReference pageReference = writer.GetPageReference(newPage);
                    destination.Set(0, pageReference);
                }

                if (destination != null)
                {
                    pdfAnnotation.Put(PdfName.DEST, destination);
                }

                foreach (PdfName key in parameters.Keys)
                {
                    pdfAnnotation.hashMap[key] = parameters[key];
                }

                return pdfAnnotation;
            }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder("Imported link: location [");
                stringBuilder.Append(llx);
                stringBuilder.Append(' ');
                stringBuilder.Append(lly);
                stringBuilder.Append(' ');
                stringBuilder.Append(urx);
                stringBuilder.Append(' ');
                stringBuilder.Append(ury);
                stringBuilder.Append("] destination ");
                stringBuilder.Append(destination);
                stringBuilder.Append(" parameters ");
                stringBuilder.Append(parameters);
                return stringBuilder.ToString();
            }
        }

        public static readonly PdfName HIGHLIGHT_NONE = PdfName.N;

        public static readonly PdfName HIGHLIGHT_INVERT = PdfName.I;

        public static readonly PdfName HIGHLIGHT_OUTLINE = PdfName.O;

        public static readonly PdfName HIGHLIGHT_PUSH = PdfName.P;

        public static readonly PdfName HIGHLIGHT_TOGGLE = PdfName.T;

        public const int FLAGS_INVISIBLE = 1;

        public const int FLAGS_HIDDEN = 2;

        public const int FLAGS_PRINT = 4;

        public const int FLAGS_NOZOOM = 8;

        public const int FLAGS_NOROTATE = 16;

        public const int FLAGS_NOVIEW = 32;

        public const int FLAGS_READONLY = 64;

        public const int FLAGS_LOCKED = 128;

        public const int FLAGS_TOGGLENOVIEW = 256;

        public const int FLAGS_LOCKEDCONTENTS = 512;

        public static readonly PdfName APPEARANCE_NORMAL = PdfName.N;

        public static readonly PdfName APPEARANCE_ROLLOVER = PdfName.R;

        public static readonly PdfName APPEARANCE_DOWN = PdfName.D;

        public static readonly PdfName AA_ENTER = PdfName.E;

        public static readonly PdfName AA_EXIT = PdfName.X;

        public static readonly PdfName AA_DOWN = PdfName.D;

        public static readonly PdfName AA_UP = PdfName.U;

        public static readonly PdfName AA_FOCUS = PdfName.FO;

        public static readonly PdfName AA_BLUR = PdfName.BL;

        public static readonly PdfName AA_JS_KEY = PdfName.K;

        public static readonly PdfName AA_JS_FORMAT = PdfName.F;

        public static readonly PdfName AA_JS_CHANGE = PdfName.V;

        public static readonly PdfName AA_JS_OTHER_CHANGE = PdfName.C;

        public const int MARKUP_HIGHLIGHT = 0;

        public const int MARKUP_UNDERLINE = 1;

        public const int MARKUP_STRIKEOUT = 2;

        public const int MARKUP_SQUIGGLY = 3;

        protected internal PdfWriter writer;

        protected internal PdfIndirectReference reference;

        protected internal HashSet2<PdfTemplate> templates;

        protected internal bool form;

        protected internal bool annotation = true;

        protected internal bool used;

        private int placeInPage = -1;

        protected PdfName role;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        private AccessibleElementId id;

        public virtual PdfIndirectReference IndirectReference
        {
            get
            {
                if (reference == null)
                {
                    reference = writer.PdfIndirectReference;
                }

                return reference;
            }
        }

        public virtual PdfContentByte DefaultAppearanceString
        {
            set
            {
                byte[] array = value.InternalBuffer.ToByteArray();
                int num = array.Length;
                for (int i = 0; i < num; i++)
                {
                    if (array[i] == 10)
                    {
                        array[i] = 32;
                    }
                }

                Put(PdfName.DA, new PdfString(array));
            }
        }

        public virtual int Flags
        {
            set
            {
                if (value == 0)
                {
                    Remove(PdfName.F);
                }
                else
                {
                    Put(PdfName.F, new PdfNumber(value));
                }
            }
        }

        public virtual PdfBorderArray Border
        {
            set
            {
                Put(PdfName.BORDER, value);
            }
        }

        public virtual PdfBorderDictionary BorderStyle
        {
            set
            {
                Put(PdfName.BS, value);
            }
        }

        public virtual string AppearanceState
        {
            set
            {
                if (value == null)
                {
                    Remove(PdfName.AS);
                }
                else
                {
                    Put(PdfName.AS, new PdfName(value));
                }
            }
        }

        public virtual BaseColor Color
        {
            set
            {
                Put(PdfName.C, new PdfColor(value));
            }
        }

        public virtual string Title
        {
            set
            {
                if (value == null)
                {
                    Remove(PdfName.T);
                }
                else
                {
                    Put(PdfName.T, new PdfString(value, "UnicodeBig"));
                }
            }
        }

        public virtual PdfAnnotation Popup
        {
            set
            {
                Put(PdfName.POPUP, value.IndirectReference);
                value.Put(PdfName.PARENT, IndirectReference);
            }
        }

        public virtual PdfAction Action
        {
            set
            {
                Put(PdfName.A, value);
            }
        }

        [Obsolete("Use GetTemplates() instead")]
        public virtual Dictionary<PdfTemplate, object> Templates
        {
            get
            {
                if (templates == null)
                {
                    return null;
                }

                return templates.InternalSet;
            }
        }

        public virtual int Page
        {
            set
            {
                Put(PdfName.P, writer.GetPageReference(value));
            }
        }

        public virtual int PlaceInPage
        {
            get
            {
                return placeInPage;
            }
            set
            {
                placeInPage = value;
            }
        }

        public virtual int Rotate
        {
            set
            {
                Put(PdfName.ROTATE, new PdfNumber(value));
            }
        }

        internal PdfDictionary MK
        {
            get
            {
                PdfDictionary pdfDictionary = (PdfDictionary)Get(PdfName.MK);
                if (pdfDictionary == null)
                {
                    pdfDictionary = new PdfDictionary();
                    Put(PdfName.MK, pdfDictionary);
                }

                return pdfDictionary;
            }
        }

        public virtual int MKRotation
        {
            set
            {
                MK.Put(PdfName.R, new PdfNumber(value));
            }
        }

        public virtual BaseColor MKBorderColor
        {
            set
            {
                if (value == null)
                {
                    MK.Remove(PdfName.BC);
                }
                else
                {
                    MK.Put(PdfName.BC, GetMKColor(value));
                }
            }
        }

        public virtual BaseColor MKBackgroundColor
        {
            set
            {
                if (value == null)
                {
                    MK.Remove(PdfName.BG);
                }
                else
                {
                    MK.Put(PdfName.BG, GetMKColor(value));
                }
            }
        }

        public virtual string MKNormalCaption
        {
            set
            {
                MK.Put(PdfName.CA, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual string MKRolloverCaption
        {
            set
            {
                MK.Put(PdfName.RC, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual string MKAlternateCaption
        {
            set
            {
                MK.Put(PdfName.AC, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual PdfTemplate MKNormalIcon
        {
            set
            {
                MK.Put(PdfName.I, value.IndirectReference);
            }
        }

        public virtual PdfTemplate MKRolloverIcon
        {
            set
            {
                MK.Put(PdfName.RI, value.IndirectReference);
            }
        }

        public virtual PdfTemplate MKAlternateIcon
        {
            set
            {
                MK.Put(PdfName.IX, value.IndirectReference);
            }
        }

        public virtual int MKTextPosition
        {
            set
            {
                MK.Put(PdfName.TP, new PdfNumber(value));
            }
        }

        public virtual IPdfOCG Layer
        {
            set
            {
                Put(PdfName.OC, value.Ref);
            }
        }

        public virtual string Name
        {
            set
            {
                Put(PdfName.NM, new PdfString(value));
            }
        }

        public virtual PdfName Role
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        public virtual AccessibleElementId ID
        {
            get
            {
                if (id == null)
                {
                    id = new AccessibleElementId();
                }

                return id;
            }
            set
            {
                id = value;
            }
        }

        public virtual bool IsInline => false;

        public PdfAnnotation(PdfWriter writer, Rectangle rect)
        {
            this.writer = writer;
            if (rect != null)
            {
                Put(PdfName.RECT, new PdfRectangle(rect));
            }
        }

        public PdfAnnotation(PdfWriter writer, float llx, float lly, float urx, float ury, PdfString title, PdfString content)
        {
            this.writer = writer;
            Put(PdfName.SUBTYPE, PdfName.TEXT);
            Put(PdfName.T, title);
            Put(PdfName.RECT, new PdfRectangle(llx, lly, urx, ury));
            Put(PdfName.CONTENTS, content);
        }

        public PdfAnnotation(PdfWriter writer, float llx, float lly, float urx, float ury, PdfAction action)
        {
            this.writer = writer;
            Put(PdfName.SUBTYPE, PdfName.LINK);
            Put(PdfName.RECT, new PdfRectangle(llx, lly, urx, ury));
            Put(PdfName.A, action);
            Put(PdfName.BORDER, new PdfBorderArray(0f, 0f, 0f));
            Put(PdfName.C, new PdfColor(0, 0, 255));
        }

        public static PdfAnnotation CreateScreen(PdfWriter writer, Rectangle rect, string clipTitle, PdfFileSpecification fs, string mimeType, bool playOnDisplay)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.SCREEN);
            pdfAnnotation.Put(PdfName.F, new PdfNumber(4));
            pdfAnnotation.Put(PdfName.TYPE, PdfName.ANNOT);
            pdfAnnotation.SetPage();
            PdfIndirectReference indirectReference = pdfAnnotation.IndirectReference;
            PdfAction objecta = PdfAction.Rendition(clipTitle, fs, mimeType, indirectReference);
            PdfIndirectReference indirectReference2 = writer.AddToBody(objecta).IndirectReference;
            if (playOnDisplay)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(new PdfName("PV"), indirectReference2);
                pdfAnnotation.Put(PdfName.AA, pdfDictionary);
            }

            pdfAnnotation.Put(PdfName.A, indirectReference2);
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateText(PdfWriter writer, Rectangle rect, string title, string contents, bool open, string icon)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.TEXT);
            if (title != null)
            {
                pdfAnnotation.Put(PdfName.T, new PdfString(title, "UnicodeBig"));
            }

            if (contents != null)
            {
                pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            }

            if (open)
            {
                pdfAnnotation.Put(PdfName.OPEN, PdfBoolean.PDFTRUE);
            }

            if (icon != null)
            {
                pdfAnnotation.Put(PdfName.NAME, new PdfName(icon));
            }

            return pdfAnnotation;
        }

        protected static PdfAnnotation CreateLink(PdfWriter writer, Rectangle rect, PdfName highlight)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.LINK);
            if (!highlight.Equals(HIGHLIGHT_INVERT))
            {
                pdfAnnotation.Put(PdfName.H, highlight);
            }

            return pdfAnnotation;
        }

        public static PdfAnnotation CreateLink(PdfWriter writer, Rectangle rect, PdfName highlight, PdfAction action)
        {
            PdfAnnotation pdfAnnotation = CreateLink(writer, rect, highlight);
            pdfAnnotation.PutEx(PdfName.A, action);
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateLink(PdfWriter writer, Rectangle rect, PdfName highlight, string namedDestination)
        {
            PdfAnnotation pdfAnnotation = CreateLink(writer, rect, highlight);
            pdfAnnotation.Put(PdfName.DEST, new PdfString(namedDestination, "UnicodeBig"));
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateLink(PdfWriter writer, Rectangle rect, PdfName highlight, int page, PdfDestination dest)
        {
            PdfAnnotation pdfAnnotation = CreateLink(writer, rect, highlight);
            PdfIndirectReference pageReference = writer.GetPageReference(page);
            dest.AddPage(pageReference);
            pdfAnnotation.Put(PdfName.DEST, dest);
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateFreeText(PdfWriter writer, Rectangle rect, string contents, PdfContentByte defaultAppearance)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.FREETEXT);
            pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            pdfAnnotation.DefaultAppearanceString = defaultAppearance;
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateLine(PdfWriter writer, Rectangle rect, string contents, float x1, float y1, float x2, float y2)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.LINE);
            pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            pdfAnnotation.Put(value: new PdfArray(new PdfNumber(x1))
            {
                new PdfNumber(y1),
                new PdfNumber(x2),
                new PdfNumber(y2)
            }, key: PdfName.L);
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateSquareCircle(PdfWriter writer, Rectangle rect, string contents, bool square)
        {
            PdfAnnotation pdfAnnotation = ((!square) ? writer.CreateAnnotation(rect, PdfName.CIRCLE) : writer.CreateAnnotation(rect, PdfName.SQUARE));
            pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateMarkup(PdfWriter writer, Rectangle rect, string contents, int type, float[] quadPoints)
        {
            PdfName subtype = PdfName.HIGHLIGHT;
            switch (type)
            {
                case 1:
                    subtype = PdfName.UNDERLINE;
                    break;
                case 2:
                    subtype = PdfName.STRIKEOUT;
                    break;
                case 3:
                    subtype = PdfName.SQUIGGLY;
                    break;
            }

            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, subtype);
            pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < quadPoints.Length; i++)
            {
                pdfArray.Add(new PdfNumber(quadPoints[i]));
            }

            pdfAnnotation.Put(PdfName.QUADPOINTS, pdfArray);
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateStamp(PdfWriter writer, Rectangle rect, string contents, string name)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.STAMP);
            pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            pdfAnnotation.Put(PdfName.NAME, new PdfName(name));
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateInk(PdfWriter writer, Rectangle rect, string contents, float[][] inkList)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.INK);
            pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < inkList.Length; i++)
            {
                PdfArray pdfArray2 = new PdfArray();
                float[] array = inkList[i];
                for (int j = 0; j < array.Length; j++)
                {
                    pdfArray2.Add(new PdfNumber(array[j]));
                }

                pdfArray.Add(pdfArray2);
            }

            pdfAnnotation.Put(PdfName.INKLIST, pdfArray);
            return pdfAnnotation;
        }

        public static PdfAnnotation CreateFileAttachment(PdfWriter writer, Rectangle rect, string contents, byte[] fileStore, string file, string fileDisplay)
        {
            return CreateFileAttachment(writer, rect, contents, PdfFileSpecification.FileEmbedded(writer, file, fileDisplay, fileStore));
        }

        public static PdfAnnotation CreateFileAttachment(PdfWriter writer, Rectangle rect, string contents, PdfFileSpecification fs)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.FILEATTACHMENT);
            if (contents != null)
            {
                pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            }

            pdfAnnotation.Put(PdfName.FS, fs.Reference);
            return pdfAnnotation;
        }

        public static PdfAnnotation CreatePopup(PdfWriter writer, Rectangle rect, string contents, bool open)
        {
            PdfAnnotation pdfAnnotation = writer.CreateAnnotation(rect, PdfName.POPUP);
            if (contents != null)
            {
                pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            }

            if (open)
            {
                pdfAnnotation.Put(PdfName.OPEN, PdfBoolean.PDFTRUE);
            }

            return pdfAnnotation;
        }

        public static PdfAnnotation CreatePolygonPolyline(PdfWriter writer, Rectangle rect, string contents, bool polygon, PdfArray vertices)
        {
            PdfAnnotation pdfAnnotation = null;
            pdfAnnotation = ((!polygon) ? writer.CreateAnnotation(rect, PdfName.POLYLINE) : writer.CreateAnnotation(rect, PdfName.POLYGON));
            pdfAnnotation.Put(PdfName.CONTENTS, new PdfString(contents, "UnicodeBig"));
            pdfAnnotation.Put(PdfName.VERTICES, new PdfArray(vertices));
            return pdfAnnotation;
        }

        public virtual void SetHighlighting(PdfName highlight)
        {
            if (highlight.Equals(HIGHLIGHT_INVERT))
            {
                Remove(PdfName.H);
            }
            else
            {
                Put(PdfName.H, highlight);
            }
        }

        public virtual void SetAppearance(PdfName ap, PdfTemplate template)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)Get(PdfName.AP);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
            }

            pdfDictionary.Put(ap, template.IndirectReference);
            Put(PdfName.AP, pdfDictionary);
            if (form)
            {
                if (templates == null)
                {
                    templates = new HashSet2<PdfTemplate>();
                }

                templates.Add(template);
            }
        }

        public virtual void SetAppearance(PdfName ap, string state, PdfTemplate template)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)Get(PdfName.AP);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
            }

            PdfObject pdfObject = pdfDictionary.Get(ap);
            PdfDictionary pdfDictionary2 = ((pdfObject == null || !pdfObject.IsDictionary()) ? new PdfDictionary() : ((PdfDictionary)pdfObject));
            pdfDictionary2.Put(new PdfName(state), template.IndirectReference);
            pdfDictionary.Put(ap, pdfDictionary2);
            Put(PdfName.AP, pdfDictionary);
            if (form)
            {
                if (templates == null)
                {
                    templates = new HashSet2<PdfTemplate>();
                }

                templates.Add(template);
            }
        }

        public virtual void SetAdditionalActions(PdfName key, PdfAction action)
        {
            PdfObject pdfObject = Get(PdfName.AA);
            PdfDictionary pdfDictionary = ((pdfObject == null || !pdfObject.IsDictionary()) ? new PdfDictionary() : ((PdfDictionary)pdfObject));
            pdfDictionary.Put(key, action);
            Put(PdfName.AA, pdfDictionary);
        }

        internal virtual bool IsUsed()
        {
            return used;
        }

        public virtual void SetUsed()
        {
            used = true;
        }

        public virtual HashSet2<PdfTemplate> GetTemplates()
        {
            return templates;
        }

        public virtual bool IsForm()
        {
            return form;
        }

        public virtual bool IsAnnotation()
        {
            return annotation;
        }

        public virtual void SetPage()
        {
            Put(PdfName.P, writer.CurrentPage);
        }

        public static PdfAnnotation ShallowDuplicate(PdfAnnotation annot)
        {
            PdfAnnotation pdfAnnotation;
            if (annot.IsForm())
            {
                pdfAnnotation = new PdfFormField(annot.writer);
                PdfFormField obj = (PdfFormField)pdfAnnotation;
                PdfFormField pdfFormField = (PdfFormField)annot;
                obj.parent = pdfFormField.parent;
                obj.kids = pdfFormField.kids;
            }
            else
            {
                pdfAnnotation = annot.writer.CreateAnnotation(null, (PdfName)annot.Get(PdfName.SUBTYPE));
            }

            pdfAnnotation.Merge(annot);
            pdfAnnotation.form = annot.form;
            pdfAnnotation.annotation = annot.annotation;
            pdfAnnotation.templates = annot.templates;
            return pdfAnnotation;
        }

        public static PdfArray GetMKColor(BaseColor color)
        {
            PdfArray pdfArray = new PdfArray();
            switch (ExtendedColor.GetType(color))
            {
                case 1:
                    pdfArray.Add(new PdfNumber(((GrayColor)color).Gray));
                    break;
                case 2:
                    {
                        CMYKColor cMYKColor = (CMYKColor)color;
                        pdfArray.Add(new PdfNumber(cMYKColor.Cyan));
                        pdfArray.Add(new PdfNumber(cMYKColor.Magenta));
                        pdfArray.Add(new PdfNumber(cMYKColor.Yellow));
                        pdfArray.Add(new PdfNumber(cMYKColor.Black));
                        break;
                    }
                case 3:
                case 4:
                case 5:
                    throw new Exception(MessageLocalization.GetComposedMessage("separations.patterns.and.shadings.are.not.allowed.in.mk.dictionary"));
                default:
                    pdfArray.Add(new PdfNumber((float)color.R / 255f));
                    pdfArray.Add(new PdfNumber((float)color.G / 255f));
                    pdfArray.Add(new PdfNumber((float)color.B / 255f));
                    break;
            }

            return pdfArray;
        }

        public virtual void SetMKIconFit(PdfName scale, PdfName scalingType, float leftoverLeft, float leftoverBottom, bool fitInBounds)
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            if (!scale.Equals(PdfName.A))
            {
                pdfDictionary.Put(PdfName.SW, scale);
            }

            if (!scalingType.Equals(PdfName.P))
            {
                pdfDictionary.Put(PdfName.S, scalingType);
            }

            if (leftoverLeft != 0.5f || leftoverBottom != 0.5f)
            {
                PdfArray pdfArray = new PdfArray(new PdfNumber(leftoverLeft));
                pdfArray.Add(new PdfNumber(leftoverBottom));
                pdfDictionary.Put(PdfName.A, pdfArray);
            }

            if (fitInBounds)
            {
                pdfDictionary.Put(PdfName.FB, PdfBoolean.PDFTRUE);
            }

            MK.Put(PdfName.IF, pdfDictionary);
        }

        public virtual void ApplyCTM(AffineTransform ctm)
        {
            PdfArray asArray = GetAsArray(PdfName.RECT);
            if (asArray != null)
            {
                Put(value: ((asArray.Size != 4) ? new PdfRectangle(asArray.GetAsNumber(0).FloatValue, asArray.GetAsNumber(1).FloatValue) : new PdfRectangle(asArray.GetAsNumber(0).FloatValue, asArray.GetAsNumber(1).FloatValue, asArray.GetAsNumber(2).FloatValue, asArray.GetAsNumber(3).FloatValue)).Transform(ctm), key: PdfName.RECT);
            }
        }

        [Obsolete]
        public void ApplyCTM(Matrix ctm)
        {
            PdfArray asArray = GetAsArray(PdfName.RECT);
            if (asArray != null)
            {
                Put(value: ((asArray.Size != 4) ? new PdfRectangle(asArray.GetAsNumber(0).FloatValue, asArray.GetAsNumber(1).FloatValue) : new PdfRectangle(asArray.GetAsNumber(0).FloatValue, asArray.GetAsNumber(1).FloatValue, asArray.GetAsNumber(2).FloatValue, asArray.GetAsNumber(3).FloatValue)).Transform(ctm), key: PdfName.RECT);
            }
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 13, this);
            base.ToPdf(writer, os);
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            if (accessibleAttributes != null)
            {
                if (!accessibleAttributes.ContainsKey(key))
                {
                    return null;
                }

                return accessibleAttributes[key];
            }

            return null;
        }

        public virtual void SetAccessibleAttribute(PdfName key, PdfObject value)
        {
            if (accessibleAttributes == null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            }

            accessibleAttributes[key] = value;
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleAttributes()
        {
            return accessibleAttributes;
        }
    }
}
