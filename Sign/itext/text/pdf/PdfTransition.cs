using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfTransition
    {
        public const int SPLITVOUT = 1;

        public const int SPLITHOUT = 2;

        public const int SPLITVIN = 3;

        public const int SPLITHIN = 4;

        public const int BLINDV = 5;

        public const int BLINDH = 6;

        public const int INBOX = 7;

        public const int OUTBOX = 8;

        public const int LRWIPE = 9;

        public const int RLWIPE = 10;

        public const int BTWIPE = 11;

        public const int TBWIPE = 12;

        public const int DISSOLVE = 13;

        public const int LRGLITTER = 14;

        public const int TBGLITTER = 15;

        public const int DGLITTER = 16;

        protected int duration;

        protected int type;

        public virtual int Duration => duration;

        public virtual int Type => type;

        public virtual PdfDictionary TransitionDictionary
        {
            get
            {
                PdfDictionary pdfDictionary = new PdfDictionary(PdfName.TRANS);
                switch (type)
                {
                    case 1:
                        pdfDictionary.Put(PdfName.S, PdfName.SPLIT);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DM, PdfName.V);
                        pdfDictionary.Put(PdfName.M, PdfName.O);
                        break;
                    case 2:
                        pdfDictionary.Put(PdfName.S, PdfName.SPLIT);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DM, PdfName.H);
                        pdfDictionary.Put(PdfName.M, PdfName.O);
                        break;
                    case 3:
                        pdfDictionary.Put(PdfName.S, PdfName.SPLIT);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DM, PdfName.V);
                        pdfDictionary.Put(PdfName.M, PdfName.I);
                        break;
                    case 4:
                        pdfDictionary.Put(PdfName.S, PdfName.SPLIT);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DM, PdfName.H);
                        pdfDictionary.Put(PdfName.M, PdfName.I);
                        break;
                    case 5:
                        pdfDictionary.Put(PdfName.S, PdfName.BLINDS);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DM, PdfName.V);
                        break;
                    case 6:
                        pdfDictionary.Put(PdfName.S, PdfName.BLINDS);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DM, PdfName.H);
                        break;
                    case 7:
                        pdfDictionary.Put(PdfName.S, PdfName.BOX);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.M, PdfName.I);
                        break;
                    case 8:
                        pdfDictionary.Put(PdfName.S, PdfName.BOX);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.M, PdfName.O);
                        break;
                    case 9:
                        pdfDictionary.Put(PdfName.S, PdfName.WIPE);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DI, new PdfNumber(0));
                        break;
                    case 10:
                        pdfDictionary.Put(PdfName.S, PdfName.WIPE);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DI, new PdfNumber(180));
                        break;
                    case 11:
                        pdfDictionary.Put(PdfName.S, PdfName.WIPE);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DI, new PdfNumber(90));
                        break;
                    case 12:
                        pdfDictionary.Put(PdfName.S, PdfName.WIPE);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DI, new PdfNumber(270));
                        break;
                    case 13:
                        pdfDictionary.Put(PdfName.S, PdfName.DISSOLVE);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        break;
                    case 14:
                        pdfDictionary.Put(PdfName.S, PdfName.GLITTER);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DI, new PdfNumber(0));
                        break;
                    case 15:
                        pdfDictionary.Put(PdfName.S, PdfName.GLITTER);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DI, new PdfNumber(270));
                        break;
                    case 16:
                        pdfDictionary.Put(PdfName.S, PdfName.GLITTER);
                        pdfDictionary.Put(PdfName.D, new PdfNumber(duration));
                        pdfDictionary.Put(PdfName.DI, new PdfNumber(315));
                        break;
                }

                return pdfDictionary;
            }
        }

        public PdfTransition()
            : this(6)
        {
        }

        public PdfTransition(int type)
            : this(type, 1)
        {
        }

        public PdfTransition(int type, int duration)
        {
            this.duration = duration;
            this.type = type;
        }
    }
}
