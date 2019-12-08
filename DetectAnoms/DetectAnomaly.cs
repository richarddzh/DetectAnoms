using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectAnoms
{
    public class DetectAnomaly
    {
        /// <summary>
        /// Get the test statistics for anomaly. |x - median|/median absolute deviation.
        /// </summary>
        /// <param name="a">Input data</param>
        /// <param name="fftLength">Must be 2^k and k >= 6</param>
        /// <param name="decompositionLoop">Loop in decomposition to get seasonal component</param>
        /// <param name="trendSmooth">Interval to smooth the trend</param>
        public static double[] Detect(double[] a, int fftLength, int trendSmooth, int decompositionLoop)
        {
            var complexValues = a.Select(x => new FourierTransform.Complex(x, 0)).ToArray();
            FourierTransform.FFT(complexValues, fftLength);
            var left = fftLength / 32;
            var right = fftLength / 8;
            var index = Enumerable.Range(left, right - left + 1)
                .Select(i => Tuple.Create(i, complexValues[i].GetModulus()))
                .OrderByDescending(x => x.Item2)
                .First().Item1;
            var maxPeriod = fftLength / (index - 1);
            var minPeriod = fftLength / (index + 1);
            var decomp = SeasonalTrendDecomposition.Decompose(a, maxPeriod, minPeriod, trendSmooth, decompositionLoop);
            var median = SeasonalTrendDecomposition.GetMedian(decomp.Residual, 0, decomp.Residual.Length - 1);
            var diff = decomp.Residual.Select(x => Math.Abs(x - median)).ToArray();
            // Median Absolute Deviation to replace std error as a more robust measure.
            var mad = SeasonalTrendDecomposition.GetMedian(diff, 0, diff.Length - 1);
            if (mad == 0)
            {
                mad = diff.Average();
            }

            return diff.Select(x => x / mad).ToArray();
        }
    }
}
