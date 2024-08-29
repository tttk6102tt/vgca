
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Plugin.UI
{
    partial class frmRevisionViewer
    {
        private IContainer component;
        private RevisionViewer revisionViewer1;

        public frmRevisionViewer(string path, string revisionName)
        {
            this.InitializeComponent();
            this.revisionViewer1.AbsolutePath = path;
            this.Text = revisionName;
        }

        private void Form_Shown(
          object sender,
          EventArgs e)
        {
            this.revisionViewer1.RevisionShow();
            this.TopMost = true;
            this.TopMost = false;
        }

        private void Form_FormClosing(
          object sender,
          FormClosingEventArgs e)
        {
            try
            {
                if (!File.Exists(this.revisionViewer1.AbsolutePath))
                    return;
                File.Delete(this.revisionViewer1.AbsolutePath);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.component != null)
                {
                    this.component.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(frmRevisionViewer));
            this.revisionViewer1 = new RevisionViewer();
            this.SuspendLayout();
            this.revisionViewer1.AbsolutePath = (string)null;
            this.revisionViewer1.BackColor = SystemColors.Control;
            this.revisionViewer1.Dock = DockStyle.Fill;
            this.revisionViewer1.Location = new Point(0, 0);
            this.revisionViewer1.Name = "revisionViewer1";
            this.revisionViewer1.Size = new Size(934, 662);
            this.revisionViewer1.TabIndex = 0;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.ClientSize = new Size(934, 662);
            this.Controls.Add((Control)this.revisionViewer1);
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.Name = "frmRevisionViewer";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Revision Viewer";
            this.FormClosing += new FormClosingEventHandler(this.Form_FormClosing);
            this.Shown += new EventHandler(this.Form_Shown);
            this.ResumeLayout(false);
        }
    }
}