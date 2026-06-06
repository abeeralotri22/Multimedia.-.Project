using NAudio.Wave;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class CompressionForm : Form
    {
        private string _inputFilePath;

     //pass the filepath from the first window
        public CompressionForm(string filePath)
        {
            InitializeComponent();
            _inputFilePath = filePath;
            if (cmbAlgorithmType.Items.Count > 0)
                cmbAlgorithmType.SelectedIndex = 0;
        }


        private void CompressionForm_Load(object sender, EventArgs e)
        {
          
            if (cmbSamplingRate.Items.Count > 0) cmbSamplingRate.SelectedIndex = 2; // default is 16000
            if (cmbPredictorType.Items.Count > 0) cmbPredictorType.SelectedIndex = 0;    // default is First-Order
        }
        private void RenderDpcmParameters()
        {
            // Create Label for Sampling Rate
            Label lblRate = new Label { Text = "Sampling Rate:", Location = new Point(10, 15), AutoSize = true };

            // Create ComboBox for Sampling Rate
            cmbSamplingRate = new ComboBox { Location = new Point(150, 12), Width = 120 };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "44100" });
            cmbSamplingRate.SelectedIndex = 1;

            // Create Label for Quantization Bits
            Label lblBits = new Label { Text = "Quantization Bits:", Location = new Point(10, 55), AutoSize = true };

            // Create NumericUpDown for Quantization Bits
            numQuantBits = new NumericUpDown { Location = new Point(150, 52), Width = 120, Minimum = 2, Maximum = 8, Value = 4 };

            // Create Label for Predictor Type
            Label lblPredictor = new Label { Text = "Predictor Filter:", Location = new Point(10, 95), AutoSize = true };

            // Create ComboBox for Predictor Type
            cmbPredictorType = new ComboBox { Location = new Point(150, 92), Width = 120 };
            cmbPredictorType.Items.AddRange(new object[] { "First-Order", "Second-Order" });
            cmbPredictorType.SelectedIndex = 0;

            // 3. Add all created controls into the container panel
            pnlParameters.Controls.AddRange(new Control[] { lblRate, cmbSamplingRate, lblBits, numQuantBits, lblPredictor, cmbPredictorType });
        }
        private void cmbAlgorithmType_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlParameters.Controls.Clear();

            string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();

            //  custom settings controls according to each algorithem
            if (selectedAlgorithm == "DPCM")
            {
                RenderDpcmParameters();
            }
            else if (selectedAlgorithm == "abeeer") { }
            else if (selectedAlgorithm == "batool") { }
            else if (selectedAlgorithm == "ghody") { }
        }

        private void btnRunCompression_Click(object sender, EventArgs e)
        {
            string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();

            // 4. Evaluate variables depending on which UI layout is active
            if (selectedAlgorithm == "DPCM")
            { // set defaults for safe
            string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
            if (!int.TryParse(samplingRateText, out int targetSamplingRate))
            {
                targetSamplingRate = 16000;
            }

            int quantizationBits = (int)numQuantBits.Value;
            int predictorType = cmbPredictorType.SelectedIndex == 1 ? 1 : 0;

            //open a dialog for saving
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "DPCM Compressed Audio|*.dpcm";
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(_inputFilePath) + "_compressed.dpcm";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        btnRunCompression.Enabled = false;
                        this.Cursor = Cursors.WaitCursor;

                        // 3. Process the file using DPCM algorithm
                        ExecuteDpcmCompression(_inputFilePath, saveFileDialog.FileName, targetSamplingRate, quantizationBits, predictorType);

                        MessageBox.Show("Audio compressed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Compression failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnRunCompression.Enabled = true;
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void ExecuteDpcmCompression(string inputPath, string outputPath, int targetSampleRate, int bits, int predictorType)
        {
            using (var reader = new AudioFileReader(inputPath))
            {
                var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
                var sampleProvider = resampler.ToSampleProvider();

                int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
                float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
                int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);

                short[] pcmSamples = new short[samplesRead];
                for (int i = 0; i < samplesRead; i++)
                {
                    pcmSamples[i] = (short)Math.Max(-32768, Math.Min(32767, floatBuffer[i] * 32767f));
                }

                using (BinaryWriter writer = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
                {
                    writer.Write(new char[] { 'D', 'P', 'C', 'M' });
                    writer.Write(targetSampleRate);
                    writer.Write((byte)bits);
                    writer.Write(samplesRead);

                    short predictedValue = 0;
                    short prevSample1 = 0;
                    short prevSample2 = 0;

                    int maxLevels = (int)Math.Pow(2, bits);
                    int minQuantizedLevel = -(maxLevels / 2);
                    int maxQuantizedLevel = (maxLevels / 2) - 1;
                    short stepSize = (short)Math.Max(1, 32768 / (maxLevels * 2));

                    for (int n = 0; n < samplesRead; n++)
                    {
                        if (predictorType == 0 || n < 2)
                        {
                            predictedValue = prevSample1;
                        }
                        else
                        {
                            predictedValue = (short)Math.Max(-32768, Math.Min(32767, (2 * prevSample1) - prevSample2));
                        }

                        int error = pcmSamples[n] - predictedValue;
                        int quantizedErrorIndex = (int)Math.Round((double)error / stepSize);
                        quantizedErrorIndex = Math.Max(minQuantizedLevel, Math.Min(maxQuantizedLevel, quantizedErrorIndex));

                        int reconstructedError = quantizedErrorIndex * stepSize;
                        int reconstructedSample = predictedValue + reconstructedError;
                        reconstructedSample = Math.Max(-32768, Math.Min(32767, reconstructedSample));

                        writer.Write((short)quantizedErrorIndex);

                        prevSample2 = prevSample1;
                        prevSample1 = (short)reconstructedSample;
                    }
                }
            }
        }

private void btnRunDecompression_Click(object sender, EventArgs e)
{
    OpenFileDialog openFileDialog = new OpenFileDialog();
    openFileDialog.Filter = "DPCM Compressed Audio|*.dpcm";

    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "WAVE Audio File|*.wav";
        saveFileDialog.FileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "_decompressed.wav";

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                btnRunDecompression.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                // Execute the reverse process
                ExecuteDpcmDecompression(openFileDialog.FileName, saveFileDialog.FileName);

                MessageBox.Show("Audio decompressed back to WAV successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decompression failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRunDecompression.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }
    }
}

private void ExecuteDpcmDecompression(string inputPath, string outputPath)
{
    using (BinaryReader reader = new BinaryReader(File.Open(inputPath, FileMode.Open)))
    {
        // 1. Read and validate our custom Header Metadata
        char[] magicCookie = reader.ReadChars(4);
        string magicStr = new string(magicCookie);
        if (magicStr != "DPCM")
        {
            throw new InvalidDataException("Not a valid DPCM compressed file structure.");
        }

        int sampleRate = reader.ReadInt32();
        byte bits = reader.ReadByte();
        int totalSamples = reader.ReadInt32();

        // 2. Allocate space for the output 16-bit PCM array
        short[] decompressedSamples = new short[totalSamples];

        // 3. Rebuild the reconstruction math constants
        int maxLevels = (int)Math.Pow(2, bits);
        short stepSize = (short)Math.Max(1, 32768 / (maxLevels * 2));

        short predictedValue = 0;
        short prevSample1 = 0;
        short prevSample2 = 0;

        // 4. Run the Decoder Loop (The exact twin of our internal compressor reconstruction)
        for (int n = 0; n < totalSamples; n++)
        {
            // Determine Predictor type dynamically by checking your dropdown state 
            // (or hardcode/store predictor flag in header if desired)
            int predictorType = cmbPredictorType.SelectedIndex == 1 ? 1 : 0;

            if (predictorType == 0 || n < 2)
            {
                predictedValue = prevSample1;
            }
            else
            {
                predictedValue = (short)Math.Max(-32768, Math.Min(32767, (2 * prevSample1) - prevSample2));
            }

            // Read the small 4-bit error code written on disk
            short quantizedErrorIndex = reader.ReadInt16();

            // Reverse the scaling step size back up to full 16-bit dynamics
            int reconstructedError = quantizedErrorIndex * stepSize;
            int reconstructedSample = predictedValue + reconstructedError;

            // Hard clamp boundaries to avoid register overflows
            reconstructedSample = Math.Max(-32768, Math.Min(32767, reconstructedSample));

            // Save the restored sample into our playback array
            decompressedSamples[n] = (short)reconstructedSample;

            // Shift history states forward exactly like the compressor did
            prevSample2 = prevSample1;
            prevSample1 = (short)reconstructedSample;
        }

                // 5. Package the restored raw shorts array into a standard, playable .WAV file via NAudio
      
                var waveFormat = new WaveFormat(sampleRate, 16, 1); // Mono, 16-bit matching metadata rules
                using (var waveWriter = new WaveFileWriter(outputPath, waveFormat)) // <-- Fixed class name here
                {
                    waveWriter.WriteSamples(decompressedSamples, 0, decompressedSamples.Length);
                }
            }
}
    }
}