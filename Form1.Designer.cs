//namespace WindowsFormsApp2
//{
//    partial class Form1
//    {
//        /// <summary>
//        /// Required designer variable.
//        /// </summary>
//        private System.ComponentModel.IContainer components = null;

//        /// <summary>
//        /// Clean up any resources being used.
//        /// </summary>
//        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Windows Form Designer generated code

//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            this.PlayAudiobtn = new System.Windows.Forms.Button();
//            this.DragDropLabel = new System.Windows.Forms.Label();
//            this.waveformPictureBox = new System.Windows.Forms.PictureBox();
//            this.InsertAudiobtn = new System.Windows.Forms.Button();
//            this.InfoLabel = new System.Windows.Forms.Label();
//            this.btnOpenCompression = new System.Windows.Forms.Button();
//            ((System.ComponentModel.ISupportInitialize)(this.waveformPictureBox)).BeginInit();
//            this.SuspendLayout();
//            // 
//            // PlayAudiobtn
//            // 
//            this.PlayAudiobtn.Location = new System.Drawing.Point(242, 364);
//            this.PlayAudiobtn.Name = "PlayAudiobtn";
//            this.PlayAudiobtn.Size = new System.Drawing.Size(195, 60);
//            this.PlayAudiobtn.TabIndex = 0;
//            this.PlayAudiobtn.Text = "Play Audio ▶︎ ";
//            this.PlayAudiobtn.UseVisualStyleBackColor = true;
//            this.PlayAudiobtn.Click += new System.EventHandler(this.PlayAudiobtn_Click);
//            // 
//            // DragDropLabel
//            // 
//            this.DragDropLabel.AllowDrop = true;
//            this.DragDropLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
//            this.DragDropLabel.Location = new System.Drawing.Point(28, 25);
//            this.DragDropLabel.Name = "DragDropLabel"; 
//            this.DragDropLabel.Padding = new System.Windows.Forms.Padding(12);
//            this.DragDropLabel.Size = new System.Drawing.Size(502, 92);
//            this.DragDropLabel.TabIndex = 1;
//            this.DragDropLabel.Text = "Drag and drop here";
//            this.DragDropLabel.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragDrop);
//            this.DragDropLabel.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragEnter);
//            // 
//            // waveformPictureBox
//            // 
//            this.waveformPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
//            this.waveformPictureBox.Location = new System.Drawing.Point(32, 123);
//            this.waveformPictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
//            this.waveformPictureBox.Name = "waveformPictureBox";
//            this.waveformPictureBox.Size = new System.Drawing.Size(1665, 235);
//            this.waveformPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
//            this.waveformPictureBox.TabIndex = 3;
//            this.waveformPictureBox.TabStop = false;
//            // 
//            // InsertAudiobtn
//            // 
//            this.InsertAudiobtn.Location = new System.Drawing.Point(591, 25);
//            this.InsertAudiobtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
//            this.InsertAudiobtn.Name = "InsertAudiobtn";
//            this.InsertAudiobtn.Size = new System.Drawing.Size(195, 60);
//            this.InsertAudiobtn.TabIndex = 4;
//            this.InsertAudiobtn.Text = "Insert Audio";
//            this.InsertAudiobtn.UseVisualStyleBackColor = true;
//            this.InsertAudiobtn.Click += new System.EventHandler(this.InsertAudiobtn_Click);
//            // 
//            // InfoLabel
//            // 
//            this.InfoLabel.AutoSize = true;
//            this.InfoLabel.BackColor = System.Drawing.SystemColors.AppWorkspace;
//            this.InfoLabel.Location = new System.Drawing.Point(28, 376);
//            this.InfoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
//            this.InfoLabel.Name = "InfoLabel";
//            this.InfoLabel.Padding = new System.Windows.Forms.Padding(14, 12, 14, 12);
//            this.InfoLabel.Size = new System.Drawing.Size(163, 44);
//            this.InfoLabel.TabIndex = 5;
//            this.InfoLabel.Text = "Audio Information";
//            // 
//            // btnOpenCompression
//            // 
//            this.btnOpenCompression.Location = new System.Drawing.Point(826, 25);
//            this.btnOpenCompression.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
//            this.btnOpenCompression.Name = "btnOpenCompression";
//            this.btnOpenCompression.Size = new System.Drawing.Size(195, 60);
//            this.btnOpenCompression.TabIndex = 6;
//            this.btnOpenCompression.Text = "Open Compression";
//            this.btnOpenCompression.UseVisualStyleBackColor = true;
//            this.btnOpenCompression.Click += new System.EventHandler(this.btnOpenCompression_Click);
//            // 
//            // Form1
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.ClientSize = new System.Drawing.Size(2564, 1570);
//            this.Controls.Add(this.btnOpenCompression);
//            this.Controls.Add(this.InfoLabel);
//            this.Controls.Add(this.InsertAudiobtn);
//            this.Controls.Add(this.waveformPictureBox);
//            this.Controls.Add(this.DragDropLabel);
//            this.Controls.Add(this.PlayAudiobtn);
//            this.Name = "Form1";
//            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
//            this.Text = "Form1";
//            this.Load += new System.EventHandler(this.Form1_Load);
//            ((System.ComponentModel.ISupportInitialize)(this.waveformPictureBox)).EndInit();
//            this.ResumeLayout(false);
//            this.PerformLayout();

