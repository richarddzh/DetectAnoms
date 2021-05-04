using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectAnoms
{
    class Program
    {
        static void Main(string[] args)
        {
            var trans = new string[]
            {
                "end",
                "entry",
                "tril",
                "mail",
                "map",
                "blend",
                "straigh",
                "height",
            };

            var fpgrowth = new FPGrowth<char>();
            fpgrowth.Fit(trans, null, 2);
            Trace.Listeners.Add(new ConsoleTraceListener());
            foreach (var item in Enumerable.Zip(fpgrowth.FreqItems, fpgrowth.FreqItemCount, (x, y) => Tuple.Create(x, y)))
            {
                Console.WriteLine("{0}: {1}",
                    string.Join(", ", item.Item1),
                    item.Item2);
            }

            Console.ReadKey();
        }
    }
}
