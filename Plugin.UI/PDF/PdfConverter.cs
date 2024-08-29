using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;

namespace Plugin.UI.PDF
{
    public class PdfConverter
    {
        public static void Word2PDF(string fileName)
        {
            string destFilename = string.Format("{0}\\{1}.pdf", (object)Path.GetDirectoryName(fileName), (object)Path.GetFileNameWithoutExtension(fileName));
            PdfConverter.Word2PDF(fileName, destFilename);
        }

        public static void Word2PDF(string fileName, string destFilename)
        {
            Microsoft.Office.Interop.Word.Application applicationClass = new Microsoft.Office.Interop.Word.Application();
            Document document = (Document)null;
            object FileName = (object)fileName;
            object missing = Type.Missing;
            string OutputFileName = destFilename;
            WdExportFormat ExportFormat = WdExportFormat.wdExportFormatPDF;
            bool OpenAfterExport = false;
            WdExportOptimizeFor OptimizeFor = WdExportOptimizeFor.wdExportOptimizeForPrint;
            WdExportRange Range = WdExportRange.wdExportAllDocument;
            int From = 0;
            int To = 0;
            WdExportItem wdExportItem = WdExportItem.wdExportDocumentContent;
            bool IncludeDocProps = true;
            bool KeepIRM = true;
            WdExportCreateBookmarks CreateBookmarks = WdExportCreateBookmarks.wdExportCreateWordBookmarks;
            bool DocStructureTags = true;
            bool BitmapMissingFonts = true;
            bool UseISO19005_1 = false;
            try
            {
                document = applicationClass.Documents.Open(ref FileName, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
                if (document == null)
                    return;
                document.ExportAsFixedFormat(OutputFileName, ExportFormat, OpenAfterExport, OptimizeFor, Range, From, To, wdExportItem, IncludeDocProps, KeepIRM, CreateBookmarks, DocStructureTags, BitmapMissingFonts, UseISO19005_1, ref missing);
            }
            catch (InvalidCastException ex)
            {
                throw new Exception("Module chuyển đổi chưa được đăng ký với hệ thống.\n\rĐể sử dụng được tính năng này,phần mềm MS Office cần phải được sửa chữa lại.");
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode != -2147467259)
                    throw new Exception("Trên máy tình cần cài đặt MS Word 2007 hoặc phiên bản mới hơn để sử dụng tình năng này.");
                throw new Exception("Module chuyển đổi chưa được cài đặt trên hệ thống.");
            }
            catch (Exception ex)
            {
                throw new Exception("Trên máy tình cần cài đặt MS Word 2007 hoặc phiên bản mới hơn để sử dụng tình năng này.");
            }
            finally
            {
                if (document != null)
                {
                    document.Close(ref missing, ref missing, ref missing);
                }
                if (applicationClass != null)
                {
                    applicationClass.Quit(ref missing, ref missing, ref missing);
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public static void PowerPoint2PDF(string fileName, string destFilename)
        {

        }

        public static void Excel2PDF(string fileName, string destFilename)
        {
            string Filename1 = fileName;
            object missing = Type.Missing;
            object Filename2 = (object)destFilename;
            Microsoft.Office.Interop.Excel.Application applicationClass = (Microsoft.Office.Interop.Excel.Application)null;
            Workbook workbook = (Workbook)null;
            try
            {
                applicationClass = new Microsoft.Office.Interop.Excel.Application();
                workbook = applicationClass.Workbooks.Open(Filename1, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);
                workbook.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, Filename2, (object)XlFixedFormatQuality.xlQualityStandard, (object)true, (object)false, missing, missing, missing, missing);
            }
            catch (InvalidCastException ex)
            {
                throw new Exception("Module chuyển đổi chưa được đăng ký với hệ thống.\n\rĐể sử dụng được tính năng này,phần mềm MS Office cần phải được sửa chữa lại.");
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode != -2147467259)
                    throw new Exception("Trên máy tình cần cài đặt MS Word 2007 hoặc phiên bản mới hơn để sử dụng tình năng này.");
                throw new Exception("Module chuyển đổi chưa được cài đặt trên hệ thống.");
            }
            catch (Exception ex)
            {
                throw new Exception("Trên máy tình cần cài đặt MS Office 2007 hoặc phiên bản mới hơn để sử dụng tình năng này.");
            }
            finally
            {
                workbook?.Close((object)true, missing, missing);
                if (applicationClass != null)
                {
                    applicationClass.Quit();
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }


        public static void Excel2PDF_bk(string fileName, string destFilename)
        {
            Microsoft.Office.Interop.Excel.XlFixedFormatType Type = Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF;
            string Filename1 = fileName;
            object missing = null;
            object Filename2 = (object)destFilename;
            Microsoft.Office.Interop.Excel.Application applicationClass = (Microsoft.Office.Interop.Excel.Application)null;
            Workbook workbook = (Workbook)null;
            try
            {
                applicationClass = new Microsoft.Office.Interop.Excel.Application();
                workbook = applicationClass.Workbooks.Open(Filename1, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);
                workbook.ExportAsFixedFormat(Type, Filename2, (object)XlFixedFormatQuality.xlQualityStandard, (object)true, (object)false, missing, missing, missing, missing);
            }
            catch (InvalidCastException ex)
            {
                throw new Exception("Module chuyển đổi chưa được đăng ký với hệ thống.\n\rĐể sử dụng được tính năng này,phần mềm MS Office cần phải được sửa chữa lại.");
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode != -2147467259)
                    throw new Exception("Trên máy tình cần cài đặt MS Word 2007 hoặc phiên bản mới hơn để sử dụng tình năng này.");
                throw new Exception("Module chuyển đổi chưa được cài đặt trên hệ thống.");
            }
            catch (Exception ex)
            {
                throw new Exception("Trên máy tình cần cài đặt MS Office 2007 hoặc phiên bản mới hơn để sử dụng tình năng này.");
            }
            finally
            {
                workbook?.Close((object)true, missing, missing);
                if (applicationClass != null)
                {
                    applicationClass.Quit();
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
