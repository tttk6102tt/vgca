﻿namespace Sign.Org.BouncyCastle.Security.Certificates
{
    [Serializable]
    public class CertificateNotYetValidException : CertificateException
    {
        public CertificateNotYetValidException()
        {
        }

        public CertificateNotYetValidException(string message)
            : base(message)
        {
        }

        public CertificateNotYetValidException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
