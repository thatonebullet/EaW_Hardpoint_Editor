using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardpointEditor
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Welcome to Hardpoint Editor v0.1");

            Console.WriteLine("Choose the location of your file: ");
            string loc = Console.ReadLine();
            

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
