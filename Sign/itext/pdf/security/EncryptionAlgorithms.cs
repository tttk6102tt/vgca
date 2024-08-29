namespace Sign.itext.pdf.security
{
    public static class EncryptionAlgorithms
    {
        private static readonly Dictionary<string, string> algorithmNames;

        static EncryptionAlgorithms()
        {
            algorithmNames = new Dictionary<string, string>();
            algorithmNames["1.2.840.113549.1.1.1"] = "RSA";
            algorithmNames["1.2.840.10040.4.1"] = "DSA";
            algorithmNames["1.2.840.113549.1.1.2"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.4"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.5"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.14"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.11"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.12"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.13"] = "RSA";
            algorithmNames["1.2.840.10040.4.3"] = "DSA";
            algorithmNames["2.16.840.1.101.3.4.3.1"] = "DSA";
            algorithmNames["2.16.840.1.101.3.4.3.2"] = "DSA";
            algorithmNames["1.3.14.3.2.29"] = "RSA";
            algorithmNames["1.3.36.3.3.1.2"] = "RSA";
            algorithmNames["1.3.36.3.3.1.3"] = "RSA";
            algorithmNames["1.3.36.3.3.1.4"] = "RSA";
            algorithmNames["1.2.643.2.2.19"] = "ECGOST3410";
        }

        public static string GetAlgorithm(string oid)
        {
            if (algorithmNames.TryGetValue(oid, out var value))
            {
                return value;
            }

            return oid;
        }
    }
}
