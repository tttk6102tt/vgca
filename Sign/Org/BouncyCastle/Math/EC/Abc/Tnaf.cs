namespace Sign.Org.BouncyCastle.Math.EC.Abc
{
    internal class Tnaf
    {
        private static readonly BigInteger MinusOne = BigInteger.One.Negate();

        private static readonly BigInteger MinusTwo = BigInteger.Two.Negate();

        private static readonly BigInteger MinusThree = BigInteger.Three.Negate();

        private static readonly BigInteger Four = BigInteger.ValueOf(4L);

        public const sbyte Width = 4;

        public const sbyte Pow2Width = 16;

        public static readonly ZTauElement[] Alpha0 = new ZTauElement[9]
        {
            null,
            new ZTauElement(BigInteger.One, BigInteger.Zero),
            null,
            new ZTauElement(MinusThree, MinusOne),
            null,
            new ZTauElement(MinusOne, MinusOne),
            null,
            new ZTauElement(BigInteger.One, MinusOne),
            null
        };

        public static readonly sbyte[][] Alpha0Tnaf = new sbyte[8][]
        {
            null,
            new sbyte[1] { 1 },
            null,
            new sbyte[3] { -1, 0, 1 },
            null,
            new sbyte[3] { 1, 0, 1 },
            null,
            new sbyte[4] { -1, 0, 0, 1 }
        };

        public static readonly ZTauElement[] Alpha1 = new ZTauElement[9]
        {
            null,
            new ZTauElement(BigInteger.One, BigInteger.Zero),
            null,
            new ZTauElement(MinusThree, BigInteger.One),
            null,
            new ZTauElement(MinusOne, BigInteger.One),
            null,
            new ZTauElement(BigInteger.One, BigInteger.One),
            null
        };

        public static readonly sbyte[][] Alpha1Tnaf = new sbyte[8][]
        {
            null,
            new sbyte[1] { 1 },
            null,
            new sbyte[3] { -1, 0, 1 },
            null,
            new sbyte[3] { 1, 0, 1 },
            null,
            new sbyte[4] { -1, 0, 0, -1 }
        };

        public static BigInteger Norm(sbyte mu, ZTauElement lambda)
        {
            BigInteger bigInteger = lambda.u.Multiply(lambda.u);
            BigInteger bigInteger2 = lambda.u.Multiply(lambda.v);
            BigInteger value = lambda.v.Multiply(lambda.v).ShiftLeft(1);
            return mu switch
            {
                1 => bigInteger.Add(bigInteger2).Add(value),
                -1 => bigInteger.Subtract(bigInteger2).Add(value),
                _ => throw new ArgumentException("mu must be 1 or -1"),
            };
        }

        public static SimpleBigDecimal Norm(sbyte mu, SimpleBigDecimal u, SimpleBigDecimal v)
        {
            SimpleBigDecimal simpleBigDecimal = u.Multiply(u);
            SimpleBigDecimal b = u.Multiply(v);
            SimpleBigDecimal b2 = v.Multiply(v).ShiftLeft(1);
            return mu switch
            {
                1 => simpleBigDecimal.Add(b).Add(b2),
                -1 => simpleBigDecimal.Subtract(b).Add(b2),
                _ => throw new ArgumentException("mu must be 1 or -1"),
            };
        }

        public static ZTauElement Round(SimpleBigDecimal lambda0, SimpleBigDecimal lambda1, sbyte mu)
        {
            int scale = lambda0.Scale;
            if (lambda1.Scale != scale)
            {
                throw new ArgumentException("lambda0 and lambda1 do not have same scale");
            }

            if (mu != 1 && mu != -1)
            {
                throw new ArgumentException("mu must be 1 or -1");
            }

            BigInteger bigInteger = lambda0.Round();
            BigInteger bigInteger2 = lambda1.Round();
            SimpleBigDecimal simpleBigDecimal = lambda0.Subtract(bigInteger);
            SimpleBigDecimal simpleBigDecimal2 = lambda1.Subtract(bigInteger2);
            SimpleBigDecimal simpleBigDecimal3 = simpleBigDecimal.Add(simpleBigDecimal);
            simpleBigDecimal3 = ((mu != 1) ? simpleBigDecimal3.Subtract(simpleBigDecimal2) : simpleBigDecimal3.Add(simpleBigDecimal2));
            SimpleBigDecimal simpleBigDecimal4 = simpleBigDecimal2.Add(simpleBigDecimal2).Add(simpleBigDecimal2);
            SimpleBigDecimal b = simpleBigDecimal4.Add(simpleBigDecimal2);
            SimpleBigDecimal simpleBigDecimal5;
            SimpleBigDecimal simpleBigDecimal6;
            if (mu == 1)
            {
                simpleBigDecimal5 = simpleBigDecimal.Subtract(simpleBigDecimal4);
                simpleBigDecimal6 = simpleBigDecimal.Add(b);
            }
            else
            {
                simpleBigDecimal5 = simpleBigDecimal.Add(simpleBigDecimal4);
                simpleBigDecimal6 = simpleBigDecimal.Subtract(b);
            }

            sbyte b2 = 0;
            sbyte b3 = 0;
            if (simpleBigDecimal3.CompareTo(BigInteger.One) >= 0)
            {
                if (simpleBigDecimal5.CompareTo(MinusOne) < 0)
                {
                    b3 = mu;
                }
                else
                {
                    b2 = 1;
                }
            }
            else if (simpleBigDecimal6.CompareTo(BigInteger.Two) >= 0)
            {
                b3 = mu;
            }

            if (simpleBigDecimal3.CompareTo(MinusOne) < 0)
            {
                if (simpleBigDecimal5.CompareTo(BigInteger.One) >= 0)
                {
                    b3 = (sbyte)(-mu);
                }
                else
                {
                    b2 = -1;
                }
            }
            else if (simpleBigDecimal6.CompareTo(MinusTwo) < 0)
            {
                b3 = (sbyte)(-mu);
            }

            BigInteger u = bigInteger.Add(BigInteger.ValueOf(b2));
            BigInteger v = bigInteger2.Add(BigInteger.ValueOf(b3));
            return new ZTauElement(u, v);
        }

        public static SimpleBigDecimal ApproximateDivisionByN(BigInteger k, BigInteger s, BigInteger vm, sbyte a, int m, int c)
        {
            int num = (m + 5) / 2 + c;
            BigInteger val = k.ShiftRight(m - num - 2 + a);
            BigInteger bigInteger = s.Multiply(val);
            BigInteger val2 = bigInteger.ShiftRight(m);
            BigInteger value = vm.Multiply(val2);
            BigInteger bigInteger2 = bigInteger.Add(value);
            BigInteger bigInteger3 = bigInteger2.ShiftRight(num - c);
            if (bigInteger2.TestBit(num - c - 1))
            {
                bigInteger3 = bigInteger3.Add(BigInteger.One);
            }

            return new SimpleBigDecimal(bigInteger3, c);
        }

        public static sbyte[] TauAdicNaf(sbyte mu, ZTauElement lambda)
        {
            if (mu != 1 && mu != -1)
            {
                throw new ArgumentException("mu must be 1 or -1");
            }

            int bitLength = Norm(mu, lambda).BitLength;
            sbyte[] array = new sbyte[(bitLength > 30) ? (bitLength + 4) : 34];
            int num = 0;
            int num2 = 0;
            BigInteger bigInteger = lambda.u;
            BigInteger bigInteger2 = lambda.v;
            while (!bigInteger.Equals(BigInteger.Zero) || !bigInteger2.Equals(BigInteger.Zero))
            {
                if (bigInteger.TestBit(0))
                {
                    array[num] = (sbyte)BigInteger.Two.Subtract(bigInteger.Subtract(bigInteger2.ShiftLeft(1)).Mod(Four)).IntValue;
                    bigInteger = ((array[num] != 1) ? bigInteger.Add(BigInteger.One) : bigInteger.ClearBit(0));
                    num2 = num;
                }
                else
                {
                    array[num] = 0;
                }

                BigInteger bigInteger3 = bigInteger;
                BigInteger bigInteger4 = bigInteger.ShiftRight(1);
                bigInteger = ((mu != 1) ? bigInteger2.Subtract(bigInteger4) : bigInteger2.Add(bigInteger4));
                bigInteger2 = bigInteger3.ShiftRight(1).Negate();
                num++;
            }

            num2++;
            sbyte[] array2 = new sbyte[num2];
            Array.Copy(array, 0, array2, 0, num2);
            return array2;
        }

        public static F2mPoint Tau(F2mPoint p)
        {
            if (p.IsInfinity)
            {
                return p;
            }

            ECFieldElement x = p.X;
            ECFieldElement y = p.Y;
            return new F2mPoint(p.Curve, x.Square(), y.Square(), p.IsCompressed);
        }

        public static sbyte GetMu(F2mCurve curve)
        {
            BigInteger bigInteger = curve.A.ToBigInteger();
            if (bigInteger.SignValue == 0)
            {
                return -1;
            }

            if (bigInteger.Equals(BigInteger.One))
            {
                return 1;
            }

            throw new ArgumentException("No Koblitz curve (ABC), TNAF multiplication not possible");
        }

        public static BigInteger[] GetLucas(sbyte mu, int k, bool doV)
        {
            if (mu != 1 && mu != -1)
            {
                throw new ArgumentException("mu must be 1 or -1");
            }

            BigInteger bigInteger;
            BigInteger bigInteger2;
            if (doV)
            {
                bigInteger = BigInteger.Two;
                bigInteger2 = BigInteger.ValueOf(mu);
            }
            else
            {
                bigInteger = BigInteger.Zero;
                bigInteger2 = BigInteger.One;
            }

            for (int i = 1; i < k; i++)
            {
                BigInteger bigInteger3 = null;
                bigInteger3 = ((mu != 1) ? bigInteger2.Negate() : bigInteger2);
                BigInteger bigInteger4 = bigInteger3.Subtract(bigInteger.ShiftLeft(1));
                bigInteger = bigInteger2;
                bigInteger2 = bigInteger4;
            }

            return new BigInteger[2] { bigInteger, bigInteger2 };
        }

        public static BigInteger GetTw(sbyte mu, int w)
        {
            if (w == 4)
            {
                if (mu == 1)
                {
                    return BigInteger.ValueOf(6L);
                }

                return BigInteger.ValueOf(10L);
            }

            BigInteger[] lucas = GetLucas(mu, w, doV: false);
            BigInteger m = BigInteger.Zero.SetBit(w);
            BigInteger val = lucas[1].ModInverse(m);
            return BigInteger.Two.Multiply(lucas[0]).Multiply(val).Mod(m);
        }

        public static BigInteger[] GetSi(F2mCurve curve)
        {
            if (!curve.IsKoblitz)
            {
                throw new ArgumentException("si is defined for Koblitz curves only");
            }

            int m = curve.M;
            int intValue = curve.A.ToBigInteger().IntValue;
            sbyte mu = curve.GetMu();
            int intValue2 = curve.H.IntValue;
            int k = m + 3 - intValue;
            BigInteger[] lucas = GetLucas(mu, k, doV: false);
            BigInteger bigInteger;
            BigInteger bigInteger2;
            switch (mu)
            {
                case 1:
                    bigInteger = BigInteger.One.Subtract(lucas[1]);
                    bigInteger2 = BigInteger.One.Subtract(lucas[0]);
                    break;
                case -1:
                    bigInteger = BigInteger.One.Add(lucas[1]);
                    bigInteger2 = BigInteger.One.Add(lucas[0]);
                    break;
                default:
                    throw new ArgumentException("mu must be 1 or -1");
            }

            BigInteger[] array = new BigInteger[2];
            switch (intValue2)
            {
                case 2:
                    array[0] = bigInteger.ShiftRight(1);
                    array[1] = bigInteger2.ShiftRight(1).Negate();
                    break;
                case 4:
                    array[0] = bigInteger.ShiftRight(2);
                    array[1] = bigInteger2.ShiftRight(2).Negate();
                    break;
                default:
                    throw new ArgumentException("h (Cofactor) must be 2 or 4");
            }

            return array;
        }

        public static ZTauElement PartModReduction(BigInteger k, int m, sbyte a, BigInteger[] s, sbyte mu, sbyte c)
        {
            BigInteger bigInteger = ((mu != 1) ? s[0].Subtract(s[1]) : s[0].Add(s[1]));
            BigInteger vm = GetLucas(mu, m, doV: true)[1];
            SimpleBigDecimal lambda = ApproximateDivisionByN(k, s[0], vm, a, m, c);
            SimpleBigDecimal lambda2 = ApproximateDivisionByN(k, s[1], vm, a, m, c);
            ZTauElement zTauElement = Round(lambda, lambda2, mu);
            BigInteger u = k.Subtract(bigInteger.Multiply(zTauElement.u)).Subtract(BigInteger.ValueOf(2L).Multiply(s[1]).Multiply(zTauElement.v));
            BigInteger v = s[1].Multiply(zTauElement.u).Subtract(s[0].Multiply(zTauElement.v));
            return new ZTauElement(u, v);
        }

        public static F2mPoint MultiplyRTnaf(F2mPoint p, BigInteger k)
        {
            F2mCurve obj = (F2mCurve)p.Curve;
            int m = obj.M;
            sbyte a = (sbyte)obj.A.ToBigInteger().IntValue;
            sbyte mu = obj.GetMu();
            BigInteger[] si = obj.GetSi();
            ZTauElement lambda = PartModReduction(k, m, a, si, mu, 10);
            return MultiplyTnaf(p, lambda);
        }

        public static F2mPoint MultiplyTnaf(F2mPoint p, ZTauElement lambda)
        {
            sbyte[] u = TauAdicNaf(((F2mCurve)p.Curve).GetMu(), lambda);
            return MultiplyFromTnaf(p, u);
        }

        public static F2mPoint MultiplyFromTnaf(F2mPoint p, sbyte[] u)
        {
            F2mPoint f2mPoint = (F2mPoint)((F2mCurve)p.Curve).Infinity;
            for (int num = u.Length - 1; num >= 0; num--)
            {
                f2mPoint = Tau(f2mPoint);
                if (u[num] == 1)
                {
                    f2mPoint = f2mPoint.AddSimple(p);
                }
                else if (u[num] == -1)
                {
                    f2mPoint = f2mPoint.SubtractSimple(p);
                }
            }

            return f2mPoint;
        }

        public static sbyte[] TauAdicWNaf(sbyte mu, ZTauElement lambda, sbyte width, BigInteger pow2w, BigInteger tw, ZTauElement[] alpha)
        {
            if (mu != 1 && mu != -1)
            {
                throw new ArgumentException("mu must be 1 or -1");
            }

            int bitLength = Norm(mu, lambda).BitLength;
            sbyte[] array = new sbyte[(bitLength > 30) ? (bitLength + 4 + width) : (34 + width)];
            BigInteger value = pow2w.ShiftRight(1);
            BigInteger bigInteger = lambda.u;
            BigInteger bigInteger2 = lambda.v;
            int num = 0;
            while (!bigInteger.Equals(BigInteger.Zero) || !bigInteger2.Equals(BigInteger.Zero))
            {
                if (bigInteger.TestBit(0))
                {
                    BigInteger bigInteger3 = bigInteger.Add(bigInteger2.Multiply(tw)).Mod(pow2w);
                    sbyte b = (array[num] = ((bigInteger3.CompareTo(value) < 0) ? ((sbyte)bigInteger3.IntValue) : ((sbyte)bigInteger3.Subtract(pow2w).IntValue)));
                    bool flag = true;
                    if (b < 0)
                    {
                        flag = false;
                        b = (sbyte)(-b);
                    }

                    if (flag)
                    {
                        bigInteger = bigInteger.Subtract(alpha[b].u);
                        bigInteger2 = bigInteger2.Subtract(alpha[b].v);
                    }
                    else
                    {
                        bigInteger = bigInteger.Add(alpha[b].u);
                        bigInteger2 = bigInteger2.Add(alpha[b].v);
                    }
                }
                else
                {
                    array[num] = 0;
                }

                BigInteger bigInteger4 = bigInteger;
                bigInteger = ((mu != 1) ? bigInteger2.Subtract(bigInteger.ShiftRight(1)) : bigInteger2.Add(bigInteger.ShiftRight(1)));
                bigInteger2 = bigInteger4.ShiftRight(1).Negate();
                num++;
            }

            return array;
        }

        public static F2mPoint[] GetPreComp(F2mPoint p, sbyte a)
        {
            F2mPoint[] array = new F2mPoint[16]
            {
                null, p, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null
            };
            sbyte[][] array2 = ((a != 0) ? Alpha1Tnaf : Alpha0Tnaf);
            int num = array2.Length;
            for (int i = 3; i < num; i += 2)
            {
                array[i] = MultiplyFromTnaf(p, array2[i]);
            }

            return array;
        }
    }
}
