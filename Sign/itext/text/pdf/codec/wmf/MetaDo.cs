using Sign.itext.error_messages;
using Sign.itext.pdf;
using System.Net;
using System.Text;

namespace Sign.itext.text.pdf.codec.wmf
{
    public class MetaDo
    {
        public const int META_SETBKCOLOR = 513;

        public const int META_SETBKMODE = 258;

        public const int META_SETMAPMODE = 259;

        public const int META_SETROP2 = 260;

        public const int META_SETRELABS = 261;

        public const int META_SETPOLYFILLMODE = 262;

        public const int META_SETSTRETCHBLTMODE = 263;

        public const int META_SETTEXTCHAREXTRA = 264;

        public const int META_SETTEXTCOLOR = 521;

        public const int META_SETTEXTJUSTIFICATION = 522;

        public const int META_SETWINDOWORG = 523;

        public const int META_SETWINDOWEXT = 524;

        public const int META_SETVIEWPORTORG = 525;

        public const int META_SETVIEWPORTEXT = 526;

        public const int META_OFFSETWINDOWORG = 527;

        public const int META_SCALEWINDOWEXT = 1040;

        public const int META_OFFSETVIEWPORTORG = 529;

        public const int META_SCALEVIEWPORTEXT = 1042;

        public const int META_LINETO = 531;

        public const int META_MOVETO = 532;

        public const int META_EXCLUDECLIPRECT = 1045;

        public const int META_INTERSECTCLIPRECT = 1046;

        public const int META_ARC = 2071;

        public const int META_ELLIPSE = 1048;

        public const int META_FLOODFILL = 1049;

        public const int META_PIE = 2074;

        public const int META_RECTANGLE = 1051;

        public const int META_ROUNDRECT = 1564;

        public const int META_PATBLT = 1565;

        public const int META_SAVEDC = 30;

        public const int META_SETPIXEL = 1055;

        public const int META_OFFSETCLIPRGN = 544;

        public const int META_TEXTOUT = 1313;

        public const int META_BITBLT = 2338;

        public const int META_STRETCHBLT = 2851;

        public const int META_POLYGON = 804;

        public const int META_POLYLINE = 805;

        public const int META_ESCAPE = 1574;

        public const int META_RESTOREDC = 295;

        public const int META_FILLREGION = 552;

        public const int META_FRAMEREGION = 1065;

        public const int META_INVERTREGION = 298;

        public const int META_PAINTREGION = 299;

        public const int META_SELECTCLIPREGION = 300;

        public const int META_SELECTOBJECT = 301;

        public const int META_SETTEXTALIGN = 302;

        public const int META_CHORD = 2096;

        public const int META_SETMAPPERFLAGS = 561;

        public const int META_EXTTEXTOUT = 2610;

        public const int META_SETDIBTODEV = 3379;

        public const int META_SELECTPALETTE = 564;

        public const int META_REALIZEPALETTE = 53;

        public const int META_ANIMATEPALETTE = 1078;

        public const int META_SETPALENTRIES = 55;

        public const int META_POLYPOLYGON = 1336;

        public const int META_RESIZEPALETTE = 313;

        public const int META_DIBBITBLT = 2368;

        public const int META_DIBSTRETCHBLT = 2881;

        public const int META_DIBCREATEPATTERNBRUSH = 322;

        public const int META_STRETCHDIB = 3907;

        public const int META_EXTFLOODFILL = 1352;

        public const int META_DELETEOBJECT = 496;

        public const int META_CREATEPALETTE = 247;

        public const int META_CREATEPATTERNBRUSH = 505;

        public const int META_CREATEPENINDIRECT = 762;

        public const int META_CREATEFONTINDIRECT = 763;

        public const int META_CREATEBRUSHINDIRECT = 764;

        public const int META_CREATEREGION = 1791;

        public PdfContentByte cb;

        public InputMeta meta;

        private int left;

        private int top;

        private int right;

        private int bottom;

        private int inch;

        private MetaState state = new MetaState();

