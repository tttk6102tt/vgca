using Sign.itext.error_messages;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.collection;
using Sign.SystemItext.util;

namespace Sign.itext.pdf
{
    public class PdfAction : PdfDictionary
    {
        public const int FIRSTPAGE = 1;

        public const int PREVPAGE = 2;

        public const int NEXTPAGE = 3;

        public const int LASTPAGE = 4;

        public const int PRINTDIALOG = 5;

        public const int SUBMIT_EXCLUDE = 1;

        public const int SUBMIT_INCLUDE_NO_VALUE_FIELDS = 2;

        public const int SUBMIT_HTML_FORMAT = 4;

        public const int SUBMIT_HTML_GET = 8;

        public const int SUBMIT_COORDINATES = 16;

        public const int SUBMIT_XFDF = 32;

        public const int SUBMIT_INCLUDE_APPEND_SAVES = 64;

        public const int SUBMIT_INCLUDE_ANNOTATIONS = 128;

        public const int SUBMIT_PDF = 256;

        public const int SUBMIT_CANONICAL_FORMAT = 512;

        public const int SUBMIT_EXCL_NON_USER_ANNOTS = 1024;

        public const int SUBMIT_EXCL_F_KEY = 2048;

        public const int SUBMIT_EMBED_FORM = 8196;

        public const int RESET_EXCLUDE = 1;

        public PdfAction()
        {
        }

        public PdfAction(Uri url)
            : this(url.AbsoluteUri)
        {
        }

        public PdfAction(Uri url, bool isMap)
            : this(url.AbsoluteUri, isMap)
        {
        }

        public PdfAction(string url)
            : this(url, isMap: false)
        {
        }

        public PdfAction(string url, bool isMap)
        {
            Put(PdfName.S, PdfName.URI);
            Put(PdfName.URI, new PdfString(url));
            if (isMap)
            {
                Put(PdfName.ISMAP, PdfBoolean.PDFTRUE);
            }
        }

        internal PdfAction(PdfIndirectReference destination)
        {
            Put(PdfName.S, PdfName.GOTO);
            Put(PdfName.D, destination);
        }

        public PdfAction(string filename, string name)
        {
            Put(PdfName.S, PdfName.GOTOR);
            Put(PdfName.F, new PdfString(filename));
            Put(PdfName.D, new PdfString(name));
        }

        public PdfAction(string filename, int page)
        {
            Put(PdfName.S, PdfName.GOTOR);
            Put(PdfName.F, new PdfString(filename));
            Put(PdfName.D, new PdfLiteral("[" + (page - 1) + " /FitH 10000]"));
        }

        public PdfAction(int named)
        {
            Put(PdfName.S, PdfName.NAMED);
            switch (named)
            {
                case 1:
                    Put(PdfName.N, PdfName.FIRSTPAGE);
                    break;
                case 4:
                    Put(PdfName.N, PdfName.LASTPAGE);
                    break;
                case 3:
                    Put(PdfName.N, PdfName.NEXTPAGE);
                    break;
                case 2:
                    Put(PdfName.N, PdfName.PREVPAGE);
                    break;
                case 5:
                    Put(PdfName.S, PdfName.JAVASCRIPT);
                    Put(PdfName.JS, new PdfString("this.print(true);\r"));
                    break;
                default:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.named.action"));
            }
        }

        public PdfAction(string application, string parameters, string operation, string defaultDir)
        {
            Put(PdfName.S, PdfName.LAUNCH);
            if (parameters == null && operation == null && defaultDir == null)
            {
                Put(PdfName.F, new PdfString(application));
                return;
            }

            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.F, new PdfString(application));
            if (parameters != null)
            {
                pdfDictionary.Put(PdfName.P, new PdfString(parameters));
            }

            if (operation != null)
            {
                pdfDictionary.Put(PdfName.O, new PdfString(operation));
            }

            if (defaultDir != null)
            {
                pdfDictionary.Put(PdfName.D, new PdfString(defaultDir));
            }

            Put(PdfName.WIN, pdfDictionary);
        }

