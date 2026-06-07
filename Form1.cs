
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Reflection.Emit;
using Emgu.CV; // (فقط هذا السطر الخاص بـ Emgu)
using NAudio.Wave;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ListView = System.Windows.Forms.ListView;



namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {

        private string audioPath = "";
        private string audioInfo = "";
        private string _originalFilePath = "";
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private System.Windows.Forms.Timer playbackTimer;
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
        private bool _isDecompressed = false;
        private RawSourceWaveStream _decompressedStream = null;
        private long _decompressedTotalBytes = 0;

        private short[] _originalSamples = null;

        private NumericUpDown numLpfCutoff;
        private NumericUpDown numStepSize;
        private NumericUpDown numInitStepSize;
        private NumericUpDown numAdaptationFactor;
        private NumericUpDown numMaxStepSize;
        private NumericUpDown numHistoryBits;
        private System.Windows.Forms.ComboBox cmbSampleRate;
        private System.Windows.Forms.ComboBox cmbChannels;
        //ghody

        private NumericUpDown numLevels;
        private NumericUpDown numStep;
        private NumericUpDown numPredictionOrder;
        private System.Windows.Forms.ComboBox cmbMode;
        private CheckBox chkIsMono;
        private System.Windows.Forms.ComboBox cmbSampleRate1; // وهذا أيضاً


        // REQUIREMENT 7 + 8
        private List<float> _ratioHistory = new List<float>();
        private List<float> _speedHistory = new List<float>();
        private long _originalFileSize = 0;
        private DateTime _compressionStartTime;
        private System.Windows.Forms.Timer _chartTimer;
        private CancellationTokenSource _cancellationTokenSource;

        // COMPRESSION REPORT TRACKING FIELDS
        private string _lastUsedAlgorithmName = "";
        private TimeSpan _compressionTimeTaken;
        private long _uncompressedNativeArraySize = 0;

        // MODERN REPORT UI COMPONENTS
        private Panel pnlReportSidebar;
        private Label lblReportTitle;
        private Label lblTimeValue;
        private Label uncompressedSizeFormatted;
        private Label compressedSizeFormatted;
        private System.Windows.Forms.ListView lvwParameters;
        private TableLayoutPanel ptlMetricsTable;
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
            playbackTimer = new System.Windows.Forms.Timer();
            playbackTimer.Interval = 50;
            playbackTimer.Tick += PlaybackTimer_Tick;
            DragDropLabel.TextAlign = ContentAlignment.TopLeft;
            DragDropLabel.BorderStyle = BorderStyle.FixedSingle;

            if (cmbAlgorithmType.Items.Count > 0)
                cmbAlgorithmType.SelectedIndex = 0;
            InitializeReportSidebar();
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


        //private void PlayAudiobtn_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(audioPath))
        //        {
        //            MessageBox.Show("Please insert an audio file first.",
        //                "No Audio Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            return;
        //        }

        //        if (!isPlaying)
        //        {
        //            if (outputDevice == null || outputDevice.PlaybackState == PlaybackState.Stopped)
        //            {
        //                audioFile?.Dispose();
        //                outputDevice?.Dispose();

        //                audioFile = new AudioFileReader(audioPath);
        //                outputDevice = new WaveOutEvent();
        //                outputDevice.Init(audioFile);
        //            }

        //            outputDevice.Play();
        //            playbackTimer.Start();
        //            PlayAudiobtn.Text = "Pause ❚❚";
        //            isPlaying = true;
        //        }
        //        else
        //        {
        //            outputDevice.Pause();
        //            playbackTimer.Stop();
        //            PlayAudiobtn.Text = "Play Audio ▶︎";
        //            isPlaying = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}

        private void PlayAudiobtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isDecompressed && _decompressedPcmBytes != null)
                {
                    // Play decompressed PCM
                    if (!isPlaying)
                    {
                        if (outputDevice == null || outputDevice.PlaybackState == PlaybackState.Stopped)
                        {
                            outputDevice?.Dispose();

                            int sampleRate = GetDecompressedSampleRate();
                            var waveFormat = new WaveFormat(sampleRate, 16, 1);
                            var memStream = new MemoryStream(_decompressedPcmBytes);
                            var rawStream = new RawSourceWaveStream(memStream, waveFormat);

                            // Keep reference so we can track position for the playhead
                            _decompressedStream = rawStream;
                            _decompressedTotalBytes = _decompressedPcmBytes.Length;

                            outputDevice = new WaveOutEvent();
                            outputDevice.Init(rawStream);
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
                else
                {
                    // Original flow — play from file
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


        private void DrawWaveformFromPcm(byte[] pcmBytes, int sampleRate)
        {
            int width = waveformPictureBox.Width;
            int height = waveformPictureBox.Height;
            Bitmap bmp = new Bitmap(width, height);

            int totalSamples = pcmBytes.Length / 2; // 16-bit = 2 bytes per sample
            int samplesPerPixel = Math.Max(1, totalSamples / width);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                using (Pen pen = new Pen(Color.DarkGreen)) // green = decompressed
                {
                    for (int x = 0; x < width; x++)
                    {
                        int startSample = x * samplesPerPixel;
                        float max = 0f;

                        for (int i = 0; i < samplesPerPixel; i++)
                        {
                            int idx = (startSample + i) * 2;
                            if (idx + 1 >= pcmBytes.Length) break;
                            short sample = BitConverter.ToInt16(pcmBytes, idx);
                            float normalized = Math.Abs(sample / 32768f);
                            if (normalized > max) max = normalized;
                        }

                        int y = (int)(max * height / 2);
                        g.DrawLine(pen, x, height / 2 - y, x, height / 2 + y);
                    }
                }
            }

            if (waveformBitmap != null) waveformBitmap.Dispose();
            waveformBitmap = bmp;

            if (waveformPictureBox.Image != null) waveformPictureBox.Image.Dispose();
            waveformPictureBox.Image = (Bitmap)bmp.Clone();
        }


        //private void PlaybackTimer_Tick(object sender, EventArgs e)
        //{
        //    if (audioFile == null || waveformBitmap == null) return;

        //    if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Stopped)
        //    {
        //        playbackTimer.Stop();
        //        isPlaying = false;
        //        PlayAudiobtn.Text = "Play Audio ▶︎";
        //        return;
        //    }

        //    Bitmap frame = (Bitmap)waveformBitmap.Clone();
        //    using (Graphics g = Graphics.FromImage(frame))
        //    {
        //        double progress = audioFile.CurrentTime.TotalSeconds / audioFile.TotalTime.TotalSeconds;
        //        int x = (int)(progress * waveformPictureBox.Width);
        //        g.DrawLine(Pens.Red, x, 0, x, waveformPictureBox.Height);
        //    }

        //    if (waveformPictureBox.Image != null)
        //        waveformPictureBox.Image.Dispose();

        //    waveformPictureBox.Image = frame;
        //}


        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (waveformBitmap == null) return;

            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                playbackTimer.Stop();
                isPlaying = false;
                PlayAudiobtn.Text = "Play Audio ▶︎";
                return;
            }

            double progress = 0;

            if (_isDecompressed && _decompressedStream != null && _decompressedTotalBytes > 0)
            {
                progress = (double)_decompressedStream.Position / _decompressedTotalBytes;
            }
            else if (audioFile != null)
            {
                progress = audioFile.CurrentTime.TotalSeconds / audioFile.TotalTime.TotalSeconds;
            }

            Bitmap frame = (Bitmap)waveformBitmap.Clone();
            using (Graphics g = Graphics.FromImage(frame))
            {
                int x = (int)(progress * waveformPictureBox.Width);
                g.DrawLine(Pens.Red, x, 0, x, waveformPictureBox.Height);
            }

            if (waveformPictureBox.Image != null) waveformPictureBox.Image.Dispose();
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
            _originalFilePath = filePath;
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
            PlayAudiobtn.Enabled = true;
            _isDecompressed = false;
            _decompressedStream = null;

            _ratioHistory.Clear();
            _speedHistory.Clear();
            progressBar.Value = 0;
            lblProgressPercent.Text = "Ready";
            lblChartRatio.Text = "Compression Ratio:";
            lblChartSpeed.Text = "Processing Speed:";
            DrawChart(chartCompressRatio, _ratioHistory, Color.Blue, "Ratio %", 100f);
            DrawChart(chartSpeed, _speedHistory, Color.Green, "Samples/sec", 1f);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_workingAudioCopyPath) || !File.Exists(_workingAudioCopyPath))
            {
                MessageBox.Show("Insert an audio first",
                    "No Audio Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Stop anything currently playing so the reset starts from a clean audio state.
            playbackTimer.Stop();
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;
            audioFile?.Dispose();
            audioFile = null;
            _decompressedStream?.Dispose();
            _decompressedStream = null;

            // The working copy is the untouched version of the uploaded audio, so make it active again.
            audioPath = _workingAudioCopyPath;
            _inputFilePath = _workingAudioCopyPath;
            _copied_audio = null;
            _decompressedPcmBytes = null;
            _decompressedTotalBytes = 0;
            _isDecompressed = false;
            isPlaying = false;
            PlayAudiobtn.Text = "Play Audio ▶︎";

            // Clear algorithm metadata so decompression cannot reuse old compressed state after reset.
            _compressedMetadata = null;
            _dmMetadata = null;
            _admMetadata = null;
            _compandingMetadata = null;
            _originalSamples = null;

            DrawWaveform(audioPath);
            ClearChartsForReset();
            ResetSelectedAlgorithmParameters();

            btnRunCompression.Enabled = true;
            btnRunDecompression.Enabled = true;
            PlayAudiobtn.Enabled = true;
            btnCancelCompression.Enabled = false;
        }

        private void ClearChartsForReset()
        {
            // Empty both chart histories and redraw blank chart frames.
            _chartTimer?.Stop();
            _ratioHistory.Clear();
            _speedHistory.Clear();
            progressBar.Value = 0;
            lblProgressPercent.Text = "Ready";
            lblChartRatio.Text = "Compression Ratio:";
            lblChartSpeed.Text = "Processing Speed:";
            DrawChart(chartCompressRatio, _ratioHistory, Color.Blue, "Ratio %", 100f);
            DrawChart(chartSpeed, _speedHistory, Color.Green, "Samples/sec", 1f);
        }

        private void ResetSelectedAlgorithmParameters()
        {
            // Re-rendering the selected algorithm panel restores every parameter control to its default value.
            if (cmbAlgorithmType.SelectedItem != null)
            {
                cmbAlgorithmType_SelectedIndexChanged(cmbAlgorithmType, EventArgs.Empty);
            }
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

            _chartTimer = new System.Windows.Forms.Timer();
            _chartTimer.Interval = 100;
            lblChartRatio.AutoSize = true;
            lblChartSpeed.AutoSize = true;
            _chartTimer.Tick += (s, ev) =>
            {
                if (_ratioHistory.Count > 0)
                {
                    float currentRatio = _ratioHistory[_ratioHistory.Count - 1];
                    lblChartRatio.Text = $"Compression Ratio: {currentRatio:F2}%";
                    DrawChart(chartCompressRatio, _ratioHistory, Color.Blue, "Ratio %", 100f);
                }

                if (_speedHistory.Count > 0)
                {
                    float currentSpeed = _speedHistory[_speedHistory.Count - 1];
                    string speedText = currentSpeed >= 1000000
                        ? $"Processing Speed: {currentSpeed / 1000000:F2}M samples/sec"
                        : currentSpeed >= 1000
                            ? $"Processing Speed: {currentSpeed / 1000:F1}K samples/sec"
                            : $"Processing Speed: {currentSpeed:F0} samples/sec";
                    lblChartSpeed.Text = speedText;

                    float maxSpeed = _speedHistory.Max();
                    DrawChart(chartSpeed, _speedHistory, Color.Green, "Samples/sec", maxSpeed);
                }
            };
        }

        // ALGORITHMS
        private void RenderDpcmParameters()
        {
            Label lblRate = new Label { Text = "Sampling Rate:", Location = new Point(10, 15), AutoSize = true };

            cmbSamplingRate = new System.Windows.Forms.ComboBox { Location = new Point(150, 12), Width = 120 };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "44100" });
            cmbSamplingRate.SelectedIndex = 1;

            Label lblBits = new Label { Text = "Quantization Bits:", Location = new Point(10, 55), AutoSize = true };
            numQuantBits = new NumericUpDown { Location = new Point(150, 52), Width = 120, Minimum = 2, Maximum = 8, Value =6 };

            Label lblPredictor = new Label { Text = "Predictor Filter:", Location = new Point(10, 95), AutoSize = true };
            cmbPredictorType = new System.Windows.Forms.ComboBox { Location = new Point(150, 92), Width = 120 };
            cmbPredictorType.Items.AddRange(new object[] { "First-Order", "Second-Order" });
            cmbPredictorType.SelectedIndex = 0;

            pnlParameters.Controls.AddRange(new Control[] { lblRate, cmbSamplingRate, lblBits, numQuantBits, lblPredictor, cmbPredictorType });
        }

        private void RenderMuLawParameters()
        {
            Label lblRate = new Label { Text = "Sample Rate:", Location = new Point(10, 15), AutoSize = true };

            cmbSamplingRate = new System.Windows.Forms.ComboBox { Location = new Point(160, 12), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "22050", "44100" });
            // Mu-Law uses 8000 Hz as the requested default sample rate.
            cmbSamplingRate.SelectedIndex = 0;

            Label lblBits = new Label { Text = "Bit Depth:", Location = new Point(10, 55), AutoSize = true };
            // NumericUpDown keeps the bit depth constrained to the requested 2-16 range.
            numQuantBits = new NumericUpDown { Location = new Point(160, 52), Width = 140, Minimum = 2, Maximum = 16, Value = 8 };

            Label lblChannels = new Label { Text = "Channels:", Location = new Point(10, 95), AutoSize = true };
            cmbChannels = new System.Windows.Forms.ComboBox { Location = new Point(160, 92), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbChannels.Items.AddRange(new object[] { "Mono (1 Channel)", "Same as Original" });
            // Mono is selected first so the panel opens with the requested default.
            cmbChannels.SelectedIndex = 0;

            pnlParameters.Controls.AddRange(new Control[] { lblRate, cmbSamplingRate, lblBits, numQuantBits, lblChannels, cmbChannels });
        }

        private void RenderALawParameters()
        {
            Label lblRate = new Label { Text = "Sample Rate:", Location = new Point(10, 15), AutoSize = true };

            cmbSamplingRate = new System.Windows.Forms.ComboBox { Location = new Point(160, 12), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSamplingRate.Items.AddRange(new object[] { "8000", "16000", "22050", "44100" });
            // A-Law uses the same sample-rate options and 8000 Hz default as Mu-Law.
            cmbSamplingRate.SelectedIndex = 0;

            Label lblBits = new Label { Text = "Bit Depth:", Location = new Point(10, 55), AutoSize = true };
            // The requested default bit depth is 8, with valid values from 2 through 16.
            numQuantBits = new NumericUpDown { Location = new Point(160, 52), Width = 140, Minimum = 2, Maximum = 16, Value = 8 };

            Label lblChannels = new Label { Text = "Channels:", Location = new Point(10, 95), AutoSize = true };
            cmbChannels = new System.Windows.Forms.ComboBox { Location = new Point(160, 92), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbChannels.Items.AddRange(new object[] { "Mono (1 Channel)", "Same as Original" });
            // The first item is the requested Mono default.
            cmbChannels.SelectedIndex = 0;

            pnlParameters.Controls.AddRange(new Control[] { lblRate, cmbSamplingRate, lblBits, numQuantBits, lblChannels, cmbChannels });
        }

        private void RenderDMParameters()
        {
            Label lblRate = new Label { Text = "Sampling Rate:", Location = new Point(10, 15), AutoSize = true };
            cmbSamplingRate = new System.Windows.Forms.ComboBox { Location = new Point(160, 12), Width = 120 };
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
            cmbSamplingRate = new System.Windows.Forms.ComboBox { Location = new Point(160, 12), Width = 120 };
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
            cmbMode = new System.Windows.Forms.ComboBox { Location = new Point(160, 92), Width = 120 };
            cmbMode.Items.AddRange(new object[] { "Simple", "Linear", "Adaptive" });
            cmbMode.SelectedIndex = 0;

            // 4. Sample Rate (ComboBox)
            Label lblRate = new Label { Text = "Target Sample Rate:", Location = new Point(10, 135), AutoSize = true };
            cmbSampleRate = new System.Windows.Forms.ComboBox { Location = new Point(160, 132), Width = 120 };
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
            ResetCompressionReport();
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

                // --- التعديل هنا: تخزين النسخة الأصلية للذاكرة ---
                _originalSamples = pcmSamples;
                // ----------------------------------------------



                CompressionEngine engine = new CompressionEngine();
                var result = engine.Compress(pcmSamples, levels, mode, stepSize, channels, targetSampleRate, predictorOrder);

                _copied_audio = result.CompressedData; // تحديث المصفوفة العامة
                return result; // إرجاع النتيجة
            }
        }

        //private void btnRunCompression_Click(object sender, EventArgs e)
        //{
        //    string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();

        //    if (selectedAlgorithm == "DPCM")
        //    {
        //        string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
        //        if (!int.TryParse(samplingRateText, out int targetSamplingRate))
        //        {
        //            targetSamplingRate = 16000;
        //        }

        //        int quantizationBits = (int)numQuantBits.Value;
        //        int predictorType = cmbPredictorType.SelectedIndex == 1 ? 1 : 0;

        //        try
        //        {
        //            btnRunCompression.Enabled = false;
        //            this.Cursor = Cursors.WaitCursor;

        //            ExecuteDpcmCompressionToMemory(_inputFilePath, targetSamplingRate, quantizationBits, predictorType);

        //            long originalSize = new FileInfo(_inputFilePath).Length;
        //            long compressedSize = _copied_audio.Length;
        //            double ratio = (double)originalSize / compressedSize;

        //            string originalSizeFormatted = FormatBytes(originalSize);
        //            string compressedSizeFormatted = FormatBytes(compressedSize);

        //            MessageBox.Show($"Compressed to Memory!\n\n" +
        //                            $"Original Size: {originalSizeFormatted}\n" +
        //                            $"In-Memory Size: {compressedSizeFormatted}\n" +
        //                            $"Compression Ratio: {ratio:F2}x",
        //                            "Metrics Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Compression failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        finally
        //        {
        //            btnRunCompression.Enabled = true;
        //            this.Cursor = Cursors.Default;
        //        }
        //    }
        //    else if (selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
        //    {
        //        string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
        //        if (!int.TryParse(samplingRateText, out int targetSamplingRate))
        //        {
        //            targetSamplingRate = 8000;
        //        }

        //        int bitDepth = (int)numQuantBits.Value;
        //        bool useMono = cmbChannels == null || cmbChannels.SelectedIndex == 0;

        //        try
        //        {
        //            btnRunCompression.Enabled = false;
        //            this.Cursor = Cursors.WaitCursor;

        //            ExecuteCompandingCompressionToMemory(_inputFilePath, selectedAlgorithm, targetSamplingRate, bitDepth, useMono);

        //            long originalSize = new FileInfo(_inputFilePath).Length;
        //            long compressedSize = _copied_audio.Length;
        //            double ratio = (double)originalSize / compressedSize;

        //            MessageBox.Show($"Compressed to Memory using {selectedAlgorithm}!\n\n" +
        //                            $"Returned File: {Path.GetFileNameWithoutExtension(_inputFilePath)}_{selectedAlgorithm.Replace("-", "").ToLower()}_compressed.wav\n" +
        //                            $"Returned Extension: .wav\n" +
        //                            $"Original Size: {FormatBytes(originalSize)}\n" +
        //                            $"Returned Size: {FormatBytes(compressedSize)}\n" +
        //                            $"Compression Ratio: {ratio:F2}x",
        //                            "Metrics Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Compression failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        finally
        //        {
        //            btnRunCompression.Enabled = true;
        //            this.Cursor = Cursors.Default;
        //        }
        //    }

        //    else if (selectedAlgorithm == "Delta Modulation")
        //    {
        //        string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
        //        if (!int.TryParse(samplingRateText, out int targetSamplingRate))
        //        {
        //            targetSamplingRate = 16000;
        //        }

        //        double stepSize = (double)numStepSize.Value;
        //        int lpfCutoff = (int)numLpfCutoff.Value;

        //        try
        //        {
        //            btnRunCompression.Enabled = false;
        //            this.Cursor = Cursors.WaitCursor;

        //            ExecuteDmCompressionToMemory(_inputFilePath, targetSamplingRate, stepSize, lpfCutoff);

        //            long originalSize = new FileInfo(_inputFilePath).Length;
        //            long compressedSize = _copied_audio.Length;
        //            double ratio = (double)originalSize / compressedSize;

        //            string originalSizeFormatted = FormatBytes(originalSize);
        //            string compressedSizeFormatted = FormatBytes(compressedSize);

        //            MessageBox.Show($"Compressed to Memory using DM!\n\n" +
        //                            $"Original Size: {originalSizeFormatted}\n" +
        //                            $"In-Memory Size: {compressedSizeFormatted}\n" +
        //                            $"Compression Ratio: {ratio:F2}x",
        //                            "Metrics Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Compression failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        finally
        //        {
        //            btnRunCompression.Enabled = true;
        //            this.Cursor = Cursors.Default;
        //        }
        //    }
        //    else if (selectedAlgorithm == "Adaptive Delta Modulation")
        //    {
        //        string samplingRateText = cmbSamplingRate.SelectedItem != null ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
        //        if (!int.TryParse(samplingRateText, out int targetSamplingRate))
        //        {
        //            targetSamplingRate = 16000;
        //        }

        //        double initStepSize = (double)numInitStepSize.Value;
        //        double adaptationFactor = (double)numAdaptationFactor.Value;
        //        double maxStepSize = (double)numMaxStepSize.Value;
        //        int historyBits = (int)numHistoryBits.Value;
        //        int lpfCutoff = (int)numLpfCutoff.Value;

        //        try
        //        {
        //            btnRunCompression.Enabled = false;
        //            this.Cursor = Cursors.WaitCursor;

        //            ExecuteAdmCompressionToMemory(_inputFilePath, targetSamplingRate, initStepSize, adaptationFactor, maxStepSize, historyBits, lpfCutoff);

        //            long originalSize = new FileInfo(_inputFilePath).Length;
        //            long compressedSize = _copied_audio.Length;
        //            double ratio = (double)originalSize / compressedSize;

        //            string originalSizeFormatted = FormatBytes(originalSize);
        //            string compressedSizeFormatted = FormatBytes(compressedSize);

        //            MessageBox.Show($"Compressed to Memory using ADM!\n\n" +
        //                            $"Original Size: {originalSizeFormatted}\n" +
        //                            $"In-Memory Size: {compressedSizeFormatted}\n" +
        //                            $"Compression Ratio: {ratio:F2}x",
        //                            "Metrics Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Compression failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        finally
        //        {
        //            btnRunCompression.Enabled = true;
        //            this.Cursor = Cursors.Default;
        //        }

        //    }
        //    else if (selectedAlgorithm == "Adaptive Predictive")
        //    {
        //        try
        //        {
        //            // 1. التأكد من أن الأدوات ليست فارغة قبل القراءة
        //            if (cmbSampleRate.SelectedItem == null || numLevels == null || numPredictionOrder == null)
        //            {
        //                MessageBox.Show("خطأ: لم يتم تهيئة الأدوات بشكل صحيح.");
        //                return;
        //            }

        //            // 2. قراءة القيم
        //            int sampleRate = int.Parse(cmbSampleRate.SelectedItem.ToString());
        //            int levels = (int)numLevels.Value;
        //            double stepSize = (double)numStep.Value;
        //            // استبدلي سطر الـ mode في btnRunCompression_Click بهذا:
        //            string selectedModeText = cmbMode.SelectedItem.ToString();
        //            CompressionEngine.PredictionMode mode = (CompressionEngine.PredictionMode)Enum.Parse(typeof(CompressionEngine.PredictionMode), selectedModeText); bool isMono = chkIsMono.Checked;
        //            int predictorOrder = (int)numPredictionOrder.Value;

        //            btnRunCompression.Enabled = false;
        //            this.Cursor = Cursors.WaitCursor;

        //            // 3. تنفيذ الضغط
        //            var result = ExecuteAdaptivePredictiveCompression(_inputFilePath, sampleRate, levels, mode, stepSize, isMono, predictorOrder);

        //            // 4. معالجة البيانات والـ Header
        //            long compressedSize = 0;
        //            using (MemoryStream ms = new MemoryStream())
        //            using (BinaryWriter writer = new BinaryWriter(ms))
        //            {
        //                writer.Write(levels);
        //                writer.Write((int)mode);
        //                writer.Write(stepSize);
        //                writer.Write(sampleRate);
        //                writer.Write(result.CompressedData.Length);
        //                writer.Write(result.CompressedData);
        //                _copied_audio = ms.ToArray();
        //                compressedSize = ms.Length; // الحجم الكلي مع الـ Header
        //            }

        //            // 5. الحسابات
        //            long originalSize = new FileInfo(_inputFilePath).Length;
        //            long rawPcmSize = (long)result.TotalSamples * 2;
        //            double realRatio = (double)rawPcmSize / compressedSize;

        //            // 6. ظهور الرسالة المطلوبة
        //            MessageBox.Show($"حجم الصوت الخام (Raw PCM): {rawPcmSize} بايت\n" +
        //                            $"الحجم النهائي (مع الـ Header): {compressedSize} بايت\n" +
        //                            $"نسبة الضغط الفعلية: {realRatio:F2}x", "تحليل الضغط");
        //        }
        //        catch (Exception ex)
        //        {
        //            // إذا حدث أي خطأ (مثل قسمة على صفر أو ملف تالف)، ستظهر هذه الرسالة وتخبركِ بالسبب
        //            MessageBox.Show($"حدث خطأ أثناء الضغط: \n{ex.Message}\n{ex.StackTrace}", "خطأ في الضغط");
        //        }
        //        finally
        //        {
        //            btnRunCompression.Enabled = true;
        //            this.Cursor = Cursors.Default;
        //        }
        //    }
        //}


        private CompressionInterface GetSelectedAlgorithm()
        {
            switch (cmbAlgorithmType.SelectedItem?.ToString())
            {
                case "DPCM": return new DpcmAlgorithm();
                case "Mu-Law": return new MuLawAlgorithm();
                case "A-Law": return new ALawAlgorithm();
                case "Delta Modulation": return new DeltaModulationAlgorithm();
                case "Adaptive Delta Modulation": return new AdaptiveDeltaModulationAlgorithm();
                //case "Adaptive Predictive": return new AdaptivePredictiveAlgorithm();
                default: return null;
            }
        }

        //private async void btnRunCompression_Click(object sender, EventArgs e)
        //{
        //    string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();
        //    if (string.IsNullOrEmpty(selectedAlgorithm)) return;

        //    try
        //    {
        //        btnRunCompression.Enabled = false;
        //        btnRunDecompression.Enabled = false;
        //        this.Cursor = Cursors.WaitCursor;

        //        ResetProgressUI();
        //        _chartTimer.Start();

        //        // ===== DPCM — يستخدم الـ interface =====
        //        if (selectedAlgorithm == "DPCM")
        //        {
        //            CompressionInterface algorithm = new DpcmAlgorithm();

        //            string samplingRateText = cmbSamplingRate.SelectedItem != null
        //                ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
        //            if (!int.TryParse(samplingRateText, out int targetSampleRate))
        //                targetSampleRate = 16000;

        //            var parameters = new Dictionary<string, object>
        //            {
        //                ["bits"] = (int)numQuantBits.Value,
        //                ["predictorType"] = cmbPredictorType.SelectedIndex
        //            };

        //            float[] samples;
        //            using (var reader = new AudioFileReader(_inputFilePath))
        //            {
        //                var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
        //                var sampleProvider = resampler.ToSampleProvider();
        //                int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
        //                float[] buffer = new float[estimatedSamples + targetSampleRate];
        //                int samplesRead = sampleProvider.Read(buffer, 0, buffer.Length);
        //                samples = new float[samplesRead];
        //                Array.Copy(buffer, samples, samplesRead);
        //                parameters["totalSamples"] = samplesRead;
        //            }

        //            byte[] result = await Task.Run(() =>
        //                algorithm.Compress(samples, targetSampleRate, parameters,
        //                    (percent, samplesProcessed, bytesWritten) =>
        //                    {
        //                        UpdateProgressUI(percent, samplesProcessed, bytesWritten);
        //                        //System.Threading.Thread.Sleep(50);
        //                    })
        //            );

        //            _copied_audio = result;
        //            _compressedMetadata = new DpcmMetadata
        //            {
        //                SampleRate = targetSampleRate,
        //                Bits = (byte)(int)parameters["bits"],
        //                TotalSamples = (int)parameters["totalSamples"]
        //            };
        //        }

        //        // ===== Mu-Law / A-Law =====
        //        else if (selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
        //        {
        //            string samplingRateText = cmbSamplingRate.SelectedItem != null
        //                ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
        //            if (!int.TryParse(samplingRateText, out int targetSamplingRate))
        //                targetSamplingRate = 8000;

        //            int bitDepth = (int)numQuantBits.Value;
        //            bool useMono = cmbChannels == null || cmbChannels.SelectedIndex == 0;

        //            //await Task.Run(() =>
        //            //    ExecuteCompandingCompressionToMemory(
        //            //        _inputFilePath, selectedAlgorithm, targetSamplingRate, bitDepth, useMono));
        //            await Task.Run(() =>
        //            ExecuteCompandingCompressionToMemory(
        //                _inputFilePath, selectedAlgorithm, targetSamplingRate, bitDepth, useMono,
        //                (percent, samplesProcessed, bytesWritten) =>
        //                    UpdateProgressUI(percent, samplesProcessed, bytesWritten)));
        //        }

        //        // ===== Delta Modulation =====
        //        else if (selectedAlgorithm == "Delta Modulation")
        //        {
        //            string samplingRateText = cmbSamplingRate.SelectedItem != null
        //                ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
        //            if (!int.TryParse(samplingRateText, out int targetSamplingRate))
        //                targetSamplingRate = 16000;

        //            double stepSize = (double)numStepSize.Value;
        //            int lpfCutoff = (int)numLpfCutoff.Value;

        //            //await Task.Run(() =>
        //            //    ExecuteDmCompressionToMemory(
        //            //        _inputFilePath, targetSamplingRate, stepSize, lpfCutoff));
        //            await Task.Run(() =>
        //            ExecuteDmCompressionToMemory(
        //                _inputFilePath, targetSamplingRate, stepSize, lpfCutoff,
        //                (percent, samplesProcessed, bytesWritten) =>
        //                    UpdateProgressUI(percent, samplesProcessed, bytesWritten)));
        //        }

        //        // ===== Adaptive Delta Modulation =====
        //        else if (selectedAlgorithm == "Adaptive Delta Modulation")
        //        {
        //            string samplingRateText = cmbSamplingRate.SelectedItem != null
        //                ? cmbSamplingRate.SelectedItem.ToString() : cmbSamplingRate.Text;
        //            if (!int.TryParse(samplingRateText, out int targetSamplingRate))
        //                targetSamplingRate = 16000;

        //            double initStepSize = (double)numInitStepSize.Value;
        //            double adaptationFactor = (double)numAdaptationFactor.Value;
        //            double maxStepSize = (double)numMaxStepSize.Value;
        //            int historyBits = (int)numHistoryBits.Value;
        //            int lpfCutoff = (int)numLpfCutoff.Value;

        //            //await Task.Run(() =>
        //            //    ExecuteAdmCompressionToMemory(
        //            //        _inputFilePath, targetSamplingRate, initStepSize,
        //            //        adaptationFactor, maxStepSize, historyBits, lpfCutoff));
        //            await Task.Run(() =>
        //            ExecuteAdmCompressionToMemory(
        //                _inputFilePath, targetSamplingRate, initStepSize,
        //                adaptationFactor, maxStepSize, historyBits, lpfCutoff,
        //                (percent, samplesProcessed, bytesWritten) =>
        //                    UpdateProgressUI(percent, samplesProcessed, bytesWritten)));
        //        }

        //        // ===== Adaptive Predictive =====
        //        else if (selectedAlgorithm == "Adaptive Predictive")
        //        {
        //            if (cmbSampleRate.SelectedItem == null || numLevels == null || numPredictionOrder == null)
        //            {
        //                MessageBox.Show("خطأ: لم يتم تهيئة الأدوات بشكل صحيح.");
        //                return;
        //            }

        //            int sampleRate = int.Parse(cmbSampleRate.SelectedItem.ToString());
        //            int levels = (int)numLevels.Value;
        //            double stepSize = (double)numStep.Value;
        //            string selectedModeText = cmbMode.SelectedItem.ToString();
        //            CompressionEngine.PredictionMode mode = (CompressionEngine.PredictionMode)
        //                Enum.Parse(typeof(CompressionEngine.PredictionMode), selectedModeText);
        //            bool isMono = chkIsMono.Checked;
        //            int predictorOrder = (int)numPredictionOrder.Value;

        //            //await Task.Run(() =>
        //            //{
        //            //    var result = ExecuteAdaptivePredictiveCompression(
        //            //        _inputFilePath, sampleRate, levels, mode, stepSize, isMono, predictorOrder);

        //            //    using (MemoryStream ms = new MemoryStream())
        //            //    using (BinaryWriter writer = new BinaryWriter(ms))
        //            //    {
        //            //        writer.Write(levels);
        //            //        writer.Write((int)mode);
        //            //        writer.Write(stepSize);
        //            //        writer.Write(sampleRate);
        //            //        writer.Write(result.CompressedData.Length);
        //            //        writer.Write(result.CompressedData);
        //            //        _copied_audio = ms.ToArray();
        //            //    }
        //            //});

        //            await Task.Run(() =>
        //            {
        //                var result = ExecuteAdaptivePredictiveCompression(
        //                    _inputFilePath, sampleRate, levels, mode, stepSize, isMono, predictorOrder);

        //                using (MemoryStream ms = new MemoryStream())
        //                using (BinaryWriter writer = new BinaryWriter(ms))
        //                {
        //                    writer.Write(levels);
        //                    writer.Write((int)mode);
        //                    writer.Write(stepSize);
        //                    writer.Write(sampleRate);
        //                    writer.Write(result.CompressedData.Length);
        //                    writer.Write(result.CompressedData);
        //                    _copied_audio = ms.ToArray();

        //                    long apOriginalSize = new FileInfo(_inputFilePath).Length;
        //                    float apRatio = apOriginalSize > 0 ? (float)ms.Length / apOriginalSize * 100f : 0f;
        //                    double apElapsed = (DateTime.Now - _compressionStartTime).TotalSeconds;
        //                    float apSpeed = apElapsed > 0 ? (float)(result.TotalSamples / apElapsed) : 0f;

        //                    _ratioHistory.Add(apRatio);
        //                    _speedHistory.Add(apSpeed);

        //                    UpdateProgressUI(99, result.TotalSamples, ms.Length);
        //                }
        //            });
        //        }

        //        // ===== نهاية الضغط =====
        //        _chartTimer.Stop();
        //        progressBar.Value = 100;
        //        lblProgressPercent.Text = "100% — Done!";

        //        if (_ratioHistory.Count > 0)
        //        {
        //            lblChartRatio.Text = $"Compression Ratio: {_ratioHistory[_ratioHistory.Count - 1]:F2}%";
        //            DrawChart(chartCompressRatio, _ratioHistory, Color.Blue, "Ratio %", 100f);
        //        }
        //        if (_speedHistory.Count > 0)
        //        {
        //            float finalSpeed = _speedHistory[_speedHistory.Count - 1];
        //            lblChartSpeed.Text = finalSpeed >= 1000000
        //                ? $"Processing Speed: {finalSpeed / 1000000:F2}M samples/sec"
        //                : finalSpeed >= 1000
        //                    ? $"Processing Speed: {finalSpeed / 1000:F1}K samples/sec"
        //                    : $"Processing Speed: {finalSpeed:F0} samples/sec";
        //            DrawChart(chartSpeed, _speedHistory, Color.Green, "Samples/sec", _speedHistory.Max());
        //        }

        //        long originalSize = new FileInfo(_inputFilePath).Length;
        //        long compressedSize = _copied_audio.Length;
        //        double ratio = (double)originalSize / compressedSize;

        //        MessageBox.Show($"Compressed to Memory!\n\n" +
        //                        $"Original Size: {FormatBytes(originalSize)}\n" +
        //                        $"In-Memory Size: {FormatBytes(compressedSize)}\n" +
        //                        $"Compression Ratio: {ratio:F2}x",
        //                        "Metrics Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Compression failed: {ex.Message}", "Error",
        //            MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        _chartTimer.Stop();
        //        btnRunCompression.Enabled = true;
        //        btnRunDecompression.Enabled = true;
        //        this.Cursor = Cursors.Default;
        //    }
        //}

        private async void btnRunCompression_Click(object sender, EventArgs e)
        {
            string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedAlgorithm)) return;

            try
            {
                btnRunCompression.Enabled = false;
                btnRunDecompression.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                ResetProgressUI();
                _cancellationTokenSource = new CancellationTokenSource();
                btnCancelCompression.Enabled = true;
                _chartTimer.Start();

                //  الخوارزميات   interface 
                if (selectedAlgorithm != "Adaptive Predictive")
                {
                    CompressionInterface algorithm = GetSelectedAlgorithm();
                    if (algorithm == null)
                    {
                        MessageBox.Show("Please select a valid algorithm.", "Warning",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //  parameters حسب الخوارزمية
                    var parameters = new Dictionary<string, object>();
                    float[] samples;
                    int targetSampleRate = 16000;

                    if (selectedAlgorithm == "DPCM")
                    {
                        string rateText = cmbSamplingRate.SelectedItem?.ToString() ?? cmbSamplingRate.Text;
                        int.TryParse(rateText, out targetSampleRate);
                        parameters["bits"] = (int)numQuantBits.Value;
                        parameters["predictorType"] = cmbPredictorType.SelectedIndex;
                    }
                    else if (selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
                    {
                        string rateText = cmbSamplingRate.SelectedItem?.ToString() ?? cmbSamplingRate.Text;
                        if (!int.TryParse(rateText, out targetSampleRate)) targetSampleRate = 8000;
                        parameters["bitDepth"] = (int)numQuantBits.Value;
                        bool useMono = cmbChannels == null || cmbChannels.SelectedIndex == 0;
                        parameters["channels"] = useMono ? 1 : 0; // 0 = same as original
                    }
                    else if (selectedAlgorithm == "Delta Modulation")
                    {
                        string rateText = cmbSamplingRate.SelectedItem?.ToString() ?? cmbSamplingRate.Text;
                        int.TryParse(rateText, out targetSampleRate);
                        parameters["stepSize"] = (float)(double)numStepSize.Value;
                        parameters["lpfCutoff"] = (int)numLpfCutoff.Value;
                    }
                    else if (selectedAlgorithm == "Adaptive Delta Modulation")
                    {
                        string rateText = cmbSamplingRate.SelectedItem?.ToString() ?? cmbSamplingRate.Text;
                        int.TryParse(rateText, out targetSampleRate);
                        parameters["initStepSize"] = (float)(double)numInitStepSize.Value;
                        parameters["adaptationFactor"] = (float)(double)numAdaptationFactor.Value;
                        parameters["maxStepSize"] = (float)(double)numMaxStepSize.Value;
                        parameters["historyBits"] = (int)numHistoryBits.Value;
                        parameters["lpfCutoff"] = (int)numLpfCutoff.Value;
                    }

                    // قراءة الصوت
                    using (var reader = new AudioFileReader(_inputFilePath))
                    {
                        int channels = 1;
                        if ((selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
                            && parameters.ContainsKey("channels") && (int)parameters["channels"] == 0)
                            channels = reader.WaveFormat.Channels;

                        parameters["channels"] = channels;

                        var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, channels));
                        var sampleProvider = resampler.ToSampleProvider();
                        int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate * channels);
                        float[] buffer = new float[estimatedSamples + targetSampleRate];
                        int samplesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                        samples = new float[samplesRead];
                        Array.Copy(buffer, samples, samplesRead);
                        parameters["totalSamples"] = samplesRead;
                    }

                    // تشغيل الضغط
                    //variables for the compression report
                    _compressionStartTime = DateTime.Now;
                    _lastUsedAlgorithmName = cmbAlgorithmType.SelectedItem?.ToString() ?? "Unknown";

                    byte[] result = await Task.Run(() =>
                    algorithm.Compress(samples, targetSampleRate, parameters,
                        (percent, samplesProcessed, bytesWritten) =>
                            UpdateProgressUI(percent, samplesProcessed, bytesWritten),
                        _cancellationTokenSource.Token));
                    _compressionTimeTaken = DateTime.Now - _compressionStartTime;
                    _copied_audio = result;

                    PlayAudiobtn.Enabled = false;
                    btnRunCompression.Enabled = false;

                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        progressBar.Value = 0;
                        lblProgressPercent.Text = "Cancelled";
                        lblChartRatio.Text = "Compression Ratio: —";
                        lblChartSpeed.Text = "Processing Speed: —";
                        _ratioHistory.Clear();
                        _speedHistory.Clear();
                        DrawChart(chartCompressRatio, _ratioHistory, Color.Blue, "Ratio %", 100f);
                        DrawChart(chartSpeed, _speedHistory, Color.Green, "Samples/sec", 1f);
                        MessageBox.Show("Compression cancelled.", "Cancelled",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // حفظ  metadata
                    if (selectedAlgorithm == "DPCM")
                    {
                        _compressedMetadata = new DpcmMetadata
                        {
                            SampleRate = targetSampleRate,
                            Bits = (byte)(int)parameters["bits"],
                            TotalSamples = (int)parameters["totalSamples"]
                        };
                    }
                    else if (selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
                    {
                        _compandingMetadata = new CompandingMetadata
                        {
                            Algorithm = selectedAlgorithm,
                            OriginalExtension = Path.GetExtension(_inputFilePath),
                            SampleRate = targetSampleRate,
                            BitDepth = (byte)(int)parameters["bitDepth"],
                            Channels = (int)parameters["channels"],
                            TotalSamples = (int)parameters["totalSamples"]
                        };
                    }
                    else if (selectedAlgorithm == "Delta Modulation")
                    {
                        _dmMetadata = new DmMetadata
                        {
                            SampleRate = targetSampleRate,
                            TotalSamples = (int)parameters["totalSamples"],
                            StepSize = (float)parameters["stepSize"],
                            LpfCutoff = (int)parameters["lpfCutoff"],
                            OriginalExtension = Path.GetExtension(_inputFilePath)
                        };
                    }
                    else if (selectedAlgorithm == "Adaptive Delta Modulation")
                    {
                        _admMetadata = new AdmMetadata
                        {
                            SampleRate = targetSampleRate,
                            TotalSamples = (int)parameters["totalSamples"],
                            InitStepSize = (float)parameters["initStepSize"],
                            AdaptationFactor = (float)parameters["adaptationFactor"],
                            MaxStepSize = (float)parameters["maxStepSize"],
                            HistoryBits = (int)parameters["historyBits"],
                            LpfCutoff = (int)parameters["lpfCutoff"],
                            OriginalExtension = Path.GetExtension(_inputFilePath)
                        };
                    }
                }

                //  Adaptive Predictive بدون interface حاليا
                else
                {
                    if (cmbSampleRate.SelectedItem == null || numLevels == null || numPredictionOrder == null)
                    {
                        MessageBox.Show("خطأ: لم يتم تهيئة الأدوات بشكل صحيح.");
                        return;
                    }

                    int sampleRate = int.Parse(cmbSampleRate.SelectedItem.ToString());
                    int levels = (int)numLevels.Value;
                    double stepSize = (double)numStep.Value;
                    string selectedModeText = cmbMode.SelectedItem.ToString();
                    CompressionEngine.PredictionMode mode = (CompressionEngine.PredictionMode)
                        Enum.Parse(typeof(CompressionEngine.PredictionMode), selectedModeText);
                    bool isMono = chkIsMono.Checked;
                    int predictorOrder = (int)numPredictionOrder.Value;
                    //variables for the compression report
                    _compressionStartTime = DateTime.Now;
                    _lastUsedAlgorithmName = cmbAlgorithmType.SelectedItem?.ToString() ?? "Unknown";

                    await Task.Run(() =>
                    {
                        var result = ExecuteAdaptivePredictiveCompression(
                            _inputFilePath, sampleRate, levels, mode, stepSize, isMono, predictorOrder);
                        _compressionTimeTaken = DateTime.Now - _compressionStartTime;

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

                            long apOriginalSize = new FileInfo(_inputFilePath).Length;
                            float apRatio = apOriginalSize > 0 ? (float)ms.Length / apOriginalSize * 100f : 0f;
                            double apElapsed = (DateTime.Now - _compressionStartTime).TotalSeconds;
                            float apSpeed = apElapsed > 0 ? (float)(result.TotalSamples / apElapsed) : 0f;

                            _ratioHistory.Add(apRatio);
                            _speedHistory.Add(apSpeed);
                            UpdateProgressUI(99, result.TotalSamples, ms.Length);
                        }
                    });
                }

                // نهاية الضغط 
                _chartTimer.Stop();
                progressBar.Value = 100;
                lblProgressPercent.Text = "100% — Done!";

                if (_ratioHistory.Count > 0)
                {
                    lblChartRatio.Text = $"Compression Ratio: {_ratioHistory[_ratioHistory.Count - 1]:F2}%";
                    DrawChart(chartCompressRatio, _ratioHistory, Color.Blue, "Ratio %", 100f);
                }
                if (_speedHistory.Count > 0)
                {
                    float finalSpeed = _speedHistory[_speedHistory.Count - 1];
                    lblChartSpeed.Text = finalSpeed >= 1000000
                        ? $"Processing Speed: {finalSpeed / 1000000:F2}M samples/sec"
                        : finalSpeed >= 1000
                            ? $"Processing Speed: {finalSpeed / 1000:F1}K samples/sec"
                            : $"Processing Speed: {finalSpeed:F0} samples/sec";
                    DrawChart(chartSpeed, _speedHistory, Color.Green, "Samples/sec", _speedHistory.Max());
                }

                long originalSize = new FileInfo(_inputFilePath).Length;
                long compressedSize = _copied_audio.Length;
                double ratio = (double)originalSize / compressedSize;

              
              
            }


            catch (Exception ex)
            {
                MessageBox.Show($"Compression failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //finally
            //{
            //    _chartTimer.Stop();
            //    btnCancelCompression.Enabled = false;
            //    btnRunCompression.Enabled = true;
            //    btnRunDecompression.Enabled = true;
            //    this.Cursor = Cursors.Default;
            //}
            finally
            {
                _chartTimer.Stop();
                btnCancelCompression.Enabled = false;
                btnRunDecompression.Enabled = true;
                this.Cursor = Cursors.Default;
                // لا تعيدي تفعيل btnRunCompression هون
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

        //private void ExecuteCompandingCompressionToMemory(string inputPath, string algorithm, int targetSampleRate, int bitDepth, bool useMono)
        //{
        //    using (var reader = new AudioFileReader(inputPath))
        //    using (var resampler = new MediaFoundationResampler(
        //        reader,
        //        new WaveFormat(targetSampleRate, 16, useMono ? 1 : reader.WaveFormat.Channels)))
        //    {
        //        var sampleProvider = resampler.ToSampleProvider();
        //        float[] samples = ReadAllSamples(sampleProvider);

        //        if (samples.Length == 0)
        //        {
        //            throw new InvalidOperationException("No audio samples were read from the copied file.");
        //        }

        //        _originalSamples = new short[samples.Length];
        //        byte[] compressedBytes = new byte[samples.Length];

        //        for (int i = 0; i < samples.Length; i++)
        //        {
        //            _originalSamples[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, samples[i] * short.MaxValue));

        //            double companded =
        //                algorithm == "A-Law"
        //                    ? ApplyALaw(samples[i])
        //                    : ApplyMuLaw(samples[i]);

        //            // The uploaded logic returns one 8-bit WAV byte for each quantized companded sample.
        //            compressedBytes[i] = QuantizeToByte(companded, bitDepth);
        //        }

        //        _copied_audio = CreateCompressedWaveBytes(
        //            compressedBytes,
        //            sampleProvider.WaveFormat.SampleRate,
        //            sampleProvider.WaveFormat.Channels);

        //        _compandingMetadata = new CompandingMetadata
        //        {
        //            Algorithm = algorithm,
        //            OriginalExtension = Path.GetExtension(inputPath),
        //            SampleRate = sampleProvider.WaveFormat.SampleRate,
        //            BitDepth = (byte)bitDepth,
        //            Channels = sampleProvider.WaveFormat.Channels,
        //            TotalSamples = samples.Length
        //        };
        //    }
        //}

        //    private void ExecuteCompandingCompressionToMemory(string inputPath, string algorithm,
        //int targetSampleRate, int bitDepth, bool useMono,
        //Action<int, long, long> reportProgress = null)
        //    {
        //        using (var reader = new AudioFileReader(inputPath))
        //        using (var resampler = new MediaFoundationResampler(
        //            reader,
        //            new WaveFormat(targetSampleRate, 16, useMono ? 1 : reader.WaveFormat.Channels)))
        //        {
        //            var sampleProvider = resampler.ToSampleProvider();
        //            float[] samples = ReadAllSamples(sampleProvider);

        //            if (samples.Length == 0)
        //                throw new InvalidOperationException("No audio samples were read from the copied file.");

        //            _originalSamples = new short[samples.Length];
        //            byte[] compressedBytes = new byte[samples.Length];

        //            for (int i = 0; i < samples.Length; i++)
        //            {
        //                _originalSamples[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, samples[i] * short.MaxValue));

        //                double companded = algorithm == "A-Law"
        //                    ? ApplyALaw(samples[i])
        //                    : ApplyMuLaw(samples[i]);

        //                compressedBytes[i] = QuantizeToByte(companded, bitDepth);

        //                if (i % 1000 == 0 && reportProgress != null)
        //                {
        //                    int percent = (int)((double)i / samples.Length * 100);
        //                    reportProgress(percent, i, i);
        //                }
        //            }

        //            _copied_audio = CreateCompressedWaveBytes(
        //                compressedBytes,
        //                sampleProvider.WaveFormat.SampleRate,
        //                sampleProvider.WaveFormat.Channels);

        //            _compandingMetadata = new CompandingMetadata
        //            {
        //                Algorithm = algorithm,
        //                OriginalExtension = Path.GetExtension(inputPath),
        //                SampleRate = sampleProvider.WaveFormat.SampleRate,
        //                BitDepth = (byte)bitDepth,
        //                Channels = sampleProvider.WaveFormat.Channels,
        //                TotalSamples = samples.Length
        //            };
        //        }
        //    }

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

        //private double ApplyMuLaw(double x)
        //{
        //    double magnitude = Math.Abs(x);

        //    return Math.Sign(x) *
        //           Math.Log(1.0 + MuLawMu * magnitude) /
        //           Math.Log(1.0 + MuLawMu);
        //}

        private double InverseMuLaw(double y)
        {
            double magnitude = Math.Abs(y);

            return Math.Sign(y) *
                   (Math.Pow(1.0 + MuLawMu, magnitude) - 1.0) /
                   MuLawMu;
        }

        //private double ApplyALaw(double x)
        //{
        //    double magnitude = Math.Abs(x);
        //    double denominator = 1.0 + Math.Log(ALawA);
        //    double companded;

        //    if (magnitude < 1.0 / ALawA)
        //    {
        //        companded = (ALawA * magnitude) / denominator;
        //    }
        //    else
        //    {
        //        companded = (1.0 + Math.Log(ALawA * magnitude)) / denominator;
        //    }

        //    return Math.Sign(x) * companded;
        //}

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

        //private byte QuantizeToByte(double sample, int bitDepth)
        //{
        //    int usableBits = Math.Max(2, Math.Min(8, bitDepth));
        //    int levels = 1 << usableBits;
        //    double normalized = (Clamp(sample, -1.0, 1.0) + 1.0) / 2.0;
        //    int index = (int)Math.Round(normalized * (levels - 1));
        //    double quantized = index / (double)(levels - 1);

        //    return (byte)Math.Round(quantized * 255);
        //}

        private double DequantizeByte(byte sample)
        {
            return (sample / 255.0) * 2.0 - 1.0;
        }

        //private float[] ReadAllSamples(ISampleProvider provider)
        //{
        //    List<float> samples = new List<float>();
        //    float[] buffer = new float[provider.WaveFormat.SampleRate * provider.WaveFormat.Channels];
        //    int read;

        //    while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
        //    {
        //        for (int i = 0; i < read; i++)
        //        {
        //            samples.Add(buffer[i]);
        //        }
        //    }

        //    return samples.ToArray();
        //}

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

        //private byte[] CreateCompressedWaveBytes(byte[] compressedBytes, int sampleRate, int channels)
        //{
        //    WaveFormat format = new WaveFormat(sampleRate, 8, channels);

        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        using (WaveFileWriter writer = new WaveFileWriter(stream, format))
        //        {
        //            // Compression returns a WAV file copy in memory instead of writing over the uploaded file.
        //            writer.Write(compressedBytes, 0, compressedBytes.Length);
        //        }

        //        return stream.ToArray();
        //    }
        //}

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

        //private void ExecuteDpcmCompressionToMemory(string inputPath, int targetSampleRate, int bits, int predictorType)
        //{
        //    using (var reader = new AudioFileReader(inputPath))
        //    {
        //        var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
        //        var sampleProvider = resampler.ToSampleProvider();

        //        int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
        //        float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
        //        int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);

        //        short[] pcmSamples = new short[samplesRead];
        //        for (int i = 0; i < samplesRead; i++)
        //        {
        //            pcmSamples[i] = (short)Math.Max(-32768, Math.Min(32767, floatBuffer[i] * 32767f));
        //        }

        //        using (MemoryStream ms = new MemoryStream())
        //        using (BinaryWriter writer = new BinaryWriter(ms))
        //        {
        //            short predictedValue = 0;
        //            short prevSample1 = 0;
        //            short prevSample2 = 0;

        //            int maxLevels = (int)Math.Pow(2, bits);
        //            int minQuantizedLevel = -(maxLevels / 2);
        //            int maxQuantizedLevel = (maxLevels / 2) - 1;
        //            short stepSize = (short)Math.Max(1, 32768 / (maxLevels * 2));

        //            for (int n = 0; n < samplesRead; n++)
        //            {
        //                if (predictorType == 0 || n < 2)
        //                {
        //                    predictedValue = prevSample1;
        //                }
        //                else
        //                {
        //                    predictedValue = (short)Math.Max(-32768, Math.Min(32767, (2 * prevSample1) - prevSample2));
        //                }

        //                int error = pcmSamples[n] - predictedValue;
        //                int quantizedErrorIndex = (int)Math.Round((double)error / stepSize);
        //                quantizedErrorIndex = Math.Max(minQuantizedLevel, Math.Min(maxQuantizedLevel, quantizedErrorIndex));

        //                int reconstructedError = quantizedErrorIndex * stepSize;
        //                int reconstructedSample = predictedValue + reconstructedError;
        //                reconstructedSample = Math.Max(-32768, Math.Min(32767, reconstructedSample));

        //                writer.Write((short)quantizedErrorIndex);

        //                prevSample2 = prevSample1;
        //                prevSample1 = (short)reconstructedSample;
        //            }

        //            writer.Flush();
        //            _copied_audio = ms.ToArray();

        //            _compressedMetadata = new DpcmMetadata
        //            {
        //                SampleRate = targetSampleRate,
        //                Bits = (byte)bits,
        //                TotalSamples = samplesRead
        //            };
        //        }
        //    }
        //}


        //private void ExecuteDmCompressionToMemory(string inputPath, int targetSampleRate, double stepSize, int lpfCutoff)
        //{
        //    using (var reader = new AudioFileReader(inputPath))
        //    {
        //        var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
        //        var sampleProvider = resampler.ToSampleProvider();

        //        int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
        //        float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
        //        int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);
        //        _dmMetadata = new DmMetadata
        //        {
        //            SampleRate = targetSampleRate,
        //            TotalSamples = samplesRead,
        //            StepSize = (float)stepSize,
        //            LpfCutoff = lpfCutoff,
        //            OriginalExtension = Path.GetExtension(inputPath)
        //        };
        //        float[] audioSamples = new float[samplesRead];
        //        Array.Copy(floatBuffer, audioSamples, samplesRead);
        //        _originalSamples = new short[samplesRead];

        //        for (int i = 0; i < samplesRead; i++)
        //        {
        //            _originalSamples[i] = (short)Math.Max(
        //                short.MinValue,
        //                Math.Min(short.MaxValue, audioSamples[i] * 32767f));
        //        }
        //        using (MemoryStream ms = new MemoryStream())
        //        using (BinaryWriter writer = new BinaryWriter(ms))
        //        {
        //            float predictedValue = 0f;
        //            float step = (float)stepSize;

        //            byte currentByte = 0;
        //            int bitCounter = 0;

        //            for (int n = 0; n < samplesRead; n++)
        //            {
        //                int bit;
        //                if (audioSamples[n] >= predictedValue)
        //                {
        //                    bit = 1;
        //                    predictedValue += step; 
        //                }
        //                else
        //                {
        //                    bit = 0;
        //                    predictedValue -= step;
        //                }

        //                currentByte |= (byte)(bit << (7 - bitCounter));
        //                bitCounter++;

        //                if (bitCounter == 8)
        //                {
        //                    writer.Write(currentByte);
        //                    currentByte = 0;
        //                    bitCounter = 0;
        //                }
        //            }

        //            if (bitCounter > 0)
        //            {
        //                writer.Write(currentByte);
        //            }

        //            writer.Flush();
        //            _copied_audio = ms.ToArray();

        //        }
        //    }
        //}

        //    private void ExecuteDmCompressionToMemory(string inputPath, int targetSampleRate,
        //double stepSize, int lpfCutoff,
        //Action<int, long, long> reportProgress = null)
        //    {
        //        using (var reader = new AudioFileReader(inputPath))
        //        {
        //            var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
        //            var sampleProvider = resampler.ToSampleProvider();

        //            int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
        //            float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
        //            int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);

        //            _dmMetadata = new DmMetadata
        //            {
        //                SampleRate = targetSampleRate,
        //                TotalSamples = samplesRead,
        //                StepSize = (float)stepSize,
        //                LpfCutoff = lpfCutoff,
        //                OriginalExtension = Path.GetExtension(inputPath)
        //            };

        //            float[] audioSamples = new float[samplesRead];
        //            Array.Copy(floatBuffer, audioSamples, samplesRead);
        //            _originalSamples = new short[samplesRead];

        //            for (int i = 0; i < samplesRead; i++)
        //                _originalSamples[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, audioSamples[i] * 32767f));

        //            using (MemoryStream ms = new MemoryStream())
        //            using (BinaryWriter writer = new BinaryWriter(ms))
        //            {
        //                float predictedValue = 0f;
        //                float step = (float)stepSize;
        //                byte currentByte = 0;
        //                int bitCounter = 0;

        //                for (int n = 0; n < samplesRead; n++)
        //                {
        //                    int bit;
        //                    if (audioSamples[n] >= predictedValue)
        //                    {
        //                        bit = 1;
        //                        predictedValue += step;
        //                    }
        //                    else
        //                    {
        //                        bit = 0;
        //                        predictedValue -= step;
        //                    }

        //                    currentByte |= (byte)(bit << (7 - bitCounter));
        //                    bitCounter++;

        //                    if (bitCounter == 8)
        //                    {
        //                        writer.Write(currentByte);
        //                        currentByte = 0;
        //                        bitCounter = 0;
        //                    }

        //                    if (n % 1000 == 0 && reportProgress != null)
        //                    {
        //                        int percent = (int)((double)n / samplesRead * 100);
        //                        reportProgress(percent, n, ms.Length);
        //                    }
        //                }

        //                if (bitCounter > 0)
        //                    writer.Write(currentByte);

        //                writer.Flush();
        //                _copied_audio = ms.ToArray();
        //            }
        //        }
        //    }


        //private void ExecuteAdmCompressionToMemory(string inputPath, int targetSampleRate, double initStepSize, double adaptationFactor, double maxStepSize, int historyBits, int lpfCutoff)
        //{
        //    using (var reader = new AudioFileReader(inputPath))
        //    {
        //        var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
        //        var sampleProvider = resampler.ToSampleProvider();

        //        int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
        //        float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
        //        int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);
        //        _admMetadata = new AdmMetadata
        //        {
        //            SampleRate = targetSampleRate,
        //            TotalSamples = samplesRead,
        //            InitStepSize = (float)initStepSize,
        //            AdaptationFactor = (float)adaptationFactor,
        //            MaxStepSize = (float)maxStepSize,
        //            HistoryBits = historyBits,
        //            LpfCutoff = lpfCutoff,
        //            OriginalExtension = Path.GetExtension(inputPath)
        //        };
        //        float[] audioSamples = new float[samplesRead];
        //        Array.Copy(floatBuffer, audioSamples, samplesRead);
        //        _originalSamples = new short[samplesRead];

        //        for (int i = 0; i < samplesRead; i++)
        //        {
        //            _originalSamples[i] = (short)Math.Max(
        //                short.MinValue,
        //                Math.Min(short.MaxValue, audioSamples[i] * 32767f));
        //        }
        //        using (MemoryStream ms = new MemoryStream())
        //        using (BinaryWriter writer = new BinaryWriter(ms))
        //        {
        //            float predictedValue = 0f;
        //            float currentStepSize = (float)initStepSize;
        //            float minStep = (float)initStepSize;
        //            float maxStep = (float)maxStepSize;
        //            float K = (float)adaptationFactor;

        //            int[] historyPattern = new int[historyBits];

        //            byte currentByte = 0;
        //            int bitCounter = 0;

        //            for (int n = 0; n < samplesRead; n++)
        //            {
        //                int bit;
        //                if (audioSamples[n] >= predictedValue)
        //                {
        //                    bit = 1;
        //                    predictedValue += currentStepSize;
        //                }
        //                else
        //                {
        //                    bit = 0;
        //                    predictedValue -= currentStepSize;
        //                }

        //                for (int i = historyBits - 1; i > 0; i--)
        //                {
        //                    historyPattern[i] = historyPattern[i - 1];
        //                }
        //                historyPattern[0] = bit;

        //                if (n >= historyBits - 1)
        //                {
        //                    bool allSame = true;
        //                    bool alternating = true;

        //                    for (int i = 1; i < historyBits; i++)
        //                    {
        //                        if (historyPattern[i] != historyPattern[0]) allSame = false;
        //                        if (historyPattern[i] == historyPattern[i - 1]) alternating = false;
        //                    }

        //                    if (allSame)
        //                    {
        //                        currentStepSize = Math.Min(maxStep, currentStepSize * K);
        //                    }
        //                    else if (alternating)
        //                    {
        //                        currentStepSize = Math.Max(minStep, currentStepSize / K);
        //                    }
        //                }

        //                currentByte |= (byte)(bit << (7 - bitCounter));
        //                bitCounter++;

        //                if (bitCounter == 8)
        //                {
        //                    writer.Write(currentByte);
        //                    currentByte = 0;
        //                    bitCounter = 0;
        //                }
        //            }

        //            if (bitCounter > 0)
        //            {
        //                writer.Write(currentByte);
        //            }

        //            writer.Flush();
        //            _copied_audio = ms.ToArray();
        //        }
        //    }
        //}

        //    private void ExecuteAdmCompressionToMemory(string inputPath, int targetSampleRate,
        //double initStepSize, double adaptationFactor, double maxStepSize,
        //int historyBits, int lpfCutoff,
        //Action<int, long, long> reportProgress = null)
        //    {
        //        using (var reader = new AudioFileReader(inputPath))
        //        {
        //            var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, 16, 1));
        //            var sampleProvider = resampler.ToSampleProvider();

        //            int estimatedSamples = (int)(reader.TotalTime.TotalSeconds * targetSampleRate);
        //            float[] floatBuffer = new float[estimatedSamples + targetSampleRate];
        //            int samplesRead = sampleProvider.Read(floatBuffer, 0, floatBuffer.Length);

        //            _admMetadata = new AdmMetadata
        //            {
        //                SampleRate = targetSampleRate,
        //                TotalSamples = samplesRead,
        //                InitStepSize = (float)initStepSize,
        //                AdaptationFactor = (float)adaptationFactor,
        //                MaxStepSize = (float)maxStepSize,
        //                HistoryBits = historyBits,
        //                LpfCutoff = lpfCutoff,
        //                OriginalExtension = Path.GetExtension(inputPath)
        //            };

        //            float[] audioSamples = new float[samplesRead];
        //            Array.Copy(floatBuffer, audioSamples, samplesRead);
        //            _originalSamples = new short[samplesRead];

        //            for (int i = 0; i < samplesRead; i++)
        //                _originalSamples[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, audioSamples[i] * 32767f));

        //            using (MemoryStream ms = new MemoryStream())
        //            using (BinaryWriter writer = new BinaryWriter(ms))
        //            {
        //                float predictedValue = 0f;
        //                float currentStepSize = (float)initStepSize;
        //                float minStep = (float)initStepSize;
        //                float maxStep = (float)maxStepSize;
        //                float K = (float)adaptationFactor;
        //                int[] historyPattern = new int[historyBits];
        //                byte currentByte = 0;
        //                int bitCounter = 0;

        //                for (int n = 0; n < samplesRead; n++)
        //                {
        //                    int bit;
        //                    if (audioSamples[n] >= predictedValue)
        //                    {
        //                        bit = 1;
        //                        predictedValue += currentStepSize;
        //                    }
        //                    else
        //                    {
        //                        bit = 0;
        //                        predictedValue -= currentStepSize;
        //                    }

        //                    for (int i = historyBits - 1; i > 0; i--)
        //                        historyPattern[i] = historyPattern[i - 1];
        //                    historyPattern[0] = bit;

        //                    if (n >= historyBits - 1)
        //                    {
        //                        bool allSame = true;
        //                        bool alternating = true;

        //                        for (int i = 1; i < historyBits; i++)
        //                        {
        //                            if (historyPattern[i] != historyPattern[0]) allSame = false;
        //                            if (historyPattern[i] == historyPattern[i - 1]) alternating = false;
        //                        }

        //                        if (allSame)
        //                            currentStepSize = Math.Min(maxStep, currentStepSize * K);
        //                        else if (alternating)
        //                            currentStepSize = Math.Max(minStep, currentStepSize / K);
        //                    }

        //                    currentByte |= (byte)(bit << (7 - bitCounter));
        //                    bitCounter++;

        //                    if (bitCounter == 8)
        //                    {
        //                        writer.Write(currentByte);
        //                        currentByte = 0;
        //                        bitCounter = 0;
        //                    }

        //                    if (n % 1000 == 0 && reportProgress != null)
        //                    {
        //                        int percent = (int)((double)n / samplesRead * 100);
        //                        reportProgress(percent, n, ms.Length);
        //                    }
        //                }

        //                if (bitCounter > 0)
        //                    writer.Write(currentByte);

        //                writer.Flush();
        //                _copied_audio = ms.ToArray();
        //            }
        //        }
        //    }


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
                //else if (selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
                //{
                //    byte[] returnedFile = ExecuteCompandingDecompressionToMemory();
                //    double compandingMse = CalculateMSE();

                //    MessageBox.Show(
                //        $"Audio decompressed completely using {selectedAlgorithm}!\n\n" +
                //        $"Returned File: {_compandingMetadata.Algorithm.Replace("-", "").ToLower()}_decompressed.wav\n" +
                //        $"Returned Extension: .wav\n" +
                //        $"Returned Size: {FormatBytes(returnedFile.Length)}\n" +
                //        $"MSE: {compandingMse:F2}\n\n" +
                //        $"Status: Ready for playing or processing.",
                //        "Success",
                //        MessageBoxButtons.OK,
                //        MessageBoxIcon.Information);

                //    return;
                //}
                else if (selectedAlgorithm == "Mu-Law" || selectedAlgorithm == "A-Law")
                {
                    byte[] returnedFile = ExecuteCompandingDecompressionToMemory();
                    double compandingMse = CalculateMSE();

                    // _decompressedPcmBytes is now set inside ExecuteCompandingDecompressionToMemory
                    _isDecompressed = true;
                    DrawWaveformFromPcm(_decompressedPcmBytes, _compandingMetadata.SampleRate);

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

                else if (selectedAlgorithm == "Adaptive Predictive")
                {
                    if (_originalSamples == null)
                    {
                        MessageBox.Show("Original audio sampling weren't stored during compressed",
                                        "Data Missing",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);

                        btnRunDecompression.Enabled = true;
                        this.Cursor = Cursors.Default;
                        return;
                    }

                    CompressionEngine engine = new CompressionEngine();

                    short[] pcmResult = engine.Decompress(
                        _copied_audio,
                        (int)numLevels.Value,
                        (CompressionEngine.PredictionMode)Enum.Parse(typeof(CompressionEngine.PredictionMode), cmbMode.Text),
                        (double)numStep.Value,
                        _originalSamples.Length,
                        (int)numPredictionOrder.Value
                    );

                    _decompressedPcmBytes = new byte[pcmResult.Length * 2];
                    Buffer.BlockCopy(pcmResult, 0, _decompressedPcmBytes, 0, _decompressedPcmBytes.Length);

                }

                //else if (selectedAlgorithm == "Adaptive Predictive")
                //{
                //    if (_originalSamples == null)
                //    {
                //        MessageBox.Show("Original audio samples weren't stored during compression.",
                //            "Data Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //        btnRunDecompression.Enabled = true;
                //        this.Cursor = Cursors.Default;
                //        return;
                //    }

                //    CompressionEngine engine = new CompressionEngine();
                //    short[] pcmResult = engine.Decompress(
                //        _copied_audio,
                //        (int)numLevels.Value,
                //        (CompressionEngine.PredictionMode)Enum.Parse(
                //            typeof(CompressionEngine.PredictionMode), cmbMode.Text),
                //        (double)numStep.Value,
                //        _originalSamples.Length,
                //        (int)numPredictionOrder.Value
                //    );

                //    _decompressedPcmBytes = new byte[pcmResult.Length * 2];
                //    Buffer.BlockCopy(pcmResult, 0, _decompressedPcmBytes, 0, _decompressedPcmBytes.Length);

                //    // Get the sample rate from the UI (AP has no metadata class)
                //    int apSampleRate = 16000;
                //    if (cmbSampleRate?.SelectedItem != null)
                //        int.TryParse(cmbSampleRate.SelectedItem.ToString(), out apSampleRate);

                //    _isDecompressed = true;
                //    DrawWaveformFromPcm(_decompressedPcmBytes, apSampleRate);
                //}





                // after all the else if branches
                if (!_isDecompressed && _decompressedPcmBytes != null)
                {
                    _isDecompressed = true;
                    DrawWaveformFromPcm(_decompressedPcmBytes, GetDecompressedSampleRate());
                }

                long decompressedSize = _decompressedPcmBytes.Length;
                double mse = CalculateMSE();
               

            }

            catch (Exception ex)
            {
                MessageBox.Show($"Decompression failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //finally
            //{
            //    btnRunDecompression.Enabled = true;
            //    this.Cursor = Cursors.Default;
            //}
            finally
            {
                btnRunDecompression.Enabled = true;
                btnRunCompression.Enabled = true;
                PlayAudiobtn.Enabled = true;
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






        // REQUIREMENT 7
        private void UpdateProgressUI(int percent, long samplesProcessed, long bytesWritten)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateProgressUI(percent, samplesProcessed, bytesWritten)));
                return;
            }

            progressBar.Value = Math.Min(percent, 100);
            lblProgressPercent.Text = $"{percent}%";

            if (_originalFileSize > 0 && bytesWritten > 0)
            {
                float ratio = ((float)bytesWritten / _originalFileSize) * 100f;
                _ratioHistory.Add(ratio);
            }


            double elapsed = (DateTime.Now - _compressionStartTime).TotalSeconds;
            if (elapsed > 0)
            {
                float speed = (float)(samplesProcessed / elapsed);
                _speedHistory.Add(speed);
            }
        }

        private void DrawChart(PictureBox box, List<float> data, Color lineColor, string label, float maxValue)
        {
            int w = box.Width;
            int h = box.Height;
            Bitmap bmp = new Bitmap(w, h);

            int paddingLeft = 55;
            int paddingBottom = 30;
            int paddingTop = 15;
            int paddingRight = 15;

            int chartW = w - paddingLeft - paddingRight;
            int chartH = h - paddingBottom - paddingTop;

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                /// خلفية الـ chart
                g.Clear(Color.White);

                // خلفية منطقة الرسم
                using (SolidBrush chartBg = new SolidBrush(Color.FromArgb(245, 245, 245)))
                    g.FillRectangle(chartBg, paddingLeft, paddingTop, chartW, chartH);

                using (Font font = new Font("Consolas", 7.5f))
                using (Pen gridPen = new Pen(Color.FromArgb(210, 210, 210), 1))
                using (Pen tickPen = new Pen(Color.FromArgb(100, 100, 100), 1))
                using (Pen borderPen = new Pen(Color.FromArgb(150, 150, 150), 1))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
                {
                    // border
                    g.DrawRectangle(borderPen, paddingLeft, paddingTop, chartW, chartH);

                    for (int i = 0; i <= 5; i++)
                    {
                        float val = maxValue * i / 5f;
                        int y = paddingTop + chartH - (int)(chartH * i / 5f);

                        g.DrawLine(gridPen, paddingLeft + 1, y, paddingLeft + chartW - 1, y);

                        // ticks على اليسار
                        g.DrawLine(tickPen, paddingLeft - 5, y, paddingLeft, y);
                        // ticks على اليمين
                        g.DrawLine(tickPen, paddingLeft + chartW, y, paddingLeft + chartW + 5, y);

                        // قيمة Y
                        string valText = maxValue >= 100000
                            ? $"{val / 1000:F0}K"
                            : maxValue >= 1000
                                ? $"{val / 1000:F1}K"
                                : $"{val:F1}";

                        SizeF textSize = g.MeasureString(valText, font);
                        g.DrawString(valText, font, textBrush,
                            paddingLeft - textSize.Width - 6,
                            y - textSize.Height / 2);
                    }

                    // X axis — 5 ticks + labels
                    int xTicks = Math.Min(5, data.Count - 1);
                    if (xTicks > 0)
                    {
                        for (int i = 0; i <= xTicks; i++)
                        {
                            int x = paddingLeft + (int)(chartW * i / (float)xTicks);

                            // grid line
                            g.DrawLine(gridPen, x, paddingTop + 1, x, paddingTop + chartH - 1);

                            // ticks فوق
                            g.DrawLine(tickPen, x, paddingTop - 5, x, paddingTop);
                            // ticks تحت
                            g.DrawLine(tickPen, x, paddingTop + chartH, x, paddingTop + chartH + 5);

                            // قيمة X
                            int dataIndex = (data.Count - 1) * i / xTicks;
                            string xLabel = $"{dataIndex}";
                            SizeF textSize = g.MeasureString(xLabel, font);
                            g.DrawString(xLabel, font, textBrush,
                                x - textSize.Width / 2,
                                paddingTop + chartH + 8);
                        }
                    }

                    // الخط نفسه
                    if (data.Count >= 2)
                    {
                        // ظل تحت الخط
                        using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                        {
                            float xStep = (float)chartW / (data.Count - 1);

                            path.StartFigure();
                            path.AddLine(paddingLeft, paddingTop + chartH,
                                         paddingLeft, paddingTop + chartH);

                            for (int i = 0; i < data.Count; i++)
                            {
                                float x = paddingLeft + i * xStep;
                                float y = paddingTop + chartH - (data[i] / maxValue * chartH);
                                y = Math.Max(paddingTop, Math.Min(paddingTop + chartH, y));
                                path.AddLine(x, y, x, y);
                            }

                            float lastX = paddingLeft + (data.Count - 1) * xStep;
                            path.AddLine(lastX, paddingTop + chartH, lastX, paddingTop + chartH);
                            path.CloseFigure();

                            using (SolidBrush fillBrush = new SolidBrush(
                                Color.FromArgb(40, lineColor.R, lineColor.G, lineColor.B)))
                                g.FillPath(fillBrush, path);
                        }

                        // الخط الرئيسي
                        using (Pen linePen = new Pen(lineColor, 2))
                        {
                            linePen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                            float xStep = (float)chartW / (data.Count - 1);

                            for (int i = 1; i < data.Count; i++)
                            {
                                float x1 = paddingLeft + (i - 1) * xStep;
                                float y1 = paddingTop + chartH - (data[i - 1] / maxValue * chartH);
                                float x2 = paddingLeft + i * xStep;
                                float y2 = paddingTop + chartH - (data[i] / maxValue * chartH);

                                y1 = Math.Max(paddingTop, Math.Min(paddingTop + chartH, y1));
                                y2 = Math.Max(paddingTop, Math.Min(paddingTop + chartH, y2));

                                g.DrawLine(linePen, x1, y1, x2, y2);
                            }
                        }

                        // نقطة آخر قيمة
                        if (data.Count > 0)
                        {
                            float xStep = (float)chartW / (data.Count - 1);
                            float dotX = paddingLeft + (data.Count - 1) * xStep;
                            float dotY = paddingTop + chartH - (data[data.Count - 1] / maxValue * chartH);
                            dotY = Math.Max(paddingTop, Math.Min(paddingTop + chartH, dotY));

                            using (SolidBrush dotBrush = new SolidBrush(Color.White))
                                g.FillEllipse(dotBrush, dotX - 4, dotY - 4, 8, 8);
                            using (SolidBrush dotCore = new SolidBrush(lineColor))
                                g.FillEllipse(dotCore, dotX - 2, dotY - 2, 4, 4);
                        }
                    }
                }
            }

            if (box.Image != null) box.Image.Dispose();
            box.Image = bmp;
        }


        private void ResetProgressUI()
        {
            _ratioHistory.Clear();
            _speedHistory.Clear();
            _originalFileSize = new FileInfo(_inputFilePath).Length;
            _compressionStartTime = DateTime.Now;
            progressBar.Value = 0;
            lblProgressPercent.Text = "Starting...";
        }

        private void btnCancelCompression_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            btnCancelCompression.Enabled = false;
            lblProgressPercent.Text = "Cancelling...";
        }
        private void btnShowReport_Click(object sender, EventArgs e)
        {
            GenerateCompressionReport();
        }





        private int GetDecompressedSampleRate()
        {
            if (_compressedMetadata != null) return _compressedMetadata.SampleRate;
            if (_dmMetadata != null) return _dmMetadata.SampleRate;
            if (_admMetadata != null) return _admMetadata.SampleRate;
            if (_compandingMetadata != null) return _compandingMetadata.SampleRate;
            // Adaptive Predictive — read from UI
            if (cmbSampleRate?.SelectedItem != null &&
                int.TryParse(cmbSampleRate.SelectedItem.ToString(), out int apRate))
                return apRate;
            return 16000;
        }
        private void GenerateCompressionReport()
        {
            if (_copied_audio == null)
            {
                MessageBox.Show("Please run a valid compression job first to generate a report.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_inputFilePath) || !System.IO.File.Exists(_inputFilePath))
            {
                MessageBox.Show("Input source audio file could not be found.", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lvwParameters.Items.Clear();
            lblReportTitle.Text = $"{_lastUsedAlgorithmName} METRICS";
            lblTimeValue.Text = $"{_compressionTimeTaken.TotalMilliseconds:N1} ms";

            long uncompressedMemoryBytes = 0;
            using (var reader = new NAudio.Wave.AudioFileReader(_inputFilePath))
            {
                long totalNativeSamples = (long)(reader.TotalTime.TotalSeconds * reader.WaveFormat.SampleRate * reader.WaveFormat.Channels);
                uncompressedMemoryBytes = totalNativeSamples * 2;
            }
            long compressedMemoryBytes = _copied_audio.Length;

            this.uncompressedSizeFormatted.Text = $"Before: {FormatBytes(uncompressedMemoryBytes)}";
            this.compressedSizeFormatted.Text = $"After: {FormatBytes(compressedMemoryBytes)}";

            double ratio = (double)(uncompressedMemoryBytes-compressedMemoryBytes)  / uncompressedMemoryBytes * 100;

            Action<string, string> addProp = (key, val) => {
                var item = new ListViewItem(key);
                item.SubItems.Add(val);
                item.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                lvwParameters.Items.Add(item);
            };

            addProp("Saving Ratio", $"{ratio:F2}%");

            switch (_lastUsedAlgorithmName)
            {
                case "DPCM":
                    if (_compressedMetadata != null)
                    {
                        addProp("Sampling Rate", $"{_compressedMetadata.SampleRate} Hz");
                        addProp("Quantization Bits", $"{_compressedMetadata.Bits} Bits");
                        addProp("Predictor Mode", cmbPredictorType.SelectedIndex == 1 ? "Second-Order" : "First-Order");
                    }
                    break;

                case "Delta Modulation":
                    if (_dmMetadata != null)
                    {
                        addProp("Sampling Rate", $"{_dmMetadata.SampleRate} Hz");
                        addProp("Quantizer Step Size", _dmMetadata.StepSize.ToString("F4"));
                        addProp("LPF Cutoff", $"{_dmMetadata.LpfCutoff} Hz");
                    }
                    break;

                case "Adaptive Delta Modulation":
                    if (_admMetadata != null)
                    {
                        addProp("Sampling Rate", $"{_admMetadata.SampleRate} Hz");
                        addProp("Initial Step Size", _admMetadata.InitStepSize.ToString("F4"));
                        addProp("Adaptation Multiplier", _admMetadata.AdaptationFactor.ToString("F2"));
                        addProp("Max Step Ceiling", _admMetadata.MaxStepSize.ToString("F2"));
                        addProp("History Bits Memory", $"{_admMetadata.HistoryBits} Bits");
                        addProp("LPF Cutoff", $"{_admMetadata.LpfCutoff} Hz");
                    }
                    break;

                case "Mu-Law":
                case "A-Law":
                    if (_compandingMetadata != null)
                    {
                        addProp("Companding Variant", _compandingMetadata.Algorithm);
                        addProp("Operational Rate", $"{_compandingMetadata.SampleRate} Hz");
                        addProp("Bit Depth Target", $"{_compandingMetadata.BitDepth} Bits");
                        addProp("Channel Configuration", _compandingMetadata.Channels == 1 ? "Mono" : "Native Stereo");
                    }
                    break;
            }

            pnlReportSidebar.Visible = true;
            pnlReportSidebar.BringToFront();
        }

        private void InitializeReportSidebar()
        {
            pnlReportSidebar = new Panel
            {
                Dock = DockStyle.Right,
                Width = 380,
                BackColor = Color.FromArgb(248, 249, 250),
                Visible = false,
                BorderStyle = BorderStyle.None
            };

            Panel leftBorder = new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Color.FromArgb(222, 226, 230) };
            pnlReportSidebar.Controls.Add(leftBorder);

            lblReportTitle = new Label
            {
                Text = "COMPRESSION REPORT",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlReportSidebar.Controls.Add(lblReportTitle);

            System.Windows.Forms.Button btnCloseSidebar = new System.Windows.Forms.Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(330, 15),
                Cursor = Cursors.Hand
            };
            btnCloseSidebar.FlatAppearance.BorderSize = 0;
            btnCloseSidebar.Click += (s, e) => pnlReportSidebar.Visible = false;
            pnlReportSidebar.Controls.Add(btnCloseSidebar);

            Label lblTimeHeader = new Label { Text = "PROCESSING TIME", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.DarkGray, Location = new Point(20, 70), AutoSize = true };
            lblTimeValue = new Label { Text = "0.00 ms", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(13, 110, 253), Location = new Point(16, 90), AutoSize = true };
            pnlReportSidebar.Controls.Add(lblTimeHeader);
            pnlReportSidebar.Controls.Add(lblTimeValue);

            Label lblSavingsHeader = new Label { Text = "AUDIO RAM MEMORY FOOTPRINT", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.DarkGray, Location = new Point(20, 155), AutoSize = true };

            uncompressedSizeFormatted = new Label { Text = "Original: 0 B", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(108, 117, 125), Location = new Point(20, 180), AutoSize = true };
            compressedSizeFormatted = new Label { Text = "Compressed: 0 B", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(40, 167, 69), Location = new Point(20, 205), AutoSize = true };

            pnlReportSidebar.Controls.Add(lblSavingsHeader);
            pnlReportSidebar.Controls.Add(uncompressedSizeFormatted);
            pnlReportSidebar.Controls.Add(compressedSizeFormatted);

            Label lblParamHeader = new Label { Text = "ALGORITHM PROPERTIES", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.DarkGray, Location = new Point(20, 245), AutoSize = true };
            pnlReportSidebar.Controls.Add(lblParamHeader);

            lvwParameters = new ListView
            {
                Location = new Point(20, 270),
                Size = new Size(340, 290),
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.None,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 249, 250)
            };
            lvwParameters.Columns.Add("Property", 160);
            lvwParameters.Columns.Add("Value", 160);
            pnlReportSidebar.Controls.Add(lvwParameters);

            this.Controls.Add(pnlReportSidebar);
        }
        private void ResetCompressionReport()
        {
            if (pnlReportSidebar == null || lblReportTitle == null) return;

            lblReportTitle.Text = "COMPRESSION REPORT";
            lblTimeValue.Text = "0.00 ms";
            uncompressedSizeFormatted.Text = "Original: 0 B";
            compressedSizeFormatted.Text = "Compressed: 0 B";

            Control[] foundControls = pnlReportSidebar.Controls.Find("lblRatioValue", true);
            if (foundControls.Length > 0 && foundControls[0] is Label lblRatio)
            {
                lblRatio.Text = "0.00x";
            }

            lvwParameters.Items.Clear();
            pnlReportSidebar.Visible = false;
        }

        // SAVE MIGHT NOT WORK ON BATOULS'
        //private void btnSave_Click(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(_originalFilePath))
        //    {
        //        MessageBox.Show("No file loaded.", "Nothing to Save",
        //            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    if (_isDecompressed && _decompressedPcmBytes != null)
        //    {
        //        SaveDecompressed();
        //    }
        //    else if (_copied_audio != null)
        //    {
        //        SaveCompressed();
        //    }
        //    else
        //    {
        //        // Original file — just copy it to wherever user wants
        //        string ext = GetOriginalExtension();
        //        string filter = ext == ".mp3" ? "MP3 Audio|*.mp3" : "WAV Audio|*.wav";

        //        SaveFileDialog dlg = new SaveFileDialog
        //        {
        //            Title = "Save Audio",
        //            Filter = filter,
        //            FileName = Path.GetFileNameWithoutExtension(_originalFilePath) + ext
        //        };

        //        if (dlg.ShowDialog() != DialogResult.OK) return;

        //        try
        //        {
        //            File.Copy(_originalFilePath, dlg.FileName, overwrite: true);
        //            MessageBox.Show($"Saved to:\n{dlg.FileName}",
        //                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Save failed: " + ex.Message, "Error",
        //                MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}

        //private string GetOriginalExtension()
        //{
        //    return Path.GetExtension(_inputFilePath).ToLower();
        //}

        //private string GetAlgoSuffix()
        //{
        //    switch (cmbAlgorithmType.SelectedItem?.ToString())
        //    {
        //        case "DPCM": return "dpcm";
        //        case "Mu-Law": return "mulaw";
        //        case "A-Law": return "alaw";
        //        case "Delta Modulation": return "dm";
        //        case "Adaptive Delta Modulation": return "adm";
        //        case "Adaptive Predictive": return "ap";
        //        default: return "compressed";
        //    }
        //}

        //private void SaveDecompressed()
        //{
        //    string ext = GetOriginalExtension();
        //    string filter = ext == ".mp3" ? "MP3 Audio|*.mp3" : "WAV Audio|*.wav";

        //    SaveFileDialog dlg = new SaveFileDialog
        //    {
        //        Title = "Save Decompressed Audio",
        //        Filter = filter,
        //        FileName = Path.GetFileNameWithoutExtension(_inputFilePath) + "_decompressed" + ext
        //    };

        //    if (dlg.ShowDialog() != DialogResult.OK) return;

        //    try
        //    {
        //        int sampleRate = GetDecompressedSampleRate();

        //        using (MemoryStream ms = new MemoryStream(_decompressedPcmBytes))
        //        using (var raw = new NAudio.Wave.RawSourceWaveStream(
        //            ms, new NAudio.Wave.WaveFormat(sampleRate, 16, 1)))
        //        using (var writer = new NAudio.Wave.WaveFileWriter(dlg.FileName, raw.WaveFormat))
        //        {
        //            byte[] buf = new byte[4096];
        //            int read;
        //            while ((read = raw.Read(buf, 0, buf.Length)) > 0)
        //                writer.Write(buf, 0, read);
        //        }

        //        MessageBox.Show($"Saved to:\n{dlg.FileName}",
        //            "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Save failed: " + ex.Message, "Error",
        //            MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        //private void SaveCompressed()
        //{
        //    string ext = GetOriginalExtension();
        //    string filter = ext == ".mp3" ? "MP3 Audio|*.mp3" : "WAV Audio|*.wav";
        //    string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();

        //    SaveFileDialog dlg = new SaveFileDialog
        //    {
        //        Title = "Save Compressed Audio",
        //        Filter = filter,
        //        FileName = Path.GetFileNameWithoutExtension(_inputFilePath) + "_" + GetAlgoSuffix() + ext
        //    };

        //    if (dlg.ShowDialog() != DialogResult.OK) return;

        //    try
        //    {
        //        using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create))
        //        using (BinaryWriter bw = new BinaryWriter(fs))
        //        {
        //            switch (selectedAlgorithm)
        //            {
        //                case "DPCM":
        //                    bw.Write("DPCM".ToCharArray());
        //                    bw.Write(_compressedMetadata.SampleRate);
        //                    bw.Write(_compressedMetadata.Bits);
        //                    bw.Write(_compressedMetadata.TotalSamples);
        //                    bw.Write(_copied_audio.Length);
        //                    bw.Write(_copied_audio);
        //                    break;

        //                case "Mu-Law":
        //                case "A-Law":
        //                    //string tag = selectedAlgorithm == "Mu-Law" ? "MULAW" : "ALAW_";
        //                    //bw.Write(tag.ToCharArray());
        //                    //bw.Write(_compandingMetadata.SampleRate);
        //                    //bw.Write(_compandingMetadata.BitDepth);
        //                    //bw.Write(_compandingMetadata.Channels);
        //                    //bw.Write(_compandingMetadata.TotalSamples);
        //                    //bw.Write(_copied_audio.Length);
        //                    bw.Write(_copied_audio);
        //                    break;

        //                case "Delta Modulation":
        //                    bw.Write("DM__".ToCharArray());
        //                    bw.Write(_dmMetadata.SampleRate);
        //                    bw.Write(_dmMetadata.TotalSamples);
        //                    bw.Write(_dmMetadata.StepSize);
        //                    bw.Write(_dmMetadata.LpfCutoff);
        //                    bw.Write(_copied_audio.Length);
        //                    bw.Write(_copied_audio);
        //                    break;

        //                case "Adaptive Delta Modulation":
        //                    bw.Write("ADM_".ToCharArray());
        //                    bw.Write(_admMetadata.SampleRate);
        //                    bw.Write(_admMetadata.TotalSamples);
        //                    bw.Write(_admMetadata.InitStepSize);
        //                    bw.Write(_admMetadata.AdaptationFactor);
        //                    bw.Write(_admMetadata.MaxStepSize);
        //                    bw.Write(_admMetadata.HistoryBits);
        //                    bw.Write(_admMetadata.LpfCutoff);
        //                    bw.Write(_copied_audio.Length);
        //                    bw.Write(_copied_audio);
        //                    break;

        //                case "Adaptive Predictive":
        //                    bw.Write("AP__".ToCharArray());
        //                    bw.Write(_copied_audio.Length);
        //                    bw.Write(_copied_audio);
        //                    break;
        //            }
        //        }

        //        MessageBox.Show($"Saved to:\n{dlg.FileName}",
        //            "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Save failed: " + ex.Message, "Error",
        //            MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        // SAVE THAT SHOULD WORK ON BATOULS'
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_originalFilePath))
            {
                MessageBox.Show("No file loaded.", "Nothing to Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isDecompressed && _decompressedPcmBytes != null)
                SaveDecompressed();
            else if (_copied_audio != null)
                SaveCompressed();
            else
            {
                string ext = GetOriginalExtension();
                string filter = ext == ".mp3" ? "MP3 Audio|*.mp3" : "WAV Audio|*.wav";

                SaveFileDialog dlg = new SaveFileDialog
                {
                    Title = "Save Audio",
                    Filter = filter,
                    FileName = Path.GetFileNameWithoutExtension(_originalFilePath) + ext
                };

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    File.Copy(_originalFilePath, dlg.FileName, overwrite: true);
                    MessageBox.Show($"Saved to:\n{dlg.FileName}",
                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save failed: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetOriginalExtension()
        {
            return Path.GetExtension(_originalFilePath).ToLower();
        }

        private string GetAlgoSuffix()
        {
            switch (cmbAlgorithmType.SelectedItem?.ToString())
            {
                case "DPCM": return "dpcm";
                case "Mu-Law": return "mulaw";
                case "A-Law": return "alaw";
                case "Delta Modulation": return "dm";
                case "Adaptive Delta Modulation": return "adm";
                case "Adaptive Predictive": return "ap";
                default: return "compressed";
            }
        }

        private void SaveDecompressed()
        {
            string ext = GetOriginalExtension();
            string filter = ext == ".mp3" ? "MP3 Audio|*.mp3" : "WAV Audio|*.wav";

            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "Save Decompressed Audio",
                Filter = filter,
                FileName = Path.GetFileNameWithoutExtension(_originalFilePath) + "_decompressed" + ext
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                int sampleRate = GetDecompressedSampleRate();

                using (MemoryStream ms = new MemoryStream(_decompressedPcmBytes))
                using (var raw = new NAudio.Wave.RawSourceWaveStream(
                    ms, new NAudio.Wave.WaveFormat(sampleRate, 16, 1)))
                using (var writer = new NAudio.Wave.WaveFileWriter(dlg.FileName, raw.WaveFormat))
                {
                    byte[] buf = new byte[4096];
                    int read;
                    while ((read = raw.Read(buf, 0, buf.Length)) > 0)
                        writer.Write(buf, 0, read);
                }

                MessageBox.Show($"Saved to:\n{dlg.FileName}",
                    "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveCompressed()
        {
            string ext = GetOriginalExtension();
            string filter = ext == ".mp3" ? "MP3 Audio|*.mp3" : "WAV Audio|*.wav";
            string selectedAlgorithm = cmbAlgorithmType.SelectedItem?.ToString();

            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "Save Compressed Audio",
                Filter = filter,
                FileName = Path.GetFileNameWithoutExtension(_originalFilePath) + "_" + GetAlgoSuffix() + ext
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    switch (selectedAlgorithm)
                    {
                        case "DPCM":
                            bw.Write("DPCM".ToCharArray());
                            bw.Write(_compressedMetadata.SampleRate);
                            bw.Write(_compressedMetadata.Bits);
                            bw.Write(_compressedMetadata.TotalSamples);
                            bw.Write(_copied_audio.Length);
                            bw.Write(_copied_audio);
                            break;

                        case "Mu-Law":
                        case "A-Law":
                            string tag = selectedAlgorithm == "Mu-Law" ? "MULAW" : "ALAW_";
                            bw.Write(tag.ToCharArray());
                            bw.Write(_compandingMetadata.SampleRate);
                            bw.Write(_compandingMetadata.BitDepth);
                            bw.Write(_compandingMetadata.Channels);
                            bw.Write(_compandingMetadata.TotalSamples);
                            // strip the 44-byte WAV header, save only raw compressed bytes
                            int wavHeaderSize = 44;
                            int rawLength = _copied_audio.Length - wavHeaderSize;
                            bw.Write(rawLength);
                            bw.Write(_copied_audio, wavHeaderSize, rawLength);
                            break;

                        case "Delta Modulation":
                            bw.Write("DM__".ToCharArray());
                            bw.Write(_dmMetadata.SampleRate);
                            bw.Write(_dmMetadata.TotalSamples);
                            bw.Write(_dmMetadata.StepSize);
                            bw.Write(_dmMetadata.LpfCutoff);
                            bw.Write(_copied_audio.Length);
                            bw.Write(_copied_audio);
                            break;

                        case "Adaptive Delta Modulation":
                            bw.Write("ADM_".ToCharArray());
                            bw.Write(_admMetadata.SampleRate);
                            bw.Write(_admMetadata.TotalSamples);
                            bw.Write(_admMetadata.InitStepSize);
                            bw.Write(_admMetadata.AdaptationFactor);
                            bw.Write(_admMetadata.MaxStepSize);
                            bw.Write(_admMetadata.HistoryBits);
                            bw.Write(_admMetadata.LpfCutoff);
                            bw.Write(_copied_audio.Length);
                            bw.Write(_copied_audio);
                            break;

                        case "Adaptive Predictive":
                            bw.Write("AP__".ToCharArray());
                            bw.Write(_copied_audio.Length);
                            bw.Write(_copied_audio);
                            break;
                    }
                }

                MessageBox.Show($"Saved to:\n{dlg.FileName}",
                    "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        
    }
}
