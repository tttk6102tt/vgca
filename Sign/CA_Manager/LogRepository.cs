namespace Sign.CA_Manager
{
    public class LogRepository
    {
        /// <summary>
        /// Lưu dữ liệu
        /// </summary>
        public void Save(string path, string content, string exceptionMessage, string stackTrace)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("--- Cuộn xuống dưới để thấy được dữ liệu mới nhất -------------------------------------------------");
                    }
                }
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("Content: " + content);
                    sw.WriteLine("ExceptionMessage: " + exceptionMessage);
                    sw.WriteLine("ExceptionStackTrace: " + stackTrace);
                    sw.WriteLine("CreateTime: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"));
                    sw.WriteLine("-------------------------------------------------------------------------------------------------------");
                    sw.WriteLine("");
                }
            }
            catch
            {
            }
        }
    }
}
