using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectAnoms
{
    public class AnomalyDetector
    {
        /// <summary>
        /// Get the test statistics for anomaly. |x - median|/median absolute deviation.
        /// The first fftLength number of elements are used to estimate the period/frequency of the seasonal component.
        /// </summary>
        /// <param name="a">Input data sequence</param>
        /// <param name="fftLength">Must be 2^k and k >= 6</param>
        /// <param name="decompositionLoop">Loop in decomposition to get seasonal component</param>
        /// <param name="trendSmooth">Window size to smooth the trend</param>
        public static double[] Detect(double[] a, int fftLength, int trendSmooth, int decompositionLoop, out SeasonalTrendDecomposition.DecompositionResult decompositionResult)
        {
            InterpolateNaN(a);
            var complexValues = a.Select(x => new FourierTransform.Complex(x, 0)).ToArray();
            FourierTransform.FFT(complexValues, fftLength);
            var left = fftLength / 64;
            var right = fftLength / 16;
            var index = Enumerable.Range(left, right - left + 1)
                .Select(i => Tuple.Create(i, complexValues[i].GetModulus()))
                .OrderByDescending(x => x.Item2)
                .First().Item1;
            var maxPeriod = fftLength / (index - 1);
            var minPeriod = fftLength / (index + 1);
            decompositionResult = SeasonalTrendDecomposition.Decompose(a, maxPeriod, minPeriod, trendSmooth, decompositionLoop);
            var median = decompositionResult.Residual.GetMedian();
            var diff = decompositionResult.Residual.Select(x => Math.Abs(x - median)).ToArray();
            // Median Absolute Deviation to replace std error as a more robust measure.
            var mad = diff.GetMedian();
            if (mad == 0)
            {
                mad = diff.SafeAverage();
            }

            return diff.Select(x => x / mad).ToArray();
        }

        /// <summary>
        /// Interpolate NaN values with its neighbors.
        /// </summary>
        private static void InterpolateNaN(double[] a)
        {
            int nanStart = -1;
            for (int i = 0; i <= a.Length; i++)
            {
                if (i < a.Length && double.IsNaN(a[i]))
                {
                    if (nanStart < 0)
                    {
                        nanStart = i;
                    }
                }
                else
                {
                    if (nanStart >= 0)
                    {
                        double left = nanStart > 0 ? a[nanStart - 1] : double.NaN;
                        double right = i < a.Length ? a[i] : left;
                        if (double.IsNaN(left))
                        {
                            left = right;
                        }

                        if (!double.IsNaN(left) && !double.IsNaN(right))
                        {
                            double unit = (right - left) / (i - nanStart + 1);
                            for (int j = nanStart; j < i; j++)
                            {
                                a[j] = left + unit * (j - nanStart + 1);
                            }
                        }

                        nanStart = -1;
                    }
                }
            }
        }
    }
}
