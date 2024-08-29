﻿using Sign.Org.BouncyCastle.Asn1;
using Sign.Org.BouncyCastle.Asn1.X9;
using Sign.Org.BouncyCastle.Crypto.Agreement.Kdf;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Security;

namespace Sign.Org.BouncyCastle.Crypto.Agreement
{
    public class ECMqvWithKdfBasicAgreement : ECMqvBasicAgreement
    {
        private readonly string algorithm;

        private readonly IDerivationFunction kdf;

        public ECMqvWithKdfBasicAgreement(string algorithm, IDerivationFunction kdf)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }

            if (kdf == null)
            {
                throw new ArgumentNullException("kdf");
            }

            this.algorithm = algorithm;
            this.kdf = kdf;
        }

        public override BigInteger CalculateAgreement(ICipherParameters pubKey)
        {
            BigInteger r = base.CalculateAgreement(pubKey);
            int defaultKeySize = GeneratorUtilities.GetDefaultKeySize(algorithm);
            DHKdfParameters parameters = new DHKdfParameters(new DerObjectIdentifier(algorithm), defaultKeySize, bigIntToBytes(r));
            kdf.Init(parameters);
            byte[] array = new byte[defaultKeySize / 8];
            kdf.GenerateBytes(array, 0, array.Length);
            return new BigInteger(1, array);
        }

        private byte[] bigIntToBytes(BigInteger r)
        {
            int byteLength = X9IntegerConverter.GetByteLength(privParams.StaticPrivateKey.Parameters.G.X);
            return X9IntegerConverter.IntegerToBytes(r, byteLength);
        }
    }
}
