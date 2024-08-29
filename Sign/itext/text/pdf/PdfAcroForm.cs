using Sign.itext.pdf;
using Sign.SystemItext.util;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfAcroForm : PdfDictionary
    {
        private PdfWriter writer;

        private Dictionary<PdfTemplate, object> fieldTemplates = new Dictionary<PdfTemplate, object>();

        private PdfArray documentFields = new PdfArray();

        private PdfArray calculationOrder = new PdfArray();

        private int sigFlags;

        public virtual bool NeedAppearances
        {
            set
            {
                Put(PdfName.NEEDAPPEARANCES, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }

        public virtual int SigFlags
        {
            set
            {
                sigFlags |= value;
            }
        }

        public PdfAcroForm(PdfWriter writer)
        {
            this.writer = writer;
        }

        public virtual void AddFieldTemplates(Dictionary<PdfTemplate, object> ft)
        {
            foreach (PdfTemplate key in ft.Keys)
            {
                fieldTemplates[key] = ft[key];
            }
        }

        public virtual void AddDocumentField(PdfIndirectReference piref)
        {
            documentFields.Add(piref);
        }

        public virtual bool IsValid()
        {
            if (documentFields.Size == 0)
            {
                return false;
            }

            Put(PdfName.FIELDS, documentFields);
            if (sigFlags != 0)
            {
                Put(PdfName.SIGFLAGS, new PdfNumber(sigFlags));
            }

            if (calculationOrder.Size > 0)
            {
                Put(PdfName.CO, calculationOrder);
            }

            if (fieldTemplates.Count == 0)
            {
                return true;
            }

            PdfDictionary pdfDictionary = new PdfDictionary();
            foreach (PdfTemplate key in fieldTemplates.Keys)
            {
                PdfFormField.MergeResources(pdfDictionary, (PdfDictionary)key.Resources);
            }

            Put(PdfName.DR, pdfDictionary);
            Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
            PdfDictionary pdfDictionary2 = (PdfDictionary)pdfDictionary.Get(PdfName.FONT);
            if (pdfDictionary2 != null)
            {
                writer.EliminateFontSubset(pdfDictionary2);
            }

            return true;
        }

        public virtual void AddCalculationOrder(PdfFormField formField)
        {
            calculationOrder.Add(formField.IndirectReference);
        }

        public virtual void AddFormField(PdfFormField formField)
        {
            writer.AddAnnotation(formField);
        }

        public virtual PdfFormField AddHtmlPostButton(string name, string caption, string value, string url, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfAction action = PdfAction.CreateSubmitForm(url, null, 4);
            PdfFormField pdfFormField = new PdfFormField(writer, llx, lly, urx, ury, action);
            SetButtonParams(pdfFormField, 65536, name, value);
            DrawButton(pdfFormField, caption, font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual PdfFormField AddResetButton(string name, string caption, string value, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfAction action = PdfAction.CreateResetForm(null, 0);
            PdfFormField pdfFormField = new PdfFormField(writer, llx, lly, urx, ury, action);
            SetButtonParams(pdfFormField, 65536, name, value);
            DrawButton(pdfFormField, caption, font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual PdfFormField AddMap(string name, string value, string url, PdfContentByte appearance, float llx, float lly, float urx, float ury)
        {
            PdfAction action = PdfAction.CreateSubmitForm(url, null, 20);
            PdfFormField pdfFormField = new PdfFormField(writer, llx, lly, urx, ury, action);
            SetButtonParams(pdfFormField, 65536, name, null);
            PdfAppearance pdfAppearance = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            pdfAppearance.Add(appearance);
            pdfFormField.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, pdfAppearance);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual void SetButtonParams(PdfFormField button, int characteristics, string name, string value)
        {
            button.Button = characteristics;
            button.Flags = 4;
            button.SetPage();
            button.FieldName = name;
            if (value != null)
            {
                button.ValueAsString = value;
            }
        }

        public virtual void DrawButton(PdfFormField button, string caption, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfAppearance pdfAppearance = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            pdfAppearance.DrawButton(0f, 0f, urx - llx, ury - lly, caption, font, fontSize);
            button.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, pdfAppearance);
        }

        public virtual PdfFormField AddHiddenField(string name, string value)
        {
            PdfFormField pdfFormField = PdfFormField.CreateEmpty(writer);
            pdfFormField.FieldName = name;
            pdfFormField.ValueAsName = value;
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual PdfFormField AddSingleLineTextField(string name, string text, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateTextField(writer, multiline: false, password: false, 0);
            SetTextFieldParams(pdfFormField, text, name, llx, lly, urx, ury);
            DrawSingleLineOfText(pdfFormField, text, font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual PdfFormField AddMultiLineTextField(string name, string text, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateTextField(writer, multiline: true, password: false, 0);
            SetTextFieldParams(pdfFormField, text, name, llx, lly, urx, ury);
            DrawMultiLineOfText(pdfFormField, text, font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual PdfFormField AddSingleLinePasswordField(string name, string text, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateTextField(writer, multiline: false, password: true, 0);
            SetTextFieldParams(pdfFormField, text, name, llx, lly, urx, ury);
            DrawSingleLineOfText(pdfFormField, text, font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual void SetTextFieldParams(PdfFormField field, string text, string name, float llx, float lly, float urx, float ury)
        {
            field.SetWidget(new Rectangle(llx, lly, urx, ury), PdfAnnotation.HIGHLIGHT_INVERT);
            field.ValueAsString = text;
            field.DefaultValueAsString = text;
            field.FieldName = name;
            field.Flags = 4;
            field.SetPage();
        }

        public virtual void DrawSingleLineOfText(PdfFormField field, string text, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfAppearance pdfAppearance = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            PdfAppearance pdfAppearance2 = (PdfAppearance)pdfAppearance.Duplicate;
            pdfAppearance2.SetFontAndSize(font, fontSize);
            pdfAppearance2.ResetRGBColorFill();
            field.DefaultAppearanceString = pdfAppearance2;
            pdfAppearance.DrawTextField(0f, 0f, urx - llx, ury - lly);
            pdfAppearance.BeginVariableText();
            pdfAppearance.SaveState();
            pdfAppearance.Rectangle(3f, 3f, urx - llx - 6f, ury - lly - 6f);
            pdfAppearance.Clip();
            pdfAppearance.NewPath();
            pdfAppearance.BeginText();
            pdfAppearance.SetFontAndSize(font, fontSize);
            pdfAppearance.ResetRGBColorFill();
            pdfAppearance.SetTextMatrix(4f, (ury - lly) / 2f - fontSize * 0.3f);
            pdfAppearance.ShowText(text);
            pdfAppearance.EndText();
            pdfAppearance.RestoreState();
            pdfAppearance.EndVariableText();
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, pdfAppearance);
        }

        public virtual void DrawMultiLineOfText(PdfFormField field, string text, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfAppearance pdfAppearance = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            PdfAppearance pdfAppearance2 = (PdfAppearance)pdfAppearance.Duplicate;
            pdfAppearance2.SetFontAndSize(font, fontSize);
            pdfAppearance2.ResetRGBColorFill();
            field.DefaultAppearanceString = pdfAppearance2;
            pdfAppearance.DrawTextField(0f, 0f, urx - llx, ury - lly);
            pdfAppearance.BeginVariableText();
            pdfAppearance.SaveState();
            pdfAppearance.Rectangle(3f, 3f, urx - llx - 6f, ury - lly - 6f);
            pdfAppearance.Clip();
            pdfAppearance.NewPath();
            pdfAppearance.BeginText();
            pdfAppearance.SetFontAndSize(font, fontSize);
            pdfAppearance.ResetRGBColorFill();
            pdfAppearance.SetTextMatrix(4f, 5f);
            StringTokenizer stringTokenizer = new StringTokenizer(text, "\n");
            float num = ury - lly;
            while (stringTokenizer.HasMoreTokens())
            {
                num -= fontSize * 1.2f;
                pdfAppearance.ShowTextAligned(0, stringTokenizer.NextToken(), 3f, num, 0f);
            }

            pdfAppearance.EndText();
            pdfAppearance.RestoreState();
            pdfAppearance.EndVariableText();
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, pdfAppearance);
        }

        public virtual PdfFormField AddCheckBox(string name, string value, bool status, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateCheckBox(writer);
            SetCheckBoxParams(pdfFormField, name, value, status, llx, lly, urx, ury);
            DrawCheckBoxAppearences(pdfFormField, value, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual void SetCheckBoxParams(PdfFormField field, string name, string value, bool status, float llx, float lly, float urx, float ury)
        {
            field.SetWidget(new Rectangle(llx, lly, urx, ury), PdfAnnotation.HIGHLIGHT_TOGGLE);
            field.FieldName = name;
            if (status)
            {
                field.ValueAsName = value;
                field.AppearanceState = value;
            }
            else
            {
                field.ValueAsName = "Off";
                field.AppearanceState = "Off";
            }

            field.Flags = 4;
            field.SetPage();
            field.BorderStyle = new PdfBorderDictionary(1f, 0);
        }

        public virtual void DrawCheckBoxAppearences(PdfFormField field, string value, float llx, float lly, float urx, float ury)
        {
            BaseFont bf = BaseFont.CreateFont("ZapfDingbats", "Cp1252", embedded: false);
            float num = ury - lly;
            PdfAppearance pdfAppearance = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            PdfAppearance pdfAppearance2 = (PdfAppearance)pdfAppearance.Duplicate;
            pdfAppearance2.SetFontAndSize(bf, num);
            pdfAppearance2.ResetRGBColorFill();
            field.DefaultAppearanceString = pdfAppearance2;
            pdfAppearance.DrawTextField(0f, 0f, urx - llx, ury - lly);
            pdfAppearance.SaveState();
            pdfAppearance.ResetRGBColorFill();
            pdfAppearance.BeginText();
            pdfAppearance.SetFontAndSize(bf, num);
            pdfAppearance.ShowTextAligned(1, "4", (urx - llx) / 2f, (ury - lly) / 2f - num * 0.3f, 0f);
            pdfAppearance.EndText();
            pdfAppearance.RestoreState();
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, value, pdfAppearance);
            PdfAppearance pdfAppearance3 = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            pdfAppearance3.DrawTextField(0f, 0f, urx - llx, ury - lly);
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, "Off", pdfAppearance3);
        }

        public virtual PdfFormField GetRadioGroup(string name, string defaultValue, bool noToggleToOff)
        {
            PdfFormField pdfFormField = PdfFormField.CreateRadioButton(writer, noToggleToOff);
            pdfFormField.FieldName = name;
            pdfFormField.ValueAsName = defaultValue;
            return pdfFormField;
        }

        public virtual void AddRadioGroup(PdfFormField radiogroup)
        {
            AddFormField(radiogroup);
        }

        public virtual PdfFormField AddRadioButton(PdfFormField radiogroup, string value, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateEmpty(writer);
            pdfFormField.SetWidget(new Rectangle(llx, lly, urx, ury), PdfAnnotation.HIGHLIGHT_TOGGLE);
            if (((PdfName)radiogroup.Get(PdfName.V)).ToString().Substring(1).Equals(value))
            {
                pdfFormField.AppearanceState = value;
            }
            else
            {
                pdfFormField.AppearanceState = "Off";
            }

            DrawRadioAppearences(pdfFormField, value, llx, lly, urx, ury);
            radiogroup.AddKid(pdfFormField);
            return pdfFormField;
        }

        public virtual void DrawRadioAppearences(PdfFormField field, string value, float llx, float lly, float urx, float ury)
        {
            PdfAppearance pdfAppearance = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            pdfAppearance.DrawRadioField(0f, 0f, urx - llx, ury - lly, on: true);
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, value, pdfAppearance);
            PdfAppearance pdfAppearance2 = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            pdfAppearance2.DrawRadioField(0f, 0f, urx - llx, ury - lly, on: false);
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, "Off", pdfAppearance2);
        }

        public virtual PdfFormField AddSelectList(string name, string[] options, string defaultValue, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateList(writer, options, 0);
            SetChoiceParams(pdfFormField, name, defaultValue, llx, lly, urx, ury);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in options)
            {
                stringBuilder.Append(value).Append('\n');
            }

            DrawMultiLineOfText(pdfFormField, stringBuilder.ToString(), font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual PdfFormField AddSelectList(string name, string[,] options, string defaultValue, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateList(writer, options, 0);
            SetChoiceParams(pdfFormField, name, defaultValue, llx, lly, urx, ury);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < options.GetLength(0); i++)
            {
                stringBuilder.Append(options[i, 1]).Append('\n');
            }

            DrawMultiLineOfText(pdfFormField, stringBuilder.ToString(), font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual PdfFormField AddComboBox(string name, string[] options, string defaultValue, bool editable, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateCombo(writer, editable, options, 0);
            SetChoiceParams(pdfFormField, name, defaultValue, llx, lly, urx, ury);
            if (defaultValue == null)
            {
                defaultValue = options[0];
            }

            DrawSingleLineOfText(pdfFormField, defaultValue, font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual PdfFormField AddComboBox(string name, string[,] options, string defaultValue, bool editable, BaseFont font, float fontSize, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateCombo(writer, editable, options, 0);
            SetChoiceParams(pdfFormField, name, defaultValue, llx, lly, urx, ury);
            string text = null;
            for (int i = 0; i < options.GetLength(0); i++)
            {
                if (options[i, 0].Equals(defaultValue))
                {
                    text = options[i, 1];
                    break;
                }
            }

            if (text == null)
            {
                text = options[0, 1];
            }

            DrawSingleLineOfText(pdfFormField, text, font, fontSize, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual void SetChoiceParams(PdfFormField field, string name, string defaultValue, float llx, float lly, float urx, float ury)
        {
            field.SetWidget(new Rectangle(llx, lly, urx, ury), PdfAnnotation.HIGHLIGHT_INVERT);
            if (defaultValue != null)
            {
                field.ValueAsString = defaultValue;
                field.DefaultValueAsString = defaultValue;
            }

            field.FieldName = name;
            field.Flags = 4;
            field.SetPage();
            field.BorderStyle = new PdfBorderDictionary(2f, 0);
        }

        public virtual PdfFormField AddSignature(string name, float llx, float lly, float urx, float ury)
        {
            PdfFormField pdfFormField = PdfFormField.CreateSignature(writer);
            SetSignatureParams(pdfFormField, name, llx, lly, urx, ury);
            DrawSignatureAppearences(pdfFormField, llx, lly, urx, ury);
            AddFormField(pdfFormField);
            return pdfFormField;
        }

        public virtual void SetSignatureParams(PdfFormField field, string name, float llx, float lly, float urx, float ury)
        {
            field.SetWidget(new Rectangle(llx, lly, urx, ury), PdfAnnotation.HIGHLIGHT_INVERT);
            field.FieldName = name;
            field.Flags = 4;
            field.SetPage();
            field.MKBorderColor = BaseColor.BLACK;
            field.MKBackgroundColor = BaseColor.WHITE;
        }

        public virtual void DrawSignatureAppearences(PdfFormField field, float llx, float lly, float urx, float ury)
        {
            PdfAppearance pdfAppearance = PdfAppearance.CreateAppearance(writer, urx - llx, ury - lly);
            pdfAppearance.SetGrayFill(1f);
            pdfAppearance.Rectangle(0f, 0f, urx - llx, ury - lly);
            pdfAppearance.Fill();
            pdfAppearance.SetGrayStroke(0f);
            pdfAppearance.SetLineWidth(1f);
            pdfAppearance.Rectangle(0.5f, 0.5f, urx - llx - 0.5f, ury - lly - 0.5f);
            pdfAppearance.ClosePathStroke();
            pdfAppearance.SaveState();
            pdfAppearance.Rectangle(1f, 1f, urx - llx - 2f, ury - lly - 2f);
            pdfAppearance.Clip();
            pdfAppearance.NewPath();
            pdfAppearance.RestoreState();
            field.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, pdfAppearance);
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 15, this);
            base.ToPdf(writer, os);
        }
    }
}
