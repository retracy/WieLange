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
                var wfm = new WaveformData(trace, 400e-3);
                Decode(wfm, 18);

                // Just decode 1 for now
                break;
            }
        }

        static private void Decode(WaveformData wfm, double vCutoff)
        {
            int indexIdle1End = -1;
            for (int i = 0; i < wfm.VoltageValues.Length; i++)
            {
                if (Average(wfm.VoltageValues, i, 10) < vCutoff)
                {
                    indexIdle1End = i - 1;
                    break;
                }
            }
            Debug.WriteLine($"Idle = {indexIdle1End}");

            int indexTimingBit1 = -1;
            for (int i = indexIdle1End + 1; i < wfm.VoltageValues.Length; i++)
            {
                if (Average(wfm.VoltageValues, i, 10) > vCutoff)
                {
                    indexTimingBit1 = i - 1;
                    break;
                }
            }
            Debug.WriteLine($"Bit1 = {indexTimingBit1}");

            // Find trailing edge of IDLE(1) using Pulse width trigger for pulse ~ 250 ms
            // Identify IDLE(1)->0 and 0->1 frame begin marker
            // TODO - Identify data->0->1->IDLE(0) frame end marker
        }

        private static double Average(double[] values, int index, int n)
        {
            if (index < n)
                n = index;
            if (values.Length - index - 1 < n)
                n = values.Length - index - 1;

            double sum = 0;
            for (int i = index - n; i <= index + n; i++)
            {
                sum += values[i];
            }
            return sum / (2 * n + 1);
        }

        public struct WaveformData
        {
            public WaveformData(List<double> values, double hscale)
            {
                HorizontalScale = hscale / (values.Count - 1);
                VoltageValues = values.ToArray();
            }

            /// <summary>
            /// Horizontal scale. Time between each point.
            /// </summary>
            public double HorizontalScale { get; set; }

            /// <summary>
            /// Voltage values. This is the value of each point on the waveform in volts.
            /// </summary>
            public double[] VoltageValues { get; set; }
        }
    }
}
