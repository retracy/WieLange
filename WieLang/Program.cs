using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace WieLang
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists(args[0]))
                return;

            List<float>[] data = null;
            using (var reader = new StreamReader(args[0]))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (data == null)
                    {
                        data = new List<float>[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            data[i] = new List<float>();
                        }
                    }

                    for (int i = 0; i < values.Length; i++)
                    {
                        data[i].Add(float.Parse(values[i]));
                    }
                }
            }

            foreach (var trace in data)
            {
                Decode(trace);
            }
        }

        static private void Decode(IEnumerable<float> trace)
        {
            // Find trailing edge of IDLE(1) using Pulse width trigger for pulse > 240 ms
            // Identify IDLE(1)->0 and 0->1 frame begin marker
            // Identify data->0->1->IDLE(0) frame end marker
        }
    }
}
