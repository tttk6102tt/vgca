
namespace Plugin.UI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>  
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.preview = new Plugin.UI.ucPDFViewer();
            this.SuspendLayout();
            // 
            // preview
            // 
            this.preview.BackColor = System.Drawing.SystemColors.Control;
            this.preview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.preview.Location = new System.Drawing.Point(0, 0);
            this.preview.MaxZoomFactor = 6F;
            this.preview.MinimumSize = new System.Drawing.Size(675, 315);
            this.preview.MinZoomFactor = 0.15F;
            this.preview.Name = "preview";
            this.preview.PageMargin = new System.Windows.Forms.Padding(0, 2, 4, 2);
            this.preview.Rotation = Plugin.UI.ImageRotation.None;
            this.preview.Size = new System.Drawing.Size(800, 450);
            this.preview.TabIndex = 0;
            this.preview.ZoomStep = 0.25F;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.preview);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Tag = "UGjhuqFtIELhuqFjaCBExrDGoW5n";
            this.Text = "HiPT vSign Beta 1.0";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        public ucPDFViewer preview;
    }
}

