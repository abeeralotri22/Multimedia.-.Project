using System;
using System.Collections.Generic;
using System.IO;
namespace WindowsFormsApp2
{
    public class CompressionResult
    {
        public byte[] CompressedData { get; set; }
        public double CompressionRatio { get; set; }
        public int SmallCount { get; set; }
        public int BigCount { get; set; }

        public int Channels { get; set; }
        public double StepSize { get; set; }
        public string PredictionMode { get; set; }
        public int QuantizationLevels { get; set; }
        public int TotalSamples { get; set; }
        public int SampleRate { get; set; }
    }
    public class CompressionEngine
    {

        public enum PredictionMode { Simple, Average, Linear , Adaptive }

        public CompressionResult Compress(short[] data, int quantizationLevels, PredictionMode mode, double stepSize, int channels, int sampleRate, int predictorOrder)
        {
            int smallCount = 0;
            int bigCount = 0;
            int bitsPerSample = (int)Math.Ceiling(Math.Log(quantizationLevels, 2));
            if (bitsPerSample == 0) bitsPerSample = 1;

            List<byte> compressedList = new List<byte>();
            byte currentByte = 0;
            int bitCount = 0;

            void WriteBits(int value, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    if (((value >> i) & 1) == 1) currentByte |= (byte)(1 << bitCount);
                    bitCount++;
                    if (bitCount == 8) { compressedList.Add(currentByte); currentByte = 0; bitCount = 0; }
                }
            }

            short[] history = new short[Math.Max(1, predictorOrder)];
            for (int h = 0; h < history.Length; h++) history[h] = data[0];

            for (int i = 0; i < data.Length; i++)
            {
                short predicted = 0;

                if (predictorOrder > 0)
                {
                    switch (mode)
                    {
                        case PredictionMode.Linear:
                            if (history.Length >= 2)
                            {
                                predicted = (short)(2 * history[0] - history[1]);
                            }
                            else
                            {
                                predicted = history[0];
                            }
                            break;


                        case PredictionMode.Adaptive:
                            if (history.Length >= 2)
                            {
                                predicted = (short)(history[0] + (history[0] - history[1]) * 0.25);
                            }
                            else
                            {
                                predicted = history[0];
                            }
                            break;
                    }
                }

                double diff = (double)data[i] - predicted;
                double modeSpecificStep = stepSize; double factor = 1.0; 
                if (mode == PredictionMode.Linear) factor = 0.8;
                if (mode == PredictionMode.Adaptive) factor = 1.0;
                if (mode == PredictionMode.Average) factor = 1.5;

                double effectiveStep = stepSize * factor;
                int val = (int)Math.Round((diff / effectiveStep) + (quantizationLevels / 2.0));
                val = Math.Max(0, Math.Min(quantizationLevels - 1, val));
                
                int threshold = Math.Max(1, (int)(quantizationLevels * (0.1 + (predictorOrder * 0.02))));
                //int threshold = Math.Max(1, (int)(quantizationLevels * 0.1));
                bool isSmall = (val >= (quantizationLevels / 2 - threshold) && val <= (quantizationLevels / 2 + threshold));

                if (isSmall)
                {
                    WriteBits(0, 1);
                    smallCount++;
                }
                else
                {
                    WriteBits(1, 1);
                    WriteBits(val, bitsPerSample);
                    bigCount++;
                }

                for (int j = history.Length - 1; j > 0; j--)
                {
                    history[j] = history[j - 1];
                }
                history[0] = data[i];
            }

            if (bitCount > 0) compressedList.Add(currentByte);

            return new CompressionResult
            {
                CompressedData = compressedList.ToArray(),
                SmallCount = smallCount,
                BigCount = bigCount,
                CompressionRatio = (double)(data.Length * 2) / (compressedList.Count + 1),
                Channels = channels,
                StepSize = stepSize,
                PredictionMode = mode.ToString(),
                QuantizationLevels = quantizationLevels,
                TotalSamples = data.Length,
                SampleRate = sampleRate
            };
        }


        public short[] Decompress(byte[] compressedData, int quantizationLevels, PredictionMode mode, double stepSize, int totalSamples, int predictorOrder)
        {
            int bitsPerSample = (int)Math.Ceiling(Math.Log(quantizationLevels, 2));
            if (bitsPerSample == 0) bitsPerSample = 1;

            short[] output = new short[totalSamples];
            short[] history = new short[Math.Max(1, predictorOrder)];

            int bitIndex = 0;
            int byteIndex = 0;

            int ReadBit()
            {
                if (byteIndex >= compressedData.Length) return 0;
                int bit = (compressedData[byteIndex] >> bitIndex) & 1;
                bitIndex++;
                if (bitIndex == 8) { bitIndex = 0; byteIndex++; }
                return bit;
            }

            int ReadBits(int count)
            {
                int value = 0;
                for (int i = 0; i < count; i++)
                {
                    if (ReadBit() == 1) value |= (1 << i);
                }
                return value;
            }

            int threshold = Math.Max(1, (int)(quantizationLevels * (0.1 + (predictorOrder * 0.02))));

            for (int i = 0; i < totalSamples; i++)
            {
                short predicted = 0;

                if (predictorOrder > 0)
                {
                    switch (mode)
                    {
                        case PredictionMode.Linear:
                            predicted = (history.Length >= 2) ? (short)(2 * history[0] - history[1]) : history[0];
                            break;
                        case PredictionMode.Adaptive:
                            predicted = (history.Length >= 2) ? (short)(history[0] + (history[0] - history[1]) * 0.25) : history[0];
                            break;
                        case PredictionMode.Average:
                            predicted = (history.Length >= 2) ? (short)((history[0] + history[1]) / 2) : history[0];
                            break;
                        default:
                            predicted = history[0];
                            break;
                    }
                }

                int isBig = ReadBit();
                int val;

                if (isBig == 0)
                {
                    val = (quantizationLevels / 2);
                }
                else
                {
                    val = ReadBits(bitsPerSample);
                }

                double factor = (mode == PredictionMode.Linear) ? 0.8 : (mode == PredictionMode.Average ? 1.5 : 1.0);
                double effectiveStep = stepSize * factor;

                int diff = (int)Math.Round((val - (quantizationLevels / 2.0)) * effectiveStep);
                output[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, predicted + diff));

                for (int j = history.Length - 1; j > 0; j--) history[j] = history[j - 1];
                history[0] = output[i];
            }

            return output;
        }
    }
}