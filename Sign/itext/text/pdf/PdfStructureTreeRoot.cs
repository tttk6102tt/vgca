using Sign.itext.pdf;
using Sign.itext.text.pdf.interfaces;

namespace Sign.itext.text.pdf
{
    public class PdfStructureTreeRoot : PdfDictionary, IPdfStructureElement
    {
        private Dictionary<int, PdfObject> parentTree = new Dictionary<int, PdfObject>();

        private PdfIndirectReference reference;

        private PdfDictionary classMap;

        internal Dictionary<PdfName, PdfObject> classes;

        private Dictionary<int, PdfIndirectReference> numTree;

        private PdfWriter writer;

        public virtual PdfWriter Writer => writer;

        public virtual Dictionary<int, PdfIndirectReference> NumTree
        {
            get
            {
                if (numTree == null)
                {
                    CreateNumTree();
                }

                return numTree;
            }
        }

        public virtual PdfIndirectReference Reference => reference;

        internal PdfStructureTreeRoot(PdfWriter writer)
            : base(PdfName.STRUCTTREEROOT)
        {
            this.writer = writer;
            reference = writer.PdfIndirectReference;
        }

        private void CreateNumTree()
        {
            if (numTree != null)
            {
                return;
            }

            numTree = new Dictionary<int, PdfIndirectReference>();
            foreach (int key in parentTree.Keys)
            {
                PdfObject pdfObject = parentTree[key];
                if (pdfObject.IsArray())
                {
                    PdfArray objecta = (PdfArray)pdfObject;
                    numTree[key] = writer.AddToBody(objecta).IndirectReference;
                }
                else if (pdfObject is PdfIndirectReference)
                {
                    numTree[key] = (PdfIndirectReference)pdfObject;
                }
            }
        }

        public virtual void MapRole(PdfName used, PdfName standard)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)Get(PdfName.ROLEMAP);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
                Put(PdfName.ROLEMAP, pdfDictionary);
            }

            pdfDictionary.Put(used, standard);
        }

        public virtual void MapClass(PdfName name, PdfObject obj)
        {
            if (classMap == null)
            {
                classMap = new PdfDictionary();
                classes = new Dictionary<PdfName, PdfObject>();
            }

            classes.Add(name, obj);
        }

        public virtual PdfObject GetMappedClass(PdfName name)
        {
            if (classes == null)
            {
                return null;
            }

            classes.TryGetValue(name, out var value);
            return value;
        }

        internal void SetPageMark(int page, PdfIndirectReference struc)
        {
            PdfArray pdfArray;
            if (!parentTree.ContainsKey(page))
            {
                pdfArray = new PdfArray();
                parentTree[page] = pdfArray;
            }
            else
            {
                pdfArray = (PdfArray)parentTree[page];
            }

            pdfArray.Add(struc);
        }

        internal void SetAnnotationMark(int structParentIndex, PdfIndirectReference struc)
        {
            parentTree[structParentIndex] = struc;
        }

        private void NodeProcess(PdfDictionary struc, PdfIndirectReference reference)
        {
            PdfObject pdfObject = struc.Get(PdfName.K);
            if (pdfObject != null && pdfObject.IsArray())
            {
                PdfArray pdfArray = (PdfArray)pdfObject;
                for (int i = 0; i < pdfArray.Size; i++)
                {
                    PdfDictionary asDict = pdfArray.GetAsDict(i);
                    if (asDict != null && PdfName.STRUCTELEM.Equals(asDict.Get(PdfName.TYPE)) && pdfArray.GetPdfObject(i) is PdfStructureElement)
                    {
                        PdfStructureElement pdfStructureElement = (PdfStructureElement)asDict;
                        pdfArray.Set(i, pdfStructureElement.Reference);
                        NodeProcess(pdfStructureElement, pdfStructureElement.Reference);
                    }
                }
            }

            if (reference != null)
            {
                writer.AddToBody(struc, reference);
            }
        }

        internal void BuildTree()
        {
            CreateNumTree();
            PdfDictionary pdfDictionary = PdfNumberTree.WriteTree(numTree, writer);
            if (pdfDictionary != null)
            {
                Put(PdfName.PARENTTREE, writer.AddToBody(pdfDictionary).IndirectReference);
            }

            if (classMap != null && classes.Count > 0)
            {
                foreach (KeyValuePair<PdfName, PdfObject> @class in classes)
                {
                    PdfObject value = @class.Value;
                    if (value.IsDictionary())
                    {
                        classMap.Put(@class.Key, writer.AddToBody(value).IndirectReference);
                    }
                    else
                    {
                        if (!value.IsArray())
                        {
                            continue;
                        }

                        PdfArray pdfArray = new PdfArray();
                        PdfArray pdfArray2 = (PdfArray)value;
                        for (int i = 0; i < pdfArray2.Size; i++)
                        {
                            if (pdfArray2.GetPdfObject(i).IsDictionary())
                            {
                                pdfArray.Add(writer.AddToBody(pdfArray2.GetAsDict(i)).IndirectReference);
                            }
                        }

                        classMap.Put(@class.Key, pdfArray);
                    }
                }

                Put(PdfName.CLASSMAP, writer.AddToBody(classMap).IndirectReference);
            }

            NodeProcess(this, reference);
        }

        public virtual PdfObject GetAttribute(PdfName name)
        {
            PdfDictionary asDict = GetAsDict(PdfName.A);
            if (asDict != null && asDict.Contains(name))
            {
                return asDict.Get(name);
            }

            return null;
        }

        public virtual void SetAttribute(PdfName name, PdfObject obj)
        {
            PdfDictionary pdfDictionary = GetAsDict(PdfName.A);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
                Put(PdfName.A, pdfDictionary);
            }

            pdfDictionary.Put(name, obj);
        }
    }
}
