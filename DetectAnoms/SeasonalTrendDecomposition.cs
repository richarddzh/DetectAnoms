using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DetectAnoms
{
    /// <summary>
    /// [1] Cleveland, Robert B., William S. Cleveland, and Irma Terpenning.
    /// "STL: A seasonal-trend decomposition procedure based on loess."
    /// Journal of Official Statistics 6.1 (1990): 3.
    /// [2] Hochenbaum, Jordan, Owen S. Vallis, and Arun Kejariwal.
    /// "Automatic Anomaly Detection in the Cloud Via Statistical Learning."
    /// arXiv preprint arXiv:1704.07706 (2017).
    /// </summary>
    public class SeasonalTrendDecomposition
    {
        public static DecompositionResult Decompose(double[] a, int maxPeriod, int minPeriod, int trendSmooth, int loop)
        {
            var trend = GetTrend(a, trendSmooth);
            var detrend = Enumerable.Range(0, a.Length)
                .Select(i => a[i] - trend[i])
                .ToArray();
            var periodAvg = a.Take(maxPeriod).ToArray();
            var seasonal = new double[a.Length];
            for (int i = 0; i < loop; i++)
            {
                periodAvg = SeasonalMean(detrend, periodAvg, maxPeriod, minPeriod, seasonal);
            }

            var residual = Enumerable.Range(0, a.Length)
                .Select(i => detrend[i] - seasonal[i])
                .ToArray();

            return new DecompositionResult(trend, seasonal, residual);
        }

        public static double[] SeasonalMean(double[] a, double[] seed, int maxPeriod, int minPeriod, double[] seasonal)
        {
            seed = RemoveAverage(seed);
            var periods = new List<double[]>();
            var periodOffsets = new List<int>();
            periods.Add(RemoveAverage(a.Take(maxPeriod).ToArray()));
            periodOffsets.Add(0);
            int offset = 0;
            while (offset < a.Length)
            {
                int left = offset + minPeriod;
                int right = offset + maxPeriod;
                var index = Enumerable.Range(left, right - left + 1)
                    .Where(x => x + maxPeriod <= a.Length)
                    .ToList();
                if (!index.Any())
                {
                    periodOffsets.Add(left);
                    break;
                }

                var newPeriod = index.Select(x => Tuple.Create(x, RemoveAverage(a.Skip(x).Take(maxPeriod).ToArray())))
                    .Select(x => Tuple.Create(x.Item1, x.Item2, Difference(seed, x.Item2)))
                    .OrderBy(x => x.Item3)
                    .First();
                offset = newPeriod.Item1;
                periods.Add(newPeriod.Item2);
                periodOffsets.Add(offset);
            }

            var periodAvg = Enumerable.Range(0, maxPeriod)
                .Select(i => periods.Select(x => x[i]).ToArray())
                .Select(x => GetMedian(x, 0, x.Length - 1))
                .ToArray();

            foreach (var index in periodOffsets)
            {
                CopyArray(periodAvg, 0, maxPeriod, seasonal, index);
            }

            return periodAvg;
        }

        private static void CopyArray(double[] source, int sourceOffset, int sourceLength, double[] dest, int destOffset)
        {
            sourceLength = Math.Min(sourceLength, source.Length - sourceOffset);
            sourceLength = Math.Min(sourceLength, dest.Length - destOffset);
            for (int i = 0; i < sourceLength; i++)
            {
                dest[i + destOffset] = source[i + sourceOffset];
            }
        }

        private static double Difference(double[] a, double[] b)
        {
            return Enumerable.Range(0, a.Length)
                .Select(i => (a[i] - b[i]) * (a[i] - b[i]))
                .Sum();
        }

        private static double[] RemoveAverage(double[] a)
        {
            var avg = a.Average();
            return a.Select(x => x - avg).ToArray();
        }

        public static double[] GetTrend(double[] a, int period)
        {
            var result = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                int left = Math.Max(0, i - period);
                int right = Math.Min(a.Length - 1, i + period);
                result[i] = GetMedian(a, left, right);
            }

            return result;
        }

        public static double GetMedian(double[] a, int left, int right)
        {
            a = a.Skip(left).Take(right - left + 1).ToArray();
            right = right - left;
            left = 0;
            int m = (left + right) / 2;
            while (left <= right)
            {
                int i = left + 1;
                int j = right;
                while (i <= j)
                {
                    if (a[i] < a[left])
                    {
                        i++;
                    }
                    else if (a[j] >= a[left])
                    {
                        j--;
                    }
                    else
                    {
                        double temp = a[i];
                        a[i] = a[j];
                        a[j] = temp;
                        i++;
                        j--;
                    }
                }

                if (i - 1 == m)
                {
                    return a[left];
                }
                else
                {
                    if (i - 1 != left)
                    {
                        double temp = a[i - 1];
                        a[i - 1] = a[left];
                        a[left] = temp;
                    }

                    if (m < i - 1)
                    {
                        right = i - 2;
                    }
                    else
                    {
                        left = i;
                    }
                }
            }

            return a[left];
        }

        public class DecompositionResult
        {
            public double[] Seasonal { get; private set; }
            public double[] Trend { get; private set; }
            public double[] Residual { get; private set; }

            public DecompositionResult(double[] trend, double[] seasonal, double[] residual)
            {
                this.Seasonal = seasonal;
                this.Trend = trend;
                this.Residual = residual;
            }
        }
    }
}
