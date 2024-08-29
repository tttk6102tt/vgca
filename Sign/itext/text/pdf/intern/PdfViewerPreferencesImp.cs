using Sign.itext.pdf;
using Sign.itext.text.pdf.interfaces;

namespace Sign.itext.text.pdf.intern
{
    public class PdfViewerPreferencesImp : IPdfViewerPreferences
    {
        public static readonly PdfName[] VIEWER_PREFERENCES = new PdfName[17]
        {
            PdfName.HIDETOOLBAR,
            PdfName.HIDEMENUBAR,
            PdfName.HIDEWINDOWUI,
            PdfName.FITWINDOW,
            PdfName.CENTERWINDOW,
            PdfName.DISPLAYDOCTITLE,
            PdfName.NONFULLSCREENPAGEMODE,
            PdfName.DIRECTION,
            PdfName.VIEWAREA,
            PdfName.VIEWCLIP,
            PdfName.PRINTAREA,
            PdfName.PRINTCLIP,
            PdfName.PRINTSCALING,
            PdfName.DUPLEX,
            PdfName.PICKTRAYBYPDFSIZE,
            PdfName.PRINTPAGERANGE,
            PdfName.NUMCOPIES
        };

        public static readonly PdfName[] NONFULLSCREENPAGEMODE_PREFERENCES = new PdfName[4]
        {
            PdfName.USENONE,
            PdfName.USEOUTLINES,
            PdfName.USETHUMBS,
            PdfName.USEOC
        };

        public static readonly PdfName[] DIRECTION_PREFERENCES = new PdfName[2]
        {
            PdfName.L2R,
            PdfName.R2L
        };

        public static readonly PdfName[] PAGE_BOUNDARIES = new PdfName[5]
        {
            PdfName.MEDIABOX,
            PdfName.CROPBOX,
            PdfName.BLEEDBOX,
            PdfName.TRIMBOX,
            PdfName.ARTBOX
        };

        public static readonly PdfName[] PRINTSCALING_PREFERENCES = new PdfName[2]
        {
            PdfName.APPDEFAULT,
            PdfName.NONE
        };

        public static readonly PdfName[] DUPLEX_PREFERENCES = new PdfName[3]
        {
            PdfName.SIMPLEX,
            PdfName.DUPLEXFLIPSHORTEDGE,
            PdfName.DUPLEXFLIPLONGEDGE
        };

        private int pageLayoutAndMode;

        private PdfDictionary viewerPreferences = new PdfDictionary();

        private const int viewerPreferencesMask = 16773120;

        public virtual int PageLayoutAndMode => pageLayoutAndMode;

        public virtual int ViewerPreferences
        {
            set
            {
                pageLayoutAndMode |= value;
                if (((uint)value & 0xFFF000u) != 0)
                {
                    pageLayoutAndMode = -16773121 & pageLayoutAndMode;
                    if (((uint)value & 0x1000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.HIDETOOLBAR, PdfBoolean.PDFTRUE);
                    }

                    if (((uint)value & 0x2000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.HIDEMENUBAR, PdfBoolean.PDFTRUE);
                    }

                    if (((uint)value & 0x4000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.HIDEWINDOWUI, PdfBoolean.PDFTRUE);
                    }

                    if (((uint)value & 0x8000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.FITWINDOW, PdfBoolean.PDFTRUE);
                    }

                    if (((uint)value & 0x10000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.CENTERWINDOW, PdfBoolean.PDFTRUE);
                    }

                    if (((uint)value & 0x20000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.DISPLAYDOCTITLE, PdfBoolean.PDFTRUE);
                    }

                    if (((uint)value & 0x40000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.NONFULLSCREENPAGEMODE, PdfName.USENONE);
                    }
                    else if (((uint)value & 0x80000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.NONFULLSCREENPAGEMODE, PdfName.USEOUTLINES);
                    }
                    else if (((uint)value & 0x100000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.NONFULLSCREENPAGEMODE, PdfName.USETHUMBS);
                    }
                    else if (((uint)value & 0x200000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.NONFULLSCREENPAGEMODE, PdfName.USEOC);
                    }

                    if (((uint)value & 0x400000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.DIRECTION, PdfName.L2R);
                    }
                    else if (((uint)value & 0x800000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.DIRECTION, PdfName.R2L);
                    }

                    if (((uint)value & 0x1000000u) != 0)
                    {
                        viewerPreferences.Put(PdfName.PRINTSCALING, PdfName.NONE);
                    }
                }
            }
        }

