using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace WindowsFormsApp2
{
    public class MuLawAlgorithm : CompressionInterface
    {
        private const double MuLawMu = 255.0;

        public string Name => "Mu-Law";

        public byte[] Compress(float[] samples, int sampleRate,
                               Dictionary<string, object> parameters,
                               Action<int, long, long> reportProgress,
                               CancellationToken cancellationToken = default)
        {
            int bitDepth = (int)parameters["bitDepth"];
            int channels = (int)parameters["channels"];

            byte[] compressedBytes = new byte[samples.Length];

            for (int i = 0; i < samples.Length; i++)
            {
                double companded = ApplyMuLaw(samples[i]);
                compressedBytes[i] = QuantizeToByte(companded, bitDepth);

                if (i % 250 == 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;
                    int percent = (int)((double)i / samples.Length * 100);
                    reportProgress(percent, i, i);
                }
            }

            return CreateCompressedWaveBytes(compressedBytes, sampleRate, channels);
        }


        private double ApplyMuLaw(double x)
        {
            double magnitude = Math.Abs(x);
            return Math.Sign(x) *
                   Math.Log(1.0 + MuLawMu * magnitude) /
                   Math.Log(1.0 + MuLawMu);
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

       

        private double Clamp(double value, double min, double max)
        {
            return value < min ? min : value > max ? max : value;
        }

     

        private byte[] CreateCompressedWaveBytes(byte[] compressedBytes, int sampleRate, int channels)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (WaveFileWriter writer = new WaveFileWriter(stream, new WaveFormat(sampleRate, 8, channels)))
                    writer.Write(compressedBytes, 0, compressedBytes.Length);
                return stream.ToArray();
            }
        }
    }
}