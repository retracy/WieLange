using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace WieLang
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists(args[0]))
                return;

            List<double>[] data = null;
            using (var reader = new StreamReader(args[0]))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (data == null)
                    {
                        data = new List<double>[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            data[i] = new List<double>();
                        }
                    }

                    for (int i = 0; i < values.Length; i++)
                    {
                        data[i].Add(double.Parse(values[i]));
                    }
                }
            }

            foreach (var trace in data)
            {
                EtiDecoder decoder = new EtiDecoder(trace.ToArray(), 18);
                Debug.WriteLine(decoder.Decode());
            }
        }
    }
}
