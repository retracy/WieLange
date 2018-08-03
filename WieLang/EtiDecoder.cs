using System;

namespace WieLang
{
    public class EtiDecoder
    {
        public EtiDecoder(double [] values, double vThreshold)
        {
            this.values = values;
            this.vThreshold = vThreshold;
        }

        /// <summary>
        /// Decode ETI waveform
        /// </summary>
        /// <returns></returns>
        public int Decode()
        {
            // Find trailing edge of IDLE(1) using pulse width trigger for pulse width ~ 250 ms
            // After oscilloscope is triggered, find exact falling edge in waveform data points

            // Identify IDLE(1)->0 timing marker
            int indexIdle1End = FindEdge(0, false);

            // Identify 0->1 frame begin marker
            int indexTimingBit1 = FindEdge(indexIdle1End + 1, true);

            // Identify data->0->1 frame end marker
            int indexCenterBit31 = indexIdle1End + (int)(30.5 * (indexTimingBit1 - indexIdle1End));
            int indexTimingBit31 = FindEdge(indexCenterBit31, true);

            // Compute high-resolution samples/bit
            double samplesPerBit = (indexTimingBit31 - indexIdle1End) / 31.0;

            // Decode 7 decimal digits encoded as BCD
            // The 7-digit resulting value indicates elapsed time in increments of 1/100 hour
            // The maximum value would be 99,999.99 hours of operation
            int eti = 0;
            for (int digit = 6; digit >= 0; digit--)
            {
                eti *= 10;
                eti += ReadBcdDigit(digit, indexIdle1End, samplesPerBit);
            }

            return eti;
        }

        private byte ReadBcdDigit(int digit, int start, double samplesPerBit)
        {
            byte value = 0;
            for (int i = 3; i >= 0; i--)
            {
                value <<= 1;
                int indexCenterBit = start + (int)(samplesPerBit * ((digit * 4) + i + 2.5));
                if (values[indexCenterBit] > vThreshold)
                {
                    value |= 1;
                }
            }
            return value;
        }

        private int FindEdge(int start, bool rising)
        {
            for (int i = start; i < values.Length; i++)
            {
                var average = Average(i, 10);
                if ((rising && average > vThreshold) || (!rising && average < vThreshold))
                {
                    return i - 1;
                }
            }

            // Could not find edge
            return -1;
        }

        private double Average(int index, int n)
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

        private readonly double[] values;
        private readonly double vThreshold;
    }
}
