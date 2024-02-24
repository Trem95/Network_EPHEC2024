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
            string inputFile = args[1];
            fileStream = File.OpenRead(inputFile);



        }
    }
}
