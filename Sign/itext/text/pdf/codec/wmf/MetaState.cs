using Sign.itext.pdf;

namespace Sign.itext.text.pdf.codec.wmf
{
    public class MetaState
    {
        public static int TA_NOUPDATECP = 0;

        public static int TA_UPDATECP = 1;

        public static int TA_LEFT = 0;

        public static int TA_RIGHT = 2;

        public static int TA_CENTER = 6;

        public static int TA_TOP = 0;

        public static int TA_BOTTOM = 8;

        public static int TA_BASELINE = 24;

        public static int TRANSPARENT = 1;

        public static int OPAQUE = 2;

        public static int ALTERNATE = 1;

        public static int WINDING = 2;

        public Stack<MetaState> savedStates;

        public List<MetaObject> MetaObjects;

        public Point currentPoint;

        public MetaPen currentPen;

        public MetaBrush currentBrush;

        public MetaFont currentFont;

        public BaseColor currentBackgroundColor = BaseColor.WHITE;

        public BaseColor currentTextColor = BaseColor.BLACK;

        public int backgroundMode = OPAQUE;

        public int polyFillMode = ALTERNATE;

        public int lineJoin = 1;

        public int textAlign;

        public int offsetWx;

        public int offsetWy;

        public int extentWx;

        public int extentWy;

        public float scalingX;

        public float scalingY;

        public virtual MetaState metaState
        {
            set
            {
                savedStates = value.savedStates;
                MetaObjects = value.MetaObjects;
                currentPoint = value.currentPoint;
                currentPen = value.currentPen;
                currentBrush = value.currentBrush;
                currentFont = value.currentFont;
                currentBackgroundColor = value.currentBackgroundColor;
                currentTextColor = value.currentTextColor;
                backgroundMode = value.backgroundMode;
                polyFillMode = value.polyFillMode;
                textAlign = value.textAlign;
                lineJoin = value.lineJoin;
                offsetWx = value.offsetWx;
                offsetWy = value.offsetWy;
                extentWx = value.extentWx;
                extentWy = value.extentWy;
                scalingX = value.scalingX;
                scalingY = value.scalingY;
            }
        }

        public virtual float ScalingX
        {
            set
            {
                scalingX = value;
            }
        }

        public virtual float ScalingY
        {
            set
            {
                scalingY = value;
            }
        }

        public virtual int OffsetWx
        {
            set
            {
                offsetWx = value;
            }
        }

        public virtual int OffsetWy
        {
            set
            {
                offsetWy = value;
            }
        }

        public virtual int ExtentWx
        {
            set
            {
                extentWx = value;
            }
        }

        public virtual int ExtentWy
        {
            set
            {
                extentWy = value;
            }
        }

        public virtual Point CurrentPoint
        {
            get
            {
                return currentPoint;
            }
            set
            {
                currentPoint = value;
            }
        }

        public virtual MetaBrush CurrentBrush => currentBrush;

        public virtual MetaPen CurrentPen => currentPen;

        public virtual MetaFont CurrentFont => currentFont;

        public virtual BaseColor CurrentBackgroundColor
        {
            get
            {
                return currentBackgroundColor;
            }
            set
            {
                currentBackgroundColor = value;
            }
        }

        public virtual BaseColor CurrentTextColor
        {
            get
            {
                return currentTextColor;
            }
            set
            {
                currentTextColor = value;
            }
        }

        public virtual int BackgroundMode
        {
            get
            {
                return backgroundMode;
            }
            set
            {
                backgroundMode = value;
            }
        }

        public virtual int TextAlign
        {
            get
            {
                return textAlign;
            }
            set
            {
                textAlign = value;
            }
        }

        public virtual int PolyFillMode
        {
            get
            {
                return polyFillMode;
            }
            set
            {
                polyFillMode = value;
            }
        }

        public virtual PdfContentByte LineJoinRectangle
        {
            set
            {
                if (lineJoin != 0)
                {
                    lineJoin = 0;
                    value.SetLineJoin(0);
                }
            }
        }

