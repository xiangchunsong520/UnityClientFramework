using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigConvertor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ProtoExporter creater = new ProtoExporter();
                creater.StartExport();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            //Console.ReadKey();
        }
    }
}