        public MetaDo(Stream meta, PdfContentByte cb)
        {
            this.cb = cb;
            this.meta = new InputMeta(meta);
        }

        public virtual void ReadAll()
        {
            if (meta.ReadInt() != -1698247209)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("not.a.placeable.windows.metafile"));
            }

            meta.ReadWord();
            left = meta.ReadShort();
            top = meta.ReadShort();
            right = meta.ReadShort();
            bottom = meta.ReadShort();
            inch = meta.ReadWord();
            state.ScalingX = (float)(right - left) / (float)inch * 72f;
            state.ScalingY = (float)(bottom - top) / (float)inch * 72f;
            state.OffsetWx = left;
            state.OffsetWy = top;
            state.ExtentWx = right - left;
            state.ExtentWy = bottom - top;
            meta.ReadInt();
            meta.ReadWord();
            meta.Skip(18);
            cb.SetLineCap(1);
            cb.SetLineJoin(1);
            while (true)
            {
                int length = meta.Length;
                int num = meta.ReadInt();
                if (num < 3)
                {
                    break;
                }

                int num2 = meta.ReadWord();
                switch (num2)
                {
                    case 247:
                    case 322:
                    case 1791:
                        state.AddMetaObject(new MetaObject());
                        break;
                    case 762:
                        {
                            MetaPen metaPen = new MetaPen();
                            metaPen.Init(meta);
                            state.AddMetaObject(metaPen);
                            break;
                        }
                    case 764:
                        {
                            MetaBrush metaBrush = new MetaBrush();
                            metaBrush.Init(meta);
                            state.AddMetaObject(metaBrush);
                            break;
                        }
                    case 763:
                        {
                            MetaFont metaFont = new MetaFont();
                            metaFont.Init(meta);
                            state.AddMetaObject(metaFont);
                            break;
                        }
                    case 301:
                        {
                            int index3 = meta.ReadWord();
                            state.SelectMetaObject(index3, cb);
                            break;
                        }
                    case 496:
                        {
                            int index2 = meta.ReadWord();
                            state.DeleteMetaObject(index2);
                            break;
                        }
                    case 30:
                        state.SaveState(cb);
                        break;
                    case 295:
                        {
                            int index = meta.ReadShort();
                            state.RestoreState(index, cb);
                            break;
                        }
                    case 523:
                        state.OffsetWy = meta.ReadShort();
                        state.OffsetWx = meta.ReadShort();
                        break;
                    case 524:
                        state.ExtentWy = meta.ReadShort();
                        state.ExtentWx = meta.ReadShort();
                        break;
                    case 532:
                        {
                            int y12 = meta.ReadShort();
                            Point currentPoint2 = new Point(meta.ReadShort(), y12);
                            state.CurrentPoint = currentPoint2;
                            break;
                        }
                    case 531:
                        {
                            int y9 = meta.ReadShort();
                            int x9 = meta.ReadShort();
                            Point currentPoint = state.CurrentPoint;
                            cb.MoveTo(state.TransformX(currentPoint.x), state.TransformY(currentPoint.y));
                            cb.LineTo(state.TransformX(x9), state.TransformY(y9));
                            cb.Stroke();
                            state.CurrentPoint = new Point(x9, y9);
                            break;
                        }
                    case 805:
                        {
                            state.LineJoinPolygon = cb;
                            int num21 = meta.ReadWord();
                            int x8 = meta.ReadShort();
                            int y8 = meta.ReadShort();
                            cb.MoveTo(state.TransformX(x8), state.TransformY(y8));
                            for (int n = 1; n < num21; n++)
                            {
                                x8 = meta.ReadShort();
                                y8 = meta.ReadShort();
                                cb.LineTo(state.TransformX(x8), state.TransformY(y8));
                            }

                            cb.Stroke();
                            break;
                        }
                    case 804:
                        if (!IsNullStrokeFill(isRectangle: false))
                        {
                            int num18 = meta.ReadWord();
                            int x3 = meta.ReadShort();
                            int y3 = meta.ReadShort();
                            cb.MoveTo(state.TransformX(x3), state.TransformY(y3));
                            for (int l = 1; l < num18; l++)
                            {
                                int x4 = meta.ReadShort();
                                int y4 = meta.ReadShort();
                                cb.LineTo(state.TransformX(x4), state.TransformY(y4));
                            }

                            cb.LineTo(state.TransformX(x3), state.TransformY(y3));
                            StrokeAndFill();
                        }

                        break;
                    case 1336:
                        {
                            if (IsNullStrokeFill(isRectangle: false))
                            {
                                break;
                            }

                            int[] array6 = new int[meta.ReadWord()];
                            for (int num45 = 0; num45 < array6.Length; num45++)
                            {
                                array6[num45] = meta.ReadWord();
                            }

                            foreach (int num47 in array6)
                            {
                                int x12 = meta.ReadShort();
                                int y13 = meta.ReadShort();
                                cb.MoveTo(state.TransformX(x12), state.TransformY(y13));
                                for (int num48 = 1; num48 < num47; num48++)
                                {
                                    int x13 = meta.ReadShort();
                                    int y14 = meta.ReadShort();
                                    cb.LineTo(state.TransformX(x13), state.TransformY(y14));
                                }

                                cb.LineTo(state.TransformX(x12), state.TransformY(y13));
                            }

                            StrokeAndFill();
                            break;
                        }
                    case 1048:
                        if (!IsNullStrokeFill(state.LineNeutral))
                        {
                            int y10 = meta.ReadShort();
                            int x10 = meta.ReadShort();
                            int y11 = meta.ReadShort();
                            int x11 = meta.ReadShort();
                            cb.Arc(state.TransformX(x11), state.TransformY(y10), state.TransformX(x10), state.TransformY(y11), 0f, 360f);
                            StrokeAndFill();
                        }

                        break;
                    case 2071:
                        if (!IsNullStrokeFill(state.LineNeutral))
                        {
                            float yDot3 = state.TransformY(meta.ReadShort());
                            float xDot3 = state.TransformX(meta.ReadShort());
                            float yDot4 = state.TransformY(meta.ReadShort());
                            float xDot4 = state.TransformX(meta.ReadShort());
                            float num22 = state.TransformY(meta.ReadShort());
                            float num23 = state.TransformX(meta.ReadShort());
                            float num24 = state.TransformY(meta.ReadShort());
                            float num25 = state.TransformX(meta.ReadShort());
                            float xCenter = (num23 + num25) / 2f;
                            float yCenter = (num24 + num22) / 2f;
                            float arc3 = GetArc(xCenter, yCenter, xDot4, yDot4);
                            float arc4 = GetArc(xCenter, yCenter, xDot3, yDot3);
                            arc4 -= arc3;
                            if (arc4 <= 0f)
                            {
                                arc4 += 360f;
                            }

                            cb.Arc(num25, num22, num23, num24, arc3, arc4);
                            cb.Stroke();
                        }

                        break;
                    case 2074:
                        {
                            if (IsNullStrokeFill(state.LineNeutral))
                            {
                                break;
                            }

                            float yDot = state.TransformY(meta.ReadShort());
                            float xDot = state.TransformX(meta.ReadShort());
                            float yDot2 = state.TransformY(meta.ReadShort());
                            float xDot2 = state.TransformX(meta.ReadShort());
                            float num11 = state.TransformY(meta.ReadShort());
                            float num12 = state.TransformX(meta.ReadShort());
                            float num13 = state.TransformY(meta.ReadShort());
                            float num14 = state.TransformX(meta.ReadShort());
                            float num15 = (num12 + num14) / 2f;
                            float num16 = (num13 + num11) / 2f;
                            float arc = GetArc(num15, num16, xDot2, yDot2);
                            float arc2 = GetArc(num15, num16, xDot, yDot);
                            arc2 -= arc;
                            if (arc2 <= 0f)
                            {
                                arc2 += 360f;
                            }

                            List<float[]> list = PdfContentByte.BezierArc(num14, num11, num12, num13, arc, arc2);
                            if (list.Count != 0)
                            {
                                float[] array2 = list[0];
                                cb.MoveTo(num15, num16);
                                cb.LineTo(array2[0], array2[1]);
                                for (int j = 0; j < list.Count; j++)
                                {
                                    array2 = list[j];
                                    cb.CurveTo(array2[2], array2[3], array2[4], array2[5], array2[6], array2[7]);
                                }

                                cb.LineTo(num15, num16);
                                StrokeAndFill();
                            }

                            break;
                        }
                    case 2096:
                        {
                            if (IsNullStrokeFill(state.LineNeutral))
                            {
                                break;
                            }

                            float yDot5 = state.TransformY(meta.ReadShort());
                            float xDot5 = state.TransformX(meta.ReadShort());
                            float yDot6 = state.TransformY(meta.ReadShort());
                            float xDot6 = state.TransformX(meta.ReadShort());
                            float num40 = state.TransformY(meta.ReadShort());
                            float num41 = state.TransformX(meta.ReadShort());
                            float num42 = state.TransformY(meta.ReadShort());
                            float num43 = state.TransformX(meta.ReadShort());
                            float xCenter2 = (num41 + num43) / 2f;
                            float yCenter2 = (num42 + num40) / 2f;
                            float arc5 = GetArc(xCenter2, yCenter2, xDot6, yDot6);
                            float arc6 = GetArc(xCenter2, yCenter2, xDot5, yDot5);
                            arc6 -= arc5;
                            if (arc6 <= 0f)
                            {
                                arc6 += 360f;
                            }

                            List<float[]> list2 = PdfContentByte.BezierArc(num43, num40, num41, num42, arc5, arc6);
                            if (list2.Count != 0)
                            {
                                float[] array5 = list2[0];
                                xCenter2 = array5[0];
                                yCenter2 = array5[1];
                                cb.MoveTo(xCenter2, yCenter2);
                                for (int num44 = 0; num44 < list2.Count; num44++)
                                {
                                    array5 = list2[num44];
                                    cb.CurveTo(array5[2], array5[3], array5[4], array5[5], array5[6], array5[7]);
                                }

                                cb.LineTo(xCenter2, yCenter2);
                                StrokeAndFill();
                            }

                            break;
                        }
                    case 1051:
                        if (!IsNullStrokeFill(isRectangle: true))
                        {
                            float num36 = state.TransformY(meta.ReadShort());
                            float num37 = state.TransformX(meta.ReadShort());
                            float num38 = state.TransformY(meta.ReadShort());
                            float num39 = state.TransformX(meta.ReadShort());
                            cb.Rectangle(num39, num36, num37 - num39, num38 - num36);
                            StrokeAndFill();
                        }

                        break;
                    case 1564:
                        if (!IsNullStrokeFill(isRectangle: true))
                        {
                            float num30 = state.TransformY(0) - state.TransformY(meta.ReadShort());
                            float num31 = state.TransformX(meta.ReadShort()) - state.TransformX(0);
                            float num32 = state.TransformY(meta.ReadShort());
                            float num33 = state.TransformX(meta.ReadShort());
                            float num34 = state.TransformY(meta.ReadShort());
                            float num35 = state.TransformX(meta.ReadShort());
                            cb.RoundRectangle(num35, num32, num33 - num35, num34 - num32, (num30 + num31) / 4f);
                            StrokeAndFill();
                        }

                        break;
                    case 1046:
                        {
                            float num26 = state.TransformY(meta.ReadShort());
                            float num27 = state.TransformX(meta.ReadShort());
                            float num28 = state.TransformY(meta.ReadShort());
                            float num29 = state.TransformX(meta.ReadShort());
                            cb.Rectangle(num29, num26, num27 - num29, num28 - num26);
                            cb.EoClip();
                            cb.NewPath();
                            break;
                        }
                    case 2610:
                        {
                            int y5 = meta.ReadShort();
                            int x5 = meta.ReadShort();
                            int num19 = meta.ReadWord();
                            int num20 = meta.ReadWord();
                            int x6 = 0;
                            int y6 = 0;
                            int x7 = 0;
                            int y7 = 0;
                            if (((uint)num20 & 6u) != 0)
                            {
                                x6 = meta.ReadShort();
                                y6 = meta.ReadShort();
                                x7 = meta.ReadShort();
                                y7 = meta.ReadShort();
                            }

                            byte[] array4 = new byte[num19];
                            int m;
                            for (m = 0; m < num19; m++)
                            {
                                byte b2 = (byte)meta.ReadByte();
                                if (b2 == 0)
                                {
                                    break;
                                }

                                array4[m] = b2;
                            }

                            string string2;
                            try
                            {
                                string2 = Encoding.GetEncoding(1252).GetString(array4, 0, m);
                            }
                            catch
                            {
                                string2 = Encoding.ASCII.GetString(array4, 0, m);
                            }

                            OutputText(x5, y5, num20, x6, y6, x7, y7, string2);
                            break;
                        }
                    case 1313:
                        {
                            int num17 = meta.ReadWord();
                            byte[] array3 = new byte[num17];
                            int k;
                            for (k = 0; k < num17; k++)
                            {
                                byte b = (byte)meta.ReadByte();
                                if (b == 0)
                                {
                                    break;
                                }

                                array3[k] = b;
                            }

                            string @string;
                            try
                            {
                                @string = Encoding.GetEncoding(1252).GetString(array3, 0, k);
                            }
                            catch
                            {
                                @string = Encoding.ASCII.GetString(array3, 0, k);
                            }

                            num17 = (num17 + 1) & 0xFFFE;
                            meta.Skip(num17 - k);
                            int y2 = meta.ReadShort();
                            int x2 = meta.ReadShort();
                            OutputText(x2, y2, 0, 0, 0, 0, 0, @string);
                            break;
                        }
                    case 513:
                        state.CurrentBackgroundColor = meta.ReadColor();
                        break;
                    case 521:
                        state.CurrentTextColor = meta.ReadColor();
                        break;
                    case 302:
                        state.TextAlign = meta.ReadWord();
                        break;
                    case 258:
                        state.BackgroundMode = meta.ReadWord();
                        break;
                    case 262:
                        state.PolyFillMode = meta.ReadWord();
                        break;
                    case 1055:
                        {
                            BaseColor colorFill = meta.ReadColor();
                            int y = meta.ReadShort();
                            int x = meta.ReadShort();
                            cb.SaveState();
                            cb.SetColorFill(colorFill);
                            cb.Rectangle(state.TransformX(x), state.TransformY(y), 0.2f, 0.2f);
                            cb.Fill();
                            cb.RestoreState();
                            break;
                        }
                    case 2881:
                    case 3907:
                        {
                            meta.ReadInt();
                            if (num2 == 3907)
                            {
                                meta.ReadWord();
                            }

                            int num3 = meta.ReadShort();
                            int num4 = meta.ReadShort();
                            int num5 = meta.ReadShort();
                            int num6 = meta.ReadShort();
                            float num7 = state.TransformY(meta.ReadShort()) - state.TransformY(0);
                            float num8 = state.TransformX(meta.ReadShort()) - state.TransformX(0);
                            float num9 = state.TransformY(meta.ReadShort());
                            float num10 = state.TransformX(meta.ReadShort());
                            byte[] array = new byte[num * 2 - (meta.Length - length)];
                            for (int i = 0; i < array.Length; i++)
                            {
                                array[i] = (byte)meta.ReadByte();
                            }

                            try
                            {
                                Image image = BmpImage.GetImage(new MemoryStream(array), noHeader: true, array.Length);
                                cb.SaveState();
                                cb.Rectangle(num10, num9, num8, num7);
                                cb.Clip();
                                cb.NewPath();
                                image.ScaleAbsolute(num8 * image.Width / (float)num4, (0f - num7) * image.Height / (float)num3);
                                image.SetAbsolutePosition(num10 - num8 * (float)num6 / (float)num4, num9 + num7 * (float)num5 / (float)num3 - image.ScaledHeight);
                                cb.AddImage(image);
                                cb.RestoreState();
                            }
                            catch
                            {
                            }

                            break;
                        }
                }

                meta.Skip(num * 2 - (meta.Length - length));
            }

