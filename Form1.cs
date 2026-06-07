
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
using Emgu.CV; // (فقط هذا السطر الخاص بـ Emgu)


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
        private const double MuLawMu = 255.0;
        private const double ALawA = 87.6;

        // ALGORITHMS
        private string _inputFilePath;
        private string _workingAudioCopyPath;
        private byte[] _copied_audio = null;
        private DpcmMetadata _compressedMetadata = null;
        private DmMetadata _dmMetadata = null;
        private AdmMetadata _admMetadata = null;
        private CompandingMetadata _compandingMetadata = null;
        private byte[] _decompressedPcmBytes = null;

        private short[] _originalSamples = null;

        private NumericUpDown numLpfCutoff;
        private NumericUpDown numStepSize;
        private NumericUpDown numInitStepSize;
        private NumericUpDown numAdaptationFactor;
        private NumericUpDown numMaxStepSize;
        private NumericUpDown numHistoryBits;
        private ComboBox cmbSampleRate;
        private ComboBox cmbChannels;
        //ghody

        private NumericUpDown numLevels;
        private NumericUpDown numStep;
        private NumericUpDown numPredictionOrder;
        private ComboBox cmbMode;
        private CheckBox chkIsMono;
        private ComboBox cmbSampleRate1; // وهذا أيضاً
        public class DpcmMetadata
        {
            public string OriginalExtension { get; set; }

            public int SampleRate { get; set; }
            public byte Bits { get; set; }
            public int TotalSamples { get; set; }

        }

        public class DmMetadata
        {
            public string OriginalExtension { get; set; }

            public int SampleRate { get; set; }
            public int TotalSamples { get; set; }

            public float StepSize { get; set; }
            public int LpfCutoff { get; set; }
        }

        public class AdmMetadata
        {
            public string OriginalExtension { get; set; }

            public int SampleRate { get; set; }
            public int TotalSamples { get; set; }

            public float InitStepSize { get; set; }
            public float AdaptationFactor { get; set; }
            public float MaxStepSize { get; set; }
            public int HistoryBits { get; set; }
            public int LpfCutoff { get; set; }
        }

        public class CompandingMetadata
        {
            public string Algorithm { get; set; }
            public string OriginalExtension { get; set; }
            public int SampleRate { get; set; }
            public byte BitDepth { get; set; }
            public int Channels { get; set; }
            public int TotalSamples { get; set; }
        }
        public Form1()
        {
            InitializeComponent();
            ConfigureAlgorithmDropdown();
            try
            {
                var mat = new Emgu.CV.Mat();
                // MessageBox.Show("Emgu CV is working!"); // يمكنك فك التعليق لاحقاً
            }
            catch (Exception ex)
            {
                MessageBox.Show("Emgu CV not found! Error: " + ex.Message);
            }
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

        private void ConfigureAlgorithmDropdown()
        {
            // Rebuild the dropdown in code so Visual Studio designer cache cannot keep the old "batool" item alive.
            cmbAlgorithmType.Items.Clear();
            cmbAlgorithmType.Items.AddRange(new object[]
            {
                "DPCM",
                "Mu-Law",
                "A-Law",
                "Delta Modulation",
                "Adaptive Delta Modulation",
                "Adaptive Predictive"
            });
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

        private string CopyUploadedAudioToWorkingFile(string filePath)
        {
            string copyFolder = Path.Combine(Application.StartupPath, "audio_working_copies");
            Directory.CreateDirectory(copyFolder);

            string copyName =
                Path.GetFileNameWithoutExtension(filePath) +
                "_" +
                Guid.NewGuid().ToString("N") +
                Path.GetExtension(filePath);

            string copyPath = Path.Combine(copyFolder, copyName);
            // Every algorithm reads this copied file so the uploaded original stays untouched.
            File.Copy(filePath, copyPath);
            return copyPath;
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

            _workingAudioCopyPath = CopyUploadedAudioToWorkingFile(filePath);
            audioPath = _workingAudioCopyPath;
            _inputFilePath = _workingAudioCopyPath;
            _copied_audio = null;
            _decompressedPcmBytes = null;
            _compressedMetadata = null;
            _dmMetadata = null;
            _admMetadata = null;
            _compandingMetadata = null;

            FileInfo fileInfo = new FileInfo(filePath);

            using (AudioFileReader reader = new AudioFileReader(audioPath))
            {
                audioInfo =
                    $"File Name: {Path.GetFileName(filePath)}\n\n" +
                    $"File Size: {(fileInfo.Length / 1024.0 / 1024.0):F2} MB\n\n" +
                    $"Duration: {reader.TotalTime:mm\\:ss}\n\n" +
                    $"Sample Rate: {reader.WaveFormat.SampleRate} Hz\n\n" +
                    $"Channels: {reader.WaveFormat.Channels}\n\n" +
                    $"Bit Depth: {reader.WaveFormat.BitsPerSample} bits\n\n" +
                    $"Encoding: {reader.WaveFormat.Encoding}";
                InfoLabel.Text = audioInfo;
            }

            DragDropLabel.Text = Path.GetFileName(filePath);

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

        private void RenderMuLawParameters()
        {
            Label lblRate = new Label { Text = "Sample Rate:", Location = new Point(10, 15), AutoSize = true };

            cmbSamplingRate = new ComboBox { Location = new Point(160, 12), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "22050", "44100" });
            // Mu-Law uses 8000 Hz as the requested default sample rate.
            cmbSamplingRate.SelectedIndex = 0;

            Label lblBits = new Label { Text = "Bit Depth:", Location = new Point(10, 55), AutoSize = true };
            // NumericUpDown keeps the bit depth constrained to the requested 2-16 range.
            numQuantBits = new NumericUpDown { Location = new Point(160, 52), Width = 140, Minimum = 2, Maximum = 16, Value = 8 };

            Label lblChannels = new Label { Text = "Channels:", Location = new Point(10, 95), AutoSize = true };
            cmbChannels = new ComboBox { Location = new Point(160, 92), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbChannels.Items.AddRange(new object[] { "Mono (1 Channel)", "Same as Original" });
            // Mono is selected first so the panel opens with the requested default.
            cmbChannels.SelectedIndex = 0;

            pnlParameters.Controls.AddRange(new Control[] { lblRate, cmbSamplingRate, lblBits, numQuantBits, lblChannels, cmbChannels });
        }

        private void RenderALawParameters()
        {
            Label lblRate = new Label { Text = "Sample Rate:", Location = new Point(10, 15), AutoSize = true };

            cmbSamplingRate = new ComboBox { Location = new Point(160, 12), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "22050", "44100" });
            // A-Law uses the same sample-rate options and 8000 Hz default as Mu-Law.
            cmbSamplingRate.SelectedIndex = 0;

            Label lblBits = new Label { Text = "Bit Depth:", Location = new Point(10, 55), AutoSize = true };
            // The requested default bit depth is 8, with valid values from 2 through 16.
            numQuantBits = new NumericUpDown { Location = new Point(160, 52), Width = 140, Minimum = 2, Maximum = 16, Value = 8 };

            Label lblChannels = new Label { Text = "Channels:", Location = new Point(10, 95), AutoSize = true };
            cmbChannels = new ComboBox { Location = new Point(160, 92), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbChannels.Items.AddRange(new object[] { "Mono (1 Channel)", "Same as Original" });
            // The first item is the requested Mono default.
            cmbChannels.SelectedIndex = 0;

            pnlParameters.Controls.AddRange(new Control[] { lblRate, cmbSamplingRate, lblBits, numQuantBits, lblChannels, cmbChannels });
        }

        private void RenderDMParameters()
        {
            Label lblRate = new Label { Text = "Sampling Rate:", Location = new Point(10, 15), AutoSize = true };
            cmbSamplingRate = new ComboBox { Location = new Point(160, 12), Width = 120 };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "32000", "44100" });
            cmbSamplingRate.SelectedIndex = 1;

            Label lblStepSize = new Label { Text = "Fixed Step Size:", Location = new Point(10, 55), AutoSize = true };
            numStepSize = new NumericUpDown
            {
                Location = new Point(160, 52),
                Width = 120,
                Minimum = 0.001M,
                Maximum = 1.0M,
                DecimalPlaces = 3,
                Increment = 0.005M,
                Value = 0.02M
            };

            Label lblLpfCutoff = new Label { Text = "LPF Cutoff Freq:", Location = new Point(10, 95), AutoSize = true };
            numLpfCutoff = new NumericUpDown
            {
                Location = new Point(160, 92),
                Width = 120,
                Minimum = 1000,
                Maximum = 20000,
                Increment = 100,
                Value = 3400
            };

            pnlParameters.Controls.Clear();
            pnlParameters.Controls.AddRange(new Control[] { lblRate, cmbSamplingRate, lblStepSize, numStepSize, lblLpfCutoff, numLpfCutoff });
        }


        private void RenderADMParameters()
        {
            Label lblRate = new Label { Text = "Sampling Rate:", Location = new Point(10, 15), AutoSize = true };
            cmbSamplingRate = new ComboBox { Location = new Point(160, 12), Width = 120 };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "32000", "44100" });
            cmbSamplingRate.SelectedIndex = 1;

            Label lblInitStep = new Label { Text = "Initial Step Size:", Location = new Point(10, 55), AutoSize = true };
            numInitStepSize = new NumericUpDown
            {
                Location = new Point(160, 52),
                Width = 120,
                Minimum = 0.001M,
                Maximum = 0.5M,
                DecimalPlaces = 3,
                Increment = 0.005M,
                Value = 0.01M
            };

            Label lblAdaptation = new Label { Text = "Adaptation Factor:", Location = new Point(10, 95), AutoSize = true };
            numAdaptationFactor = new NumericUpDown
            {
                Location = new Point(160, 92),
                Width = 120,
                Minimum = 1.1M,
                Maximum = 3.0M,
                DecimalPlaces = 2,
                Increment = 0.1M,
                Value = 1.5M
            };

            Label lblMaxStep = new Label { Text = "Max Step Size:", Location = new Point(10, 135), AutoSize = true };
            numMaxStepSize = new NumericUpDown
            {
                Location = new Point(160, 132),
                Width = 120,
                Minimum = 0.05M,
                Maximum = 2.0M,
                DecimalPlaces = 2,
                Increment = 0.05M,
                Value = 0.3M
            };

            Label lblHistory = new Label { Text = "History Bits (Memory):", Location = new Point(10, 175), AutoSize = true };
            numHistoryBits = new NumericUpDown
            {
                Location = new Point(160, 172),
                Width = 120,
                Minimum = 2,
                Maximum = 5,
                Value = 3
            };

            Label lblLpfCutoff = new Label { Text = "LPF Cutoff Freq:", Location = new Point(10, 215), AutoSize = true };
            numLpfCutoff = new NumericUpDown
            {
                Location = new Point(160, 212),
                Width = 120,
                Minimum = 1000,
                Maximum = 20000,
                Increment = 100,
                Value = 3400
            };

            pnlParameters.Controls.Clear();
            pnlParameters.Controls.AddRange(new Control[] {
        lblRate, cmbSamplingRate,
        lblInitStep, numInitStepSize,
        lblAdaptation, numAdaptationFactor,
        lblMaxStep, numMaxStepSize,
        lblHistory, numHistoryBits,
        lblLpfCutoff, numLpfCutoff
    });
        }

        private void RenderAdaptivePredictiveParameters()
{
    pnlParameters.Controls.Clear();

    // 1. Levels
    Label lblLevels = new Label { Text = "Levels:", Location = new Point(10, 15), AutoSize = true };
    numLevels = new NumericUpDown { Location = new Point(160, 12), Width = 120, Minimum = 2, Maximum = 16, Value = 4 };

    // 2. Step Size
    Label lblStep = new Label { Text = "Step Size:", Location = new Point(10, 55), AutoSize = true };
    numStep = new NumericUpDown { Location = new Point(160, 52), Width = 120, Minimum = 1, Maximum = 1000, Value = 50 };

    // 3. Mode
    Label lblMode = new Label { Text = "Mode:", Location = new Point(10, 95), AutoSize = true };
    cmbMode = new ComboBox { Location = new Point(160, 92), Width = 120 };
    cmbMode.Items.AddRange(new object[] { "Simple", "Linear", "Adaptive" });
    cmbMode.SelectedIndex = 0;

    // 4. Sample Rate (ComboBox)
    Label lblRate = new Label { Text = "Target Sample Rate:", Location = new Point(10, 135), AutoSize = true };
    cmbSampleRate = new ComboBox { Location = new Point(160, 132), Width = 120 };
    cmbSampleRate.Items.AddRange(new object[] { "8000", "16000", "22050", "44100" });
    cmbSampleRate.SelectedIndex = 1;

    // 5. Predictor Order (هنا التعديل: نستخدم المتغير العام numPredictionOrder)
    Label lblOrder = new Label { Text = "Predictor Order:", Location = new Point(10, 175), AutoSize = true };
    numPredictionOrder = new NumericUpDown { Location = new Point(160, 172), Width = 120, Minimum = 1, Maximum = 100, Value = 1 };

    // 6. CheckBox
    chkIsMono = new CheckBox { Text = "Use Mono", Location = new Point(10, 215), AutoSize = true, Checked = true };

    pnlParameters.Controls.AddRange(new Control[] {
        lblLevels, numLevels,
        lblStep, numStep,
        lblMode, cmbMode,
        lblRate, cmbSampleRate,
        lblOrder, numPredictionOrder, // أضفناها هنا
        chkIsMono
    });
}
        private void cmbAlgorithmType_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlParameters.Controls.Clear();
            string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();

            if (selectedAlgorithm == "DPCM")
            {
                RenderDpcmParameters();
            }
            else if (selectedAlgorithm == "Mu-Law")
            {
                RenderMuLawParameters();
            }
            else if (selectedAlgorithm == "A-Law")
            {
                RenderALawParameters();
            }
            else if (selectedAlgorithm == "Delta Modulation")
            {
                RenderDMParameters();
            }
            else if (selectedAlgorithm == "Adaptive Delta Modulation")
            {
                RenderADMParameters();
            }
            else if (selectedAlgorithm == "Adaptive Predictive") 
            {
                RenderAdaptivePredictiveParameters();
            }
        }


        // غيري void إلى CompressionResult
        private CompressionResult ExecuteAdaptivePredictiveCompression(string inputPath, int targetSampleRate, int levels, CompressionEngine.PredictionMode mode, double stepSize, bool isMono, int predictorOrder)
        {
            using (var reader = new AudioFileReader(inputPath))
            {
                int channels = isMono ? 1 : reader.WaveFormat.Channels;
                var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, channels));
                var sampleProvider = resampler.ToSampleProvider();

                int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate * channels);
                float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
                int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);

                short[] pcmSamples = new short[samplesRead];
                for (int i = 0; i < samplesRead; i++)
                {
                    pcmSamples[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, floatBuffer[i] * short.MaxValue));
                }

                CompressionEngine engine = new CompressionEngine();
                var result = engine.Compress(pcmSamples, levels, mode, stepSize, channels, targetSampleRate, predictorOrder);

                _copied_audio = result.CompressedData; // تحديث المصفوفة العامة
                return result; // إرجاع النتيجة
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
            else if (selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
            {
                string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
                if (!int.TryParse(samplingRateText, out int targetSamplingRate))
                {
                    targetSamplingRate = 8000;
                }

                int bitDepth = (int)numQuantBits.Value;
                bool useMono = cmbChannels == null || cmbChannels.SelectedIndex == 0;

                try
                {
                    btnRunCompression.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;

                    ExecuteCompandingCompressionToMemory(_inputFilePath, selectedAlgorithm, targetSamplingRate, bitDepth, useMono);

                    long originalSize = new FileInfo(_inputFilePath).Length;
                    long compressedSize = _copied_audio.Length;
                    double ratio = (double)originalSize / compressedSize;

                    MessageBox.Show($"Compressed to Memory using {selectedAlgorithm}!\n\n" +
                                    $"Returned File: {Path.GetFileNameWithoutExtension(_inputFilePath)}_{selectedAlgorithm.Replace("-", "").ToLower()}_compressed.wav\n" +
                                    $"Returned Extension: .wav\n" +
                                    $"Original Size: {FormatBytes(originalSize)}\n" +
                                    $"Returned Size: {FormatBytes(compressedSize)}\n" +
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

            else if (selectedAlgorithm == "Delta Modulation")
            {
                string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
                if (!int.TryParse(samplingRateText, out int targetSamplingRate))
                {
                    targetSamplingRate = 16000;
                }

                double stepSize = (double)numStepSize.Value;
                int lpfCutoff = (int)numLpfCutoff.Value;

                try
                {
                    btnRunCompression.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;

                    ExecuteDmCompressionToMemory(_inputFilePath, targetSamplingRate, stepSize, lpfCutoff);

                    long originalSize = new FileInfo(_inputFilePath).Length;
                    long compressedSize = _copied_audio.Length;
                    double ratio = (double)originalSize / compressedSize;

                    string originalSizeFormatted = FormatBytes(originalSize);
                    string compressedSizeFormatted = FormatBytes(compressedSize);

                    MessageBox.Show($"Compressed to Memory using DM!\n\n" +
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
            else if (selectedAlgorithm == "Adaptive Delta Modulation")
            {
                string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
                if (!int.TryParse(samplingRateText, out int targetSamplingRate))
                {
                    targetSamplingRate = 16000;
                }

                double initStepSize = (double)numInitStepSize.Value;
                double adaptationFactor = (double)numAdaptationFactor.Value;
                double maxStepSize = (double)numMaxStepSize.Value;
                int historyBits = (int)numHistoryBits.Value;
                int lpfCutoff = (int)numLpfCutoff.Value;

                try
                {
                    btnRunCompression.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;

                    ExecuteAdmCompressionToMemory(_inputFilePath, targetSamplingRate, initStepSize, adaptationFactor, maxStepSize, historyBits, lpfCutoff);

                    long originalSize = new FileInfo(_inputFilePath).Length;
                    long compressedSize = _copied_audio.Length;
                    double ratio = (double)originalSize / compressedSize;

                    string originalSizeFormatted = FormatBytes(originalSize);
                    string compressedSizeFormatted = FormatBytes(compressedSize);

                    MessageBox.Show($"Compressed to Memory using ADM!\n\n" +
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
            else if (selectedAlgorithm == "Adaptive Predictive")
            {
                try
                {
                    // 1. التأكد من أن الأدوات ليست فارغة قبل القراءة
                    if (cmbSampleRate.SelectedItem == null || numLevels == null || numPredictionOrder == null)
                    {
                        MessageBox.Show("خطأ: لم يتم تهيئة الأدوات بشكل صحيح.");
                        return;
                    }

                    // 2. قراءة القيم
                    int sampleRate = int.Parse(cmbSampleRate.SelectedItem.ToString());
                    int levels = (int)numLevels.Value;
                    double stepSize = (double)numStep.Value;
                    // استبدلي سطر الـ mode في btnRunCompression_Click بهذا:
                    string selectedModeText = cmbMode.SelectedItem.ToString();
                    CompressionEngine.PredictionMode mode = (CompressionEngine.PredictionMode)Enum.Parse(typeof(CompressionEngine.PredictionMode), selectedModeText); bool isMono = chkIsMono.Checked;
                    int predictorOrder = (int)numPredictionOrder.Value;

                    btnRunCompression.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;

                    // 3. تنفيذ الضغط
                    var result = ExecuteAdaptivePredictiveCompression(_inputFilePath, sampleRate, levels, mode, stepSize, isMono, predictorOrder);

                    // 4. معالجة البيانات والـ Header
                    long compressedSize = 0;
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(levels);
                        writer.Write((int)mode);
                        writer.Write(stepSize);
                        writer.Write(sampleRate);
                        writer.Write(result.CompressedData.Length);
                        writer.Write(result.CompressedData);
                        _copied_audio = ms.ToArray();
                        compressedSize = ms.Length; // الحجم الكلي مع الـ Header
                    }

                    // 5. الحسابات
                    long originalSize = new FileInfo(_inputFilePath).Length;
                    long rawPcmSize = (long)result.TotalSamples * 2;
                    double realRatio = (double)rawPcmSize / compressedSize;

                    // 6. ظهور الرسالة المطلوبة
                    MessageBox.Show($"حجم الصوت الخام (Raw PCM): {rawPcmSize} بايت\n" +
                                    $"الحجم النهائي (مع الـ Header): {compressedSize} بايت\n" +
                                    $"نسبة الضغط الفعلية: {realRatio:F2}x", "تحليل الضغط");
                }
                catch (Exception ex)
                {
                    // إذا حدث أي خطأ (مثل قسمة على صفر أو ملف تالف)، ستظهر هذه الرسالة وتخبركِ بالسبب
                    MessageBox.Show($"حدث خطأ أثناء الضغط: \n{ex.Message}\n{ex.StackTrace}", "خطأ في الضغط");
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

        private void ExecuteCompandingCompressionToMemory(string inputPath, string algorithm, int targetSampleRate, int bitDepth, bool useMono)
        {
            using (var reader = new AudioFileReader(inputPath))
            using (var resampler = new MediaFoundationResampler(
                reader,
                new WaveFormat(targetSampleRate, 16, useMono ? 1 : reader.WaveFormat.Channels)))
            {
                var sampleProvider = resampler.ToSampleProvider();
                float[] samples = ReadAllSamples(sampleProvider);

                if (samples.Length == 0)
                {
                    throw new InvalidOperationException("No audio samples were read from the copied file.");
                }

                _originalSamples = new short[samples.Length];
                byte[] compressedBytes = new byte[samples.Length];

                for (int i = 0; i < samples.Length; i++)
                {
                    _originalSamples[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, samples[i] * short.MaxValue));

                    double companded =
                        algorithm == "A-Law"
                            ? ApplyALaw(samples[i])
                            : ApplyMuLaw(samples[i]);

                    // The uploaded logic returns one 8-bit WAV byte for each quantized companded sample.
                    compressedBytes[i] = QuantizeToByte(companded, bitDepth);
                }

                _copied_audio = CreateCompressedWaveBytes(
                    compressedBytes,
                    sampleProvider.WaveFormat.SampleRate,
                    sampleProvider.WaveFormat.Channels);

                _compandingMetadata = new CompandingMetadata
                {
                    Algorithm = algorithm,
                    OriginalExtension = Path.GetExtension(inputPath),
                    SampleRate = sampleProvider.WaveFormat.SampleRate,
                    BitDepth = (byte)bitDepth,
                    Channels = sampleProvider.WaveFormat.Channels,
                    TotalSamples = samples.Length
                };
            }
        }

        private byte[] ExecuteCompandingDecompressionToMemory()
        {
            if (_compandingMetadata == null)
            {
                throw new InvalidOperationException("No Mu-Law or A-Law metadata found. Run compression first.");
            }

            using (var reader = new WaveFileReader(new MemoryStream(_copied_audio)))
            {
                if (reader.WaveFormat.BitsPerSample != 8)
                {
                    throw new InvalidOperationException("Please choose an 8-bit compressed WAV file to decompress.");
                }

                byte[] compressedBytes = ReadAllBytes(reader);

                if (compressedBytes.Length == 0)
                {
                    throw new InvalidOperationException("No compressed audio samples were read from the compressed copy.");
                }

                float[] expandedSamples = new float[compressedBytes.Length];
                short[] decompressedShorts = new short[compressedBytes.Length];

                for (int i = 0; i < compressedBytes.Length; i++)
                {
                    double companded = DequantizeByte(compressedBytes[i]);
                    double expanded =
                        _compandingMetadata.Algorithm == "A-Law"
                            ? InverseALaw(companded)
                            : InverseMuLaw(companded);

                    expandedSamples[i] = (float)Clamp(expanded, -1.0, 1.0);
                    decompressedShorts[i] = (short)(expandedSamples[i] * short.MaxValue);
                }

                _decompressedPcmBytes = new byte[decompressedShorts.Length * 2];
                Buffer.BlockCopy(decompressedShorts, 0, _decompressedPcmBytes, 0, _decompressedPcmBytes.Length);

                return CreateDecompressedWaveBytes(
                    expandedSamples,
                    reader.WaveFormat.SampleRate,
                    reader.WaveFormat.Channels);
            }
        }

        private double ApplyMuLaw(double x)
        {
            double magnitude = Math.Abs(x);

            return Math.Sign(x) *
                   Math.Log(1.0 + MuLawMu * magnitude) /
                   Math.Log(1.0 + MuLawMu);
        }

        private double InverseMuLaw(double y)
        {
            double magnitude = Math.Abs(y);

            return Math.Sign(y) *
                   (Math.Pow(1.0 + MuLawMu, magnitude) - 1.0) /
                   MuLawMu;
        }

        private double ApplyALaw(double x)
        {
            double magnitude = Math.Abs(x);
            double denominator = 1.0 + Math.Log(ALawA);
            double companded;

            if (magnitude < 1.0 / ALawA)
            {
                companded = (ALawA * magnitude) / denominator;
            }
            else
            {
                companded = (1.0 + Math.Log(ALawA * magnitude)) / denominator;
            }

            return Math.Sign(x) * companded;
        }

        private double InverseALaw(double y)
        {
            double magnitude = Math.Abs(y);
            double denominator = 1.0 + Math.Log(ALawA);
            double boundary = 1.0 / denominator;
            double expanded;

            if (magnitude < boundary)
            {
                expanded = (magnitude * denominator) / ALawA;
            }
            else
            {
                expanded = Math.Exp(magnitude * denominator - 1.0) / ALawA;
            }

            return Math.Sign(y) * expanded;
        }

        private byte QuantizeToByte(double sample, int bitDepth)
        {
            int usableBits = Math.Max(2, Math.Min(8, bitDepth));
            int levels = 1 << usableBits;
            double normalized = (Clamp(sample, -1.0, 1.0) + 1.0) / 2.0;
            int index = (int)Math.Round(normalized * (levels - 1));
            double quantized = index / (double)(levels - 1);

            return (byte)Math.Round(quantized * 255);
        }

        private double DequantizeByte(byte sample)
        {
            return (sample / 255.0) * 2.0 - 1.0;
        }

        private float[] ReadAllSamples(ISampleProvider provider)
        {
            List<float> samples = new List<float>();
            float[] buffer = new float[provider.WaveFormat.SampleRate * provider.WaveFormat.Channels];
            int read;

            while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    samples.Add(buffer[i]);
                }
            }

            return samples.ToArray();
        }

        private byte[] ReadAllBytes(WaveStream reader)
        {
            List<byte> bytes = new List<byte>();
            byte[] buffer = new byte[Math.Max(1, reader.WaveFormat.AverageBytesPerSecond)];
            int read;

            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    bytes.Add(buffer[i]);
                }
            }

            return bytes.ToArray();
        }

        private byte[] CreateCompressedWaveBytes(byte[] compressedBytes, int sampleRate, int channels)
        {
            WaveFormat format = new WaveFormat(sampleRate, 8, channels);

            using (MemoryStream stream = new MemoryStream())
            {
                using (WaveFileWriter writer = new WaveFileWriter(stream, format))
                {
                    // Compression returns a WAV file copy in memory instead of writing over the uploaded file.
                    writer.Write(compressedBytes, 0, compressedBytes.Length);
                }

                return stream.ToArray();
            }
        }

        private byte[] CreateDecompressedWaveBytes(float[] samples, int sampleRate, int channels)
        {
            byte[] buffer = new byte[samples.Length * 2];

            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)(Clamp(samples[i], -1.0, 1.0) * short.MaxValue);
                byte[] bytes = BitConverter.GetBytes(value);

                buffer[i * 2] = bytes[0];
                buffer[i * 2 + 1] = bytes[1];
            }

            using (MemoryStream stream = new MemoryStream())
            {
                using (WaveFileWriter writer = new WaveFileWriter(stream, new WaveFormat(sampleRate, 16, channels)))
                {
                    // Decompression returns a playable 16-bit WAV copy in memory.
                    writer.Write(buffer, 0, buffer.Length);
                }

                return stream.ToArray();
            }
        }

        private double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
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


        private void ExecuteDmCompressionToMemory(string inputPath, int targetSampleRate, double stepSize, int lpfCutoff)
        {
            using (var reader = new AudioFileReader(inputPath))
            {
                var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
                var sampleProvider = resampler.ToSampleProvider();

                int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
                float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
                int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);
                _dmMetadata = new DmMetadata
                {
                    SampleRate = targetSampleRate,
                    TotalSamples = samplesRead,
                    StepSize = (float)stepSize,
                    LpfCutoff = lpfCutoff,
                    OriginalExtension = Path.GetExtension(inputPath)
                };
                float[] audioSamples = new float[samplesRead];
                Array.Copy(floatBuffer, audioSamples, samplesRead);
                _originalSamples = new short[samplesRead];

                for (int i = 0; i < samplesRead; i++)
                {
                    _originalSamples[i] = (short)Math.Max(
                        short.MinValue,
                        Math.Min(short.MaxValue, audioSamples[i] * 32767f));
                }
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    float predictedValue = 0f;
                    float step = (float)stepSize;

                    byte currentByte = 0;
                    int bitCounter = 0;

                    for (int n = 0; n < samplesRead; n++)
                    {
                        int bit;
                        if (audioSamples[n] >= predictedValue)
                        {
                            bit = 1;
                            predictedValue += step; 
                        }
                        else
                        {
                            bit = 0;
                            predictedValue -= step;
                        }

                        currentByte |= (byte)(bit << (7 - bitCounter));
                        bitCounter++;

                        if (bitCounter == 8)
                        {
                            writer.Write(currentByte);
                            currentByte = 0;
                            bitCounter = 0;
                        }
                    }

                    if (bitCounter > 0)
                    {
                        writer.Write(currentByte);
                    }

                    writer.Flush();
                    _copied_audio = ms.ToArray();

                }
            }
        }


        private void ExecuteAdmCompressionToMemory(string inputPath, int targetSampleRate, double initStepSize, double adaptationFactor, double maxStepSize, int historyBits, int lpfCutoff)
        {
            using (var reader = new AudioFileReader(inputPath))
            {
                var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
                var sampleProvider = resampler.ToSampleProvider();

                int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
                float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
                int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);
                _admMetadata = new AdmMetadata
                {
                    SampleRate = targetSampleRate,
                    TotalSamples = samplesRead,
                    InitStepSize = (float)initStepSize,
                    AdaptationFactor = (float)adaptationFactor,
                    MaxStepSize = (float)maxStepSize,
                    HistoryBits = historyBits,
                    LpfCutoff = lpfCutoff,
                    OriginalExtension = Path.GetExtension(inputPath)
                };
                float[] audioSamples = new float[samplesRead];
                Array.Copy(floatBuffer, audioSamples, samplesRead);
                _originalSamples = new short[samplesRead];

                for (int i = 0; i < samplesRead; i++)
                {
                    _originalSamples[i] = (short)Math.Max(
                        short.MinValue,
                        Math.Min(short.MaxValue, audioSamples[i] * 32767f));
                }
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    float predictedValue = 0f;
                    float currentStepSize = (float)initStepSize;
                    float minStep = (float)initStepSize;
                    float maxStep = (float)maxStepSize;
                    float K = (float)adaptationFactor;

                    int[] historyPattern = new int[historyBits];

                    byte currentByte = 0;
                    int bitCounter = 0;

                    for (int n = 0; n < samplesRead; n++)
                    {
                        int bit;
                        if (audioSamples[n] >= predictedValue)
                        {
                            bit = 1;
                            predictedValue += currentStepSize;
                        }
                        else
                        {
                            bit = 0;
                            predictedValue -= currentStepSize;
                        }

                        for (int i = historyBits - 1; i > 0; i--)
                        {
                            historyPattern[i] = historyPattern[i - 1];
                        }
                        historyPattern[0] = bit;

                        if (n >= historyBits - 1)
                        {
                            bool allSame = true;
                            bool alternating = true;

                            for (int i = 1; i < historyBits; i++)
                            {
                                if (historyPattern[i] != historyPattern[0]) allSame = false;
                                if (historyPattern[i] == historyPattern[i - 1]) alternating = false;
                            }

                            if (allSame)
                            {
                                currentStepSize = Math.Min(maxStep, currentStepSize * K);
                            }
                            else if (alternating)
                            {
                                currentStepSize = Math.Max(minStep, currentStepSize / K);
                            }
                        }

                        currentByte |= (byte)(bit << (7 - bitCounter));
                        bitCounter++;

                        if (bitCounter == 8)
                        {
                            writer.Write(currentByte);
                            currentByte = 0;
                            bitCounter = 0;
                        }
                    }

                    if (bitCounter > 0)
                    {
                        writer.Write(currentByte);
                    }

                    writer.Flush();
                    _copied_audio = ms.ToArray();
                }
            }
        }


        private void btnRunDecompression_Click(object sender, EventArgs e)
        {
            if (_copied_audio == null)
            {
                MessageBox.Show("No compressed data found in memory! Run compression first.",
                    "Notice",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();
            try
            {
                btnRunDecompression.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                if (selectedAlgorithm == "DPCM")
                {
                    ExecuteDpcmDecompressionToMemory();
                }
                else if (selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
                {
                    byte[] returnedFile = ExecuteCompandingDecompressionToMemory();
                    double compandingMse = CalculateMSE();

                    MessageBox.Show(
                        $"Audio decompressed completely using {selectedAlgorithm}!\n\n" +
                        $"Returned File: {_compandingMetadata.Algorithm.Replace("-", "").ToLower()}_decompressed.wav\n" +
                        $"Returned Extension: .wav\n" +
                        $"Returned Size: {FormatBytes(returnedFile.Length)}\n" +
                        $"MSE: {compandingMse:F2}\n\n" +
                        $"Status: Ready for playing or processing.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }
                else if (selectedAlgorithm == "Delta Modulation")
                {
                    ExecuteDmDecompressionToMemory();
                }
                else if (selectedAlgorithm == "Adaptive Delta Modulation")
                {
                    ExecuteAdmDecompressionToMemory();
                }
                

                long decompressedSize = _decompressedPcmBytes.Length;
                string decompressedSizeFormatted = FormatBytes(decompressedSize);

                double mse = CalculateMSE();

                MessageBox.Show(
                    $"Audio decompressed completely in memory!\n\n" +
                    $"Decompressed Size: {decompressedSizeFormatted}\n" +
                    $"MSE: {mse:F2}\n\n" +
                    $"Status: Ready for playing or processing.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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

        private void ExecuteDmDecompressionToMemory()
        {
            using (MemoryStream msInput = new MemoryStream(_copied_audio))
            using (BinaryReader reader = new BinaryReader(msInput))
            {
                int sampleRate = _dmMetadata.SampleRate;
                int totalSamples = _dmMetadata.TotalSamples;

                short[] decompressedShorts = new short[totalSamples];

                float predictedValue = 0f;
                float step = _dmMetadata.StepSize;

                int byteIndex = 0;
                byte currentByte = 0;
                int bitCounter = 8;

                for (int n = 0; n < totalSamples; n++)
                {
                    if (bitCounter == 8)
                    {
                        if (reader.BaseStream.Position >= reader.BaseStream.Length) break;
                        currentByte = reader.ReadByte();
                        bitCounter = 0;
                    }

                    int bit = (currentByte >> (7 - bitCounter)) & 1;
                    bitCounter++;

                    if (bit == 1)
                    {
                        predictedValue += step;
                    }
                    else
                    {
                        predictedValue -= step;
                    }

                    predictedValue = Math.Max(-1f, Math.Min(1f, predictedValue));
                    decompressedShorts[n] = (short)Math.Max(-32768, Math.Min(32767, predictedValue * 32767f));
                }

                _decompressedPcmBytes = new byte[decompressedShorts.Length * 2];
                Buffer.BlockCopy(decompressedShorts, 0, _decompressedPcmBytes, 0, _decompressedPcmBytes.Length);
            }
        }

        private void ExecuteAdmDecompressionToMemory()
        {
            using (MemoryStream msInput = new MemoryStream(_copied_audio))
            using (BinaryReader reader = new BinaryReader(msInput))
            {
                int sampleRate = _admMetadata.SampleRate;
                int totalSamples = _admMetadata.TotalSamples;

                short[] decompressedShorts = new short[totalSamples];

                float predictedValue = 0f;
                float currentStepSize = _admMetadata.InitStepSize;
                float minStep = _admMetadata.InitStepSize;
                float maxStep = _admMetadata.MaxStepSize;
                float K = _admMetadata.AdaptationFactor;
                int historyBits = _admMetadata.HistoryBits;

                int[] historyPattern = new int[historyBits];

                int byteIndex = 0;
                byte currentByte = 0;
                int bitCounter = 8;

                for (int n = 0; n < totalSamples; n++)
                {
                    if (bitCounter == 8)
                    {
                        if (reader.BaseStream.Position >= reader.BaseStream.Length)
                            break;

                        currentByte = reader.ReadByte();
                        bitCounter = 0;
                    }

                    int bit = (currentByte >> (7 - bitCounter)) & 1;
                    bitCounter++;

                    if (bit == 1)
                    {
                        predictedValue += currentStepSize;
                    }
                    else
                    {
                        predictedValue -= currentStepSize;
                    }

                    for (int i = historyBits - 1; i > 0; i--)
                    {
                        historyPattern[i] = historyPattern[i - 1];
                    }

                    historyPattern[0] = bit;

                    if (n >= historyBits - 1)
                    {
                        bool allSame = true;
                        bool alternating = true;

                        for (int i = 1; i < historyBits; i++)
                        {
                            if (historyPattern[i] != historyPattern[0])
                                allSame = false;

                            if (historyPattern[i] == historyPattern[i - 1])
                                alternating = false;
                        }

                        if (allSame)
                        {
                            currentStepSize = Math.Min(maxStep, currentStepSize * K);
                        }
                        else if (alternating)
                        {
                            currentStepSize = Math.Max(minStep, currentStepSize / K);
                        }
                    }

                    predictedValue = Math.Max(-1f, Math.Min(1f, predictedValue));

                    decompressedShorts[n] =
                        (short)Math.Max(-32768,
                        Math.Min(32767, predictedValue * 32767f));
                }

                _decompressedPcmBytes = new byte[decompressedShorts.Length * 2];

                Buffer.BlockCopy(
                    decompressedShorts,
                    0,
                    _decompressedPcmBytes,
                    0,
                    _decompressedPcmBytes.Length);
            }
        }
        private double CalculateMSE()
        {
            if (_originalSamples == null || _decompressedPcmBytes == null)
                return -1;

            short[] reconstructed =
                new short[_decompressedPcmBytes.Length / 2];

            Buffer.BlockCopy(
                _decompressedPcmBytes,
                0,
                reconstructed,
                0,
                _decompressedPcmBytes.Length);

            int count = Math.Min(
                _originalSamples.Length,
                reconstructed.Length);

            double mse = 0;

            for (int i = 0; i < count; i++)
            {
                double error =
                    _originalSamples[i] - reconstructed[i];

                mse += error * error;
            }

            mse /= count;

            return mse;
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
