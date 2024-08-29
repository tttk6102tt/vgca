namespace Sign.itext.text.log
{
    public class LoggerFactory
    {
        private static LoggerFactory myself;

        private ILogger logger = new NoOpLogger();

        static LoggerFactory()
        {
            myself = new LoggerFactory();
        }

        public static ILogger GetLogger(Type klass)
        {
            return myself.logger.GetLogger(klass);
        }

        public static ILogger GetLogger(string name)
        {
            return myself.logger.GetLogger(name);
        }

        public static LoggerFactory GetInstance()
        {
            return myself;
        }

        private LoggerFactory()
        {
        }

        public virtual void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public virtual ILogger Logger()
        {
            return logger;
        }
    }
}
