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
            this.cmbSamplingRate = new System.Windows.Forms.ComboBox();
            this.cmbPredictorType = new System.Windows.Forms.ComboBox();
            this.btnRunCompression = new System.Windows.Forms.Button();
            this.numQuantBits = new System.Windows.Forms.NumericUpDown();
            this.btnRunDecompression = new System.Windows.Forms.Button();
            this.cmbAlgorithmType = new System.Windows.Forms.ComboBox();
            this.pnlParameters = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantBits)).BeginInit();
            this.pnlParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbSamplingRate
            // 
            this.cmbSamplingRate.FormattingEnabled = true;
            this.cmbSamplingRate.Items.AddRange(new object[] {
            "8000",
            "11025",
            "16000",
            "22050",
            "44100"});
            this.cmbSamplingRate.Location = new System.Drawing.Point(54, 12);
            this.cmbSamplingRate.Name = "cmbSamplingRate";
            this.cmbSamplingRate.Size = new System.Drawing.Size(121, 21);
            this.cmbSamplingRate.TabIndex = 0;
            this.cmbSamplingRate.Text = "Sampling Rate";
            // 
            // cmbPredictorType
            // 
            this.cmbPredictorType.FormattingEnabled = true;
            this.cmbPredictorType.Items.AddRange(new object[] {
            "First-Order (Previous Sample)",
            "Second-Order (Linear Extrapolation)"});
            this.cmbPredictorType.Location = new System.Drawing.Point(54, 65);
            this.cmbPredictorType.Name = "cmbPredictorType";
            this.cmbPredictorType.Size = new System.Drawing.Size(121, 21);
            this.cmbPredictorType.TabIndex = 1;
            this.cmbPredictorType.Text = "Predictor Type";
            // 
            // btnRunCompression
            // 
            this.btnRunCompression.Location = new System.Drawing.Point(545, 222);
            this.btnRunCompression.Name = "btnRunCompression";
            this.btnRunCompression.Size = new System.Drawing.Size(156, 23);
            this.btnRunCompression.TabIndex = 2;
            this.btnRunCompression.Text = "Compress";
            this.btnRunCompression.UseVisualStyleBackColor = true;
            this.btnRunCompression.Click += new System.EventHandler(this.btnRunCompression_Click);
            // 
            // numQuantBits
            // 
            this.numQuantBits.Location = new System.Drawing.Point(55, 39);
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
            this.numQuantBits.Size = new System.Drawing.Size(120, 20);
            this.numQuantBits.TabIndex = 3;
            this.numQuantBits.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // btnRunDecompression
            // 
            this.btnRunDecompression.Location = new System.Drawing.Point(545, 272);
            this.btnRunDecompression.Name = "btnRunDecompression";
            this.btnRunDecompression.Size = new System.Drawing.Size(156, 23);
            this.btnRunDecompression.TabIndex = 4;
            this.btnRunDecompression.Text = "Decompress";
            this.btnRunDecompression.UseVisualStyleBackColor = true;
            // 
            // cmbAlgorithmType
            // 
            this.cmbAlgorithmType.FormattingEnabled = true;
            this.cmbAlgorithmType.Items.AddRange(new object[] {
            "DPCM",
            "batool ",
            "abeer",
            "ghody"});
            this.cmbAlgorithmType.Location = new System.Drawing.Point(490, 37);
            this.cmbAlgorithmType.Name = "cmbAlgorithmType";
            this.cmbAlgorithmType.Size = new System.Drawing.Size(284, 21);
            this.cmbAlgorithmType.TabIndex = 5;
            this.cmbAlgorithmType.SelectedIndexChanged += new System.EventHandler(this.cmbAlgorithmType_SelectedIndexChanged);
            // 
            // pnlParameters
            // 
            this.pnlParameters.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.pnlParameters.Controls.Add(this.cmbSamplingRate);
            this.pnlParameters.Controls.Add(this.numQuantBits);
            this.pnlParameters.Controls.Add(this.cmbPredictorType);
            this.pnlParameters.Location = new System.Drawing.Point(490, 73);
            this.pnlParameters.Name = "pnlParameters";
            this.pnlParameters.Size = new System.Drawing.Size(284, 126);
            this.pnlParameters.TabIndex = 6;
            // 
            // CompressionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pnlParameters);
            this.Controls.Add(this.cmbAlgorithmType);
            this.Controls.Add(this.btnRunDecompression);
            this.Controls.Add(this.btnRunCompression);
            this.Name = "CompressionForm";
            this.Text = "Audio DPCM Compression Parameters";
            this.Load += new System.EventHandler(this.CompressionForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numQuantBits)).EndInit();
            this.pnlParameters.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbSamplingRate;
        private System.Windows.Forms.ComboBox cmbPredictorType;
        private System.Windows.Forms.Button btnRunCompression;
        private System.Windows.Forms.NumericUpDown numQuantBits;
        private System.Windows.Forms.Button btnRunDecompression;
        private System.Windows.Forms.ComboBox cmbAlgorithmType;
        private System.Windows.Forms.Panel pnlParameters;
    }
}