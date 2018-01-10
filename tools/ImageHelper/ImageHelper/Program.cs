using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine(args.Length);
            try
            {
                int size = int.Parse(args[2]);
                ImageClass.MakeThumbnail(args[0], args[1], size, size, "HW");
                //Console.ReadKey();
            }
            catch(Exception ex) 
            {
                //Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }
    }
}
