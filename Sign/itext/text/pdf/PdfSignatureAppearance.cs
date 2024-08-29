using Sign.itext.error_messages;
using Sign.itext.io;
using Sign.itext.pdf;
using Sign.itext.text.pdf.security;
using Sign.Org.BouncyCastle.X509;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfSignatureAppearance
    {
        public interface ISignatureEvent
        {
            void GetSignatureDictionary(PdfDictionary sig);
        }

        public enum RenderingMode
        {
            DESCRIPTION,
            NAME_AND_DESCRIPTION,
            GRAPHIC_AND_DESCRIPTION,
            GRAPHIC
        }

        public const int NOT_CERTIFIED = 0;

        public const int CERTIFIED_NO_CHANGES_ALLOWED = 1;

        public const int CERTIFIED_FORM_FILLING = 2;

        public const int CERTIFIED_FORM_FILLING_AND_ANNOTATIONS = 3;

        private int certificationLevel;

        private string reasonCaption = "Reason: ";

        private string locationCaption = "Location: ";

        private string reason;

        private string location;

        private DateTime signDate;

        private string signatureCreator;

        private string contact;

        private FileStream raf;

        private byte[] bout;

        private long[] range;

        private X509Certificate signCertificate;

        private PdfDictionary cryptoDictionary;

        private ISignatureEvent signatureEvent;

        private string fieldName;

        private int page = 1;

        private Rectangle rect;

        private Rectangle pageRect;

        private RenderingMode renderingMode;

        private Image signatureGraphic;

        private bool acro6Layers = true;

        private PdfTemplate[] app = new PdfTemplate[5];

        private bool reuseAppearance;

        public const string questionMark = "% DSUnknown\nq\n1 G\n1 g\n0.1 0 0 0.1 9 0 cm\n0 J 0 j 4 M []0 d\n1 i \n0 g\n313 292 m\n313 404 325 453 432 529 c\n478 561 504 597 504 645 c\n504 736 440 760 391 760 c\n286 760 271 681 265 626 c\n265 625 l\n100 625 l\n100 828 253 898 381 898 c\n451 898 679 878 679 650 c\n679 555 628 499 538 435 c\n488 399 467 376 467 292 c\n313 292 l\nh\n308 214 170 -164 re\nf\n0.44 G\n1.2 w\n1 1 0.4 rg\n287 318 m\n287 430 299 479 406 555 c\n451 587 478 623 478 671 c\n478 762 414 786 365 786 c\n260 786 245 707 239 652 c\n239 651 l\n74 651 l\n74 854 227 924 355 924 c\n425 924 653 904 653 676 c\n653 581 602 525 512 461 c\n462 425 441 402 441 318 c\n287 318 l\nh\n282 240 170 -164 re\nB\nQ\n";

        private Image image;

        private float imageScale;

        private string layer2Text;

        private Font layer2Font;

        private int runDirection = 1;

        private string layer4Text;

        private PdfTemplate frm;

        private const float TOP_SECTION = 0.3f;

        private const float MARGIN = 2f;

        private PdfStamper stamper;

        private PdfStamperImp writer;

        private ByteBuffer sigout;

        private Stream originalout;

        private string tempFile;

        private Dictionary<PdfName, PdfLiteral> exclusionLocations;

        private int boutLen;

        private bool preClosed;

        public virtual int CertificationLevel
        {
            get
            {
                return certificationLevel;
            }
            set
            {
                certificationLevel = value;
            }
        }

        public virtual string Reason
        {
            get
            {
                return reason;
            }
            set
            {
                reason = value;
            }
        }

        public virtual string ReasonCaption
        {
            set
            {
                reasonCaption = value;
            }
        }

        public virtual string Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        public virtual string LocationCaption
        {
            set
            {
                locationCaption = value;
            }
        }

        public virtual string SignatureCreator
        {
            get
            {
                return signatureCreator;
            }
            set
            {
                signatureCreator = value;
            }
        }

        public virtual string Contact
        {
            get
            {
                return contact;
            }
            set
            {
                contact = value;
            }
        }

        public virtual DateTime SignDate
        {
            get
            {
                return signDate;
            }
            set
            {
                signDate = value;
            }
        }

        public virtual PdfDictionary CryptoDictionary
        {
            get
            {
                return cryptoDictionary;
            }
            set
            {
                cryptoDictionary = value;
            }
        }

        public virtual X509Certificate Certificate
        {
            get
            {
                return signCertificate;
            }
            set
            {
                signCertificate = value;
            }
        }

        public virtual ISignatureEvent SignatureEvent
        {
            get
            {
                return signatureEvent;
            }
            set
            {
                signatureEvent = value;
            }
        }

        public virtual string FieldName => fieldName;

        public virtual int Page => page;

        public virtual Rectangle Rect => rect;

        public virtual Rectangle PageRect => pageRect;

        public virtual RenderingMode SignatureRenderingMode
        {
            get
            {
                return renderingMode;
            }
            set
            {
                renderingMode = value;
            }
        }

        public virtual Image SignatureGraphic
        {
            get
            {
                return signatureGraphic;
            }
            set
            {
                signatureGraphic = value;
            }
        }

        public virtual bool Acro6Layers
        {
            get
            {
                return acro6Layers;
            }
            set
            {
                acro6Layers = value;
            }
        }

        public virtual bool ReuseAppearance
        {
            set
            {
                reuseAppearance = value;
            }
        }

        public virtual Image Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
            }
        }

        public virtual float ImageScale
        {
            get
            {
                return imageScale;
            }
            set
            {
                imageScale = value;
            }
        }

        public virtual string Layer2Text
        {
            get
            {
                return layer2Text;
            }
            set
            {
                layer2Text = value;
            }
        }

        public virtual Font Layer2Font
        {
            get
            {
                return layer2Font;
            }
            set
            {
                layer2Font = value;
            }
        }

        public virtual int RunDirection
        {
            get
            {
                return runDirection;
            }
            set
            {
                if (value < 0 || value > 3)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.run.direction.1", runDirection));
                }

                runDirection = value;
            }
        }

        public virtual string Layer4Text
        {
            get
            {
                return layer4Text;
            }
            set
            {
                layer4Text = value;
            }
        }

        public virtual PdfStamper Stamper => stamper;

        public virtual ByteBuffer Sigout
        {
            get
            {
                return sigout;
            }
            set
            {
                sigout = value;
            }
        }

        public virtual Stream Originalout
        {
            get
            {
                return originalout;
            }
            set
            {
                originalout = value;
            }
        }

        public virtual string TempFile => tempFile;

        public PdfSignatureAppearance(PdfStamperImp writer)
        {
            this.writer = writer;
            signDate = DateTime.Now;
            fieldName = GetNewSigName();
            signatureCreator = Version.GetInstance().GetVersion;
        }

        public virtual Stream GetRangeStream()
        {
            return new RASInputStream(new RandomAccessSourceFactory().CreateRanged(GetUnderlyingSource(), range));
        }

        private IRandomAccessSource GetUnderlyingSource()
        {
            RandomAccessSourceFactory randomAccessSourceFactory = new RandomAccessSourceFactory();
            if (raf != null)
            {
                return randomAccessSourceFactory.CreateSource(raf);
            }

            return randomAccessSourceFactory.CreateSource(bout);
        }

        public virtual void AddDeveloperExtension(PdfDeveloperExtension de)
        {
            writer.AddDeveloperExtension(de);
        }

        public virtual string GetNewSigName()
        {
            AcroFields acroFields = writer.GetAcroFields();
            string text = "Signature";
            int num = 0;
            bool flag = false;
            while (!flag)
            {
                num++;
                string text2 = text + num;
                if (acroFields.GetFieldItem(text2) != null)
                {
                    continue;
                }

                text2 += ".";
                flag = true;
                foreach (string key in acroFields.Fields.Keys)
                {
                    if (key.StartsWith(text2))
                    {
                        flag = false;
                        break;
                    }
                }
            }

            return text + num;
        }

        public virtual bool IsInvisible()
        {
            if (rect != null && rect.Width != 0f)
            {
                return rect.Height == 0f;
            }

            return true;
        }

        public virtual void SetVisibleSignature(Rectangle pageRect, int page, string fieldName)
        {
            if (fieldName != null)
            {
                if (fieldName.IndexOf('.') >= 0)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("field.names.cannot.contain.a.dot"));
                }

                if (writer.GetAcroFields().GetFieldItem(fieldName) != null)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.field.1.already.exists", fieldName));
                }

                this.fieldName = fieldName;
            }

            if (page < 1 || page > writer.reader.NumberOfPages)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.page.number.1", page));
            }

            this.pageRect = new Rectangle(pageRect);
            this.pageRect.Rotation = 90;
            this.pageRect.Normalize();
            rect = new Rectangle(this.pageRect.Width, this.pageRect.Height);
            this.page = page;
        }

        public virtual void SetVisibleSignature(string fieldName)
        {
            AcroFields.Item fieldItem = writer.GetAcroFields().GetFieldItem(fieldName);
            if (fieldItem == null)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.field.1.does.not.exist", fieldName));
            }

            PdfDictionary merged = fieldItem.GetMerged(0);
            if (!PdfName.SIG.Equals(PdfReader.GetPdfObject(merged.Get(PdfName.FT))))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.field.1.is.not.a.signature.field", fieldName));
            }

            this.fieldName = fieldName;
            PdfArray asArray = merged.GetAsArray(PdfName.RECT);
            float floatValue = asArray.GetAsNumber(0).FloatValue;
            float floatValue2 = asArray.GetAsNumber(1).FloatValue;
            float floatValue3 = asArray.GetAsNumber(2).FloatValue;
            float floatValue4 = asArray.GetAsNumber(3).FloatValue;
            pageRect = new Rectangle(floatValue, floatValue2, floatValue3, floatValue4);
            pageRect.Normalize();
            page = fieldItem.GetPage(0);
            int pageRotation = writer.reader.GetPageRotation(page);
            Rectangle pageSizeWithRotation = writer.reader.GetPageSizeWithRotation(page);
            switch (pageRotation)
            {
                case 90:
                    pageRect = new Rectangle(pageRect.Bottom, pageSizeWithRotation.Top - pageRect.Left, pageRect.Top, pageSizeWithRotation.Top - pageRect.Right);
                    break;
                case 180:
                    pageRect = new Rectangle(pageSizeWithRotation.Right - pageRect.Left, pageSizeWithRotation.Top - pageRect.Bottom, pageSizeWithRotation.Right - pageRect.Right, pageSizeWithRotation.Top - pageRect.Top);
                    break;
                case 270:
                    pageRect = new Rectangle(pageSizeWithRotation.Right - pageRect.Bottom, pageRect.Left, pageSizeWithRotation.Right - pageRect.Top, pageRect.Right);
                    break;
            }

            if (pageRotation != 0)
            {
                pageRect.Normalize();
            }

            rect = new Rectangle(pageRect.Width, pageRect.Height);
        }

        public virtual PdfTemplate GetLayer(int layer)
        {
            if (layer < 0 || layer >= app.Length)
            {
                return null;
            }

            PdfTemplate pdfTemplate = app[layer];
            if (pdfTemplate == null)
            {
                pdfTemplate = (app[layer] = new PdfTemplate(writer));
                pdfTemplate.BoundingBox = rect;
                writer.AddDirectTemplateSimple(pdfTemplate, new PdfName("n" + layer));
            }

            return pdfTemplate;
        }

        public virtual PdfTemplate GetTopLayer()
        {
            if (frm == null)
            {
                frm = new PdfTemplate(writer);
                frm.BoundingBox = rect;
                writer.AddDirectTemplateSimple(frm, new PdfName("FRM"));
            }

            return frm;
        }

        public virtual PdfTemplate GetAppearance()
        {
            if (IsInvisible())
            {
                PdfTemplate pdfTemplate = new PdfTemplate(writer);
                pdfTemplate.BoundingBox = new Rectangle(0f, 0f);
                writer.AddDirectTemplateSimple(pdfTemplate, null);
                return pdfTemplate;
            }

            if (app[0] == null && !reuseAppearance)
            {
                CreateBlankN0();
            }

            if (app[1] == null && !acro6Layers)
            {
                PdfTemplate pdfTemplate2 = (app[1] = new PdfTemplate(writer));
                pdfTemplate2.BoundingBox = new Rectangle(100f, 100f);
                writer.AddDirectTemplateSimple(pdfTemplate2, new PdfName("n1"));
                pdfTemplate2.SetLiteral("% DSUnknown\nq\n1 G\n1 g\n0.1 0 0 0.1 9 0 cm\n0 J 0 j 4 M []0 d\n1 i \n0 g\n313 292 m\n313 404 325 453 432 529 c\n478 561 504 597 504 645 c\n504 736 440 760 391 760 c\n286 760 271 681 265 626 c\n265 625 l\n100 625 l\n100 828 253 898 381 898 c\n451 898 679 878 679 650 c\n679 555 628 499 538 435 c\n488 399 467 376 467 292 c\n313 292 l\nh\n308 214 170 -164 re\nf\n0.44 G\n1.2 w\n1 1 0.4 rg\n287 318 m\n287 430 299 479 406 555 c\n451 587 478 623 478 671 c\n478 762 414 786 365 786 c\n260 786 245 707 239 652 c\n239 651 l\n74 651 l\n74 854 227 924 355 924 c\n425 924 653 904 653 676 c\n653 581 602 525 512 461 c\n462 425 441 402 441 318 c\n287 318 l\nh\n282 240 170 -164 re\nB\nQ\n");
            }

            if (app[2] == null)
            {
                string text2;
                if (layer2Text == null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("Digitally signed by ");
                    string text = null;
                    CertificateInfo.X509Name subjectFields = CertificateInfo.GetSubjectFields(signCertificate);
                    if (subjectFields != null)
                    {
                        text = subjectFields.GetField("CN");
                        if (text == null)
                        {
                            text = subjectFields.GetField("E");
                        }
                    }

                    if (text == null)
                    {
                        text = "";
                    }

                    stringBuilder.Append(text).Append('\n');
                    stringBuilder.Append("Date: ").Append(signDate.ToString("yyyy.MM.dd HH:mm:ss zzz"));
                    if (reason != null)
                    {
                        stringBuilder.Append('\n').Append(reasonCaption).Append(reason);
                    }

                    if (location != null)
                    {
                        stringBuilder.Append('\n').Append(locationCaption).Append(location);
                    }

                    text2 = stringBuilder.ToString();
                }
                else
                {
                    text2 = layer2Text;
                }

                PdfTemplate pdfTemplate3 = (app[2] = new PdfTemplate(writer));
                pdfTemplate3.BoundingBox = rect;
                writer.AddDirectTemplateSimple(pdfTemplate3, new PdfName("n2"));
                if (image != null)
                {
                    if (imageScale == 0f)
                    {
                        pdfTemplate3.AddImage(image, rect.Width, 0f, 0f, rect.Height, 0f, 0f);
                    }
                    else
                    {
                        float num = imageScale;
                        if (imageScale < 0f)
                        {
                            num = Math.Min(rect.Width / image.Width, rect.Height / image.Height);
                        }

                        float num2 = image.Width * num;
                        float num3 = image.Height * num;
                        float e = (rect.Width - num2) / 2f;
                        float f = (rect.Height - num3) / 2f;
                        pdfTemplate3.AddImage(image, num2, 0f, 0f, num3, e, f);
                    }
                }

                Font font = ((layer2Font != null) ? new Font(layer2Font) : new Font());
                float num4 = font.Size;
                Rectangle rectangle = null;
                Rectangle rectangle2 = null;
                if (renderingMode == RenderingMode.NAME_AND_DESCRIPTION || (renderingMode == RenderingMode.GRAPHIC_AND_DESCRIPTION && SignatureGraphic != null))
                {
                    rectangle2 = new Rectangle(2f, 2f, rect.Width / 2f - 2f, rect.Height - 2f);
                    rectangle = new Rectangle(rect.Width / 2f + 1f, 2f, rect.Width - 1f, rect.Height - 2f);
                    if (rect.Height > rect.Width)
                    {
                        rectangle2 = new Rectangle(2f, rect.Height / 2f, rect.Width - 2f, rect.Height);
                        rectangle = new Rectangle(2f, 2f, rect.Width - 2f, rect.Height / 2f - 2f);
                    }
                }
                else if (renderingMode == RenderingMode.GRAPHIC)
                {
                    if (signatureGraphic == null)
                    {
                        throw new InvalidOperationException(MessageLocalization.GetComposedMessage("a.signature.image.should.be.present.when.rendering.mode.is.graphic.only"));
                    }

                    rectangle2 = new Rectangle(2f, 2f, rect.Width - 2f, rect.Height - 2f);
                }
                else
                {
                    rectangle = new Rectangle(2f, 2f, rect.Width - 2f, rect.Height * 0.7f - 2f);
                }

                if (renderingMode == RenderingMode.NAME_AND_DESCRIPTION)
                {
                    string text3 = CertificateInfo.GetSubjectFields(signCertificate).GetField("CN");
                    if (text3 == null)
                    {
                        text3 = CertificateInfo.GetSubjectFields(signCertificate).GetField("E");
                    }

                    if (text3 == null)
                    {
                        text3 = "";
                    }

                    Rectangle rectangle3 = new Rectangle(rectangle2.Width - 2f, rectangle2.Height - 2f);
                    float leading = ColumnText.FitText(font, text3, rectangle3, -1f, runDirection);
                    ColumnText columnText = new ColumnText(pdfTemplate3);
                    columnText.RunDirection = runDirection;
                    columnText.SetSimpleColumn(new Phrase(text3, font), rectangle2.Left, rectangle2.Bottom, rectangle2.Right, rectangle2.Top, leading, 0);
                    columnText.Go();
                }
                else if (renderingMode == RenderingMode.GRAPHIC_AND_DESCRIPTION)
                {
                    if (signatureGraphic == null)
                    {
                        throw new InvalidOperationException(MessageLocalization.GetComposedMessage("a.signature.image.should.be.present.when.rendering.mode.is.graphic.and.description"));
                    }

                    ColumnText columnText2 = new ColumnText(pdfTemplate3);
                    columnText2.RunDirection = runDirection;
                    columnText2.SetSimpleColumn(rectangle2.Left, rectangle2.Bottom, rectangle2.Right, rectangle2.Top, 0f, 2);
                    Image instance = Image.GetInstance(SignatureGraphic);
                    instance.ScaleToFit(rectangle2.Width, rectangle2.Height);
                    Paragraph paragraph = new Paragraph();
                    float num5 = 0f;
                    float num6 = 0f - instance.ScaledHeight + 15f;
                    num5 += (rectangle2.Width - instance.ScaledWidth) / 2f;
                    paragraph.Add(new Chunk(offsetY: num6 - (rectangle2.Height - instance.ScaledHeight) / 2f, image: instance, offsetX: num5 + (rectangle2.Width - instance.ScaledWidth) / 2f, changeLeading: false));
                    columnText2.AddElement(paragraph);
                    columnText2.Go();
                }
                else if (renderingMode == RenderingMode.GRAPHIC)
                {
                    ColumnText columnText3 = new ColumnText(pdfTemplate3);
                    columnText3.RunDirection = runDirection;
                    columnText3.SetSimpleColumn(rectangle2.Left, rectangle2.Bottom, rectangle2.Right, rectangle2.Top, 0f, 2);
                    Image instance2 = Image.GetInstance(signatureGraphic);
                    instance2.ScaleToFit(rectangle2.Width, rectangle2.Height);
                    Paragraph paragraph2 = new Paragraph(rectangle2.Height);
                    float offsetX = (rectangle2.Width - instance2.ScaledWidth) / 2f;
                    float offsetY2 = (rectangle2.Height - instance2.ScaledHeight) / 2f;
                    paragraph2.Add(new Chunk(instance2, offsetX, offsetY2, changeLeading: false));
                    columnText3.AddElement(paragraph2);
                    columnText3.Go();
                }

                if (renderingMode != RenderingMode.GRAPHIC)
                {
                    if (num4 <= 0f)
                    {
                        Rectangle rectangle4 = new Rectangle(rectangle.Width, rectangle.Height);
                        num4 = ColumnText.FitText(font, text2, rectangle4, 12f, runDirection);
                    }

                    ColumnText columnText4 = new ColumnText(pdfTemplate3);
                    columnText4.RunDirection = runDirection;
                    columnText4.SetSimpleColumn(new Phrase(text2, font), rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Top, num4, 0);
                    columnText4.Go();
                }
            }

            if (app[3] == null && !acro6Layers)
            {
                PdfTemplate pdfTemplate4 = (app[3] = new PdfTemplate(writer));
                pdfTemplate4.BoundingBox = new Rectangle(100f, 100f);
                writer.AddDirectTemplateSimple(pdfTemplate4, new PdfName("n3"));
                pdfTemplate4.SetLiteral("% DSBlank\n");
            }

            if (app[4] == null && !acro6Layers)
            {
                PdfTemplate pdfTemplate5 = (app[4] = new PdfTemplate(writer));
                pdfTemplate5.BoundingBox = new Rectangle(0f, rect.Height * 0.7f, rect.Right, rect.Top);
                writer.AddDirectTemplateSimple(pdfTemplate5, new PdfName("n4"));
                Font font2 = ((layer2Font != null) ? new Font(layer2Font) : new Font());
                float size = font2.Size;
                string text4 = "Signature Not Verified";
                if (layer4Text != null)
                {
                    text4 = layer4Text;
                }

                Rectangle rectangle5 = new Rectangle(rect.Width - 4f, rect.Height * 0.3f - 4f);
                size = ColumnText.FitText(font2, text4, rectangle5, 15f, runDirection);
                ColumnText columnText5 = new ColumnText(pdfTemplate5);
                columnText5.RunDirection = runDirection;
                columnText5.SetSimpleColumn(new Phrase(text4, font2), 2f, 0f, rect.Width - 2f, rect.Height - 2f, size, 0);
                columnText5.Go();
            }

            int pageRotation = writer.reader.GetPageRotation(page);
            Rectangle rectangle6 = new Rectangle(rect);
            for (int num7 = pageRotation; num7 > 0; num7 -= 90)
            {
                rectangle6 = rectangle6.Rotate();
            }

            if (frm == null)
            {
                frm = new PdfTemplate(writer);
                frm.BoundingBox = rectangle6;
                writer.AddDirectTemplateSimple(frm, new PdfName("FRM"));
                float num8 = Math.Min(rect.Width, rect.Height) * 0.9f;
                float e2 = (rect.Width - num8) / 2f;
                float f2 = (rect.Height - num8) / 2f;
                num8 /= 100f;
                switch (pageRotation)
                {
                    case 90:
                        frm.ConcatCTM(0f, 1f, -1f, 0f, rect.Height, 0f);
                        break;
                    case 180:
                        frm.ConcatCTM(-1f, 0f, 0f, -1f, rect.Width, rect.Height);
                        break;
                    case 270:
                        frm.ConcatCTM(0f, -1f, 1f, 0f, 0f, rect.Width);
                        break;
                }

                if (reuseAppearance)
                {
                    PdfIndirectReference normalAppearance = writer.GetAcroFields().GetNormalAppearance(FieldName);
                    if (normalAppearance != null)
                    {
                        frm.AddTemplateReference(normalAppearance, new PdfName("n0"), 1f, 0f, 0f, 1f, 0f, 0f);
                    }
                    else
                    {
                        reuseAppearance = false;
                        if (app[0] == null)
                        {
                            CreateBlankN0();
                        }
                    }
                }

                if (!reuseAppearance)
                {
                    frm.AddTemplate(app[0], 0f, 0f);
                }

                if (!acro6Layers)
                {
                    frm.AddTemplate(app[1], num8, 0f, 0f, num8, e2, f2);
                }

                frm.AddTemplate(app[2], 0f, 0f);
                if (!acro6Layers)
                {
                    frm.AddTemplate(app[3], num8, 0f, 0f, num8, e2, f2);
                    frm.AddTemplate(app[4], 0f, 0f);
                }
            }

            PdfTemplate pdfTemplate6 = new PdfTemplate(writer);
            pdfTemplate6.BoundingBox = rectangle6;
            writer.AddDirectTemplateSimple(pdfTemplate6, null);
            pdfTemplate6.AddTemplate(frm, 0f, 0f);
            return pdfTemplate6;
        }

        private void CreateBlankN0()
        {
            PdfTemplate pdfTemplate = (app[0] = new PdfTemplate(writer));
            pdfTemplate.BoundingBox = new Rectangle(100f, 100f);
            writer.AddDirectTemplateSimple(pdfTemplate, new PdfName("n0"));
            pdfTemplate.SetLiteral("% DSBlank\n");
        }

        public virtual void SetStamper(PdfStamper stamper)
        {
            this.stamper = stamper;
        }

        public virtual void SetTempFile(string tempFile)
        {
            this.tempFile = tempFile;
        }

        public virtual bool IsPreClosed()
        {
            return preClosed;
        }

        public virtual void PreClose(Dictionary<PdfName, int> exclusionSizes)
        {
            if (preClosed)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("document.already.pre.closed"));
            }

            stamper.MergeVerification();
            preClosed = true;
            AcroFields acroFields = writer.GetAcroFields();
            string name = FieldName;
            bool num = acroFields.DoesSignatureFieldExist(name);
            PdfIndirectReference pdfIndirectReference = writer.PdfIndirectReference;
            writer.SigFlags = 3;
            PdfDictionary pdfDictionary = null;
            if (num)
            {
                PdfDictionary widget = acroFields.GetFieldItem(name).GetWidget(0);
                writer.MarkUsed(widget);
                pdfDictionary = widget.GetAsDict(PdfName.LOCK);
                widget.Put(PdfName.P, writer.GetPageReference(Page));
                widget.Put(PdfName.V, pdfIndirectReference);
                PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(widget.Get(PdfName.F));
                int num2 = 0;
                if (pdfObjectRelease != null && pdfObjectRelease.IsNumber())
                {
                    num2 = ((PdfNumber)pdfObjectRelease).IntValue;
                }

                num2 |= 0x80;
                widget.Put(PdfName.F, new PdfNumber(num2));
                PdfDictionary pdfDictionary2 = new PdfDictionary();
                pdfDictionary2.Put(PdfName.N, GetAppearance().IndirectReference);
                widget.Put(PdfName.AP, pdfDictionary2);
            }
            else
            {
                PdfFormField pdfFormField = PdfFormField.CreateSignature(writer);
                pdfFormField.FieldName = name;
                pdfFormField.Put(PdfName.V, pdfIndirectReference);
                pdfFormField.Flags = 132;
                int num3 = Page;
                if (!IsInvisible())
                {
                    pdfFormField.SetWidget(PageRect, null);
                }
                else
                {
                    pdfFormField.SetWidget(new Rectangle(0f, 0f), null);
                }

                pdfFormField.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, GetAppearance());
                pdfFormField.Page = num3;
                writer.AddAnnotation(pdfFormField, num3);
            }

            exclusionLocations = new Dictionary<PdfName, PdfLiteral>();
            if (cryptoDictionary == null)
            {
                throw new DocumentException("No crypto dictionary defined.");
            }

            PdfLiteral value = new PdfLiteral(80);
            exclusionLocations[PdfName.BYTERANGE] = value;
            cryptoDictionary.Put(PdfName.BYTERANGE, value);
            foreach (KeyValuePair<PdfName, int> exclusionSize in exclusionSizes)
            {
                PdfName key = exclusionSize.Key;
                value = new PdfLiteral(exclusionSize.Value);
                exclusionLocations[key] = value;
                cryptoDictionary.Put(key, value);
            }

            if (certificationLevel > 0)
            {
                AddDocMDP(cryptoDictionary);
            }

            if (pdfDictionary != null)
            {
                AddFieldMDP(cryptoDictionary, pdfDictionary);
            }

            if (signatureEvent != null)
            {
                signatureEvent.GetSignatureDictionary(cryptoDictionary);
            }

            writer.AddToBody(cryptoDictionary, pdfIndirectReference, inObjStm: false);
            if (certificationLevel > 0)
            {
                PdfDictionary pdfDictionary3 = new PdfDictionary();
                pdfDictionary3.Put(new PdfName("DocMDP"), pdfIndirectReference);
                writer.reader.Catalog.Put(new PdfName("Perms"), pdfDictionary3);
            }

            writer.Close(stamper.MoreInfo);
            range = new long[exclusionLocations.Count * 2];
            long position = exclusionLocations[PdfName.BYTERANGE].Position;
            exclusionLocations.Remove(PdfName.BYTERANGE);
            int num4 = 1;
            foreach (PdfLiteral value2 in exclusionLocations.Values)
            {
                long position2 = value2.Position;
                range[num4++] = position2;
                range[num4++] = value2.PosLength + position2;
            }

            Array.Sort(range, 1, range.Length - 2);
            for (int i = 3; i < range.Length - 2; i += 2)
            {
                range[i] -= range[i - 1];
            }

            if (tempFile == null)
            {
                bout = sigout.Buffer;
                boutLen = sigout.Size;
                range[range.Length - 1] = boutLen - range[range.Length - 2];
                ByteBuffer byteBuffer = new ByteBuffer();
                byteBuffer.Append('[');
                for (int j = 0; j < range.Length; j++)
                {
                    byteBuffer.Append(range[j]).Append(' ');
                }

                byteBuffer.Append(']');
                Array.Copy(byteBuffer.Buffer, 0L, bout, position, byteBuffer.Size);
                return;
            }

            try
            {
                raf = new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite);
                long length = raf.Length;
                range[range.Length - 1] = length - range[range.Length - 2];
                ByteBuffer byteBuffer2 = new ByteBuffer();
                byteBuffer2.Append('[');
                for (int k = 0; k < range.Length; k++)
                {
                    byteBuffer2.Append(range[k]).Append(' ');
                }

                byteBuffer2.Append(']');
                raf.Seek(position, SeekOrigin.Begin);
                raf.Write(byteBuffer2.Buffer, 0, byteBuffer2.Size);
            }
            catch (IOException ex)
            {
                try
                {
                    raf.Close();
                }
                catch
                {
                }

                try
                {
                    File.Delete(tempFile);
                }
                catch
                {
                }

                throw ex;
            }
        }

        private void AddDocMDP(PdfDictionary crypto)
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            PdfDictionary pdfDictionary2 = new PdfDictionary();
            pdfDictionary2.Put(PdfName.P, new PdfNumber(certificationLevel));
            pdfDictionary2.Put(PdfName.V, new PdfName("1.2"));
            pdfDictionary2.Put(PdfName.TYPE, PdfName.TRANSFORMPARAMS);
            pdfDictionary.Put(PdfName.TRANSFORMMETHOD, PdfName.DOCMDP);
            pdfDictionary.Put(PdfName.TYPE, PdfName.SIGREF);
            pdfDictionary.Put(PdfName.TRANSFORMPARAMS, pdfDictionary2);
            if (writer.GetPdfVersion().Version < '6')
            {
                pdfDictionary.Put(new PdfName("DigestValue"), new PdfString("aa"));
                PdfArray pdfArray = new PdfArray();
                pdfArray.Add(new PdfNumber(0));
                pdfArray.Add(new PdfNumber(0));
                pdfDictionary.Put(new PdfName("DigestLocation"), pdfArray);
                pdfDictionary.Put(new PdfName("DigestMethod"), new PdfName("MD5"));
            }

            pdfDictionary.Put(PdfName.DATA, writer.reader.Trailer.Get(PdfName.ROOT));
            PdfArray pdfArray2 = new PdfArray();
            pdfArray2.Add(pdfDictionary);
            crypto.Put(PdfName.REFERENCE, pdfArray2);
        }

        private void AddFieldMDP(PdfDictionary crypto, PdfDictionary fieldLock)
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            PdfDictionary pdfDictionary2 = new PdfDictionary();
            pdfDictionary2.Merge(fieldLock);
            pdfDictionary2.Put(PdfName.TYPE, PdfName.TRANSFORMPARAMS);
            pdfDictionary2.Put(PdfName.V, new PdfName("1.2"));
            pdfDictionary.Put(PdfName.TRANSFORMMETHOD, PdfName.FIELDMDP);
            pdfDictionary.Put(PdfName.TYPE, PdfName.SIGREF);
            pdfDictionary.Put(PdfName.TRANSFORMPARAMS, pdfDictionary2);
            pdfDictionary.Put(new PdfName("DigestValue"), new PdfString("aa"));
            PdfArray pdfArray = new PdfArray();
            pdfArray.Add(new PdfNumber(0));
            pdfArray.Add(new PdfNumber(0));
            pdfDictionary.Put(new PdfName("DigestLocation"), pdfArray);
            pdfDictionary.Put(new PdfName("DigestMethod"), new PdfName("MD5"));
            pdfDictionary.Put(PdfName.DATA, writer.reader.Trailer.Get(PdfName.ROOT));
            PdfArray pdfArray2 = crypto.GetAsArray(PdfName.REFERENCE);
            if (pdfArray2 == null)
            {
                pdfArray2 = new PdfArray();
            }

            pdfArray2.Add(pdfDictionary);
            crypto.Put(PdfName.REFERENCE, pdfArray2);
        }

        public virtual void Close(PdfDictionary update)
        {
            try
            {
                if (!preClosed)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("preclose.must.be.called.first"));
                }

                ByteBuffer byteBuffer = new ByteBuffer();
                foreach (PdfName key in update.Keys)
                {
                    PdfObject pdfObject = update.Get(key);
                    PdfLiteral pdfLiteral = exclusionLocations[key];
                    if (pdfLiteral == null)
                    {
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("the.key.1.didn.t.reserve.space.in.preclose", key.ToString()));
                    }

                    byteBuffer.Reset();
                    pdfObject.ToPdf(null, byteBuffer);
                    if (byteBuffer.Size > pdfLiteral.PosLength)
                    {
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("the.key.1.is.too.big.is.2.reserved.3", key.ToString(), byteBuffer.Size, pdfLiteral.PosLength));
                    }

                    if (tempFile == null)
                    {
                        Array.Copy(byteBuffer.Buffer, 0L, bout, pdfLiteral.Position, byteBuffer.Size);
                        continue;
                    }

                    raf.Seek(pdfLiteral.Position, SeekOrigin.Begin);
                    raf.Write(byteBuffer.Buffer, 0, byteBuffer.Size);
                }

                if (update.Size != exclusionLocations.Count)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.update.dictionary.has.less.keys.than.required"));
                }

                if (tempFile == null)
                {
                    originalout.Write(bout, 0, boutLen);
                }
                else
                {
                    if (originalout == null)
                    {
                        return;
                    }

                    raf.Seek(0L, SeekOrigin.Begin);
                    long num = raf.Length;
                    byte[] array = new byte[8192];
                    while (num > 0)
                    {
                        int num2 = raf.Read(array, 0, (int)Math.Min(array.Length, num));
                        if (num2 < 0)
                        {
                            throw new EndOfStreamException(MessageLocalization.GetComposedMessage("unexpected.eof"));
                        }

                        originalout.Write(array, 0, num2);
                        num -= num2;
                    }

                    return;
                }
            }
            finally
            {
                writer.reader.Close();
                if (tempFile != null)
                {
                    try
                    {
                        raf.Close();
                    }
                    catch
                    {
                    }

                    if (originalout != null)
                    {
                        try
                        {
                            File.Delete(tempFile);
                        }
                        catch
                        {
                        }
                    }
                }

                if (originalout != null)
                {
                    try
                    {
                        originalout.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
