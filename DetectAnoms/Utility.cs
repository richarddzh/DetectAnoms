using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectAnoms
{
    public static class Utility
    {
        /// <summary>
        /// Safe average without overflow issue.
        /// </summary>
        public static double SafeAverage(this IEnumerable<double> values)
        {
            double avg = 0;
            int count = 1;
            foreach (var x in values)
            {
                avg += (x - avg) / count;
                count++;
            }

            return avg;
        }

        /// <summary>
        /// Get median with quick sort like partition.
        /// </summary>
        public static double GetMedian(this IEnumerable<double> input)
        {
            double[] a = input.ToArray();
            int left = 0;
            int right = a.Length - 1;
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
    }
}
