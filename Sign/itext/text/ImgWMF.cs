using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.pdf.codec.wmf;
using System.Net;

namespace Sign.itext.text
{
    public class ImgWMF : Image
    {
        public ImgWMF(Image image)
            : base(image)
        {
        }

        public ImgWMF(Uri url)
            : base(url)
        {
            ProcessParameters();
        }

        public ImgWMF(string filename)
            : this(Utilities.ToURL(filename))
        {
        }

        public ImgWMF(byte[] img)
            : base((Uri)null)
        {
            rawData = img;
            originalData = img;
            ProcessParameters();
        }

        private void ProcessParameters()
        {
            type = 35;
            originalType = 6;
            Stream stream = null;
            try
            {
                string text;
                if (rawData == null)
                {
                    WebRequest webRequest = WebRequest.Create(url);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    stream = webRequest.GetResponse().GetResponseStream();
                    text = url.ToString();
                }
                else
                {
                    stream = new MemoryStream(rawData);
                    text = "Byte array";
                }

                InputMeta inputMeta = new InputMeta(stream);
                if (inputMeta.ReadInt() != -1698247209)
                {
                    throw new BadElementException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.placeable.windows.metafile", text));
                }

                inputMeta.ReadWord();
                int num = inputMeta.ReadShort();
                int num2 = inputMeta.ReadShort();
                int num3 = inputMeta.ReadShort();
                int num4 = inputMeta.ReadShort();
                int num5 = inputMeta.ReadWord();
                dpiX = 72;
                dpiY = 72;
                scaledHeight = (float)(num4 - num2) / (float)num5 * 72f;
                Top = scaledHeight;
                scaledWidth = (float)(num3 - num) / (float)num5 * 72f;
                Right = scaledWidth;
            }
            finally
            {
                stream?.Close();
                plainWidth = Width;
                plainHeight = Height;
            }
        }

        public virtual void ReadWMF(PdfTemplate template)
        {
            TemplateData = template;
            template.Width = Width;
            template.Height = Height;
            Stream stream = null;
            try
            {
                if (rawData == null)
                {
                    WebRequest webRequest = WebRequest.Create(url);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    stream = webRequest.GetResponse().GetResponseStream();
                }
                else
                {
                    stream = new MemoryStream(rawData);
                }

                new MetaDo(stream, template).ReadAll();
            }
            finally
            {
                stream?.Close();
            }
        }
    }
}
