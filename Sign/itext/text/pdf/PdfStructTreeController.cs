using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfStructTreeController
    {
        public enum ReturnType
        {
            BELOW,
            FOUND,
            ABOVE,
            NOTFOUND
        }

        private PdfDictionary structTreeRoot;

        private PdfCopy writer;

        private PdfStructureTreeRoot structureTreeRoot;

        private PdfDictionary parentTree;

        protected internal PdfReader reader;

        private PdfDictionary roleMap;

        private PdfDictionary sourceRoleMap;

        private PdfDictionary sourceClassMap;

        private PdfIndirectReference nullReference;

        protected internal PdfStructTreeController(PdfReader reader, PdfCopy writer)
        {
            if (!writer.IsTagged())
            {
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("no.structtreeroot.found"));
            }

            this.writer = writer;
            structureTreeRoot = writer.StructureTreeRoot;
            structureTreeRoot.Put(PdfName.PARENTTREE, new PdfDictionary(PdfName.STRUCTELEM));
            SetReader(reader);
        }

        protected internal virtual void SetReader(PdfReader reader)
        {
            this.reader = reader;
            PdfObject obj = reader.Catalog.Get(PdfName.STRUCTTREEROOT);
            obj = GetDirectObject(obj);
            if (obj == null || !obj.IsDictionary())
            {
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("no.structtreeroot.found"));
            }

            structTreeRoot = (PdfDictionary)obj;
            obj = GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            if (obj == null || !obj.IsDictionary())
            {
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("the.document.does.not.contain.parenttree"));
            }

            parentTree = (PdfDictionary)obj;
            sourceRoleMap = null;
            sourceClassMap = null;
            nullReference = null;
        }

        public static bool CheckTagged(PdfReader reader)
        {
            PdfObject obj = reader.Catalog.Get(PdfName.STRUCTTREEROOT);
            obj = GetDirectObject(obj);
            if (obj == null || !obj.IsDictionary())
            {
                return false;
            }

            obj = GetDirectObject(((PdfDictionary)obj).Get(PdfName.PARENTTREE));
            if (obj == null || !obj.IsDictionary())
            {
                return false;
            }

            return true;
        }

        public static PdfObject GetDirectObject(PdfObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            while (obj.IsIndirect())
            {
                obj = PdfReader.GetPdfObjectRelease(obj);
            }

            return obj;
        }

        public virtual void CopyStructTreeForPage(PdfNumber sourceArrayNumber, int newArrayNumber)
        {
            if (CopyPageMarks(parentTree, sourceArrayNumber, newArrayNumber) == ReturnType.NOTFOUND)
            {
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("structparent.not.found"));
            }
        }

        private ReturnType CopyPageMarks(PdfDictionary parentTree, PdfNumber arrayNumber, int newArrayNumber)
        {
            PdfArray pdfArray = (PdfArray)GetDirectObject(parentTree.Get(PdfName.NUMS));
            if (pdfArray == null)
            {
                PdfArray pdfArray2 = (PdfArray)GetDirectObject(parentTree.Get(PdfName.KIDS));
                if (pdfArray2 == null)
                {
                    return ReturnType.NOTFOUND;
                }

                int num = pdfArray2.Size / 2;
                int num2 = 0;
                while (true)
                {
                    PdfDictionary pdfDictionary = (PdfDictionary)GetDirectObject(pdfArray2.GetPdfObject(num + num2));
                    switch (CopyPageMarks(pdfDictionary, arrayNumber, newArrayNumber))
                    {
                        case ReturnType.FOUND:
                            return ReturnType.FOUND;
                        case ReturnType.ABOVE:
                            num2 += num;
                            num /= 2;
                            if (num == 0)
                            {
                                num = 1;
                            }

                            if (num + num2 == pdfArray2.Size)
                            {
                                return ReturnType.ABOVE;
                            }

                            break;
                        case ReturnType.BELOW:
                            if (num + num2 == 0)
                            {
                                return ReturnType.BELOW;
                            }

                            if (num == 0)
                            {
                                return ReturnType.NOTFOUND;
                            }

                            num /= 2;
                            break;
                        default:
                            return ReturnType.NOTFOUND;
                    }
                }
            }

            if (pdfArray.Size == 0)
            {
                return ReturnType.NOTFOUND;
            }

            return FindAndCopyMarks(pdfArray, arrayNumber.IntValue, newArrayNumber);
        }

        private ReturnType FindAndCopyMarks(PdfArray pages, int arrayNumber, int newArrayNumber)
        {
            if (pages.GetAsNumber(0).IntValue > arrayNumber)
            {
                return ReturnType.BELOW;
            }

            if (pages.GetAsNumber(pages.Size - 2).IntValue < arrayNumber)
            {
                return ReturnType.ABOVE;
            }

            int num = pages.Size / 4;
            int num2 = 0;
            while (true)
            {
                int intValue = pages.GetAsNumber((num2 + num) * 2).IntValue;
                if (intValue == arrayNumber)
                {
                    PdfObject pdfObject = pages.GetPdfObject((num2 + num) * 2 + 1);
                    PdfObject inp = pdfObject;
                    while (pdfObject.IsIndirect())
                    {
                        pdfObject = PdfReader.GetPdfObjectRelease(pdfObject);
                    }

                    if (pdfObject.IsArray())
                    {
                        PdfObject pdfObject2 = null;
                        foreach (PdfObject item in (PdfArray)pdfObject)
                        {
                            if (item.IsNull())
                            {
                                if (nullReference == null)
                                {
                                    nullReference = writer.AddToBody(new PdfNull()).IndirectReference;
                                }

                                structureTreeRoot.SetPageMark(newArrayNumber, nullReference);
                                continue;
                            }

                            PdfObject pdfObject3 = writer.CopyObject(item, keepStruct: true, directRootKids: false);
                            if (pdfObject2 == null)
                            {
                                pdfObject2 = pdfObject3;
                            }

                            structureTreeRoot.SetPageMark(newArrayNumber, (PdfIndirectReference)pdfObject3);
                        }

                        AttachStructTreeRootKids(pdfObject2);
                    }
                    else
                    {
                        if (!pdfObject.IsDictionary())
                        {
                            return ReturnType.NOTFOUND;
                        }

                        if (GetKDict((PdfDictionary)pdfObject) == null)
                        {
                            return ReturnType.NOTFOUND;
                        }

                        PdfObject pdfObject4 = writer.CopyObject(inp, keepStruct: true, directRootKids: false);
                        structureTreeRoot.SetAnnotationMark(newArrayNumber, (PdfIndirectReference)pdfObject4);
                    }

                    return ReturnType.FOUND;
                }

                if (intValue < arrayNumber)
                {
                    num2 += num;
                    num /= 2;
                    if (num == 0)
                    {
                        num = 1;
                    }

                    if (num + num2 == pages.Size)
                    {
                        return ReturnType.NOTFOUND;
                    }
                }
                else
                {
                    if (num + num2 == 0)
                    {
                        return ReturnType.BELOW;
                    }

                    if (num == 0)
                    {
                        break;
                    }

                    num /= 2;
                }
            }

            return ReturnType.NOTFOUND;
        }

        protected internal virtual void AttachStructTreeRootKids(PdfObject firstNotNullKid)
        {
            PdfObject pdfObject = structTreeRoot.Get(PdfName.K);
            if (pdfObject == null || (!pdfObject.IsArray() && !pdfObject.IsIndirect()))
            {
                AddKid(structureTreeRoot, firstNotNullKid);
                return;
            }

            if (pdfObject.IsIndirect())
            {
                AddKid(pdfObject);
                return;
            }

            foreach (PdfObject item in (PdfArray)pdfObject)
            {
                AddKid(item);
            }
        }

        internal static PdfDictionary GetKDict(PdfDictionary obj)
        {
            PdfDictionary asDict = obj.GetAsDict(PdfName.K);
            if (asDict != null)
            {
                if (PdfName.OBJR.Equals(asDict.GetAsName(PdfName.TYPE)))
                {
                    return asDict;
                }
            }
            else
            {
                PdfArray asArray = obj.GetAsArray(PdfName.K);
                if (asArray == null)
                {
                    return null;
                }

                for (int i = 0; i < asArray.Size; i++)
                {
                    asDict = asArray.GetAsDict(i);
                    if (asDict != null && PdfName.OBJR.Equals(asDict.GetAsName(PdfName.TYPE)))
                    {
                        return asDict;
                    }
                }
            }

            return null;
        }

        private void AddKid(PdfObject obj)
        {
            if (obj.IsIndirect())
            {
                PRIndirectReference pRIndirectReference = (PRIndirectReference)obj;
                RefKey key = new RefKey(pRIndirectReference);
                if (!writer.indirects.ContainsKey(key))
                {
                    writer.CopyIndirect(pRIndirectReference, keepStructure: true, directRootKids: false);
                }

                PdfIndirectReference @ref = writer.indirects[key].Ref;
                if (writer.updateRootKids)
                {
                    AddKid(structureTreeRoot, @ref);
                    writer.StructureTreeRootKidsForReaderImported(reader);
                }
            }
        }

        private static PdfArray GetDirectArray(PdfArray input)
        {
            PdfArray pdfArray = new PdfArray();
            for (int i = 0; i < input.Size; i++)
            {
                PdfObject directObject = GetDirectObject(input.GetPdfObject(i));
                if (directObject != null)
                {
                    if (directObject.IsArray())
                    {
                        pdfArray.Add(GetDirectArray((PdfArray)directObject));
                    }
                    else if (directObject.IsDictionary())
                    {
                        pdfArray.Add(GetDirectDict((PdfDictionary)directObject));
                    }
                    else
                    {
                        pdfArray.Add(directObject);
                    }
                }
            }

            return pdfArray;
        }

        private static PdfDictionary GetDirectDict(PdfDictionary input)
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            foreach (KeyValuePair<PdfName, PdfObject> item in input.hashMap)
            {
                PdfObject directObject = GetDirectObject(item.Value);
                if (directObject != null)
                {
                    if (directObject.IsArray())
                    {
                        pdfDictionary.Put(item.Key, GetDirectArray((PdfArray)directObject));
                    }
                    else if (directObject.IsDictionary())
                    {
                        pdfDictionary.Put(item.Key, GetDirectDict((PdfDictionary)directObject));
                    }
                    else
                    {
                        pdfDictionary.Put(item.Key, directObject);
                    }
                }
            }

            return pdfDictionary;
        }

        public static bool CompareObjects(PdfObject value1, PdfObject value2)
        {
            value2 = GetDirectObject(value2);
            if (value2 == null)
            {
                return false;
            }

            if (value1.Type != value2.Type)
            {
                return false;
            }

            if (value1.IsBoolean())
            {
                if (value1 == value2)
                {
                    return true;
                }

                if (value2 is PdfBoolean)
                {
                    return ((PdfBoolean)value1).BooleanValue == ((PdfBoolean)value2).BooleanValue;
                }

                return false;
            }

            if (value1.IsName())
            {
                return value1.Equals(value2);
            }

            if (value1.IsNumber())
            {
                if (value1 == value2)
                {
                    return true;
                }

                if (value2 is PdfNumber)
                {
                    return ((PdfNumber)value1).DoubleValue == ((PdfNumber)value2).DoubleValue;
                }

                return false;
            }

            if (value1.IsNull())
            {
                if (value1 == value2)
                {
                    return true;
                }

                if (value2 is PdfNull)
                {
                    return true;
                }

                return false;
            }

            if (value1.IsString())
            {
                if (value1 == value2)
                {
                    return true;
                }

                if (value2 is PdfString)
                {
                    if (value2 != null || value1.ToString() != null)
                    {
                        return value1.ToString() == value2.ToString();
                    }

                    return true;
                }

                return false;
            }

            if (value1.IsArray())
            {
                PdfArray pdfArray = (PdfArray)value1;
                PdfArray pdfArray2 = (PdfArray)value2;
                if (pdfArray.Size != pdfArray2.Size)
                {
                    return false;
                }

                for (int i = 0; i < pdfArray.Size; i++)
                {
                    if (!CompareObjects(pdfArray.GetPdfObject(i), pdfArray2.GetPdfObject(i)))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (value1.IsDictionary())
            {
                PdfDictionary pdfDictionary = (PdfDictionary)value1;
                PdfDictionary pdfDictionary2 = (PdfDictionary)value2;
                if (pdfDictionary.Size != pdfDictionary2.Size)
                {
                    return false;
                }

                foreach (PdfName key in pdfDictionary.hashMap.Keys)
                {
                    if (!CompareObjects(pdfDictionary.Get(key), pdfDictionary2.Get(key)))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        internal void AddClass(PdfObject obj)
        {
            obj = GetDirectObject(obj);
            if (obj.IsDictionary())
            {
                PdfObject pdfObject = ((PdfDictionary)obj).Get(PdfName.C);
                if (pdfObject == null)
                {
                    return;
                }

                if (pdfObject.IsArray())
                {
                    PdfArray pdfArray = (PdfArray)pdfObject;
                    for (int i = 0; i < pdfArray.Size; i++)
                    {
                        AddClass(pdfArray.GetPdfObject(i));
                    }
                }
                else if (pdfObject.IsName())
                {
                    AddClass(pdfObject);
                }
            }
            else
            {
                if (!obj.IsName())
                {
                    return;
                }

                PdfName pdfName = (PdfName)obj;
                if (sourceClassMap == null)
                {
                    obj = GetDirectObject(structTreeRoot.Get(PdfName.CLASSMAP));
                    if (obj == null || !obj.IsDictionary())
                    {
                        return;
                    }

                    sourceClassMap = (PdfDictionary)obj;
                }

                obj = GetDirectObject(sourceClassMap.Get(pdfName));
                if (obj == null)
                {
                    return;
                }

                PdfObject mappedClass = structureTreeRoot.GetMappedClass(pdfName);
                if (mappedClass != null)
                {
                    if (!CompareObjects(mappedClass, obj))
                    {
                        throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("conflict.in.classmap", pdfName));
                    }
                }
                else if (obj.IsDictionary())
                {
                    structureTreeRoot.MapClass(pdfName, GetDirectDict((PdfDictionary)obj));
                }
                else if (obj.IsArray())
                {
                    structureTreeRoot.MapClass(pdfName, GetDirectArray((PdfArray)obj));
                }
            }
        }

        internal void AddRole(PdfName structType)
        {
            if (structType == null)
            {
                return;
            }

            foreach (PdfName standardStructElem in writer.GetStandardStructElems())
            {
                if (standardStructElem.Equals(structType))
                {
                    return;
                }
            }

            PdfObject directObject;
            if (sourceRoleMap == null)
            {
                directObject = GetDirectObject(structTreeRoot.Get(PdfName.ROLEMAP));
                if (directObject == null || !directObject.IsDictionary())
                {
                    return;
                }

                sourceRoleMap = (PdfDictionary)directObject;
            }

            directObject = sourceRoleMap.Get(structType);
            if (directObject == null || !directObject.IsName())
            {
                return;
            }

            PdfObject pdfObject;
            if (roleMap == null)
            {
                roleMap = new PdfDictionary();
                structureTreeRoot.Put(PdfName.ROLEMAP, roleMap);
                roleMap.Put(structType, directObject);
            }
            else if ((pdfObject = roleMap.Get(structType)) != null)
            {
                if (!pdfObject.Equals(directObject))
                {
                    throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("conflict.in.rolemap", directObject));
                }
            }
            else
            {
                roleMap.Put(structType, directObject);
            }
        }

        protected virtual void AddKid(PdfDictionary parent, PdfObject kid)
        {
            PdfObject pdfObject = parent.Get(PdfName.K);
            PdfArray pdfArray;
            if (pdfObject is PdfArray)
            {
                pdfArray = (PdfArray)pdfObject;
            }
            else
            {
                pdfArray = new PdfArray();
                if (pdfObject != null)
                {
                    pdfArray.Add(pdfObject);
                }
            }

            pdfArray.Add(kid);
            parent.Put(PdfName.K, pdfArray);
        }
    }
}
