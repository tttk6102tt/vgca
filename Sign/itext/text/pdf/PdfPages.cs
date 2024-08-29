using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfPages
    {
        private List<PdfIndirectReference> pages = new List<PdfIndirectReference>();

        private List<PdfIndirectReference> parents = new List<PdfIndirectReference>();

        private int leafSize = 10;

        private PdfWriter writer;

        private PdfIndirectReference topParent;

        internal PdfIndirectReference TopParent => topParent;

        internal PdfPages(PdfWriter writer)
        {
            this.writer = writer;
        }

        internal void AddPage(PdfDictionary page)
        {
            if (pages.Count % leafSize == 0)
            {
                parents.Add(writer.PdfIndirectReference);
            }

            PdfIndirectReference value = parents[parents.Count - 1];
            page.Put(PdfName.PARENT, value);
            PdfIndirectReference currentPage = writer.CurrentPage;
            writer.AddToBody(page, currentPage);
            pages.Add(currentPage);
        }

        internal PdfIndirectReference AddPageRef(PdfIndirectReference pageRef)
        {
            if (pages.Count % leafSize == 0)
            {
                parents.Add(writer.PdfIndirectReference);
            }

            pages.Add(pageRef);
            return parents[parents.Count - 1];
        }

        internal PdfIndirectReference WritePageTree()
        {
            if (pages.Count == 0)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("the.document.has.no.pages"));
            }

            int num = 1;
            List<PdfIndirectReference> list = parents;
            List<PdfIndirectReference> list2 = pages;
            List<PdfIndirectReference> list3 = new List<PdfIndirectReference>();
            while (true)
            {
                num *= leafSize;
                int num2 = leafSize;
                int num3 = list2.Count % leafSize;
                if (num3 == 0)
                {
                    num3 = leafSize;
                }

                for (int i = 0; i < list.Count; i++)
                {
                    int num4 = num;
                    int count;
                    if (i == list.Count - 1)
                    {
                        count = num3;
                        num4 = pages.Count % num;
                        if (num4 == 0)
                        {
                            num4 = num;
                        }
                    }
                    else
                    {
                        count = num2;
                    }

                    PdfDictionary pdfDictionary = new PdfDictionary(PdfName.PAGES);
                    pdfDictionary.Put(PdfName.COUNT, new PdfNumber(num4));
                    PdfArray pdfArray = new PdfArray();
                    List<PdfObject> arrayList = pdfArray.ArrayList;
                    foreach (PdfIndirectReference item in list2.GetRange(i * num2, count))
                    {
                        arrayList.Add(item);
                    }

                    pdfDictionary.Put(PdfName.KIDS, pdfArray);
                    if (list.Count > 1)
                    {
                        if (i % leafSize == 0)
                        {
                            list3.Add(writer.PdfIndirectReference);
                        }

                        pdfDictionary.Put(PdfName.PARENT, list3[i / leafSize]);
                    }

                    writer.AddToBody(pdfDictionary, list[i]);
                }

                if (list.Count == 1)
                {
                    break;
                }

                list2 = list;
                list = list3;
                list3 = new List<PdfIndirectReference>();
            }

            topParent = list[0];
            return topParent;
        }

        internal void SetLinearMode(PdfIndirectReference topParent)
        {
            if (parents.Count > 1)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("linear.page.mode.can.only.be.called.with.a.single.parent"));
            }

            if (topParent != null)
            {
                this.topParent = topParent;
                parents.Clear();
                parents.Add(topParent);
            }

            leafSize = 10000000;
        }

        internal void AddPage(PdfIndirectReference page)
        {
            pages.Add(page);
        }

        internal int ReorderPages(int[] order)
        {
            if (order == null)
            {
                return pages.Count;
            }

            if (parents.Count > 1)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("page.reordering.requires.a.single.parent.in.the.page.tree.call.pdfwriter.setlinearmode.after.open"));
            }

            if (order.Length != pages.Count)
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("page.reordering.requires.an.array.with.the.same.size.as.the.number.of.pages"));
            }

            int count = pages.Count;
            bool[] array = new bool[count];
            for (int i = 0; i < count; i++)
            {
                int num = order[i];
                if (num < 1 || num > count)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("page.reordering.requires.pages.between.1.and.1.found.2", count, num));
                }

                if (array[num - 1])
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("page.reordering.requires.no.page.repetition.page.1.is.repeated", num));
                }

                array[num - 1] = true;
            }

            PdfIndirectReference[] array2 = pages.ToArray();
            for (int j = 0; j < count; j++)
            {
                pages[j] = array2[order[j] - 1];
            }

            return count;
        }
    }
}
