﻿using Sign.Org.BouncyCastle.Math;

namespace Sign.Org.BouncyCastle.Crypto.Parameters
{
    public class RsaBlindingParameters : ICipherParameters
    {
        private readonly RsaKeyParameters publicKey;

        private readonly BigInteger blindingFactor;

        public RsaKeyParameters PublicKey => publicKey;

        public BigInteger BlindingFactor => blindingFactor;

        public RsaBlindingParameters(RsaKeyParameters publicKey, BigInteger blindingFactor)
        {
            if (publicKey.IsPrivate)
            {
                throw new ArgumentException("RSA parameters should be for a public key");
            }

            this.publicKey = publicKey;
            this.blindingFactor = blindingFactor;
        }
    }
}
