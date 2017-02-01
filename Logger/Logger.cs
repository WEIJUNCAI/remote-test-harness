/////////////////////////////////////////////////////////////////////////////
//  Logger.cs - log test logs                                              //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module implements test harness logger which log load, test process 
 *   and results and write log to screen or files.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   InternalMessage.cs
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20 Sep 2016
 *   ver 1.5 : 05 Nov 2016
 */

using MessageService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace TestHarness
{
    public class Logger : MarshalByRefObject// accept a testresult class from loader 
                                            // and convert it to txt
                                            // or dsiplay.  display(xml) or display(testresult)
    {
        private List<TestInfo> inPutTestList; // all test data in a single request
        string threadName = string.Empty;

        public Logger()
        {
            inPutTestList = new List<TestInfo>();
            threadName = "Child thread:" + Thread.CurrentThread.ManagedThreadId.ToString();
        }

        public void SetDataAndResultList(List<TestInfo> tdrList)
        {
            inPutTestList = new List<TestInfo>(tdrList);
        }

        public void display()
        {
            Console.WriteLine("\n({0})Currnet Domain: {1}", threadName, AppDomain.CurrentDomain.FriendlyName);
            foreach (TestInfo list in inPutTestList)
                list.show();
        }

        public string WriteToString()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            foreach (TestInfo tdr in inPutTestList) // for one test in one request
            {
                sw.WriteLine("");
                sw.WriteLine("=====================================");
                sw.WriteLine("\n  {0,-12} : {1}", "test request", tdr.requestName);
                sw.WriteLine("\n  {0,-12} : {1}", "test name", tdr.testName);
                sw.WriteLine("\n  {0,12} : {1}", "author", tdr.authorName);
                sw.WriteLine("\n  {0,12} : {1}", "time stamp", tdr.testTime);
                sw.WriteLine("\n  {0,12} : {1}", "load status", tdr.stat.loadMessage);
                sw.WriteLine("\n  {0,12} : {1}", "test driver", tdr.testDriverName);
                foreach (string library in tdr.testCodeName)
                {
                    sw.WriteLine("\n  {0,12} : {1}", "library", library);
                }

                sw.WriteLine("\n  {0,12} : {1}", "Test result", tdr.testResult);
            }
            return sb.ToString();
        }

        public void WriteLog(string path)
        {
            Console.WriteLine("\n\n({0})Storing logs and results of all tests in " +
                             inPutTestList[0].requestName +
                             " into repository,\ncurrent domain: " +
                             AppDomain.CurrentDomain.FriendlyName, threadName);
            string fileName = inPutTestList[0].authorName + " " + inPutTestList[0].testTime.ToString("dd-MM-yyyy-HH_mm_ss") + ".txt";
            Console.WriteLine("\nThe name of this test log: {0}", fileName);
            using (FileStream fs = new FileStream(Path.Combine(path, fileName), FileMode.Append))
            using (TextWriter sw = new StreamWriter(fs))
            {
                foreach (TestInfo tdr in inPutTestList) // for one test in one request
                {
                    sw.WriteLine("");
                    sw.WriteLine("======================================");
                    sw.WriteLine("\n  {0,-12} : {1}", "test request", tdr.requestName);
                    sw.WriteLine("\n  {0,-12} : {1}", "test name", tdr.testName);
                    sw.WriteLine("\n  {0,12} : {1}", "author", tdr.authorName);
                    sw.WriteLine("\n  {0,12} : {1}", "time stamp", tdr.testTime);
                    sw.WriteLine("\n  {0,12} : {1}", "load status", tdr.stat.loadMessage);
                    sw.WriteLine("\n  {0,12} : {1}", "test driver", tdr.testDriverName);
                    foreach (string library in tdr.testCodeName)
                    {
                        sw.WriteLine("\n  {0,12} : {1}", "library", library);
                    }
                    sw.WriteLine("\n  {0,12} : {1}", "Test result", tdr.testResult);
                }

            }
        }

        public void writeLogSummary(string path)
        {
            string fileName = inPutTestList[0].authorName + " " + inPutTestList[0].testTime.ToString("dd-MM-yyyy-HH_mm_ss") + "Summary.txt";

            using (FileStream fs = new FileStream(Path.Combine(path, fileName), FileMode.Append))
            using (TextWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                sw.WriteLine("\n  {0,-12} : {1}", "test request", inPutTestList[0].requestName);

                foreach (TestInfo tdr in inPutTestList) // for one test in one request
                {
                    sw.WriteLine("");
                    sw.WriteLine("==================================");
                    sw.WriteLine("\n  {0,-12} : {1}", "test name", tdr.testName);
                    sw.WriteLine("\n  {0,12} : {1}", "author", tdr.authorName);
                    sw.WriteLine("\n  {0,12} : {1}", "time stamp", tdr.testTime);
                    sw.WriteLine("\n  {0,12} : {1}", "Test result", tdr.testResult);
                }

            }
        }

    }
}

