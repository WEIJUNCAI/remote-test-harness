/////////////////////////////////////////////////////////////////////////////
//  Loader.cs - load test libraries                                        //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains implementation of loader class and methods to load 
 *   DLL files and test them.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   InternalMessage.cs, ITest.cs
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20 Sep 2016
 *   ver 1.5 : 05 Nov 2016
 */

using MessageService;
using Project4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace TestHarness
{
    public class Loader : MarshalByRefObject
        {

        string threadName = string.Empty;
        
            public Loader()
            {
                inputTestList = new List<TestInfo>();
                outPutTestList = new List<TestInfo>();
                threadName = "Child thread:" + Thread.CurrentThread.ManagedThreadId.ToString();
            }


            private List<TestInfo> inputTestList;
            private List<TestInfo> outPutTestList;
        

            public void SetTestList(List<TestInfo> list)
            {
                inputTestList = new List<TestInfo>(list);
            }

            private Assembly LoadDriver(string path, TestInfo currtest)
            {
                Console.Write("\n ({1}) loading: \"{0}\"", currtest.testDriverName, threadName); //load driver
                Assembly tdriver = Assembly.LoadFrom(Path.Combine(path, currtest.testDriverName));
                return tdriver;
            }

            private void LoadTestCode(string path, TestInfo currtest)
            {
                foreach (string file in currtest.testCodeName)  // load all test code dll file
                {                                           // needed in this test
                    Console.Write("\n ({1}) loading: \"{0}\"", file, threadName);
                    Assembly assem = Assembly.LoadFrom(Path.Combine(path, file));
                }
            }


            private void SetFailLoadTD(ref TestInfo tdr)
            {
                tdr.stat.status = false;
                tdr.stat.loadMessage = "Fail loading test driver";
                tdr.testCodeName = new List<string> { "N/A" };
                outPutTestList.Add(tdr);
            }

            private void SetFailLoadTC(ref TestInfo tdr)
            {
                tdr.stat.status = false;
                tdr.stat.loadMessage = "Fail loading test code";
                outPutTestList.Add(tdr);
            }

            private void SetTestDriver(ref TestInfo tdr, Assembly tdriver)
            {
                Type[] types = tdriver.GetExportedTypes();

                foreach (Type t in types) //each public type in a driver
                {
                    if (t.IsClass && typeof(ITest).IsAssignableFrom(t))  // does this type derive from ITest ?
                    {
                        Type[] ti = t.GetInterfaces();

                        DemonstrateITest(ti);
                        
                        ITest driver = (ITest)Activator.CreateInstance(t);    // create instance of test driver

                        // save type name and reference to created type on managed heap
                        tdr.stat.status = true;
                        tdr.stat.loadMessage = "succesfully loading test diver and code";
                        tdr.testDriver = driver;
                        outPutTestList.Add(tdr);
                    }
                }
            }


            private void DemonstrateITest(Type[] ti)
            {
                foreach (var tt in ti)
                {
                    Console.WriteLine("\n({0})Demonstrating test driver's interfaces:", threadName);
                    Console.WriteLine(tt.ToString());

                    foreach (var testFunction in tt.GetMethods())
                    {
                        Console.WriteLine("\n({0})Demonstrating test() function in ITest interface:", threadName);
                        Console.WriteLine(testFunction);
                    }
                }
            }

            //----< load test dlls to invoke >-------------------------------

       public bool LoadTests(string path)
            {
                Console.WriteLine("\n({0})Currnet Domain: " + AppDomain.CurrentDomain.FriendlyName, threadName);
                if (inputTestList.Count() != 0)
                {
                        foreach (TestInfo currtest in inputTestList)// for each test in request
                        {
                        TestInfo tdr = new TestInfo();
                        try
                        {
                            tdr.requestName = currtest.requestName;
                            tdr.testName = currtest.testName;
                            Assembly tdriver;
                            try
                            {tdriver = LoadDriver(path, currtest);}
                            catch (Exception ex)
                            {
                                SetFailLoadTD(ref tdr);
                                Console.Write("\n  {0}", ex.Message);
                                continue;
                            }
                            finally
                            {
                                tdr.authorName = currtest.authorName;
                                tdr.testDriverName = currtest.testDriverName;
                            }
                            try
                            {LoadTestCode(path, currtest);}
                            catch (Exception ex)
                            {
                                SetFailLoadTC(ref tdr);
                                Console.Write("\n  {0}", ex.Message);
                                continue;
                            }
                            finally
                            {tdr.testCodeName = new List<string>(currtest.testCodeName);}
                            SetTestDriver(ref tdr, tdriver);
                        }
                        catch (Exception exx)
                        {Console.Write("\n  {0}", exx.Message);}
                    }
                    return outPutTestList.Count > 0;
                }
                else
                {
                    Console.Write("({0})no tests in this request!\n", threadName);
                    return false;
                }
            }

            //----< run all the tests on list made in LoadTests >------------


        

         public List<TestInfo> run()
            {
                if (outPutTestList.Count == 0)
                    return null;
                foreach (TestInfo td in outPutTestList)  // enumerate the test list
                {
                    if (td.stat.status) { 
                        try
                        {
                            Console.WriteLine("\n  ({1})testing {0}", td.testName, threadName);
                            if (td.testDriver.test() == true)
                            {
                                Console.WriteLine("\n  ({0})test passed", threadName);
                                td.testResult = "Passed";
                            }
                            else
                            {
                                Console.WriteLine("\n  ({0})test failed", threadName);
                                td.testResult = "Failed";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Write("\n  {0}", ex.Message);
                        }
                        finally
                        {
                            td.testTime = DateTime.Now;
                        }
                   }
                    else
                    {
                        Console.WriteLine("\n  ({1})testing {0}", td.testName, threadName);
                        Console.WriteLine("\n  ({0})Not tested", threadName);
                        td.testResult= "N/A";
                        td.testTime = DateTime.Now;
                    }
                }
                return outPutTestList;
            }
        }

        //test stub

        //static void Main(string[] args)
        //{


        //    // using string path = "../../../Tests/TestDriver.dll" from command line;

        //    if (args.Count() == 0)
        //    {
        //        Console.Write("\n  Please enter path to libraries on command line\n\n");
        //        return;
        //    }
        //    string path = args[0];

        //    TestHarness th = new TestHarness();
        //    if (th.LoadTests(path))
        //        th.run();
        //    else
        //        Console.Write("\n  couldn't load tests");

        //    Console.Write("\n\n");
        //}
    }
