namespace Sign.itext.text.log
{
    public interface ICounter
    {
        ICounter GetCounter(Type klass);

        void Read(long l);

        void Written(long l);
    }
}
