﻿using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Cms
{
    internal class CounterSignatureDigestCalculator : IDigestCalculator
    {
        private readonly string alg;

        private readonly byte[] data;

        internal CounterSignatureDigestCalculator(string alg, byte[] data)
        {
            this.alg = alg;
            this.data = data;
        }

        public byte[] GetDigest()
        {
            return DigestUtilities.DoFinal(CmsSignedHelper.Instance.GetDigestInstance(alg), data);
        }
    }
}
