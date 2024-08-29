using Sign.Org.BouncyCastle.Asn1.Utilities.Collections;
using Sign.Org.BouncyCastle.Crypto.Parameters;
using Sign.Org.BouncyCastle.Math;
using Sign.Org.BouncyCastle.Math.EC;
using Sign.Org.BouncyCastle.Utilities;
using System.Collections;

namespace Sign.Org.BouncyCastle.Asn1.CryptoPro
{
    public sealed class ECGost3410NamedCurves
    {
        internal static readonly IDictionary objIds;

        internal static readonly IDictionary parameters;

        internal static readonly IDictionary names;

        public static IEnumerable Names => new EnumerableProxy(objIds.Keys);

        private ECGost3410NamedCurves()
        {
        }

        static ECGost3410NamedCurves()
        {
            objIds = Platform.CreateHashtable();
            parameters = Platform.CreateHashtable();
            names = Platform.CreateHashtable();
            BigInteger q = new BigInteger("115792089237316195423570985008687907853269984665640564039457584007913129639319");
            BigInteger n = new BigInteger("115792089237316195423570985008687907853073762908499243225378155805079068850323");
            FpCurve fpCurve = new FpCurve(q, new BigInteger("115792089237316195423570985008687907853269984665640564039457584007913129639316"), new BigInteger("166"));
            ECDomainParameters value = new ECDomainParameters(fpCurve, fpCurve.CreatePoint(BigInteger.One, new BigInteger("64033881142927202683649881450433473985931760268884941288852745803908878638612"), withCompression: false), n);
            parameters[CryptoProObjectIdentifiers.GostR3410x2001CryptoProA] = value;
            BigInteger q2 = new BigInteger("115792089237316195423570985008687907853269984665640564039457584007913129639319");
            n = new BigInteger("115792089237316195423570985008687907853073762908499243225378155805079068850323");
            fpCurve = new FpCurve(q2, new BigInteger("115792089237316195423570985008687907853269984665640564039457584007913129639316"), new BigInteger("166"));
            value = new ECDomainParameters(fpCurve, fpCurve.CreatePoint(BigInteger.One, new BigInteger("64033881142927202683649881450433473985931760268884941288852745803908878638612"), withCompression: false), n);
            parameters[CryptoProObjectIdentifiers.GostR3410x2001CryptoProXchA] = value;
            BigInteger q3 = new BigInteger("57896044618658097711785492504343953926634992332820282019728792003956564823193");
            n = new BigInteger("57896044618658097711785492504343953927102133160255826820068844496087732066703");
            fpCurve = new FpCurve(q3, new BigInteger("57896044618658097711785492504343953926634992332820282019728792003956564823190"), new BigInteger("28091019353058090096996979000309560759124368558014865957655842872397301267595"));
            value = new ECDomainParameters(fpCurve, fpCurve.CreatePoint(BigInteger.One, new BigInteger("28792665814854611296992347458380284135028636778229113005756334730996303888124"), withCompression: false), n);
            parameters[CryptoProObjectIdentifiers.GostR3410x2001CryptoProB] = value;
            BigInteger q4 = new BigInteger("70390085352083305199547718019018437841079516630045180471284346843705633502619");
            n = new BigInteger("70390085352083305199547718019018437840920882647164081035322601458352298396601");
            fpCurve = new FpCurve(q4, new BigInteger("70390085352083305199547718019018437841079516630045180471284346843705633502616"), new BigInteger("32858"));
            value = new ECDomainParameters(fpCurve, fpCurve.CreatePoint(BigInteger.Zero, new BigInteger("29818893917731240733471273240314769927240550812383695689146495261604565990247"), withCompression: false), n);
            parameters[CryptoProObjectIdentifiers.GostR3410x2001CryptoProXchB] = value;
            BigInteger q5 = new BigInteger("70390085352083305199547718019018437841079516630045180471284346843705633502619");
            n = new BigInteger("70390085352083305199547718019018437840920882647164081035322601458352298396601");
            fpCurve = new FpCurve(q5, new BigInteger("70390085352083305199547718019018437841079516630045180471284346843705633502616"), new BigInteger("32858"));
            value = new ECDomainParameters(fpCurve, fpCurve.CreatePoint(BigInteger.Zero, new BigInteger("29818893917731240733471273240314769927240550812383695689146495261604565990247"), withCompression: false), n);
            parameters[CryptoProObjectIdentifiers.GostR3410x2001CryptoProC] = value;
            objIds["GostR3410-2001-CryptoPro-A"] = CryptoProObjectIdentifiers.GostR3410x2001CryptoProA;
            objIds["GostR3410-2001-CryptoPro-B"] = CryptoProObjectIdentifiers.GostR3410x2001CryptoProB;
            objIds["GostR3410-2001-CryptoPro-C"] = CryptoProObjectIdentifiers.GostR3410x2001CryptoProC;
            objIds["GostR3410-2001-CryptoPro-XchA"] = CryptoProObjectIdentifiers.GostR3410x2001CryptoProXchA;
            objIds["GostR3410-2001-CryptoPro-XchB"] = CryptoProObjectIdentifiers.GostR3410x2001CryptoProXchB;
            names[CryptoProObjectIdentifiers.GostR3410x2001CryptoProA] = "GostR3410-2001-CryptoPro-A";
            names[CryptoProObjectIdentifiers.GostR3410x2001CryptoProB] = "GostR3410-2001-CryptoPro-B";
            names[CryptoProObjectIdentifiers.GostR3410x2001CryptoProC] = "GostR3410-2001-CryptoPro-C";
            names[CryptoProObjectIdentifiers.GostR3410x2001CryptoProXchA] = "GostR3410-2001-CryptoPro-XchA";
            names[CryptoProObjectIdentifiers.GostR3410x2001CryptoProXchB] = "GostR3410-2001-CryptoPro-XchB";
        }

        public static ECDomainParameters GetByOid(DerObjectIdentifier oid)
        {
            return (ECDomainParameters)parameters[oid];
        }

        public static ECDomainParameters GetByName(string name)
        {
            DerObjectIdentifier derObjectIdentifier = (DerObjectIdentifier)objIds[name];
            if (derObjectIdentifier != null)
            {
                return (ECDomainParameters)parameters[derObjectIdentifier];
            }

            return null;
        }

        public static string GetName(DerObjectIdentifier oid)
        {
            return (string)names[oid];
        }

        public static DerObjectIdentifier GetOid(string name)
        {
            return (DerObjectIdentifier)objIds[name];
        }
    }
}
