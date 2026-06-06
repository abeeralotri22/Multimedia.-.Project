
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Reflection.Emit;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {

        private string audioPath = "";
        private string audioInfo = "";
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private Timer playbackTimer;
        private Bitmap waveformBitmap;
        private bool isPlaying = false;

        // ALGORITHMS
        private string _inputFilePath;
        private byte[] _copied_audio = null;
        private DpcmMetadata _compressedMetadata = null;
        private byte[] _decompressedPcmBytes = null;

        public class DpcmMetadata
        {
            public int SampleRate { get; set; }
            public byte Bits { get; set; }
            public int TotalSamples { get; set; }
        }

        public Form1()
        {
            InitializeComponent();

            DragDropLabel.AllowDrop = true;

            DragDropLabel.DragEnter += DragDropLabel_DragEnter;
            DragDropLabel.DragDrop += DragDropLabel_DragDrop;
            playbackTimer = new Timer();
            playbackTimer.Interval = 50;
            playbackTimer.Tick += PlaybackTimer_Tick;
            DragDropLabel.TextAlign = ContentAlignment.TopLeft;
            DragDropLabel.BorderStyle = BorderStyle.FixedSingle;

            if (cmbAlgorithmType.Items.Count > 0)
                cmbAlgorithmType.SelectedIndex = 0;
        }

     
        private void DragDropLabel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    string ext = Path.GetExtension(files[0]).ToLower();

                    string[] allowedExtensions =
                    {
                        ".mp3",
                        ".wav",
                        ".aac",
                        ".wma",
                        ".flac",
                        ".ogg",
                        ".m4a"
                    };

                    if (allowedExtensions.Contains(ext))
                    {
                        e.Effect = DragDropEffects.Copy;
                        return;
                    }
                }
            }

            e.Effect = DragDropEffects.None;
        }

        private void DragDropLabel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                try
                {
                    LoadAudio(files[0]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Failed to load audio file.\n\n" + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }


        private void PlayAudiobtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(audioPath))
                {
                    MessageBox.Show("Please insert an audio file first.",
                        "No Audio Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!isPlaying)
                {
                    if (outputDevice == null || outputDevice.PlaybackState == PlaybackState.Stopped)
                    {
                        audioFile?.Dispose();
                        outputDevice?.Dispose();

                        audioFile = new AudioFileReader(audioPath);
                        outputDevice = new WaveOutEvent();
                        outputDevice.Init(audioFile);
                    }

                    outputDevice.Play();
                    playbackTimer.Start();
                    PlayAudiobtn.Text = "Pause ❚❚";
                    isPlaying = true;
                }
                else
                {
                    outputDevice.Pause();
                    playbackTimer.Stop();
                    PlayAudiobtn.Text = "Play Audio ▶︎";
                    isPlaying = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void DrawWaveform(string filePath)
        {
            int width = waveformPictureBox.Width;
            int height = waveformPictureBox.Height;
            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                using (Pen pen = new Pen(Color.Blue))
                using (AudioFileReader reader = new AudioFileReader(filePath))
                {
                    long totalSamples = reader.Length / reader.WaveFormat.BlockAlign;
                    int samplesPerPixel = (int)Math.Max(1, totalSamples / width);

                    int blockAlign = reader.WaveFormat.BlockAlign;
                    int floatsPerBlock = blockAlign / sizeof(float);
                    int channels = reader.WaveFormat.Channels;
                    samplesPerPixel = Math.Max(channels, (samplesPerPixel / channels) * channels);

                    float[] buffer = new float[samplesPerPixel];

                    for (int x = 0; x < width; x++)
                    {
                        int samplesRead = reader.Read(buffer, 0, buffer.Length);
                        if (samplesRead == 0) break;

                        float max = 0f;
                        for (int i = 0; i < samplesRead; i++)
                        {
                            float sample = Math.Abs(buffer[i]);
                            if (sample > max) max = sample;
                        }

                        int y = (int)(max * height / 2);
                        g.DrawLine(pen, x, height / 2 - y, x, height / 2 + y);
                    }
                }
            }

            if (waveformPictureBox.Image != null)
                waveformPictureBox.Image.Dispose();

            waveformBitmap = bmp;
            waveformPictureBox.Image = (Bitmap)bmp.Clone();
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (audioFile == null || waveformBitmap == null) return;

            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                playbackTimer.Stop();
                isPlaying = false;
                PlayAudiobtn.Text = "Play Audio ▶︎";
                return;
            }

            Bitmap frame = (Bitmap)waveformBitmap.Clone();
            using (Graphics g = Graphics.FromImage(frame))
            {
                double progress = audioFile.CurrentTime.TotalSeconds / audioFile.TotalTime.TotalSeconds;
                int x = (int)(progress * waveformPictureBox.Width);
                g.DrawLine(Pens.Red, x, 0, x, waveformPictureBox.Height);
            }

            if (waveformPictureBox.Image != null)
                waveformPictureBox.Image.Dispose();

            waveformPictureBox.Image = frame;
        }

        private void InsertAudiobtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "Select an Audio File";

            dialog.Filter =
                "Audio Files|*.mp3;*.wav;*.flac;*.aac;*.ogg;*.m4a|" +
                "All Files|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LoadAudio(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Failed to load audio file.\n\n" + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void LoadAudio(string filePath)
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            playbackTimer.Stop();

            isPlaying = false;
            PlayAudiobtn.Text = "Play Audio ▶︎";

            audioPath = filePath;
            _inputFilePath = filePath;

            FileInfo fileInfo = new FileInfo(audioPath);

            using (AudioFileReader reader = new AudioFileReader(audioPath))
            {
                audioInfo =
                    $"File Name: {Path.GetFileName(audioPath)}\n\n" +
                    $"File Size: {(fileInfo.Length / 1024.0 / 1024.0):F2} MB\n\n" +
                    $"Duration: {reader.TotalTime:mm\\:ss}\n\n" +
                    $"Sample Rate: {reader.WaveFormat.SampleRate} Hz\n\n" +
                    $"Channels: {reader.WaveFormat.Channels}\n\n" +
                    $"Bit Depth: {reader.WaveFormat.BitsPerSample} bits\n\n" +
                    $"Encoding: {reader.WaveFormat.Encoding}";
                InfoLabel.Text = audioInfo;
            }

            DragDropLabel.Text = Path.GetFileName(audioPath);

            DrawWaveform(audioPath);

            cmbAlgorithmType.Enabled = true;
            pnlParameters.Enabled = true;
            btnRunCompression.Enabled = true;
            btnRunDecompression.Enabled = true;
        }

        //private void btnOpenCompression_Click(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(this.audioPath))
        //    {
        //        MessageBox.Show("Please load an audio file first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    cmbAlgorithmType.Focus();
        //}

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            cmbAlgorithmType.Enabled = false;
            pnlParameters.Enabled = false;
            btnRunCompression.Enabled = false;
            btnRunDecompression.Enabled = false;
        }

      // ALGORITHMS
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