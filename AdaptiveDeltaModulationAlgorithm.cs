using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace WindowsFormsApp2
{
    public class AdaptiveDeltaModulationAlgorithm : CompressionInterface
    {
        public string Name => "Adaptive Delta Modulation";

        public byte[] Compress(float[] samples, int sampleRate,
                               Dictionary<string, object> parameters,
                               Action<int, long, long> reportProgress,
                               CancellationToken cancellationToken = default)
        {
            float initStepSize = Convert.ToSingle(parameters["initStepSize"]);
            float adaptationFactor = Convert.ToSingle(parameters["adaptationFactor"]);
            float maxStepSize = Convert.ToSingle(parameters["maxStepSize"]);
            int historyBits = (int)parameters["historyBits"];

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                float predictedValue = 0f;
                float currentStepSize = initStepSize;
                float minStep = initStepSize;
                int[] historyPattern = new int[historyBits];
                byte currentByte = 0;
                int bitCounter = 0;

                for (int n = 0; n < samples.Length; n++)
                {
                    int bit;
                    if (samples[n] >= predictedValue)
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
                        historyPattern[i] = historyPattern[i - 1];
                    historyPattern[0] = bit;

                    if (n >= historyBits - 1)
                    {
                        bool allSame = true, alternating = true;
                        for (int i = 1; i < historyBits; i++)
                        {
                            if (historyPattern[i] != historyPattern[0]) allSame = false;
                            if (historyPattern[i] == historyPattern[i - 1]) alternating = false;
                        }

                        if (allSame)
                            currentStepSize = Math.Min(maxStepSize, currentStepSize * adaptationFactor);
                        else if (alternating)
                            currentStepSize = Math.Max(minStep, currentStepSize / adaptationFactor);
                    }

                    currentByte |= (byte)(bit << (7 - bitCounter));
                    bitCounter++;

                    if (bitCounter == 8)
                    {
                        writer.Write(currentByte);
                        currentByte = 0;
                        bitCounter = 0;
                    }

                    if (n % 2000 == 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return null;
                        int percent = (int)((double)n / samples.Length * 100);
                        reportProgress(percent, n, ms.Length);
                    }
                }

                if (bitCounter > 0)
                    writer.Write(currentByte);

                writer.Flush();
                return ms.ToArray();
            }
        }

        
    }
}