        public virtual PdfContentByte LineJoinPolygon
        {
            set
            {
                if (lineJoin == 0)
                {
                    lineJoin = 1;
                    value.SetLineJoin(1);
                }
            }
        }

        public virtual bool LineNeutral => lineJoin == 0;

        public MetaState()
        {
            savedStates = new Stack<MetaState>();
            MetaObjects = new List<MetaObject>();
            currentPoint = new Point(0, 0);
            currentPen = new MetaPen();
            currentBrush = new MetaBrush();
            currentFont = new MetaFont();
        }

        public MetaState(MetaState state)
        {
            metaState = state;
        }

        public virtual void AddMetaObject(MetaObject obj)
        {
            for (int i = 0; i < MetaObjects.Count; i++)
            {
                if (MetaObjects[i] == null)
                {
                    MetaObjects[i] = obj;
                    return;
                }
            }

            MetaObjects.Add(obj);
        }

        public virtual void SelectMetaObject(int index, PdfContentByte cb)
        {
            MetaObject metaObject = MetaObjects[index];
            if (metaObject == null)
            {
                return;
            }

            switch (metaObject.Type)
            {
                case 2:
                    currentBrush = (MetaBrush)metaObject;
                    switch (currentBrush.Style)
                    {
                        case 0:
                            {
                                BaseColor color2 = currentBrush.Color;
                                cb.SetColorFill(color2);
                                break;
                            }
                        case 2:
                            {
                                BaseColor colorFill = currentBackgroundColor;
                                cb.SetColorFill(colorFill);
                                break;
                            }
                    }

                    break;
                case 1:
                    {
                        currentPen = (MetaPen)metaObject;
                        int style = currentPen.Style;
                        if (style != 5)
                        {
                            BaseColor color = currentPen.Color;
                            cb.SetColorStroke(color);
                            cb.SetLineWidth(Math.Abs((float)currentPen.PenWidth * scalingX / (float)extentWx));
                            switch (style)
                            {
                                case 1:
                                    cb.SetLineDash(18f, 6f, 0f);
                                    break;
                                case 3:
                                    cb.SetLiteral("[9 6 3 6]0 d\n");
                                    break;
                                case 4:
                                    cb.SetLiteral("[9 3 3 3 3 3]0 d\n");
                                    break;
                                case 2:
                                    cb.SetLineDash(3f, 0f);
                                    break;
                                default:
                                    cb.SetLineDash(0f);
                                    break;
                            }
                        }

                        break;
                    }
                case 3:
                    currentFont = (MetaFont)metaObject;
                    break;
            }
        }

        public virtual void DeleteMetaObject(int index)
        {
            MetaObjects[index] = null;
        }

        public virtual void SaveState(PdfContentByte cb)
        {
            cb.SaveState();
            MetaState item = new MetaState(this);
            savedStates.Push(item);
        }

        public virtual void RestoreState(int index, PdfContentByte cb)
        {
            int num = ((index >= 0) ? Math.Max(savedStates.Count - index, 0) : Math.Min(-index, savedStates.Count));
            if (num != 0)
            {
                MetaState metaState = null;
                while (num-- != 0)
                {
                    cb.RestoreState();
                    metaState = savedStates.Pop();
                }

                this.metaState = metaState;
            }
        }

        public virtual void Cleanup(PdfContentByte cb)
        {
            int count = savedStates.Count;
            while (count-- > 0)
            {
                cb.RestoreState();
            }
        }

        public virtual float TransformX(int x)
        {
            return ((float)x - (float)offsetWx) * scalingX / (float)extentWx;
        }

        public virtual float TransformY(int y)
        {
            return (1f - ((float)y - (float)offsetWy) / (float)extentWy) * scalingY;
        }

        public virtual float TransformAngle(float angle)
        {
            float num = ((scalingY < 0f) ? (0f - angle) : angle);
            return (float)((scalingX < 0f) ? (Math.PI - (double)num) : ((double)num));
        }
    }
}
