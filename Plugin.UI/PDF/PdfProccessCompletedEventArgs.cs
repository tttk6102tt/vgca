using Sign.PDF;

namespace Plugin.UI.PDF
{
    public class PdfProccessCompletedEventArgs : EventArgs
    {
        private ValidityProccess _validityProccess;

        private SignatureValidity _signatureValidity;

        private string _message;

        public ValidityProccess Proccess
        {
            get
            {
                return _validityProccess;
            }
        }

        public SignatureValidity State
        {
            get
            {
                return _signatureValidity;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public PdfProccessCompletedEventArgs(ValidityProccess validityProccess, SignatureValidity signatureValidity, string message)
        {
            _validityProccess = validityProccess;
            _signatureValidity = signatureValidity;
            _message = message;
        }

        public PdfProccessCompletedEventArgs(ValidityProccess validityProccess, SignatureValidity signatureValidity)
        {
            _validityProccess = validityProccess;
            _signatureValidity = signatureValidity;
            _message = "";
        }
    }
}
