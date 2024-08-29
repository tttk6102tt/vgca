using Sign.itext.error_messages;

namespace Sign.itext.text.pdf
{
    public class DeviceNColor : ExtendedColor
    {
        private PdfDeviceNColor pdfDeviceNColor;

        private float[] tints;

        public virtual PdfDeviceNColor PdfDeviceNColor => pdfDeviceNColor;

        public virtual float[] Tints => tints;

        public DeviceNColor(PdfDeviceNColor pdfDeviceNColor, float[] tints)
            : base(6)
        {
            if (pdfDeviceNColor.SpotColors.Length != tints.Length)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("devicen.color.shall.have.the.same.number.of.colorants.as.the.destination.DeviceN.color.space"));
            }

            this.pdfDeviceNColor = pdfDeviceNColor;
            this.tints = tints;
        }

        public override bool Equals(object obj)
        {
            if (obj is DeviceNColor && ((DeviceNColor)obj).tints.Length == tints.Length)
            {
                int num = 0;
                float[] array = tints;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] != ((DeviceNColor)obj).tints[num])
                    {
                        return false;
                    }

                    num++;
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int num = pdfDeviceNColor.GetHashCode();
            float[] array = tints;
            foreach (float num2 in array)
            {
                num ^= num2.GetHashCode();
            }

            return num;
        }
    }
}
