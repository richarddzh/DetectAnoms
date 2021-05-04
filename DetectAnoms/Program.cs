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
            var tree = fpgrowth.CreateFPTree(trans, null, 2);
            Trace.Listeners.Add(new ConsoleTraceListener());
            tree.Root.Dump(8, 2);

            Console.ReadKey();
        }
    }
}
