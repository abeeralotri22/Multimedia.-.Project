using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace WindowsFormsApp2
{
    public class ALawAlgorithm : CompressionInterface
    {
        private const double ALawA = 87.6;

        public string Name => "A-Law";

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
                double companded = ApplyALaw(samples[i]);
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

        

        private double ApplyALaw(double x)
        {
            double magnitude = Math.Abs(x);
            double denominator = 1.0 + Math.Log(ALawA);
            double companded = magnitude < 1.0 / ALawA
                ? (ALawA * magnitude) / denominator
                : (1.0 + Math.Log(ALawA * magnitude)) / denominator;
            return Math.Sign(x) * companded;
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
            => value < min ? min : value > max ? max : value;


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