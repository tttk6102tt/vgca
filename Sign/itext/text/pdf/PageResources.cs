using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PageResources
    {
        protected PdfDictionary fontDictionary = new PdfDictionary();

        protected PdfDictionary xObjectDictionary = new PdfDictionary();

        protected PdfDictionary colorDictionary = new PdfDictionary();

        protected PdfDictionary patternDictionary = new PdfDictionary();

        protected PdfDictionary shadingDictionary = new PdfDictionary();

        protected PdfDictionary extGStateDictionary = new PdfDictionary();

        protected PdfDictionary propertyDictionary = new PdfDictionary();

        protected Dictionary<PdfName, object> forbiddenNames;

        protected PdfDictionary originalResources;

        protected int[] namePtr = new int[1];

        protected Dictionary<PdfName, PdfName> usedNames;

        internal PdfDictionary Resources
        {
            get
            {
                PdfResources pdfResources = new PdfResources();
                if (originalResources != null)
                {
                    pdfResources.Merge(originalResources);
                }

                pdfResources.Add(PdfName.FONT, fontDictionary);
                pdfResources.Add(PdfName.XOBJECT, xObjectDictionary);
                pdfResources.Add(PdfName.COLORSPACE, colorDictionary);
                pdfResources.Add(PdfName.PATTERN, patternDictionary);
                pdfResources.Add(PdfName.SHADING, shadingDictionary);
                pdfResources.Add(PdfName.EXTGSTATE, extGStateDictionary);
                pdfResources.Add(PdfName.PROPERTIES, propertyDictionary);
                return pdfResources;
            }
        }

        internal PageResources()
        {
        }

        internal void SetOriginalResources(PdfDictionary resources, int[] newNamePtr)
        {
            if (newNamePtr != null)
            {
                namePtr = newNamePtr;
            }

            forbiddenNames = new Dictionary<PdfName, object>();
            usedNames = new Dictionary<PdfName, PdfName>();
            if (resources == null)
            {
                return;
            }

            originalResources = new PdfDictionary();
            originalResources.Merge(resources);
            foreach (PdfName key in resources.Keys)
            {
                PdfObject pdfObject = PdfReader.GetPdfObject(resources.Get(key));
                if (pdfObject == null || !pdfObject.IsDictionary())
                {
                    continue;
                }

                PdfDictionary pdfDictionary = (PdfDictionary)pdfObject;
                foreach (PdfName key2 in pdfDictionary.Keys)
                {
                    forbiddenNames[key2] = null;
                }

                PdfDictionary pdfDictionary2 = new PdfDictionary();
                pdfDictionary2.Merge(pdfDictionary);
                originalResources.Put(key, pdfDictionary2);
            }
        }

        internal PdfName TranslateName(PdfName name)
        {
            PdfName value = name;
            if (forbiddenNames != null)
            {
                usedNames.TryGetValue(name, out value);
                if (value == null)
                {
                    do
                    {
                        value = new PdfName("Xi" + namePtr[0]++);
                    }
                    while (forbiddenNames.ContainsKey(value));
                    usedNames[name] = value;
                }
            }

            return value;
        }

        internal PdfName AddFont(PdfName name, PdfIndirectReference reference)
        {
            name = TranslateName(name);
            fontDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddXObject(PdfName name, PdfIndirectReference reference)
        {
            name = TranslateName(name);
            xObjectDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddColor(PdfName name, PdfIndirectReference reference)
        {
            name = TranslateName(name);
            colorDictionary.Put(name, reference);
            return name;
        }

        internal void AddDefaultColor(PdfName name, PdfObject obj)
        {
            if (obj == null || obj.IsNull())
            {
                colorDictionary.Remove(name);
            }
            else
            {
                colorDictionary.Put(name, obj);
            }
        }

        internal void AddDefaultColor(PdfDictionary dic)
        {
            colorDictionary.Merge(dic);
        }

        internal void AddDefaultColorDiff(PdfDictionary dic)
        {
            colorDictionary.MergeDifferent(dic);
        }

        internal PdfName AddShading(PdfName name, PdfIndirectReference reference)
        {
            name = TranslateName(name);
            shadingDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddPattern(PdfName name, PdfIndirectReference reference)
        {
            name = TranslateName(name);
            patternDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddExtGState(PdfName name, PdfIndirectReference reference)
        {
            name = TranslateName(name);
            extGStateDictionary.Put(name, reference);
            return name;
        }

        internal PdfName AddProperty(PdfName name, PdfIndirectReference reference)
        {
            name = TranslateName(name);
            propertyDictionary.Put(name, reference);
            return name;
        }

        internal bool HasResources()
        {
            if (fontDictionary.Size <= 0 && xObjectDictionary.Size <= 0 && colorDictionary.Size <= 0 && patternDictionary.Size <= 0 && shadingDictionary.Size <= 0 && extGStateDictionary.Size <= 0)
            {
                return propertyDictionary.Size > 0;
            }

            return true;
        }
    }
}