            state.Cleanup(cb);
        }

        public virtual void OutputText(int x, int y, int flag, int x1, int y1, int x2, int y2, string text)
        {
            MetaFont currentFont = state.CurrentFont;
            float e = state.TransformX(x);
            float f = state.TransformY(y);
            float num = state.TransformAngle(currentFont.Angle);
            float num2 = (float)Math.Sin(num);
            float num3 = (float)Math.Cos(num);
            float fontSize = currentFont.GetFontSize(state);
            BaseFont font = currentFont.Font;
            int textAlign = state.TextAlign;
            float widthPoint = font.GetWidthPoint(text, fontSize);
            float x3 = 0f;
            float num4 = 0f;
            float fontDescriptor = font.GetFontDescriptor(3, fontSize);
            float fontDescriptor2 = font.GetFontDescriptor(8, fontSize);
            cb.SaveState();
            cb.ConcatCTM(num3, num2, 0f - num2, num3, e, f);
            if ((textAlign & MetaState.TA_CENTER) == MetaState.TA_CENTER)
            {
                x3 = (0f - widthPoint) / 2f;
            }
            else if ((textAlign & MetaState.TA_RIGHT) == MetaState.TA_RIGHT)
            {
                x3 = 0f - widthPoint;
            }

            num4 = (((textAlign & MetaState.TA_BASELINE) == MetaState.TA_BASELINE) ? 0f : (((textAlign & MetaState.TA_BOTTOM) != MetaState.TA_BOTTOM) ? (0f - fontDescriptor2) : (0f - fontDescriptor)));
            BaseColor currentBackgroundColor;
            if (state.BackgroundMode == MetaState.OPAQUE)
            {
                currentBackgroundColor = state.CurrentBackgroundColor;
                cb.SetColorFill(currentBackgroundColor);
                cb.Rectangle(x3, num4 + fontDescriptor, widthPoint, fontDescriptor2 - fontDescriptor);
                cb.Fill();
            }

            currentBackgroundColor = state.CurrentTextColor;
            cb.SetColorFill(currentBackgroundColor);
            cb.BeginText();
            cb.SetFontAndSize(font, fontSize);
            cb.SetTextMatrix(x3, num4);
            cb.ShowText(text);
            cb.EndText();
            if (currentFont.IsUnderline())
            {
                cb.Rectangle(x3, num4 - fontSize / 4f, widthPoint, fontSize / 15f);
                cb.Fill();
            }

            if (currentFont.IsStrikeout())
            {
                cb.Rectangle(x3, num4 + fontSize / 3f, widthPoint, fontSize / 15f);
                cb.Fill();
            }

            cb.RestoreState();
        }

        public virtual bool IsNullStrokeFill(bool isRectangle)
        {
            MetaPen currentPen = state.CurrentPen;
            MetaBrush currentBrush = state.CurrentBrush;
            bool num = currentPen.Style == 5;
            bool flag = currentBrush.Style switch
            {
                2 => state.BackgroundMode == MetaState.OPAQUE,
                0 => true,
                _ => false,
            };
            bool result = num && !flag;
            if (!num)
            {
                if (isRectangle)
                {
                    state.LineJoinRectangle = cb;
                }
                else
                {
                    state.LineJoinPolygon = cb;
                }
            }

            return result;
        }

        public virtual void StrokeAndFill()
        {
            MetaPen currentPen = state.CurrentPen;
            MetaBrush currentBrush = state.CurrentBrush;
            int style = currentPen.Style;
            int style2 = currentBrush.Style;
            if (style == 5)
            {
                cb.ClosePath();
                if (state.PolyFillMode == MetaState.ALTERNATE)
                {
                    cb.EoFill();
                }
                else
                {
                    cb.Fill();
                }
            }
            else if (style2 switch
            {
                2 => state.BackgroundMode == MetaState.OPAQUE,
                0 => true,
                _ => false,
            })
            {
                if (state.PolyFillMode == MetaState.ALTERNATE)
                {
                    cb.ClosePathEoFillStroke();
                }
                else
                {
                    cb.ClosePathFillStroke();
                }
            }
            else
            {
                cb.ClosePathStroke();
            }
        }

        internal static float GetArc(float xCenter, float yCenter, float xDot, float yDot)
        {
            double num = Math.Atan2(yDot - yCenter, xDot - xCenter);
            if (num < 0.0)
            {
                num += Math.PI * 2.0;
            }

            return (float)(num / Math.PI * 180.0);
        }

        public static byte[] WrapBMP(Image image)
        {
            if (image.OriginalType != 4)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("only.bmp.can.be.wrapped.in.wmf"));
            }

            byte[] array = null;
            if (image.OriginalData == null)
            {
                WebRequest webRequest = WebRequest.Create(image.Url);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                Stream responseStream = webRequest.GetResponse().GetResponseStream();
                MemoryStream memoryStream = new MemoryStream();
                int num = 0;
                while ((num = responseStream.ReadByte()) != -1)
                {
                    memoryStream.WriteByte((byte)num);
                }

                responseStream.Close();
                array = memoryStream.ToArray();
            }
            else
            {
                array = image.OriginalData;
            }

            int num2 = array.Length - 14 + 1 >> 1;
            MemoryStream memoryStream2 = new MemoryStream();
            WriteWord(memoryStream2, 1);
            WriteWord(memoryStream2, 9);
            WriteWord(memoryStream2, 768);
            WriteDWord(memoryStream2, 23 + (13 + num2) + 3);
            WriteWord(memoryStream2, 1);
            WriteDWord(memoryStream2, 14 + num2);
            WriteWord(memoryStream2, 0);
            WriteDWord(memoryStream2, 4);
            WriteWord(memoryStream2, 259);
            WriteWord(memoryStream2, 8);
            WriteDWord(memoryStream2, 5);
            WriteWord(memoryStream2, 523);
            WriteWord(memoryStream2, 0);
            WriteWord(memoryStream2, 0);
            WriteDWord(memoryStream2, 5);
            WriteWord(memoryStream2, 524);
            WriteWord(memoryStream2, (int)image.Height);
            WriteWord(memoryStream2, (int)image.Width);
            WriteDWord(memoryStream2, 13 + num2);
            WriteWord(memoryStream2, 2881);
            WriteDWord(memoryStream2, 13369376);
            WriteWord(memoryStream2, (int)image.Height);
            WriteWord(memoryStream2, (int)image.Width);
            WriteWord(memoryStream2, 0);
            WriteWord(memoryStream2, 0);
            WriteWord(memoryStream2, (int)image.Height);
            WriteWord(memoryStream2, (int)image.Width);
            WriteWord(memoryStream2, 0);
            WriteWord(memoryStream2, 0);
            memoryStream2.Write(array, 14, array.Length - 14);
            if ((array.Length & 1) == 1)
            {
                memoryStream2.WriteByte(0);
            }

            WriteDWord(memoryStream2, 3);
            WriteWord(memoryStream2, 0);
            memoryStream2.Close();
            return memoryStream2.ToArray();
        }

        public static void WriteWord(Stream os, int v)
        {
            os.WriteByte((byte)((uint)v & 0xFFu));
            os.WriteByte((byte)((uint)(v >> 8) & 0xFFu));
        }

        public static void WriteDWord(Stream os, int v)
        {
            WriteWord(os, v & 0xFFFF);
            WriteWord(os, (v >> 16) & 0xFFFF);
        }
    }
}
