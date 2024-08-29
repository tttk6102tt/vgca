using System.Drawing;

namespace Sign.PDF
{
    public struct SignatureInfo
    {
        public string SignatureName;

        public bool SignatureCoversWholeDocument;

        public DateTime SigningTime;

        public DateTime TimeStampDate;

        public byte[] SigningCertificate;

        public byte[] TimeStampCertificate;

        public Rectangle Position;

        public int PageIndex;

        public Dictionary<SignatureValidity, string> ValidityErrors;

        public bool IsTsp;

    }
}
