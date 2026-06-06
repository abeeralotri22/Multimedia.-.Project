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
using System.IO;
using System.Reflection.Emit;
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
        public Form1()
        {
            InitializeComponent();
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
            //DragDropLabel.AutoSize = false;
            DragDropLabel.TextAlign = ContentAlignment.TopLeft;
            DragDropLabel.BorderStyle = BorderStyle.FixedSingle;
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

<<<<<<< HEAD

        private void Form1_Load(object sender, EventArgs e)
        {
            // كود اختبار Emgu CV
            try
            {
                // هذا السطر يجبر المكتبة على التحميل
                var mat = new Emgu.CV.Mat();
                MessageBox.Show("تم تحميل Emgu CV بنجاح!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل Emgu CV: " + ex.Message);
            }
        }

=======
        
>>>>>>> 56d5d7449ec426c74d1939614bb7c2d5dfd7340b
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
                    // ↓ KEY FIX: check if playback stopped (finished naturally)
                    if (outputDevice == null || outputDevice.PlaybackState == PlaybackState.Stopped)
                    {
                        // Clean up old instances before creating new ones
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

                    // ↓ KEY FIX: align buffer size to BlockAlign to avoid the exception
                    int blockAlign = reader.WaveFormat.BlockAlign;
                    int floatsPerBlock = blockAlign / sizeof(float);  // usually 2 for stereo
                                                                      // Make samplesPerPixel a multiple of channels at minimum
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

            // ↓ ADD THIS: detect natural end of playback
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
        }
        private void btnOpenCompression_Click(object sender, EventArgs e)
        {
            // Ensure an audio file is actually loaded first
            if (string.IsNullOrEmpty(this.audioPath))
            {
                MessageBox.Show("Please load an audio file first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Open the compression form as a dialog box
            using (CompressionForm compressionWindow = new CompressionForm(this.audioPath))
            {
                if (compressionWindow.ShowDialog() == DialogResult.OK)
                {
                   // MessageBox.Show("Compression completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Optional: Reload the compressed file or update UI
                }
            }
        }
        

        //private void InfoLabel_Click(object sender, EventArgs e)
        //{
        //    //InfoLabel.AutoSize = false;
        //    //InfoLabel.Width = 250;
        //    //InfoLabel.Height = 120;
        //    //InfoLabel.BorderStyle = BorderStyle.FixedSingle;
        //    //InfoLabel.TextAlign = ContentAlignment.TopLeft;
        //    //InfoLabel.Font = new Font("Tahoma", 9);

        //    //InfoLabel.Text = audioInfo;
            
        //}

<<<<<<< HEAD
=======
        //private void Form1_Load(object sender, EventArgs e)
        //{

        //}
>>>>>>> 56d5d7449ec426c74d1939614bb7c2d5dfd7340b
    }


}