        public static PdfAction CreateLaunch(string application, string parameters, string operation, string defaultDir)
        {
            return new PdfAction(application, parameters, operation, defaultDir);
        }

        public static PdfAction Rendition(string file, PdfFileSpecification fs, string mimeType, PdfIndirectReference refi)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.RENDITION);
            pdfAction.Put(PdfName.R, new PdfRendition(file, fs, mimeType));
            pdfAction.Put(new PdfName("OP"), new PdfNumber(0));
            pdfAction.Put(new PdfName("AN"), refi);
            return pdfAction;
        }

        public static PdfAction JavaScript(string code, PdfWriter writer, bool unicode)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.JAVASCRIPT);
            if (unicode && code.Length < 50)
            {
                pdfAction.Put(PdfName.JS, new PdfString(code, "UnicodeBig"));
            }
            else
            {
                if (unicode || code.Length >= 100)
                {
                    try
                    {
                        PdfStream pdfStream = new PdfStream(PdfEncodings.ConvertToBytes(code, unicode ? "UnicodeBig" : "PDF"));
                        pdfStream.FlateCompress(writer.CompressionLevel);
                        pdfAction.Put(PdfName.JS, writer.AddToBody(pdfStream).IndirectReference);
                        return pdfAction;
                    }
                    catch
                    {
                        pdfAction.Put(PdfName.JS, new PdfString(code));
                        return pdfAction;
                    }
                }

                pdfAction.Put(PdfName.JS, new PdfString(code));
            }

            return pdfAction;
        }

        public static PdfAction JavaScript(string code, PdfWriter writer)
        {
            return JavaScript(code, writer, unicode: false);
        }

        internal static PdfAction CreateHide(PdfObject obj, bool hide)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.HIDE);
            pdfAction.Put(PdfName.T, obj);
            if (!hide)
            {
                pdfAction.Put(PdfName.H, PdfBoolean.PDFFALSE);
            }

            return pdfAction;
        }

        public static PdfAction CreateHide(PdfAnnotation annot, bool hide)
        {
            return CreateHide(annot.IndirectReference, hide);
        }

        public static PdfAction CreateHide(string name, bool hide)
        {
            return CreateHide(new PdfString(name), hide);
        }

        internal static PdfArray BuildArray(object[] names)
        {
            PdfArray pdfArray = new PdfArray();
            foreach (object obj in names)
            {
                if (obj is string)
                {
                    pdfArray.Add(new PdfString((string)obj));
                    continue;
                }

                if (obj is PdfAnnotation)
                {
                    pdfArray.Add(((PdfAnnotation)obj).IndirectReference);
                    continue;
                }

                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.array.must.contain.string.or.pdfannotation"));
            }

            return pdfArray;
        }

        public static PdfAction CreateHide(object[] names, bool hide)
        {
            return CreateHide(BuildArray(names), hide);
        }

        public static PdfAction CreateSubmitForm(string file, object[] names, int flags)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.SUBMITFORM);
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.F, new PdfString(file));
            pdfDictionary.Put(PdfName.FS, PdfName.URL);
            pdfAction.Put(PdfName.F, pdfDictionary);
            if (names != null)
            {
                pdfAction.Put(PdfName.FIELDS, BuildArray(names));
            }

            pdfAction.Put(PdfName.FLAGS, new PdfNumber(flags));
            return pdfAction;
        }

        public static PdfAction CreateResetForm(object[] names, int flags)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.RESETFORM);
            if (names != null)
            {
                pdfAction.Put(PdfName.FIELDS, BuildArray(names));
            }

            pdfAction.Put(PdfName.FLAGS, new PdfNumber(flags));
            return pdfAction;
        }

        public static PdfAction CreateImportData(string file)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.IMPORTDATA);
            pdfAction.Put(PdfName.F, new PdfString(file));
            return pdfAction;
        }

        public virtual void Next(PdfAction na)
        {
            PdfObject pdfObject = Get(PdfName.NEXT);
            if (pdfObject == null)
            {
                Put(PdfName.NEXT, na);
            }
            else if (pdfObject.IsDictionary())
            {
                PdfArray pdfArray = new PdfArray(pdfObject);
                pdfArray.Add(na);
                Put(PdfName.NEXT, pdfArray);
            }
            else
            {
                ((PdfArray)pdfObject).Add(na);
            }
        }

        public static PdfAction GotoLocalPage(int page, PdfDestination dest, PdfWriter writer)
        {
            PdfIndirectReference pageReference = writer.GetPageReference(page);
            dest.AddPage(pageReference);
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.GOTO);
            pdfAction.Put(PdfName.D, dest);
            return pdfAction;
        }

        public static PdfAction GotoLocalPage(string dest, bool isName)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.GOTO);
            if (isName)
            {
                pdfAction.Put(PdfName.D, new PdfName(dest));
            }
            else
            {
                pdfAction.Put(PdfName.D, new PdfString(dest, "UnicodeBig"));
            }

            return pdfAction;
        }

        public static PdfAction GotoRemotePage(string filename, string dest, bool isName, bool newWindow)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.F, new PdfString(filename));
            pdfAction.Put(PdfName.S, PdfName.GOTOR);
            if (isName)
            {
                pdfAction.Put(PdfName.D, new PdfName(dest));
            }
            else
            {
                pdfAction.Put(PdfName.D, new PdfString(dest, "UnicodeBig"));
            }

            if (newWindow)
            {
                pdfAction.Put(PdfName.NEWWINDOW, PdfBoolean.PDFTRUE);
            }

            return pdfAction;
        }

        public static PdfAction GotoEmbedded(string filename, PdfTargetDictionary target, string dest, bool isName, bool newWindow)
        {
            if (isName)
            {
                return GotoEmbedded(filename, target, new PdfName(dest), newWindow);
            }

            return GotoEmbedded(filename, target, new PdfString(dest, "UnicodeBig"), newWindow);
        }

        public static PdfAction GotoEmbedded(string filename, PdfTargetDictionary target, PdfObject dest, bool newWindow)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.GOTOE);
            pdfAction.Put(PdfName.T, target);
            pdfAction.Put(PdfName.D, dest);
            pdfAction.Put(PdfName.NEWWINDOW, new PdfBoolean(newWindow));
            if (filename != null)
            {
                pdfAction.Put(PdfName.F, new PdfString(filename));
            }

            return pdfAction;
        }

        public static PdfAction SetOCGstate(List<object> state, bool preserveRB)
        {
            PdfAction pdfAction = new PdfAction();
            pdfAction.Put(PdfName.S, PdfName.SETOCGSTATE);
            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < state.Count; i++)
            {
                object obj = state[i];
                if (obj == null)
                {
                    continue;
                }

                if (obj is PdfIndirectReference)
                {
                    pdfArray.Add((PdfIndirectReference)obj);
                    continue;
                }

                if (obj is PdfLayer)
                {
                    pdfArray.Add(((PdfLayer)obj).Ref);
                    continue;
                }

                if (obj is PdfName)
                {
                    pdfArray.Add((PdfName)obj);
                    continue;
                }

                if (obj is string)
                {
                    PdfName pdfName = null;
                    string text = (string)obj;
                    if (Util.EqualsIgnoreCase(text, "on"))
                    {
                        pdfName = PdfName.ON;
                    }
                    else if (Util.EqualsIgnoreCase(text, "off"))
                    {
                        pdfName = PdfName.OFF;
                    }
                    else
                    {
                        if (!Util.EqualsIgnoreCase(text, "toggle"))
                        {
                            throw new ArgumentException(MessageLocalization.GetComposedMessage("a.string.1.was.passed.in.state.only.on.off.and.toggle.are.allowed", text));
                        }

                        pdfName = PdfName.TOGGLE;
                    }

                    pdfArray.Add(pdfName);
                    continue;
                }

                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.type.was.passed.in.state.1", obj.GetType().ToString()));
            }

            pdfAction.Put(PdfName.STATE, pdfArray);
            if (!preserveRB)
            {
                pdfAction.Put(PdfName.PRESERVERB, PdfBoolean.PDFFALSE);
            }

            return pdfAction;
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 14, this);
            base.ToPdf(writer, os);
        }
    }
}
