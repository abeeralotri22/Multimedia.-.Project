using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public class DpcmAlgorithm : CompressionInterface
    {
        public string Name => "DPCM";

        public byte[] Compress(float[] samples, int sampleRate,
                               Dictionary<string, object> parameters,
                               Action<int, long, long> reportProgress,
                               CancellationToken cancellationToken = default)
        {
            int bits = (int)parameters["bits"];
            int predictorType = (int)parameters["predictorType"];

            int maxLevels = (int)Math.Pow(2, bits);
            int minQuantizedLevel = -(maxLevels / 2);
            int maxQuantizedLevel = (maxLevels / 2) - 1;
            short stepSize = (short)Math.Max(1, 32768 / (maxLevels * 2));

            short predictedValue = 0;
            short prevSample1 = 0;
            short prevSample2 = 0;
           

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                for (int n = 0; n < samples.Length; n++)
                {
                    short pcmSample = (short)Math.Max(-32768, Math.Min(32767, samples[n] * 32767f));

                    if (predictorType == 0 || n < 2)
                        predictedValue = prevSample1;
                    else
                        predictedValue = (short)Math.Max(-32768, Math.Min(32767, (2 * prevSample1) - prevSample2));

                    int error = pcmSample - predictedValue;
                    int quantizedErrorIndex = (int)Math.Round((double)error / stepSize);
                    quantizedErrorIndex = Math.Max(minQuantizedLevel, Math.Min(maxQuantizedLevel, quantizedErrorIndex));

                    int reconstructedSample = Math.Max(-32768, Math.Min(32767,
                        predictedValue + quantizedErrorIndex * stepSize));

                    writer.Write((short)quantizedErrorIndex);

                    prevSample2 = prevSample1;
                    prevSample1 = (short)reconstructedSample;

                    // تقرير كل 1000 سامبل
                    if (n % 2000 == 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return null;
                        int percent = (int)((double)n / samples.Length * 100);
                        reportProgress(percent, n, ms.Length);
                        //System.Threading.Thread.Sleep(1);
                    }
                }

                writer.Flush();
                return ms.ToArray();
            }
        }


    }
}
