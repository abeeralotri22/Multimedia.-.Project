using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public class AudioInfoForm : Form
    {
        private Label labelInfo;

        public AudioInfoForm(string info)
        {
            this.Text = "Audio Information";
            this.Size = new Size(300, 400);

            labelInfo = new Label();
            labelInfo.Dock = DockStyle.Fill;
            labelInfo.Font = new Font("Tahoma", 12);
            labelInfo.Text = info;
            labelInfo.AutoSize = false;
            labelInfo.Padding = new Padding(20);
            this.Controls.Add(labelInfo);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AudioInfoForm
            // 
            this.ClientSize = new System.Drawing.Size(348, 379);
            this.Name = "AudioInfoForm";
            this.Text = "Audio Information";
            this.ResumeLayout(false);


        }
    }
}