﻿using Sign.Org.BouncyCastle.Crypto;

namespace Sign.Org.BouncyCastle.Utilities.IO
{
    internal class MacOutputStream : BaseOutputStream
    {
        private readonly IMac mac;

        internal MacOutputStream(IMac mac)
        {
            this.mac = mac;
        }

        public override void Write(byte[] b, int off, int len)
        {
            mac.BlockUpdate(b, off, len);
        }

        public override void WriteByte(byte b)
        {
            mac.Update(b);
        }
    }
}
