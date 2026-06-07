namespace WindowsFormsApp2
{
    partial class CompressionForm
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
            this.btnRunCompression = new System.Windows.Forms.Button();
            this.btnRunDecompression = new System.Windows.Forms.Button();
            this.cmbAlgorithmType = new System.Windows.Forms.ComboBox();
            this.cmbPredictorType = new System.Windows.Forms.ComboBox();
            this.numQuantBits = new System.Windows.Forms.NumericUpDown();
            this.cmbSamplingRate = new System.Windows.Forms.ComboBox();
            this.pnlParameters = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantBits)).BeginInit();
            this.pnlParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRunCompression
            // 
            this.btnRunCompression.Location = new System.Drawing.Point(727, 274);
            this.btnRunCompression.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRunCompression.Name = "btnRunCompression";
            this.btnRunCompression.Size = new System.Drawing.Size(208, 28);
            this.btnRunCompression.TabIndex = 2;
            this.btnRunCompression.Text = "Compress";
            this.btnRunCompression.UseVisualStyleBackColor = true;
            this.btnRunCompression.Click += new System.EventHandler(this.btnRunCompression_Click);
            // 
            // btnRunDecompression
            // 
            this.btnRunDecompression.Location = new System.Drawing.Point(727, 334);
            this.btnRunDecompression.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRunDecompression.Name = "btnRunDecompression";
            this.btnRunDecompression.Size = new System.Drawing.Size(208, 28);
            this.btnRunDecompression.TabIndex = 4;
            this.btnRunDecompression.Text = "Decompress";
            this.btnRunDecompression.UseVisualStyleBackColor = true;
            this.btnRunDecompression.Click += new System.EventHandler(this.btnRunDecompression_Click);
            // 
            // cmbAlgorithmType
            // 
            this.cmbAlgorithmType.FormattingEnabled = true;
            this.cmbAlgorithmType.Items.AddRange(new object[] {
            "DPCM",
            "batool ",
            "abeer",
            "Adaptive Predictive"});
            this.cmbAlgorithmType.Location = new System.Drawing.Point(653, 46);
            this.cmbAlgorithmType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbAlgorithmType.Name = "cmbAlgorithmType";
            this.cmbAlgorithmType.Size = new System.Drawing.Size(377, 24);
            this.cmbAlgorithmType.TabIndex = 5;
            this.cmbAlgorithmType.SelectedIndexChanged += new System.EventHandler(this.cmbAlgorithmType_SelectedIndexChanged);
            // 
            // cmbPredictorType
            // 
            this.cmbPredictorType.FormattingEnabled = true;
            this.cmbPredictorType.Items.AddRange(new object[] {
            "First-Order (Previous Sample)",
            "Second-Order (Linear Extrapolation)"});
            this.cmbPredictorType.Location = new System.Drawing.Point(72, 80);
            this.cmbPredictorType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbPredictorType.Name = "cmbPredictorType";
            this.cmbPredictorType.Size = new System.Drawing.Size(160, 24);
            this.cmbPredictorType.TabIndex = 1;
            this.cmbPredictorType.Text = "Predictor Type";
            // 
            // numQuantBits
            // 
            this.numQuantBits.Location = new System.Drawing.Point(73, 48);
            this.numQuantBits.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
            this.numQuantBits.Size = new System.Drawing.Size(160, 22);
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
            this.cmbSamplingRate.Location = new System.Drawing.Point(72, 14);
            this.cmbSamplingRate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbSamplingRate.Name = "cmbSamplingRate";
            this.cmbSamplingRate.Size = new System.Drawing.Size(160, 24);
            this.cmbSamplingRate.TabIndex = 0;
            this.cmbSamplingRate.Text = "Sampling Rate";
            // 
            // pnlParameters
            // 
            this.pnlParameters.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.pnlParameters.Controls.Add(this.cmbSamplingRate);
            this.pnlParameters.Controls.Add(this.numQuantBits);
            this.pnlParameters.Controls.Add(this.cmbPredictorType);
            this.pnlParameters.Location = new System.Drawing.Point(653, 90);
            this.pnlParameters.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlParameters.Name = "pnlParameters";
            this.pnlParameters.Size = new System.Drawing.Size(379, 155);
            this.pnlParameters.TabIndex = 6;
            // 
            // CompressionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.pnlParameters);
            this.Controls.Add(this.cmbAlgorithmType);
            this.Controls.Add(this.btnRunDecompression);
            this.Controls.Add(this.btnRunCompression);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "CompressionForm";
            this.Text = "Audio DPCM Compression Parameters";
            this.Load += new System.EventHandler(this.CompressionForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numQuantBits)).EndInit();
            this.pnlParameters.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRunCompression;
        private System.Windows.Forms.Button btnRunDecompression;
        private System.Windows.Forms.ComboBox cmbAlgorithmType;
        private System.Windows.Forms.ComboBox cmbPredictorType;
        private System.Windows.Forms.NumericUpDown numQuantBits;
        private System.Windows.Forms.ComboBox cmbSamplingRate;
        private System.Windows.Forms.Panel pnlParameters;
    }
}