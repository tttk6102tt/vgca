namespace Sign.Org.BouncyCastle.Math.EC.Multiplier
{
    internal class WNafMultiplier : ECMultiplier
    {
        public sbyte[] WindowNaf(sbyte width, BigInteger k)
        {
            sbyte[] array = new sbyte[k.BitLength + 1];
            short num = (short)(1 << (int)width);
            BigInteger m = BigInteger.ValueOf(num);
            int num2 = 0;
            int num3 = 0;
            while (k.SignValue > 0)
            {
                if (k.TestBit(0))
                {
                    BigInteger bigInteger = k.Mod(m);
                    if (bigInteger.TestBit(width - 1))
                    {
                        array[num2] = (sbyte)(bigInteger.IntValue - num);
                    }
                    else
                    {
                        array[num2] = (sbyte)bigInteger.IntValue;
                    }

                    k = k.Subtract(BigInteger.ValueOf(array[num2]));
                    num3 = num2;
                }
                else
                {
                    array[num2] = 0;
                }

                k = k.ShiftRight(1);
                num2++;
            }

            num3++;
            sbyte[] array2 = new sbyte[num3];
            Array.Copy(array, 0, array2, 0, num3);
            return array2;
        }

        public ECPoint Multiply(ECPoint p, BigInteger k, PreCompInfo preCompInfo)
        {
            WNafPreCompInfo wNafPreCompInfo = ((preCompInfo == null || !(preCompInfo is WNafPreCompInfo)) ? new WNafPreCompInfo() : ((WNafPreCompInfo)preCompInfo));
            int bitLength = k.BitLength;
            sbyte width;
            int num;
            if (bitLength < 13)
            {
                width = 2;
                num = 1;
            }
            else if (bitLength < 41)
            {
                width = 3;
                num = 2;
            }
            else if (bitLength < 121)
            {
                width = 4;
                num = 4;
            }
            else if (bitLength < 337)
            {
                width = 5;
                num = 8;
            }
            else if (bitLength < 897)
            {
                width = 6;
                num = 16;
            }
            else if (bitLength < 2305)
            {
                width = 7;
                num = 32;
            }
            else
            {
                width = 8;
                num = 127;
            }

            int num2 = 1;
            ECPoint[] array = wNafPreCompInfo.GetPreComp();
            ECPoint eCPoint = wNafPreCompInfo.GetTwiceP();
            if (array == null)
            {
                array = new ECPoint[1] { p };
            }
            else
            {
                num2 = array.Length;
            }

            if (eCPoint == null)
            {
                eCPoint = p.Twice();
            }

            if (num2 < num)
            {
                ECPoint[] sourceArray = array;
                array = new ECPoint[num];
                Array.Copy(sourceArray, 0, array, 0, num2);
                for (int i = num2; i < num; i++)
                {
                    array[i] = eCPoint.Add(array[i - 1]);
                }
            }

            sbyte[] array2 = WindowNaf(width, k);
            int num3 = array2.Length;
            ECPoint eCPoint2 = p.Curve.Infinity;
            for (int num4 = num3 - 1; num4 >= 0; num4--)
            {
                eCPoint2 = eCPoint2.Twice();
                if (array2[num4] != 0)
                {
                    eCPoint2 = ((array2[num4] <= 0) ? eCPoint2.Subtract(array[(-array2[num4] - 1) / 2]) : eCPoint2.Add(array[(array2[num4] - 1) / 2]));
                }
            }

            wNafPreCompInfo.SetPreComp(array);
            wNafPreCompInfo.SetTwiceP(eCPoint);
            p.SetPreCompInfo(wNafPreCompInfo);
            return eCPoint2;
        }
    }
}
