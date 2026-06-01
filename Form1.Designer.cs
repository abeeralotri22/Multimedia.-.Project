namespace WindowsFormsApp2
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
            this.PlayAudiobtn = new System.Windows.Forms.Button();
            this.DragDropLabel = new System.Windows.Forms.Label();
            this.AudioInfobtn = new System.Windows.Forms.Button();
            this.waveformPictureBox = new System.Windows.Forms.PictureBox();
            this.InsertAudiobtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.waveformPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // PlayAudiobtn
            // 
            this.PlayAudiobtn.Location = new System.Drawing.Point(139, 247);
            this.PlayAudiobtn.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.PlayAudiobtn.Name = "PlayAudiobtn";
            this.PlayAudiobtn.Size = new System.Drawing.Size(148, 48);
            this.PlayAudiobtn.TabIndex = 0;
            this.PlayAudiobtn.Text = "Play Audio ▶︎ ";
            this.PlayAudiobtn.UseVisualStyleBackColor = true;
            this.PlayAudiobtn.Click += new System.EventHandler(this.PlayAudiobtn_Click);
            // 
            // DragDropLabel
            // 
            this.DragDropLabel.AllowDrop = true;
            this.DragDropLabel.AutoSize = true;
            this.DragDropLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.DragDropLabel.Location = new System.Drawing.Point(22, 20);
            this.DragDropLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DragDropLabel.Name = "DragDropLabel";
            this.DragDropLabel.Padding = new System.Windows.Forms.Padding(132, 25, 132, 25);
            this.DragDropLabel.Size = new System.Drawing.Size(393, 67);
            this.DragDropLabel.TabIndex = 1;
            this.DragDropLabel.Text = "Drag and drop here";
            this.DragDropLabel.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragDrop);
            this.DragDropLabel.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragEnter);
            // 
            // AudioInfobtn
            // 
            this.AudioInfobtn.Location = new System.Drawing.Point(587, 99);
            this.AudioInfobtn.Name = "AudioInfobtn";
            this.AudioInfobtn.Size = new System.Drawing.Size(152, 44);
            this.AudioInfobtn.TabIndex = 2;
            this.AudioInfobtn.Text = "Display Audio Info.";
            this.AudioInfobtn.UseVisualStyleBackColor = true;
            this.AudioInfobtn.Click += new System.EventHandler(this.AudioInfobtn_Click_1);
            // 
            // waveformPictureBox
            // 
            this.waveformPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveformPictureBox.Location = new System.Drawing.Point(25, 99);
            this.waveformPictureBox.Name = "waveformPictureBox";
            this.waveformPictureBox.Size = new System.Drawing.Size(390, 129);
            this.waveformPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.waveformPictureBox.TabIndex = 3;
            this.waveformPictureBox.TabStop = false;
            // 
            // InsertAudiobtn
            // 
            this.InsertAudiobtn.Location = new System.Drawing.Point(587, 42);
            this.InsertAudiobtn.Name = "InsertAudiobtn";
            this.InsertAudiobtn.Size = new System.Drawing.Size(152, 45);
            this.InsertAudiobtn.TabIndex = 4;
            this.InsertAudiobtn.Text = "Insert Audio";
            this.InsertAudiobtn.UseVisualStyleBackColor = true;
            this.InsertAudiobtn.Click += new System.EventHandler(this.InsertAudiobtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 447);
            this.Controls.Add(this.InsertAudiobtn);
            this.Controls.Add(this.waveformPictureBox);
            this.Controls.Add(this.AudioInfobtn);
            this.Controls.Add(this.DragDropLabel);
            this.Controls.Add(this.PlayAudiobtn);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.waveformPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button PlayAudiobtn;
        private System.Windows.Forms.Label DragDropLabel;
        private System.Windows.Forms.Button AudioInfobtn;
        private System.Windows.Forms.PictureBox waveformPictureBox;
        private System.Windows.Forms.Button InsertAudiobtn;
    }
}

