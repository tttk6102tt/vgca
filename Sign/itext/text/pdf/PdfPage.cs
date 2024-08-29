using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfPage : PdfDictionary
    {
        private static string[] boxStrings = new string[4] { "crop", "trim", "art", "bleed" };

        private static PdfName[] boxNames = new PdfName[4]
        {
            PdfName.CROPBOX,
            PdfName.TRIMBOX,
            PdfName.ARTBOX,
            PdfName.BLEEDBOX
        };

        public static PdfNumber PORTRAIT = new PdfNumber(0);

        public static PdfNumber LANDSCAPE = new PdfNumber(90);

        public static PdfNumber INVERTEDPORTRAIT = new PdfNumber(180);

        public static PdfNumber SEASCAPE = new PdfNumber(270);

        private PdfRectangle mediaBox;

        internal PdfRectangle MediaBox => mediaBox;

        internal PdfPage(PdfRectangle mediaBox, Dictionary<string, PdfRectangle> boxSize, PdfDictionary resources, int rotate)
            : base(PdfDictionary.PAGE)
        {
            this.mediaBox = mediaBox;
            if (mediaBox != null && (mediaBox.Width > 14400f || mediaBox.Height > 14400f))
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.page.size.must.be.smaller.than.14400.by.14400.its.1.by.2", mediaBox.Width, mediaBox.Height));
            }

            Put(PdfName.MEDIABOX, mediaBox);
            Put(PdfName.RESOURCES, resources);
            if (rotate != 0)
            {
                Put(PdfName.ROTATE, new PdfNumber(rotate));
            }

            for (int i = 0; i < boxStrings.Length; i++)
            {
                if (boxSize.ContainsKey(boxStrings[i]))
                {
                    Put(boxNames[i], boxSize[boxStrings[i]]);
                }
            }
        }

        internal PdfPage(PdfRectangle mediaBox, Dictionary<string, PdfRectangle> boxSize, PdfDictionary resources)
            : this(mediaBox, boxSize, resources, 0)
        {
        }

        public virtual bool IsParent()
        {
            return false;
        }

        internal void Add(PdfIndirectReference contents)
        {
            Put(PdfName.CONTENTS, contents);
        }

        internal PdfRectangle RotateMediaBox()
        {
            mediaBox = mediaBox.Rotate;
            Put(PdfName.MEDIABOX, mediaBox);
            return mediaBox;
        }
    }
}
