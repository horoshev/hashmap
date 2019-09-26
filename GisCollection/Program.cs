using System;
using System.Diagnostics;
using System.Linq;

namespace GisCollection
{
    class Program
    {
        static void Main()
        {
            var watch = new Stopwatch();
            watch.Start();
            for (; watch.ElapsedMilliseconds < 500;)
            {
                Console.WriteLine($".{watch.ElapsedMilliseconds}.");
            }
        }
    }
}