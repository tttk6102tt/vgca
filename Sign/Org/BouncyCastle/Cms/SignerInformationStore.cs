﻿using Sign.Org.BouncyCastle.Asn1.Cms;
using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Cms
{
    public class SignerInformationStore
    {
        private readonly IList all;

        private readonly IDictionary table = Platform.CreateHashtable();

        public int Count => all.Count;

        public SignerInformationStore(ICollection signerInfos)
        {
            foreach (SignerInformation signerInfo in signerInfos)
            {
                SignerID signerID = signerInfo.SignerID;
                IList list = (IList)table[signerID];
                if (list == null)
                {
                    list = (IList)(table[signerID] = Platform.CreateArrayList(1));
                }

                list.Add(signerInfo);
            }

            all = Platform.CreateArrayList(signerInfos);
        }

        public SignerInformation GetFirstSigner(SignerID selector)
        {
            IList list = (IList)table[selector];
            if (list != null)
            {
                return (SignerInformation)list[0];
            }

            return null;
        }

        public ICollection GetSigners()
        {
            return Platform.CreateArrayList(all);
        }

        public ICollection GetSigners(SignerID selector)
        {
            IList list = (IList)table[selector];
            if (list != null)
            {
                return Platform.CreateArrayList(list);
            }

            return Platform.CreateArrayList();
        }
    }
}