//        }

//        #endregion

//        private System.Windows.Forms.Button PlayAudiobtn;
//        private System.Windows.Forms.Label DragDropLabel;
//        private System.Windows.Forms.PictureBox waveformPictureBox;
//        private System.Windows.Forms.Button InsertAudiobtn;
//        private System.Windows.Forms.Label InfoLabel;
//        private System.Windows.Forms.Button btnOpenCompression;
//    }
//}



namespace WindowsFormsApp2
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.PlayAudiobtn = new System.Windows.Forms.Button();
            this.DragDropLabel = new System.Windows.Forms.Label();
            this.waveformPictureBox = new System.Windows.Forms.PictureBox();
            this.InsertAudiobtn = new System.Windows.Forms.Button();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.cmbAlgorithmType = new System.Windows.Forms.ComboBox();
            this.cmbPredictorType = new System.Windows.Forms.ComboBox();
            this.numQuantBits = new System.Windows.Forms.NumericUpDown();
            this.cmbSamplingRate = new System.Windows.Forms.ComboBox();
            this.pnlParameters = new System.Windows.Forms.Panel();
            this.btnRunCompression = new System.Windows.Forms.Button();
            this.btnRunDecompression = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.waveformPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantBits)).BeginInit();
            this.pnlParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // PlayAudiobtn
            // 
            this.PlayAudiobtn.Location = new System.Drawing.Point(260, 301);
            this.PlayAudiobtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.PlayAudiobtn.Name = "PlayAudiobtn";
            this.PlayAudiobtn.Size = new System.Drawing.Size(152, 48);
            this.PlayAudiobtn.TabIndex = 0;
            this.PlayAudiobtn.Text = "Play Audio ▶︎ ";
            this.PlayAudiobtn.UseVisualStyleBackColor = true;
            this.PlayAudiobtn.Click += new System.EventHandler(this.PlayAudiobtn_Click);
            // 
            // DragDropLabel
            // 
            this.DragDropLabel.AllowDrop = true;
            this.DragDropLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.DragDropLabel.Location = new System.Drawing.Point(22, 20);
            this.DragDropLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DragDropLabel.Name = "DragDropLabel";
            this.DragDropLabel.Padding = new System.Windows.Forms.Padding(9, 10, 9, 10);
            this.DragDropLabel.Size = new System.Drawing.Size(390, 74);
            this.DragDropLabel.TabIndex = 1;
            this.DragDropLabel.Text = "Drag and drop here";
            this.DragDropLabel.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragDrop);
            this.DragDropLabel.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragDropLabel_DragEnter);
            // 
            // waveformPictureBox
            // 
            this.waveformPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveformPictureBox.Location = new System.Drawing.Point(25, 98);
            this.waveformPictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.waveformPictureBox.Name = "waveformPictureBox";
            this.waveformPictureBox.Size = new System.Drawing.Size(1295, 188);
            this.waveformPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.waveformPictureBox.TabIndex = 3;
            this.waveformPictureBox.TabStop = false;
            // 
            // InsertAudiobtn
            // 
            this.InsertAudiobtn.Location = new System.Drawing.Point(460, 20);
            this.InsertAudiobtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.InsertAudiobtn.Name = "InsertAudiobtn";
            this.InsertAudiobtn.Size = new System.Drawing.Size(152, 48);
            this.InsertAudiobtn.TabIndex = 4;
            this.InsertAudiobtn.Text = "Insert Audio";
            this.InsertAudiobtn.UseVisualStyleBackColor = true;
            this.InsertAudiobtn.Click += new System.EventHandler(this.InsertAudiobtn_Click);
            // 
            // InfoLabel
            // 
            this.InfoLabel.AutoSize = true;
            this.InfoLabel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.InfoLabel.Location = new System.Drawing.Point(22, 301);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.InfoLabel.Size = new System.Drawing.Size(139, 37);
            this.InfoLabel.TabIndex = 5;
            this.InfoLabel.Text = "Audio Information";
            // 
            // cmbAlgorithmType
            // 
            this.cmbAlgorithmType.FormattingEnabled = true;
            this.cmbAlgorithmType.Items.AddRange(new object[] {
            "DPCM",
            "batool ",
            "Delta Modulation",
            "Adaptive Delta Modulation",
            "ghody"});
            this.cmbAlgorithmType.Location = new System.Drawing.Point(910, 340);
            this.cmbAlgorithmType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbAlgorithmType.Name = "cmbAlgorithmType";
            this.cmbAlgorithmType.Size = new System.Drawing.Size(331, 24);
            this.cmbAlgorithmType.TabIndex = 7;
            this.cmbAlgorithmType.SelectedIndexChanged += new System.EventHandler(this.cmbAlgorithmType_SelectedIndexChanged);
            // 
            // cmbPredictorType
            // 
            this.cmbPredictorType.FormattingEnabled = true;
            this.cmbPredictorType.Items.AddRange(new object[] {
            "First-Order (Previous Sample)",
            "Second-Order (Linear Extrapolation)"});
            this.cmbPredictorType.Location = new System.Drawing.Point(63, 80);
            this.cmbPredictorType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbPredictorType.Name = "cmbPredictorType";
            this.cmbPredictorType.Size = new System.Drawing.Size(141, 24);
            this.cmbPredictorType.TabIndex = 1;
            this.cmbPredictorType.Text = "Predictor Type";
            // 
            // numQuantBits
            // 
            this.numQuantBits.Location = new System.Drawing.Point(64, 48);
            this.numQuantBits.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numQuantBits.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numQuantBits.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numQuantBits.Name = "numQuantBits";
            this.numQuantBits.Size = new System.Drawing.Size(140, 24);
            this.numQuantBits.TabIndex = 3;
            this.numQuantBits.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // cmbSamplingRate
            // 
            this.cmbSamplingRate.FormattingEnabled = true;
            this.cmbSamplingRate.Items.AddRange(new object[] {
            "44100",
            "22050",
            "16000",
            "11025",
            "8000",
            "",
            ""});
            this.cmbSamplingRate.Location = new System.Drawing.Point(63, 14);
            this.cmbSamplingRate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbSamplingRate.Name = "cmbSamplingRate";
            this.cmbSamplingRate.Size = new System.Drawing.Size(141, 24);
            this.cmbSamplingRate.TabIndex = 0;
            this.cmbSamplingRate.Text = "Sampling Rate";
            // 
            // pnlParameters
            // 
            this.pnlParameters.AutoSize = true;
            this.pnlParameters.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.pnlParameters.Controls.Add(this.cmbSamplingRate);
            this.pnlParameters.Controls.Add(this.numQuantBits);
            this.pnlParameters.Controls.Add(this.cmbPredictorType);
            this.pnlParameters.Location = new System.Drawing.Point(910, 384);
            this.pnlParameters.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlParameters.Name = "pnlParameters";
            this.pnlParameters.Size = new System.Drawing.Size(331, 155);
            this.pnlParameters.TabIndex = 8;
            // 
            // btnRunCompression
            // 
            this.btnRunCompression.Location = new System.Drawing.Point(695, 340);
            this.btnRunCompression.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRunCompression.Name = "btnRunCompression";
            this.btnRunCompression.Size = new System.Drawing.Size(182, 28);
            this.btnRunCompression.TabIndex = 9;
            this.btnRunCompression.Text = "Compress";
            this.btnRunCompression.UseVisualStyleBackColor = true;
            this.btnRunCompression.Click += new System.EventHandler(this.btnRunCompression_Click);
            // 
            // btnRunDecompression
            // 
            this.btnRunDecompression.Location = new System.Drawing.Point(695, 384);
            this.btnRunDecompression.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRunDecompression.Name = "btnRunDecompression";
            this.btnRunDecompression.Size = new System.Drawing.Size(182, 28);
            this.btnRunDecompression.TabIndex = 10;
            this.btnRunDecompression.Text = "Decompress";
            this.btnRunDecompression.UseVisualStyleBackColor = true;
            this.btnRunDecompression.Click += new System.EventHandler(this.btnRunDecompression_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1496, 844);
            this.Controls.Add(this.btnRunDecompression);
            this.Controls.Add(this.btnRunCompression);
            this.Controls.Add(this.pnlParameters);
            this.Controls.Add(this.cmbAlgorithmType);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.InsertAudiobtn);
            this.Controls.Add(this.waveformPictureBox);
            this.Controls.Add(this.DragDropLabel);
            this.Controls.Add(this.PlayAudiobtn);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.waveformPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantBits)).EndInit();
            this.pnlParameters.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button PlayAudiobtn;
        private System.Windows.Forms.Label DragDropLabel;
        private System.Windows.Forms.PictureBox waveformPictureBox;
        private System.Windows.Forms.Button InsertAudiobtn;
        private System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.ComboBox cmbAlgorithmType;
        private System.Windows.Forms.ComboBox cmbPredictorType;
        private System.Windows.Forms.NumericUpDown numQuantBits;
        private System.Windows.Forms.ComboBox cmbSamplingRate;
        private System.Windows.Forms.Panel pnlParameters;
        private System.Windows.Forms.Button btnRunCompression;
        private System.Windows.Forms.Button btnRunDecompression;
    }
}