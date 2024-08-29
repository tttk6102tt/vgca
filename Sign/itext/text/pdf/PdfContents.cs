using Sign.itext.pdf;
using Sign.SystemItext.util.zlib;

namespace Sign.itext.text.pdf
{
    public class PdfContents : PdfStream
    {
        internal static byte[] SAVESTATE = DocWriter.GetISOBytes("q\n");

        internal static byte[] RESTORESTATE = DocWriter.GetISOBytes("Q\n");

        internal static byte[] ROTATE90 = DocWriter.GetISOBytes("0 1 -1 0 ");

        internal static byte[] ROTATE180 = DocWriter.GetISOBytes("-1 0 0 -1 ");

        internal static byte[] ROTATE270 = DocWriter.GetISOBytes("0 -1 1 0 ");

        internal static byte[] ROTATEFINAL = DocWriter.GetISOBytes(" cm\n");

        internal PdfContents(PdfContentByte under, PdfContentByte content, PdfContentByte text, PdfContentByte secondContent, Rectangle page)
        {
            Stream stream = null;
            streamBytes = new MemoryStream();
            if (Document.Compress)
            {
                compressed = true;
                stream = new ZDeflaterOutputStream(level: text?.PdfWriter.CompressionLevel ?? content.PdfWriter.CompressionLevel, outp: streamBytes);
            }
            else
            {
                stream = streamBytes;
            }

            switch (page.Rotation)
            {
                case 90:
                    {
                        stream.Write(ROTATE90, 0, ROTATE90.Length);
                        byte[] iSOBytes = DocWriter.GetISOBytes(ByteBuffer.FormatDouble(page.Top));
                        stream.Write(iSOBytes, 0, iSOBytes.Length);
                        stream.WriteByte(32);
                        stream.WriteByte(48);
                        stream.Write(ROTATEFINAL, 0, ROTATEFINAL.Length);
                        break;
                    }
                case 180:
                    {
                        stream.Write(ROTATE180, 0, ROTATE180.Length);
                        byte[] iSOBytes = DocWriter.GetISOBytes(ByteBuffer.FormatDouble(page.Right));
                        stream.Write(iSOBytes, 0, iSOBytes.Length);
                        stream.WriteByte(32);
                        iSOBytes = DocWriter.GetISOBytes(ByteBuffer.FormatDouble(page.Top));
                        stream.Write(iSOBytes, 0, iSOBytes.Length);
                        stream.Write(ROTATEFINAL, 0, ROTATEFINAL.Length);
                        break;
                    }
                case 270:
                    {
                        stream.Write(ROTATE270, 0, ROTATE270.Length);
                        stream.WriteByte(48);
                        stream.WriteByte(32);
                        byte[] iSOBytes = DocWriter.GetISOBytes(ByteBuffer.FormatDouble(page.Right));
                        stream.Write(iSOBytes, 0, iSOBytes.Length);
                        stream.Write(ROTATEFINAL, 0, ROTATEFINAL.Length);
                        break;
                    }
            }

            if (under.Size > 0)
            {
                stream.Write(SAVESTATE, 0, SAVESTATE.Length);
                under.InternalBuffer.WriteTo(stream);
                stream.Write(RESTORESTATE, 0, RESTORESTATE.Length);
            }

            if (content.Size > 0)
            {
                stream.Write(SAVESTATE, 0, SAVESTATE.Length);
                content.InternalBuffer.WriteTo(stream);
                stream.Write(RESTORESTATE, 0, RESTORESTATE.Length);
            }

            if (text != null)
            {
                stream.Write(SAVESTATE, 0, SAVESTATE.Length);
                text.InternalBuffer.WriteTo(stream);
                stream.Write(RESTORESTATE, 0, RESTORESTATE.Length);
            }

            if (secondContent.Size > 0)
            {
                secondContent.InternalBuffer.WriteTo(stream);
            }

            if (stream is ZDeflaterOutputStream)
            {
                ((ZDeflaterOutputStream)stream).Finish();
            }

            Put(PdfName.LENGTH, new PdfNumber(streamBytes.Length));
            if (compressed)
            {
                Put(PdfName.FILTER, PdfName.FLATEDECODE);
            }
        }
    }
}
