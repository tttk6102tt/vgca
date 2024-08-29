using Sign.itext.error_messages;
using System.Globalization;

namespace Sign.itext.text
{
    public class PageSize
    {
        public static readonly Rectangle LETTER = new RectangleReadOnly(612f, 792f);

        public static readonly Rectangle NOTE = new RectangleReadOnly(540f, 720f);

        public static readonly Rectangle LEGAL = new RectangleReadOnly(612f, 1008f);

        public static readonly Rectangle TABLOID = new RectangleReadOnly(792f, 1224f);

        public static readonly Rectangle EXECUTIVE = new RectangleReadOnly(522f, 756f);

        public static readonly Rectangle POSTCARD = new RectangleReadOnly(283f, 416f);

        public static readonly Rectangle A0 = new RectangleReadOnly(2384f, 3370f);

        public static readonly Rectangle A1 = new RectangleReadOnly(1684f, 2384f);

        public static readonly Rectangle A2 = new RectangleReadOnly(1191f, 1684f);

        public static readonly Rectangle A3 = new RectangleReadOnly(842f, 1191f);

        public static readonly Rectangle A4 = new RectangleReadOnly(595f, 842f);

        public static readonly Rectangle A5 = new RectangleReadOnly(420f, 595f);

        public static readonly Rectangle A6 = new RectangleReadOnly(297f, 420f);

        public static readonly Rectangle A7 = new RectangleReadOnly(210f, 297f);

        public static readonly Rectangle A8 = new RectangleReadOnly(148f, 210f);

        public static readonly Rectangle A9 = new RectangleReadOnly(105f, 148f);

        public static readonly Rectangle A10 = new RectangleReadOnly(73f, 105f);

        public static readonly Rectangle B0 = new RectangleReadOnly(2834f, 4008f);

        public static readonly Rectangle B1 = new RectangleReadOnly(2004f, 2834f);

        public static readonly Rectangle B2 = new RectangleReadOnly(1417f, 2004f);

        public static readonly Rectangle B3 = new RectangleReadOnly(1000f, 1417f);

        public static readonly Rectangle B4 = new RectangleReadOnly(708f, 1000f);

        public static readonly Rectangle B5 = new RectangleReadOnly(498f, 708f);

        public static readonly Rectangle B6 = new RectangleReadOnly(354f, 498f);

        public static readonly Rectangle B7 = new RectangleReadOnly(249f, 354f);

        public static readonly Rectangle B8 = new RectangleReadOnly(175f, 249f);

        public static readonly Rectangle B9 = new RectangleReadOnly(124f, 175f);

        public static readonly Rectangle B10 = new RectangleReadOnly(87f, 124f);

        public static readonly Rectangle ARCH_E = new RectangleReadOnly(2592f, 3456f);

        public static readonly Rectangle ARCH_D = new RectangleReadOnly(1728f, 2592f);

        public static readonly Rectangle ARCH_C = new RectangleReadOnly(1296f, 1728f);

        public static readonly Rectangle ARCH_B = new RectangleReadOnly(864f, 1296f);

        public static readonly Rectangle ARCH_A = new RectangleReadOnly(648f, 864f);

        public static readonly Rectangle FLSA = new RectangleReadOnly(612f, 936f);

        public static readonly Rectangle FLSE = new RectangleReadOnly(648f, 936f);

        public static readonly Rectangle HALFLETTER = new RectangleReadOnly(396f, 612f);

        public static readonly Rectangle _11X17 = new RectangleReadOnly(792f, 1224f);

        public static readonly Rectangle ID_1 = new RectangleReadOnly(242.65f, 153f);

        public static readonly Rectangle ID_2 = new RectangleReadOnly(297f, 210f);

        public static readonly Rectangle ID_3 = new RectangleReadOnly(354f, 249f);

        public static readonly Rectangle LEDGER = new RectangleReadOnly(1224f, 792f);

        public static readonly Rectangle CROWN_QUARTO = new RectangleReadOnly(535f, 697f);

        public static readonly Rectangle LARGE_CROWN_QUARTO = new RectangleReadOnly(569f, 731f);

        public static readonly Rectangle DEMY_QUARTO = new RectangleReadOnly(620f, 782f);

        public static readonly Rectangle ROYAL_QUARTO = new RectangleReadOnly(671f, 884f);

        public static readonly Rectangle CROWN_OCTAVO = new RectangleReadOnly(348f, 527f);

        public static readonly Rectangle LARGE_CROWN_OCTAVO = new RectangleReadOnly(365f, 561f);

        public static readonly Rectangle DEMY_OCTAVO = new RectangleReadOnly(391f, 612f);

        public static readonly Rectangle ROYAL_OCTAVO = new RectangleReadOnly(442f, 663f);

        public static readonly Rectangle SMALL_PAPERBACK = new RectangleReadOnly(314f, 504f);

        public static readonly Rectangle PENGUIN_SMALL_PAPERBACK = new RectangleReadOnly(314f, 513f);

        public static readonly Rectangle PENGUIN_LARGE_PAPERBACK = new RectangleReadOnly(365f, 561f);

        [Obsolete]
        public static readonly Rectangle LETTER_LANDSCAPE = new RectangleReadOnly(612f, 792f, 90);

        [Obsolete]
        public static readonly Rectangle LEGAL_LANDSCAPE = new RectangleReadOnly(612f, 1008f, 90);

        [Obsolete]
        public static readonly Rectangle A4_LANDSCAPE = new RectangleReadOnly(595f, 842f, 90);

        public static Rectangle GetRectangle(string name)
        {
            name = name.Trim().ToUpper(CultureInfo.InvariantCulture);
            int num = name.IndexOf(' ');
            if (num == -1)
            {
                try
                {
                    return (Rectangle)typeof(PageSize).GetField(name)!.GetValue(null);
                }
                catch (Exception)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("can.t.find.page.size.1", name));
                }
            }

            try
            {
                string s = name.Substring(0, num);
                return new Rectangle(ury: float.Parse(name.Substring(num + 1), NumberFormatInfo.InvariantInfo), urx: float.Parse(s, NumberFormatInfo.InvariantInfo));
            }
            catch (Exception ex2)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.page.size.format.2", name, ex2.Message));
            }
        }
    }
}
