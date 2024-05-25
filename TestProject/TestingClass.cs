using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    class TestingClass
    {
        static FileStream fileStream;

        static void Main(string[] args)
        {

            int test = 42;

            byte[] data = new byte[test];
            foreach (byte b in BitConverter.GetBytes(test))
            {
            Console.WriteLine(b);

            }
            Console.ReadLine();

        }
    }
}
