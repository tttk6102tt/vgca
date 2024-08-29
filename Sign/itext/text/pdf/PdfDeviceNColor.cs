using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.SystemItext.util;
using System.Globalization;

namespace Sign.itext.text.pdf
{
    public class PdfDeviceNColor : ICachedColorSpace, IPdfSpecialColorSpace
    {
        private PdfSpotColor[] spotColors;

        private ColorDetails[] colorantsDetails;

        public virtual int NumberOfColorants => spotColors.Length;

        public virtual PdfSpotColor[] SpotColors => spotColors;

        public PdfDeviceNColor(PdfSpotColor[] spotColors)
        {
            this.spotColors = spotColors;
        }

        public virtual ColorDetails[] GetColorantDetails(PdfWriter writer)
        {
            if (colorantsDetails == null)
            {
                colorantsDetails = new ColorDetails[spotColors.Length];
                int num = 0;
                PdfSpotColor[] array = spotColors;
                foreach (PdfSpotColor spc in array)
                {
                    colorantsDetails[num] = writer.AddSimple(spc);
                    num++;
                }
            }

            return colorantsDetails;
        }

        public virtual PdfObject GetPdfObject(PdfWriter writer)
        {
            PdfArray pdfArray = new PdfArray(PdfName.DEVICEN);
            PdfArray pdfArray2 = new PdfArray();
            float[] array = new float[spotColors.Length * 2];
            PdfDictionary pdfDictionary = new PdfDictionary();
            string text = "";
            int num = spotColors.Length;
            float[,] array2 = new float[4, num];
            for (int i = 0; i < num; i++)
            {
                PdfSpotColor pdfSpotColor = spotColors[i];
                array[2 * i] = 0f;
                array[2 * i + 1] = 1f;
                pdfArray2.Add(pdfSpotColor.Name);
                if (pdfDictionary.Get(pdfSpotColor.Name) != null)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("devicen.component.names.shall.be.different"));
                }

                if (colorantsDetails != null)
                {
                    pdfDictionary.Put(pdfSpotColor.Name, colorantsDetails[i].IndirectReference);
                }
                else
                {
                    pdfDictionary.Put(pdfSpotColor.Name, pdfSpotColor.GetPdfObject(writer));
                }

                BaseColor alternativeCS = pdfSpotColor.AlternativeCS;
                if (alternativeCS is ExtendedColor)
                {
                    switch (((ExtendedColor)alternativeCS).Type)
                    {
                        case 1:
                            array2[0, i] = 0f;
                            array2[1, i] = 0f;
                            array2[2, i] = 0f;
                            array2[3, i] = 1f - ((GrayColor)alternativeCS).Gray;
                            break;
                        case 2:
                            array2[0, i] = ((CMYKColor)alternativeCS).Cyan;
                            array2[1, i] = ((CMYKColor)alternativeCS).Magenta;
                            array2[2, i] = ((CMYKColor)alternativeCS).Yellow;
                            array2[3, i] = ((CMYKColor)alternativeCS).Black;
                            break;
                        case 7:
                            {
                                CMYKColor cMYKColor = ((LabColor)alternativeCS).ToCmyk();
                                array2[0, i] = cMYKColor.Cyan;
                                array2[1, i] = cMYKColor.Magenta;
                                array2[2, i] = cMYKColor.Yellow;
                                array2[3, i] = cMYKColor.Black;
                                break;
                            }
                        default:
                            throw new Exception(MessageLocalization.GetComposedMessage("only.rgb.gray.and.cmyk.are.supported.as.alternative.color.spaces"));
                    }
                }
                else
                {
                    float num2 = alternativeCS.R;
                    float num3 = alternativeCS.G;
                    float num4 = alternativeCS.B;
                    float num5 = 0f;
                    float num6 = 0f;
                    float num7 = 0f;
                    float num8 = 0f;
                    if (num2 == 0f && num3 == 0f && num4 == 0f)
                    {
                        num8 = 1f;
                    }
                    else
                    {
                        num5 = 1f - num2 / 255f;
                        num6 = 1f - num3 / 255f;
                        num7 = 1f - num4 / 255f;
                        float num9 = Math.Min(num5, Math.Min(num6, num7));
                        num5 = (num5 - num9) / (1f - num9);
                        num6 = (num6 - num9) / (1f - num9);
                        num7 = (num7 - num9) / (1f - num9);
                        num8 = num9;
                    }

                    array2[0, i] = num5;
                    array2[1, i] = num6;
                    array2[2, i] = num7;
                    array2[3, i] = num8;
                }

                text += "pop ";
            }

            pdfArray.Add(pdfArray2);
            string text2 = string.Format(NumberFormatInfo.InvariantInfo, "1.000000 {0} 1 roll ", new object[1] { num + 1 });
            pdfArray.Add(PdfName.DEVICECMYK);
            text2 = text2 + text2 + text2 + text2;
            string text3 = "";
            for (int i = num + 4; i > num; i--)
            {
                text3 += string.Format(NumberFormatInfo.InvariantInfo, "{0} -1 roll ", new object[1] { i });
                for (int num10 = num; num10 > 0; num10--)
                {
                    text3 += string.Format(NumberFormatInfo.InvariantInfo, "{0} index {1} mul 1.000000 cvr exch sub mul ", new object[2]
                    {
                        num10,
                        array2[num + 4 - i, num - num10]
                    });
                }

                text3 += string.Format(NumberFormatInfo.InvariantInfo, "1.000000 cvr exch sub {0} 1 roll ", new object[1] { i });
            }

            PdfFunction pdfFunction = PdfFunction.Type4(writer, array, new float[8] { 0f, 1f, 0f, 1f, 0f, 1f, 0f, 1f }, "{ " + text2 + text3 + text + "}");
            pdfArray.Add(pdfFunction.Reference);
            PdfDictionary pdfDictionary2 = new PdfDictionary();
            pdfDictionary2.Put(PdfName.SUBTYPE, PdfName.NCHANNEL);
            pdfDictionary2.Put(PdfName.COLORANTS, pdfDictionary);
            pdfArray.Add(pdfDictionary2);
            return pdfArray;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is PdfDeviceNColor))
            {
                return false;
            }

            PdfDeviceNColor pdfDeviceNColor = (PdfDeviceNColor)o;
            if (!Util.ArraysAreEqual(spotColors, pdfDeviceNColor.spotColors))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Util.GetArrayHashCode(spotColors);
        }
    }
}
