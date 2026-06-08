using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace WindowsFormsApp2
{
    public class DeltaModulationAlgorithm : CompressionInterface
    {
        public string Name => "Delta Modulation";

        public byte[] Compress(float[] samples, int sampleRate,
                               Dictionary<string, object> parameters,
                               Action<int, long, long> reportProgress,
                               CancellationToken cancellationToken = default)
        {
            float stepSize = Convert.ToSingle(parameters["stepSize"]);

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                float predictedValue = 0f;
                byte currentByte = 0;
                int bitCounter = 0;

                for (int n = 0; n < samples.Length; n++)
                {
                    int bit;
                    if (samples[n] >= predictedValue)
                    {
                        bit = 1;
                        predictedValue += stepSize;
                    }
                    else
                    {
                        bit = 0;
                        predictedValue -= stepSize;
                    }

                    currentByte |= (byte)(bit << (7 - bitCounter));
                    bitCounter++;

                    if (bitCounter == 8)
                    {
                        writer.Write(currentByte);
                        currentByte = 0;
                        bitCounter = 0;
                    }

                    if (n % 250 == 0)
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