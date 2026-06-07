using NAudio.Wave;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class CompressionForm : Form
    {
        private CompressionEngine _myEngine = new CompressionEngine();
        private string _inputFilePath;

        // variable to hold the compressed sound wave in memory  
        private byte[] _copied_audio = null;
        private DpcmMetadata _compressedMetadata = null;

        // variable to hold the decompressed sound wave in memory 
        private byte[] _decompressedPcmBytes = null;

        public class DpcmMetadata
        {
            public int SampleRate { get; set; }
            public byte Bits { get; set; }
            public int TotalSamples { get; set; }
        }


        public CompressionForm(string filePath)
        {
            InitializeComponent();
            _inputFilePath = filePath;
            if (cmbAlgorithmType.Items.Count > 0)
                cmbAlgorithmType.SelectedIndex = 0;
        }

        private void CompressionForm_Load(object sender, EventArgs e)
        {
        }

        private void RenderDpcmParameters()
        {
            Label lblRate = new Label { Text = "Sampling Rate:", Location = new Point(10, 15), AutoSize = true };

            cmbSamplingRate = new ComboBox { Location = new Point(150, 12), Width = 120 };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "44100" });
            cmbSamplingRate.SelectedIndex = 1;

            Label lblBits = new Label { Text = "Quantization Bits:", Location = new Point(10, 55), AutoSize = true };
            numQuantBits = new NumericUpDown { Location = new Point(150, 52), Width = 120, Minimum = 2, Maximum = 8, Value = 2 };

            Label lblPredictor = new Label { Text = "Predictor Filter:", Location = new Point(10, 95), AutoSize = true };
            cmbPredictorType = new ComboBox { Location = new Point(150, 92), Width = 120 };
            cmbPredictorType.Items.AddRange(new object[] { "First-Order", "Second-Order" });
            cmbPredictorType.SelectedIndex = 0;

            pnlParameters.Controls.AddRange(new Control[] { lblRate, cmbSamplingRate, lblBits, numQuantBits, lblPredictor, cmbPredictorType });
        }

        private void cmbAlgorithmType_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlParameters.Controls.Clear();
            string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();

            if (selectedAlgorithm == "DPCM")
            {
                RenderDpcmParameters();
            }
        }

        private void btnRunCompression_Click(object sender, EventArgs e)
        {
            string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();

            if (selectedAlgorithm == "DPCM")
            {
                string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
                if (!int.TryParse(samplingRateText, out int targetSamplingRate))
                {
                    targetSamplingRate = 16000;
                }

                int quantizationBits = (int)numQuantBits.Value;
                int predictorType = cmbPredictorType.SelectedIndex == 1 ? 1 : 0;

                try
                {
                    btnRunCompression.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;

                    ExecuteDpcmCompressionToMemory(_inputFilePath, targetSamplingRate, quantizationBits, predictorType);

                    long originalSize = new FileInfo(_inputFilePath).Length;
                    long compressedSize = _copied_audio.Length;
                    double ratio = (double)originalSize / compressedSize;

                    // Format sizes dynamically to KB or MB 
                    string originalSizeFormatted = FormatBytes(originalSize);
                    string compressedSizeFormatted = FormatBytes(compressedSize);

                    MessageBox.Show($"Compressed to Memory!\n\n" +
                                    $"Original Size: {originalSizeFormatted}\n" +
                                    $"In-Memory Size: {compressedSizeFormatted}\n" +
                                    $"Compression Ratio: {ratio:F2}x",
                                    "Metrics Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
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


        private string FormatBytes(long bytes)
        {
            double kb = bytes / 1024.0;
            double mb = kb / 1024.0;

            if (mb >= 1.0)
            {
                return $"{mb:F2} MB";
            }
            else
            {
                return $"{kb:F2} KB";
            }
        }

        private void ExecuteDpcmCompressionToMemory(string inputPath, int targetSampleRate, int bits, int predictorType)
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

                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
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

                    writer.Flush();
                    _copied_audio = ms.ToArray();

                    _compressedMetadata = new DpcmMetadata
                    {
                        SampleRate = targetSampleRate,
                        Bits = (byte)bits,
                        TotalSamples = samplesRead
                    };
                }
            }
        }

        private void btnRunDecompression_Click(object sender, EventArgs e)
        {
            if (_copied_audio == null || _compressedMetadata == null)
            {
                MessageBox.Show("No compressed data found in memory! Run compression first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnRunDecompression.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                ExecuteDpcmDecompressionToMemory();


                long decompressedSize = _decompressedPcmBytes.Length;
                string decompressedSizeFormatted = FormatBytes(decompressedSize);

                MessageBox.Show($"Audio decompressed completely in memory!\n\n" +
                                $"Decompressed Size: {decompressedSizeFormatted}\n" +
                                $"Status: Ready for playing or processing.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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


        private void ExecuteDpcmDecompressionToMemory()
        {
            using (MemoryStream msInput = new MemoryStream(_copied_audio))
            using (BinaryReader reader = new BinaryReader(msInput))
            {
                int sampleRate = _compressedMetadata.SampleRate;
                byte bits = _compressedMetadata.Bits;
                int totalSamples = _compressedMetadata.TotalSamples;


                short[] decompressedShorts = new short[totalSamples];

                int maxLevels = (int)Math.Pow(2, bits);
                short stepSize = (short)Math.Max(1, 32768 / (maxLevels * 2));

                short predictedValue = 0;
                short prevSample1 = 0;
                short prevSample2 = 0;

                for (int n = 0; n < totalSamples; n++)
                {
                    int predictorType = cmbPredictorType.SelectedIndex == 1 ? 1 : 0;

                    if (predictorType == 0 || n < 2)
                    {
                        predictedValue = prevSample1;
                    }
                    else
                    {
                        predictedValue = (short)Math.Max(-32768, Math.Min(32767, (2 * prevSample1) - prevSample2));
                    }

                    short quantizedErrorIndex = reader.ReadInt16();

                    int reconstructedError = quantizedErrorIndex * stepSize;
                    int reconstructedSample = predictedValue + reconstructedError;
                    reconstructedSample = Math.Max(-32768, Math.Min(32767, reconstructedSample));

                    decompressedShorts[n] = (short)reconstructedSample;

                    prevSample2 = prevSample1;
                    prevSample1 = (short)reconstructedSample;
                }


                _decompressedPcmBytes = new byte[decompressedShorts.Length * 2];
                Buffer.BlockCopy(decompressedShorts, 0, _decompressedPcmBytes, 0, _decompressedPcmBytes.Length);
            }
        }


        // When you want to play this back in the future, you won't need to read a file. 
        // You can feed it straight to NAudio's waveOut component like this:
        /*
        private void PlayInMemoryAudio()
        {
            if (_decompressedPcmBytes == null) return;

            var waveFormat = new WaveFormat(_compressedMetadata.SampleRate, 16, 1);
            var memoryStream = new MemoryStream(_decompressedPcmBytes);
            var rawStream = new RawSourceWaveStream(memoryStream, waveFormat);

            var waveOut = new WaveOutEvent();
            waveOut.Init(rawStream);
            waveOut.Play();
        }
        */
    }
}