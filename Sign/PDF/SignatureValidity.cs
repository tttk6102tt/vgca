namespace Sign.PDF
{
    [Flags]
    public enum SignatureValidity
    {
        None = 0x0,
        NotSigned = 0x1,
        DocumentModified = 0x2,
        NotTimestamped = 0x4,
        InvalidTimestampImprint = 0x8,
        InvalidTSACertificate = 0x10,
        InvalidSigningCertificate = 0x20,
        ErrorCheckingSigningCertificate = 0x40,
        ErrorCheckingTSACertificate = 0x80,
        NonCheckingRevokedSigningCert = 0x100,
        NonCheckingRevokedTSACert = 0x200,
        NonCoversWholeDocument = 0x400,
        ErrorCheckingSignature = 0x800,
        FatalError = 0x1000
    }
}
