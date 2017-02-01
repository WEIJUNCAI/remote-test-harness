/////////////////////////////////////////////////////////////////////
// CodeToTest2.cs - define code to be tested                       //
//                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingTests
{
    public class CodeToTest4
    {
        public void annunciator(string msg)
        {
            Console.Write("\n  Production Code: {0}", msg);
        }
        static void Main(string[] args)
        {
            CodeToTest4 ctt = new CodeToTest4();
            ctt.annunciator("this is a test");
            Console.Write("\n\n");
        }
    }
}
