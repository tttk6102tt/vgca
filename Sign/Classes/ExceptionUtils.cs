using System.Text;

namespace Sign.Classes
{
    internal class ExceptionUtils
    {
        public static string GetExceptionDetails(Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (exception != null)
            {
                Exception currentException = exception;
                stringBuilder.Append("Exception:");
                stringBuilder.Append(Environment.NewLine);

                while (currentException != null)
                {
                    stringBuilder.Append(currentException.Message);
                    stringBuilder.Append(Environment.NewLine);
                    currentException = currentException.InnerException;
                }

                if (exception.Data != null)
                {
                    foreach (object datum in exception.Data)
                    {
                        stringBuilder.Append("Data :");
                        stringBuilder.Append(datum.ToString());
                        stringBuilder.Append(Environment.NewLine);
                    }
                }
                if (exception.StackTrace != null)
                {
                    stringBuilder.Append("StackTrace:");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(exception.StackTrace.ToString());
                    stringBuilder.Append(Environment.NewLine);
                }
                if (exception.Source != null)
                {
                    stringBuilder.Append("Source:");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(exception.Source);
                    stringBuilder.Append(Environment.NewLine);
                }
                if (exception.TargetSite != null)
                {
                    stringBuilder.Append("TargetSite:");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(exception.TargetSite.ToString());
                    stringBuilder.Append(Environment.NewLine);
                }
                if (exception.GetBaseException() != null)
                {
                    stringBuilder.Append("BaseException:");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append(exception.GetBaseException());
                }
            }
            return stringBuilder.ToString();
        }

        public static void LogException(string message, Exception exception)
        {
            try
            {
                string path = string.Format("{0}\\HIPT\\Pdf\\log.txt", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(path));
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                directoryInfo = new DirectoryInfo(Path.GetDirectoryName(path));
                if (directoryInfo.Exists)
                {
                    string contents = string.Format("{0}{1}{0}=========={2}============{0}{3}", Environment.NewLine, message, DateTime.Now.ToString(), GetExceptionDetails(exception));
                    File.AppendAllText(path, contents);
                }
            }
            catch
            {
            }
        }
    }
}
