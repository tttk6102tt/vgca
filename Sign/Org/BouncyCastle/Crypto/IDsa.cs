﻿using Sign.Org.BouncyCastle.Math;

namespace Sign.Org.BouncyCastle.Crypto
{
    public interface IDsa
    {
        string AlgorithmName { get; }

        void Init(bool forSigning, ICipherParameters parameters);

        BigInteger[] GenerateSignature(byte[] message);

        bool VerifySignature(byte[] message, BigInteger r, BigInteger s);
    }
}
