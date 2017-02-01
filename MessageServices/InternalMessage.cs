/////////////////////////////////////////////////////////////////////////////
//  InternalMessage.cs - message for test harness internal communication   //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains information about a single test request
 *   needed for each part of test harness to carry out work.
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   InternalMessage.cs, ITest.cs
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 05 Nov 2016
 */

using Project4;
using System;
using System.Collections.Generic;


namespace MessageService
    {
        public class TestInfo : MarshalByRefObject  //info for a single test
        {
            public TestInfo()
            {
                requestName = string.Empty;
                requestTime = new DateTime();
                authorName = string.Empty;
                testName = string.Empty;
                testDriverName = string.Empty;
                testCodeName = new List<string>();
                testTime = new DateTime();
                testResult = string.Empty;
                stat = new TestLoadStatus();
               
            }
            
            public string requestName { get; set; }
            public DateTime requestTime { get; set; }
            public string authorName { get; set; }
            public string testName { get; set; }
            public string testDriverName { get; set; }
            public ITest testDriver;
            public List<string> testCodeName { get; set; }
            public DateTime testTime { get; set; }
            public string testResult { get; set; }
            public TestLoadStatus stat;

            public void show()
            {
                Console.WriteLine("\n  {0,-12} : {1}", "request name", requestName);
                Console.WriteLine("\n  {0,-12} : {1}", "test name", testName);
                Console.WriteLine("\n  {0,12} : {1}", "author", authorName);
                Console.WriteLine("\n  {0,12} : {1}", "time stamp", testTime);
                Console.WriteLine("\n  {0,12} : {1}", "test driver", testDriverName);
                foreach (string library in testCodeName)
                {
                    Console.WriteLine("\n  {0,12} : {1}", "library", library);
                }
                Console.WriteLine("\n  {0,12} : {1}", "Test result", testResult);
            }


        }



        public class TestLoadStatus : MarshalByRefObject
        {
            public TestLoadStatus()
            {
                status = new bool();
                loadMessage = string.Empty;
            }

            public bool status { get; set; }
            public string loadMessage { get; set; }
        }
    }
