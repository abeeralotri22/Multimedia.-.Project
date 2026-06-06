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
            this.waveformPictureBox = new System.Windows.Forms.PictureBox();
            this.InsertAudiobtn = new System.Windows.Forms.Button();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.btnOpenCompression = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.waveformPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // PlayAudiobtn
            // 
            this.PlayAudiobtn.Location = new System.Drawing.Point(246, 376);
            this.PlayAudiobtn.Name = "PlayAudiobtn";
            this.PlayAudiobtn.Size = new System.Drawing.Size(141, 44);
            this.PlayAudiobtn.TabIndex = 0;
            this.PlayAudiobtn.Text = "Play Audio ▶︎ ";
            this.PlayAudiobtn.UseVisualStyleBackColor = true;
            this.PlayAudiobtn.Click += new System.EventHandler(this.PlayAudiobtn_Click);
            // 
            // DragDropLabel
            // 
            this.DragDropLabel.AllowDrop = true;
            this.DragDropLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.DragDropLabel.Location = new System.Drawing.Point(28, 25);
            this.DragDropLabel.Name = "DragDropLabel";
            this.DragDropLabel.Padding = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.DragDropLabel.Size = new System.Drawing.Size(502, 92);
            this.DragDropLabel.TabIndex = 1;
            this.DragDropLabel.Text = "Drag and drop here";
            this.DragDropLabel.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragDrop);
            this.DragDropLabel.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragEnter);
            // 
            // waveformPictureBox
            // 
            this.waveformPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveformPictureBox.Location = new System.Drawing.Point(32, 123);
            this.waveformPictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.waveformPictureBox.Name = "waveformPictureBox";
            this.waveformPictureBox.Size = new System.Drawing.Size(1665, 235);
            this.waveformPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.waveformPictureBox.TabIndex = 3;
            this.waveformPictureBox.TabStop = false;
            // 
            // InsertAudiobtn
            // 
            this.InsertAudiobtn.Location = new System.Drawing.Point(591, 25);
            this.InsertAudiobtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.InsertAudiobtn.Name = "InsertAudiobtn";
            this.InsertAudiobtn.Size = new System.Drawing.Size(195, 57);
            this.InsertAudiobtn.TabIndex = 4;
            this.InsertAudiobtn.Text = "Insert Audio";
            this.InsertAudiobtn.UseVisualStyleBackColor = true;
            this.InsertAudiobtn.Click += new System.EventHandler(this.InsertAudiobtn_Click);
            // 
            // InfoLabel
            // 
            this.InfoLabel.AutoSize = true;
            this.InfoLabel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.InfoLabel.Location = new System.Drawing.Point(28, 376);
            this.InfoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Padding = new System.Windows.Forms.Padding(14, 12, 14, 12);
            this.InfoLabel.Size = new System.Drawing.Size(163, 44);
            this.InfoLabel.TabIndex = 5;
            this.InfoLabel.Text = "Audio Information";
            // 
            // btnOpenCompression
            // 
            this.btnOpenCompression.Location = new System.Drawing.Point(834, 29);
            this.btnOpenCompression.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOpenCompression.Name = "btnOpenCompression";
            this.btnOpenCompression.Size = new System.Drawing.Size(194, 49);
            this.btnOpenCompression.TabIndex = 6;
            this.btnOpenCompression.Text = "Open Compression";
            this.btnOpenCompression.UseVisualStyleBackColor = true;
            this.btnOpenCompression.Click += new System.EventHandler(this.btnOpenCompression_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1710, 950);
            this.Controls.Add(this.btnOpenCompression);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.InsertAudiobtn);
            this.Controls.Add(this.waveformPictureBox);
            this.Controls.Add(this.DragDropLabel);
            this.Controls.Add(this.PlayAudiobtn);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.waveformPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button PlayAudiobtn;
        private System.Windows.Forms.Label DragDropLabel;
        private System.Windows.Forms.PictureBox waveformPictureBox;
        private System.Windows.Forms.Button InsertAudiobtn;
        private System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.Button btnOpenCompression;
    }
}

