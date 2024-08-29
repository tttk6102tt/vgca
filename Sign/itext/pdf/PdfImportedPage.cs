using Sign.itext.error_messages;
using Sign.itext.text;
using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PdfImportedPage : PdfTemplate
    {
        internal PdfReaderInstance readerInstance;

        internal int pageNumber;

        internal int rotation;

        protected internal bool toCopy = true;

        public virtual PdfImportedPage FromReader => this;

        public virtual int PageNumber => pageNumber;

        public virtual int Rotation => rotation;

        public override PdfContentByte Duplicate
        {
            get
            {
                ThrowError();
                return null;
            }
        }

        internal override PdfObject Resources => readerInstance.GetResources(pageNumber);

        public override PdfTransparencyGroup Group
        {
            set
            {
                ThrowError();
            }
        }

        internal PdfReaderInstance PdfReaderInstance => readerInstance;

        internal PdfImportedPage(PdfReaderInstance readerInstance, PdfWriter writer, int pageNumber)
        {
            this.readerInstance = readerInstance;
            this.pageNumber = pageNumber;
            rotation = readerInstance.Reader.GetPageRotation(pageNumber);
            base.writer = writer;
            bBox = readerInstance.Reader.GetPageSize(pageNumber);
            SetMatrix(1f, 0f, 0f, 1f, 0f - bBox.Left, 0f - bBox.Bottom);
            type = 2;
        }

        public override void AddImage(Image image, float a, float b, float c, float d, float e, float f)
        {
            ThrowError();
        }

        public override void AddTemplate(PdfTemplate template, float a, float b, float c, float d, float e, float f)
        {
            ThrowError();
        }

        public override PdfStream GetFormXObject(int compressionLevel)
        {
            return readerInstance.GetFormXObject(pageNumber, compressionLevel);
        }

        public override void SetColorFill(PdfSpotColor sp, float tint)
        {
            ThrowError();
        }

        public override void SetColorStroke(PdfSpotColor sp, float tint)
        {
            ThrowError();
        }

        public override void SetFontAndSize(BaseFont bf, float size)
        {
            ThrowError();
        }

        internal void ThrowError()
        {
            throw new Exception(MessageLocalization.GetComposedMessage("content.can.not.be.added.to.a.pdfimportedpage"));
        }

        public virtual bool IsToCopy()
        {
            return toCopy;
        }

        public virtual void SetCopied()
        {
            toCopy = false;
        }
    }
}
