using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WindowsFormsApp2
{
    public interface CompressionInterface
    {
        string Name { get; }

        // كل خوارزمية تاخذ parameters مختلفة من الـ dictionary
        // مثال: parameters["bits"] = 4, parameters["predictorType"] = 1
        byte[] Compress(float[] samples, int sampleRate,
                        Dictionary<string, object> parameters,
                        Action<int, long, long> reportProgress);

    }
}

