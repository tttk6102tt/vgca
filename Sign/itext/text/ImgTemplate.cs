using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text
{
    public class ImgTemplate : Image
    {
        public ImgTemplate(Image image)
            : base(image)
        {
        }

        public ImgTemplate(PdfTemplate template)
            : base((Uri)null)
        {
            if (template == null)
            {
                throw new BadElementException(MessageLocalization.GetComposedMessage("the.template.can.not.be.null"));
            }

            if (template.Type == 3)
            {
                throw new BadElementException(MessageLocalization.GetComposedMessage("a.pattern.can.not.be.used.as.a.template.to.create.an.image"));
            }

            type = 35;
            scaledHeight = template.Height;
            Top = scaledHeight;
            scaledWidth = template.Width;
            Right = scaledWidth;
            TemplateData = template;
            plainWidth = Width;
            plainHeight = Height;
        }
    }
}
