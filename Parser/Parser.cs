/////////////////////////////////////////////////////////////////////////////
//  Parser.cs - parse test requests                                        //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains implementation of parser which parse incoming test request 
 *   from client to internal message for communication within test harness.
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
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace TestHarness
{


    public class Parser : MarshalByRefObject
        {
            //feild 
            private XDocument doc_;
 
            public List<TestInfo> outputTestList;

            //constuctor
            public Parser()
            {
                doc_ = new XDocument();
               
                outputTestList = new List<TestInfo>();
            }

            private MemoryStream GenerateStreamFromString(string value)
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
            }


            //display all test info in this request
            public void showParsed()  
            {
                foreach (TestInfo tInfo in outputTestList)
                {
                    Console.Write("\n  {0,-12} : {1}", "request name", tInfo.requestName);
                    Console.Write("\n  {0,-12} : {1}", "test name", tInfo.testName);
                    Console.Write("\n  {0,12} : {1}", "author", tInfo.authorName);
                    Console.Write("\n  {0,12} : {1}", "time stamp", tInfo.requestTime);
                    Console.Write("\n  {0,12} : {1}", "test driver", tInfo.testDriverName);
                    foreach (string library in tInfo.testCodeName)
                    {
                        Console.Write("\n  {0,12} : {1}", "library", library);
                    }
                }
            }


            //store parsed result into a string
            public void WriteToString(Message msg)  
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                foreach (TestInfo tInfo in outputTestList)
                {
                    sw.Write("\n  {0,-12} : {1}", "request name", tInfo.requestName);
                    sw.Write("\n  {0,-12} : {1}", "test name", tInfo.testName);
                    sw.Write("\n  {0,12} : {1}", "author", tInfo.authorName);
                    sw.Write("\n  {0,12} : {1}", "time stamp", tInfo.requestTime);
                    sw.Write("\n  {0,12} : {1}", "test driver", tInfo.testDriverName);
                    foreach (string library in tInfo.testCodeName)
                    {
                        sw.Write("\n  {0,12} : {1}", "library", library);
                    }
                }
                msg.testMessage.parsedRequest = sb.ToString();
            }

            public string ParseConnect(Message msg)
            {
                XElement cm = XElement.Parse(msg.xmlConnectMessage);

                return cm.Element("MessageConnectAddress").Value;
            }

            public List<TestInfo> doParse(Message msg) //parse all tests in a single request
            {
                parse(GenerateStreamFromString(msg.testMessage.xmlRequest));
                return outputTestList;
            }


            private bool parse(Stream xml) //parse all tests
            {
                doc_ = XDocument.Load(xml);// load xml here
                if (doc_ == null)
                    return false;
                string author = doc_.Descendants("Author").First().Value;
                string reqName = doc_.Descendants("RequestName").First().Value;
                TestInfo test = null;

                XElement[] xtests = doc_.Descendants("Test").ToArray();
                int numTests = xtests.Count();

                for (int i = 0; i < numTests; ++i)
                {
                    test = new TestInfo();
                    test.requestName = reqName;
                    test.testCodeName = new List<string>();
                    test.authorName = author;
                    test.requestTime = DateTime.Now;
                    test.testName = xtests[i].Attribute("Name").Value;
                    test.testDriverName = xtests[i].Element("TestDriver").Value;
                    IEnumerable<XElement> xtestCode = xtests[i].Elements("Library");
                    foreach (var xlibrary in xtestCode)
                    {
                        test.testCodeName.Add(xlibrary.Value);
                    }
                    outputTestList.Add(test);
                }
                return true;
            }
        }

        //test stub

        //static void Main(string[] args)
        //{


        //    SWTools.BlockingQueue<string> q = new SWTools.BlockingQueue<string>();
        //    Thread t = new Thread(() => {
        //        string msg;
        //        while (true)
        //        {
        //            msg = q.deQ();
        //            Console.Write("\n  child thread received {0}", msg);
        //            if (msg == "quit")
        //                break;
        //        }//parameter: A ThreadStart delegate that represents the methods to be invoked 
        //         //when this thread begins executing. 
        //    });
        //    t.Start();
        //    string sendMsg = "msg #";
        //    for (int i = 0; i < 20; ++i)
        //    {
        //        string temp = sendMsg + i.ToString();
        //        Console.Write("\n  main thread sending {0}", temp);
        //        q.enQ(temp);
        //    }
        //    q.enQ("quit");
        //    t.Join(); //block main thread until child thread t is complete
        //    Console.Write("\n\n");
        //}
    }

    
