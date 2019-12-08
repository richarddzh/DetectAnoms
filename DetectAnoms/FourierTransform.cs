using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectAnoms
{
    public class FourierTransform
    {
        /// <summary>
        /// Fast Fourier Transform converts data from original domain (space or time) to the frequency domain representation.
        /// Assume that length N = 2^k, after transform a[m] represents the component of period N/m.
        /// If input sample frequency is F, then the m-th component has frequency F/m. The DC component has m = 0.
        /// Assume that a[m] = x + yi (where i is the imaginary unit), the modulus |a[m]| = sqrt(x*x+y*y),
        /// the amplitude is 2*|a[m]|/N, and the phase is arctan(y/x).
        /// </summary>
        /// <param name="a">Sequence to transform, the first 2^k element is transformed</param>
        /// <param name="length">Length of input, must be 2^k</param>
        public static void FFT(Complex[] a, int length)
        {
            const double PI = Math.PI;
            BinaryIndexReverse(a, length);
            for (int h = 2; h <= length; h *= 2)
            {
                Complex wn = new Complex(Math.Cos(-2 * PI / h), Math.Sin(-2 * PI / h));
                for (int j = 0; j < length; j += h)
                {
                    Complex w = new Complex(1, 0);
                    for (int k = j; k < j + h / 2; k++)
                    {
                        Complex u = a[k];
                        Complex t = w * a[k + h / 2];
                        a[k] = u + t;
                        a[k + h / 2] = u - t;
                        w = w * wn;
                    }
                }
            }
        }

        private static void BinaryIndexReverse(Complex[] a, int length)
        {
            int i, j, k;
            for (i = 1, j = length / 2; i < length - 1; i++)
            {
                if (i < j)
                {
                    Swap(a, i, j);
                }

                k = length / 2;
                while (j >= k)
                {
                    j -= k;
                    k /= 2;
                }

                if (j < k)
                {
                    j += k;
                }
            }
        }

        private static void Swap<T>(T[] a, int i, int j)
        {
            var temp = a[i];
            a[i] = a[j];
            a[j] = temp;
        }

        public struct Complex
        {
            public double Imaginary;
            public double Real;

            public Complex(double r, double i)
            {
                this.Imaginary = i;
                this.Real = r;
            }

            public double GetModulus()
            {
                return Math.Sqrt(this.Real * this.Real + this.Imaginary * this.Imaginary);
            }

            public static Complex operator + (Complex a, Complex b)
            {
                return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
            }

            public static Complex operator - (Complex a, Complex b)
            {
                return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
            }

            public static Complex operator * (Complex a, Complex b)
            {
                return new Complex(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
            }
        }
    }
}
