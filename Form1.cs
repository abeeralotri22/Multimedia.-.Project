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

            DragDropLabel.AllowDrop = true;

            DragDropLabel.DragEnter += DragDropLabel_DragEnter;
            DragDropLabel.DragDrop += DragDropLabel_DragDrop;
            playbackTimer = new Timer();
            playbackTimer.Interval = 50; 
            playbackTimer.Tick += PlaybackTimer_Tick;
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

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
            {
                audioPath = files[0];

                FileInfo fileInfo = new FileInfo(audioPath);

                using (AudioFileReader reader = new AudioFileReader(audioPath))
                {
                    audioInfo =
                        $"File Size: {(fileInfo.Length / 1024.0 / 1024.0):F2} MB\n\n" +
                        $"Duration: {reader.TotalTime:mm\\:ss}\n\n" +
                        $"Sample Rate: {reader.WaveFormat.SampleRate} Hz\n\n" +
                        $"Channels: {reader.WaveFormat.Channels}\n\n" +
                        $"Bit Depth: {reader.WaveFormat.BitsPerSample} bits\n\n" +
                        $"Encoding: {reader.WaveFormat.Encoding}";
                }

                DragDropLabel.Text = Path.GetFileName(audioPath);

                DrawWaveform(audioPath);
            }
        }

        private void PlayAudiobtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(audioPath))
                {
                    MessageBox.Show(
                        "Please insert an audio file first.",
                        "No Audio Loaded",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                if (!isPlaying)
                {
                    if (audioFile == null)
                    {
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


        private void AudioInfobtn_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(audioPath))
            {
                MessageBox.Show("Please drag an audio file first.");
                return;
            }

            AudioInfoForm infoForm = new AudioInfoForm(audioInfo);
            infoForm.ShowDialog();
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
                    float[] buffer = new float[width];
                    int samplesPerPixel =
                        (int)(reader.Length / reader.WaveFormat.BlockAlign / width);

                    for (int x = 0; x < width; x++)
                    {
                        float max = 0;

                        for (int i = 0; i < samplesPerPixel; i++)
                        {
                            float[] sample = new float[1];

                            if (reader.Read(sample, 0, 1) == 0)
                                break;

                            max = Math.Max(max, Math.Abs(sample[0]));
                        }

                        int y = (int)(max * height / 2);

                        g.DrawLine(
                            pen,
                            x,
                            height / 2 - y,
                            x,
                            height / 2 + y);
                    }
                }
            }

            waveformBitmap = bmp;
            waveformPictureBox.Image = (Bitmap)bmp.Clone();
        }
        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (audioFile == null || waveformBitmap == null)
                return;

            Bitmap frame = (Bitmap)waveformBitmap.Clone();

            using (Graphics g = Graphics.FromImage(frame))
            {
                double progress =
                    audioFile.CurrentTime.TotalSeconds /
                    audioFile.TotalTime.TotalSeconds;

                int x =
                    (int)(progress * waveformPictureBox.Width);

                g.DrawLine(
                    Pens.Red,
                    x,
                    0,
                    x,
                    waveformPictureBox.Height);
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
            }

            DragDropLabel.Text = Path.GetFileName(audioPath);

            DrawWaveform(audioPath);
        }
    }
}