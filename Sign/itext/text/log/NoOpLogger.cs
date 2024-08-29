namespace Sign.itext.text.log
{
    public sealed class NoOpLogger : ILogger
    {
        public ILogger GetLogger(Type name)
        {
            return this;
        }

        public void Warn(string message)
        {
        }

        public void Trace(string message)
        {
        }

        public void Debug(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Error(string message, Exception e)
        {
        }

        public bool IsLogging(Level level)
        {
            return false;
        }

        public void Error(string message)
        {
        }

        public ILogger GetLogger(string name)
        {
            return this;
        }
    }
}
