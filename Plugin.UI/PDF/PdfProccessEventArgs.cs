namespace Plugin.UI.PDF
{
    public class PdfProccessEventArgs : EventArgs
    {
        private ValidityProccess ValueProcess;

        public ValidityProccess ValidateProccess
        {
            get
            {
                return ValueProcess;
            }
        }

        public PdfProccessEventArgs(ValidityProccess p)
        {
            ValueProcess = p;
        }
    }
}
