//using NAudio.Wave;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace WindowsFormsApp2
//{
//    public class AdaptivePredictiveAlgorithm : CompressionInterface
//    {
//        public string Name => "Adaptive Predictive";

//        public byte[] Compress(float[] samples, int sampleRate,
//                               Dictionary<string, object> parameters,
//                               Action<int, long, long> reportProgress)
//        {
//            int levels = (int)parameters["levels"];
//            double stepSize = Convert.ToDouble(parameters["stepSize"]);
//            int predictorOrder = (int)parameters["predictorOrder"];
//            int channels = (int)parameters["channels"];
//            CompressionEngine.PredictionMode mode =
//                (CompressionEngine.PredictionMode)parameters["mode"];

//            short[] pcmSamples = samples
//                .Select(s => (short)Math.Max(short.MinValue,
//                    Math.Min(short.MaxValue, s * short.MaxValue)))
//                .ToArray();

//            reportProgress(0, 0, 0);

//            CompressionEngine engine = new CompressionEngine();
//            var result = engine.Compress(pcmSamples, levels, mode,
//                stepSize, channels, sampleRate, predictorOrder);

//            reportProgress(99, samples.Length, result.CompressedData.Length);

//            return result.CompressedData;
//        }
//    }
//}