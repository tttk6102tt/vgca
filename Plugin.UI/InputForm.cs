using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Plugin.UI
{
  internal class InputForm : Form
  {
    private IContainer components;
    private TextBox textBox1;
    private Label label1;
    private Button btnApply;
    private Button btnCancel;

    public string Message => this.textBox1.Text.Trim();

    public InputForm(string caption = "")
    {
      this.InitializeComponent();
      this.ActiveControl = (Control) this.textBox1;
      if (string.IsNullOrEmpty(caption))
        return;
      this.Text = caption;
    }

    private void btnCancel_Click(object sender, EventArgs e) => this.Close();

    private void btnApply_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(this.textBox1.Text.Trim()))
      {
        int num = (int) MessageBox.Show("Bạn chưa nhập nội dung!", "Nhập nội dung", MessageBoxButtons.OK, MessageBoxIcon.Question);
      }
      else
        this.DialogResult = DialogResult.OK;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (InputForm));
      this.textBox1 = new TextBox();
      this.label1 = new Label();
      this.btnApply = new Button();
      this.btnCancel = new Button();
      this.SuspendLayout();
      this.textBox1.Location = new Point(12, 32);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new Size(355, 20);
      this.textBox1.TabIndex = 0;
      this.label1.AutoSize = true;
      this.label1.Location = new Point(12, 16);
      this.label1.Name = "label1";
      this.label1.Size = new Size(80, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Nhập nội dung:";
      this.btnApply.Location = new Point(191, 74);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new Size(75, 23);
      this.btnApply.TabIndex = 2;
      this.btnApply.Text = "Chọn";
      this.btnApply.UseVisualStyleBackColor = true;
      this.btnApply.Click += new EventHandler(this.btnApply_Click);
      this.btnCancel.Location = new Point(292, 74);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new Size(75, 23);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Hủy";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
      this.AcceptButton = (IButtonControl) this.btnApply;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(387, 109);
      this.Controls.Add((Control) this.btnCancel);
      this.Controls.Add((Control) this.btnApply);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.textBox1);
      this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (InputForm);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "Nhập nội dung";
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
