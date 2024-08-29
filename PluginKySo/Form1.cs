using Sign.CA_Manager;

namespace PluginKySo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "Chọn File thực hiện ký số";
            dlg.Filter = "File lựa chọn|*.pdf;.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                byte[] fi = File.ReadAllBytes(dlg.FileName);
                var cer = new WSServices();
                byte[] fiResult = cer.SignatureFile(fi);

                if (fiResult == null)
                    return;
                SaveFileDialog dig = new SaveFileDialog();
                dig.Filter = "txt files (*.pdf)|*.pdf;";
                dig.FilterIndex = 2;
                dig.RestoreDirectory = true;

                if (dig.ShowDialog() == DialogResult.OK)
                    File.WriteAllBytes(dig.FileName, fiResult);
            }
        }
    }
}