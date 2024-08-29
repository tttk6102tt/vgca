namespace Sign.itext.text.log
{
    public class NoOpCounter : ICounter
    {
        public virtual ICounter GetCounter(Type klass)
        {
            return this;
        }

        public virtual void Read(long l)
        {
        }

        public virtual void Written(long l)
        {
        }
    }
}
