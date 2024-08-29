using Sign.itext.error_messages;
using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PdfContentParser
    {
        public const int COMMAND_TYPE = 200;

        private PRTokeniser tokeniser;

        public virtual PRTokeniser Tokeniser
        {
            get
            {
                return tokeniser;
            }
            set
            {
                tokeniser = value;
            }
        }

        public PdfContentParser(PRTokeniser tokeniser)
        {
            this.tokeniser = tokeniser;
        }

        public virtual List<PdfObject> Parse(List<PdfObject> ls)
        {
            if (ls == null)
            {
                ls = new List<PdfObject>();
            }
            else
            {
                ls.Clear();
            }

            PdfObject pdfObject = null;
            while ((pdfObject = ReadPRObject()) != null)
            {
                ls.Add(pdfObject);
                if (pdfObject.Type == 200)
                {
                    break;
                }
            }

            return ls;
        }

        public virtual PRTokeniser GetTokeniser()
        {
            return tokeniser;
        }

        public virtual PdfDictionary ReadDictionary()
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            while (true)
            {
                if (!NextValidToken())
                {
                    throw new IOException(MessageLocalization.GetComposedMessage("unexpected.end.of.file"));
                }

                if (tokeniser.TokenType == PRTokeniser.TokType.END_DIC)
                {
                    break;
                }

                if (tokeniser.TokenType != PRTokeniser.TokType.OTHER || "def".CompareTo(tokeniser.StringValue) != 0)
                {
                    if (tokeniser.TokenType != PRTokeniser.TokType.NAME)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("dictionary.key.1.is.not.a.name", tokeniser.StringValue));
                    }

                    PdfName key = new PdfName(tokeniser.StringValue, lengthCheck: false);
                    PdfObject pdfObject = ReadPRObject();
                    int type = pdfObject.Type;
                    if (-type == 8)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("unexpected.gt.gt"));
                    }

                    if (-type == 6)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("unexpected.close.bracket"));
                    }

                    pdfDictionary.Put(key, pdfObject);
                }
            }

            return pdfDictionary;
        }

        public virtual PdfArray ReadArray()
        {
            PdfArray pdfArray = new PdfArray();
            while (true)
            {
                PdfObject pdfObject = ReadPRObject();
                int type = pdfObject.Type;
                if (-type == 6)
                {
                    break;
                }

                if (-type == 8)
                {
                    throw new IOException(MessageLocalization.GetComposedMessage("unexpected.gt.gt"));
                }

                pdfArray.Add(pdfObject);
            }

            return pdfArray;
        }

        public virtual PdfObject ReadPRObject()
        {
            if (!NextValidToken())
            {
                return null;
            }

            PRTokeniser.TokType tokenType = tokeniser.TokenType;
            return tokenType switch
            {
                PRTokeniser.TokType.START_DIC => ReadDictionary(),
                PRTokeniser.TokType.START_ARRAY => ReadArray(),
                PRTokeniser.TokType.STRING => new PdfString(tokeniser.StringValue, null).SetHexWriting(tokeniser.IsHexString()),
                PRTokeniser.TokType.NAME => new PdfName(tokeniser.StringValue, lengthCheck: false),
                PRTokeniser.TokType.NUMBER => new PdfNumber(tokeniser.StringValue),
                PRTokeniser.TokType.OTHER => new PdfLiteral(200, tokeniser.StringValue),
                _ => new PdfLiteral(0 - tokenType, tokeniser.StringValue),
            };
        }

        public virtual bool NextValidToken()
        {
            while (tokeniser.NextToken())
            {
                if (tokeniser.TokenType != PRTokeniser.TokType.COMMENT)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
