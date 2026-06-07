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

        public enum PredictionMode { Simple, Average, Linear }

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

            // مصفوفة تاريخ تبدأ من 0
            short[] history = new short[Math.Max(1, predictorOrder)];

            for (int i = 0; i < data.Length; i++)
            {
                short predicted = 0;

                // منطق التنبؤ المحدث لضمان استجابة المحرك
                if (predictorOrder > 0)
                {
                    if (mode == PredictionMode.Linear)
                    {
                        // Weighted Average: العينات الأحدث لها وزن أكبر (تجعل التنبؤ يتفاعل مع تغير الإشارة)
                        double weightedSum = 0;
                        int weightSum = 0;
                        for (int k = 0; k < predictorOrder; k++)
                        {
                            int weight = (predictorOrder - k);
                            weightedSum += history[k] * weight;
                            weightSum += weight;
                        }
                        predicted = (short)(weightedSum / weightSum);
                    }
                    else if (mode == PredictionMode.Average && predictorOrder >= 2)
                    {
                        // متوسط بسيط لآخر عينتين فقط
                        predicted = (short)((history[0] + history[1]) / 2);
                    }
                    else
                    {
                        // تنبؤ بسيط (قيمة العينة السابقة)
                        predicted = history[0];
                    }
                }

                double diff = (double)data[i] - predicted;
                int val = (int)Math.Round((diff / stepSize) + (quantizationLevels / 2.0));
                val = Math.Max(0, Math.Min(quantizationLevels - 1, val));

                // threshold ديناميكي يعتمد على stepSize وعدد المستويات
                int threshold = (int)(quantizationLevels * (stepSize / 200.0));
                if (threshold < 1) threshold = 1;
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

                // تحديث التاريخ
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
            // 1. تعريف المتغيرات الأساسية
            int bitsPerSample = (int)Math.Ceiling(Math.Log(quantizationLevels, 2));
            if (bitsPerSample == 0) bitsPerSample = 1;

            short[] output = new short[totalSamples];
            short[] history = new short[Math.Max(1, predictorOrder)];

            // هذه المتغيرات يجب أن تكون هنا لتعمل الـ Local Functions
            int bitIndex = 0;
            int byteIndex = 0;

            // 2. الدوال المساعدة (Local Functions)
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
                    value |= (ReadBit() << i);
                }
                return value; // الآن الدالة تعيد القيمة دائماً
            }

            // 3. حلقة فك الضغط الرئيسية
            for (int i = 0; i < totalSamples; i++)
            {
                short predicted = 0;

                // حساب التنبؤ بناءً على النمط (Mode)
                if (mode == PredictionMode.Linear && predictorOrder > 0)
                {
                    double weightedSum = 0;
                    int weightSum = 0;
                    for (int k = 0; k < history.Length; k++)
                    {
                        int weight = (history.Length - k);
                        weightedSum += history[k] * weight;
                        weightSum += weight;
                    }
                    predicted = (short)(weightedSum / weightSum);
                }
                else if (mode == PredictionMode.Average && history.Length >= 2)
                {
                    predicted = (short)((history[0] + history[1]) / 2);
                }
                else
                {
                    predicted = history[0];
                }

                // قراءة البتات
                int isBig = ReadBit();
                int val = (isBig == 0) ? (quantizationLevels / 2) : ReadBits(bitsPerSample);

                // إعادة بناء العينة
                int diff = (int)Math.Round((val - (quantizationLevels / 2.0)) * stepSize);
                output[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, predicted + diff));

                // تحديث التاريخ
                for (int j = history.Length - 1; j > 0; j--) history[j] = history[j - 1];
                history[0] = output[i];
            }

            return output; // هذه هي الـ return الأساسية للدالة
        }
    }
}