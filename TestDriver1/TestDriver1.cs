/////////////////////////////////////////////////////////////////////
// TestDriver1.cs - define a test to run                           //
//                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
*   Test driver needs to know the types and their interfaces
*   used by the code it will test.  It doesn't need to know
*   anything about the test harness.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDemo
{
    using LoadingTests;
    using Project4;

    public class TestDriver1 : ITest
    {
        private CodeToTest1 code;  // will be compiled into separate DLL

        //----< Testdriver constructor >---------------------------------
        /*
        *  For production code the test driver may need the tested code
        *  to provide a creational function.
        */
        public TestDriver1()
        {
            code = new CodeToTest1();//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }
        //----< factory function >---------------------------------------
        /*
        *   This can't be used by any code that doesn't know the name
        *   of this class.  That means the TestHarness will need to
        *   use reflection - ugh!
        *
        *   The language gives us this problem because it won't
        *   allow a static method in an interface or abstract class.
        */
        public static ITest create()
        {
            return new TestDriver1();
        }
        //----< test method is where all the testing gets done >---------

        public bool test()
        {
            code.annunciator("first being tested");
            return true;
        }
        //----< test stub - not run in test harness >--------------------

        static void Main(string[] args)
        {
            Console.Write("\n  Local test:\n");

            ITest test = TestDriver1.create();

            if (test.test() == true)
                Console.Write("\n  test passed");
            else
                Console.Write("\n  test failed");
            Console.Write("\n\n");
        }
    }
}