        public virtual PdfDictionary GetViewerPreferences()
        {
            return viewerPreferences;
        }

        private int GetIndex(PdfName key)
        {
            for (int i = 0; i < VIEWER_PREFERENCES.Length; i++)
            {
                if (VIEWER_PREFERENCES[i].Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool IsPossibleValue(PdfName value, PdfName[] accepted)
        {
            for (int i = 0; i < accepted.Length; i++)
            {
                if (accepted[i].Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void AddViewerPreference(PdfName key, PdfObject value)
        {
            switch (GetIndex(key))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 14:
                    if (value is PdfBoolean)
                    {
                        viewerPreferences.Put(key, value);
                    }

                    break;
                case 6:
                    if (value is PdfName && IsPossibleValue((PdfName)value, NONFULLSCREENPAGEMODE_PREFERENCES))
                    {
                        viewerPreferences.Put(key, value);
                    }

                    break;
                case 7:
                    if (value is PdfName && IsPossibleValue((PdfName)value, DIRECTION_PREFERENCES))
                    {
                        viewerPreferences.Put(key, value);
                    }

                    break;
                case 8:
                case 9:
                case 10:
                case 11:
                    if (value is PdfName && IsPossibleValue((PdfName)value, PAGE_BOUNDARIES))
                    {
                        viewerPreferences.Put(key, value);
                    }

                    break;
                case 12:
                    if (value is PdfName && IsPossibleValue((PdfName)value, PRINTSCALING_PREFERENCES))
                    {
                        viewerPreferences.Put(key, value);
                    }

                    break;
                case 13:
                    if (value is PdfName && IsPossibleValue((PdfName)value, DUPLEX_PREFERENCES))
                    {
                        viewerPreferences.Put(key, value);
                    }

                    break;
                case 15:
                    if (value is PdfArray)
                    {
                        viewerPreferences.Put(key, value);
                    }

                    break;
                case 16:
                    if (value is PdfNumber)
                    {
                        viewerPreferences.Put(key, value);
                    }

                    break;
            }
        }

        public virtual void AddToCatalog(PdfDictionary catalog)
        {
            catalog.Remove(PdfName.PAGELAYOUT);
            if (((uint)pageLayoutAndMode & (true ? 1u : 0u)) != 0)
            {
                catalog.Put(PdfName.PAGELAYOUT, PdfName.SINGLEPAGE);
            }
            else if (((uint)pageLayoutAndMode & 2u) != 0)
            {
                catalog.Put(PdfName.PAGELAYOUT, PdfName.ONECOLUMN);
            }
            else if (((uint)pageLayoutAndMode & 4u) != 0)
            {
                catalog.Put(PdfName.PAGELAYOUT, PdfName.TWOCOLUMNLEFT);
            }
            else if (((uint)pageLayoutAndMode & 8u) != 0)
            {
                catalog.Put(PdfName.PAGELAYOUT, PdfName.TWOCOLUMNRIGHT);
            }
            else if (((uint)pageLayoutAndMode & 0x10u) != 0)
            {
                catalog.Put(PdfName.PAGELAYOUT, PdfName.TWOPAGELEFT);
            }
            else if (((uint)pageLayoutAndMode & 0x20u) != 0)
            {
                catalog.Put(PdfName.PAGELAYOUT, PdfName.TWOPAGERIGHT);
            }

            catalog.Remove(PdfName.PAGEMODE);
            if (((uint)pageLayoutAndMode & 0x40u) != 0)
            {
                catalog.Put(PdfName.PAGEMODE, PdfName.USENONE);
            }
            else if (((uint)pageLayoutAndMode & 0x80u) != 0)
            {
                catalog.Put(PdfName.PAGEMODE, PdfName.USEOUTLINES);
            }
            else if (((uint)pageLayoutAndMode & 0x100u) != 0)
            {
                catalog.Put(PdfName.PAGEMODE, PdfName.USETHUMBS);
            }
            else if (((uint)pageLayoutAndMode & 0x200u) != 0)
            {
                catalog.Put(PdfName.PAGEMODE, PdfName.FULLSCREEN);
            }
            else if (((uint)pageLayoutAndMode & 0x400u) != 0)
            {
                catalog.Put(PdfName.PAGEMODE, PdfName.USEOC);
            }
            else if (((uint)pageLayoutAndMode & 0x800u) != 0)
            {
                catalog.Put(PdfName.PAGEMODE, PdfName.USEATTACHMENTS);
            }

            catalog.Remove(PdfName.VIEWERPREFERENCES);
            if (viewerPreferences.Size > 0)
            {
                catalog.Put(PdfName.VIEWERPREFERENCES, viewerPreferences);
            }
        }

        public static PdfViewerPreferencesImp GetViewerPreferences(PdfDictionary catalog)
        {
            PdfViewerPreferencesImp pdfViewerPreferencesImp = new PdfViewerPreferencesImp();
            int num = 0;
            PdfName pdfName = null;
            PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(catalog.Get(PdfName.PAGELAYOUT));
            if (pdfObjectRelease != null && pdfObjectRelease.IsName())
            {
                pdfName = (PdfName)pdfObjectRelease;
                if (pdfName.Equals(PdfName.SINGLEPAGE))
                {
                    num |= 1;
                }
                else if (pdfName.Equals(PdfName.ONECOLUMN))
                {
                    num |= 2;
                }
                else if (pdfName.Equals(PdfName.TWOCOLUMNLEFT))
                {
                    num |= 4;
                }
                else if (pdfName.Equals(PdfName.TWOCOLUMNRIGHT))
                {
                    num |= 8;
                }
                else if (pdfName.Equals(PdfName.TWOPAGELEFT))
                {
                    num |= 0x10;
                }
                else if (pdfName.Equals(PdfName.TWOPAGERIGHT))
                {
                    num |= 0x20;
                }
            }

            pdfObjectRelease = PdfReader.GetPdfObjectRelease(catalog.Get(PdfName.PAGEMODE));
            if (pdfObjectRelease != null && pdfObjectRelease.IsName())
            {
                pdfName = (PdfName)pdfObjectRelease;
                if (pdfName.Equals(PdfName.USENONE))
                {
                    num |= 0x40;
                }
                else if (pdfName.Equals(PdfName.USEOUTLINES))
                {
                    num |= 0x80;
                }
                else if (pdfName.Equals(PdfName.USETHUMBS))
                {
                    num |= 0x100;
                }
                else if (pdfName.Equals(PdfName.FULLSCREEN))
                {
                    num |= 0x200;
                }
                else if (pdfName.Equals(PdfName.USEOC))
                {
                    num |= 0x400;
                }
                else if (pdfName.Equals(PdfName.USEATTACHMENTS))
                {
                    num |= 0x800;
                }
            }

            pdfViewerPreferencesImp.ViewerPreferences = num;
            pdfObjectRelease = PdfReader.GetPdfObjectRelease(catalog.Get(PdfName.VIEWERPREFERENCES));
            if (pdfObjectRelease != null && pdfObjectRelease.IsDictionary())
            {
                PdfDictionary pdfDictionary = (PdfDictionary)pdfObjectRelease;
                for (int i = 0; i < VIEWER_PREFERENCES.Length; i++)
                {
                    pdfObjectRelease = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(VIEWER_PREFERENCES[i]));
                    pdfViewerPreferencesImp.AddViewerPreference(VIEWER_PREFERENCES[i], pdfObjectRelease);
                }
            }

            return pdfViewerPreferencesImp;
        }
    }
}
