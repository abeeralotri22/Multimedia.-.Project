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
            this.PlayAudiobtn.Location = new System.Drawing.Point(261, 192);
            this.PlayAudiobtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.PlayAudiobtn.Name = "PlayAudiobtn";
            this.PlayAudiobtn.Size = new System.Drawing.Size(94, 39);
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
            this.DragDropLabel.Location = new System.Drawing.Point(19, 16);
            this.DragDropLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DragDropLabel.Name = "DragDropLabel";
            this.DragDropLabel.Padding = new System.Windows.Forms.Padding(113, 20, 113, 20);
            this.DragDropLabel.Size = new System.Drawing.Size(325, 53);
            this.DragDropLabel.TabIndex = 1;
            this.DragDropLabel.Text = "Drag and drop here";
            this.DragDropLabel.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragDrop);
            this.DragDropLabel.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragEnter);
            // 
            // waveformPictureBox
            // 
            this.waveformPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveformPictureBox.Location = new System.Drawing.Point(21, 80);
            this.waveformPictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.waveformPictureBox.Name = "waveformPictureBox";
            this.waveformPictureBox.Size = new System.Drawing.Size(335, 105);
            this.waveformPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.waveformPictureBox.TabIndex = 3;
            this.waveformPictureBox.TabStop = false;
            // 
            // InsertAudiobtn
            // 
            this.InsertAudiobtn.Location = new System.Drawing.Point(503, 34);
            this.InsertAudiobtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.InsertAudiobtn.Name = "InsertAudiobtn";
            this.InsertAudiobtn.Size = new System.Drawing.Size(130, 37);
            this.InsertAudiobtn.TabIndex = 4;
            this.InsertAudiobtn.Text = "Insert Audio";
            this.InsertAudiobtn.UseVisualStyleBackColor = true;
            this.InsertAudiobtn.Click += new System.EventHandler(this.InsertAudiobtn_Click);
            // 
            // InfoLabel
            // 
            this.InfoLabel.AutoSize = true;
            this.InfoLabel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.InfoLabel.Location = new System.Drawing.Point(19, 201);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.InfoLabel.Size = new System.Drawing.Size(107, 29);
            this.InfoLabel.TabIndex = 5;
            this.InfoLabel.Text = "Audio Information";
            this.InfoLabel.Click += new System.EventHandler(this.InfoLabel_Click);
            // 
            // btnOpenCompression
            // 
            this.btnOpenCompression.Location = new System.Drawing.Point(503, 109);
            this.btnOpenCompression.Name = "btnOpenCompression";
            this.btnOpenCompression.Size = new System.Drawing.Size(129, 32);
            this.btnOpenCompression.TabIndex = 6;
            this.btnOpenCompression.Text = "Open Compression";
            this.btnOpenCompression.UseVisualStyleBackColor = true;
            this.btnOpenCompression.Click += new System.EventHandler(this.btnOpenCompression_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 416);
            this.Controls.Add(this.btnOpenCompression);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.InsertAudiobtn);
            this.Controls.Add(this.waveformPictureBox);
            this.Controls.Add(this.DragDropLabel);
            this.Controls.Add(this.PlayAudiobtn);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
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

