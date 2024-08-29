namespace Sign.itext.text.log
{
    public class CounterFactory
    {
        private static CounterFactory myself;

        private ICounter counter = new NoOpCounter();

        static CounterFactory()
        {
            myself = new CounterFactory();
        }

        private CounterFactory()
        {
        }

        public static CounterFactory getInstance()
        {
            return myself;
        }

        public static ICounter GetCounter(Type klass)
        {
            return myself.counter.GetCounter(klass);
        }

        public virtual ICounter GetCounter()
        {
            return counter;
        }

        public virtual void SetCounter(ICounter counter)
        {
            this.counter = counter;
        }
    }
}
