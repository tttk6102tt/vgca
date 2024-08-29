namespace Sign.itext.text.log
{
    public interface ILogger
    {
        ILogger GetLogger(Type klass);

        ILogger GetLogger(string name);

        bool IsLogging(Level level);

        void Warn(string message);

        void Trace(string message);

        void Debug(string message);

        void Info(string message);

        void Error(string message);

        void Error(string message, Exception e);
    }
}
