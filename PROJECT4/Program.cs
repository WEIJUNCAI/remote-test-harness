/////////////////////////////////////////////////////////////////////////////
//  Program.cs - test executive of test harness                            //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains the main for the program, methods to inject loader, logger into child Appdomain
 *   and process each test request in a child thread. 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   ExecutiveUtility.cs,
 *                       MessageServer.cs, FileServer.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20 Sep 2016
 *   ver 1.5 : 05 Nov 2016
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using MessageService;
using FileService;

namespace TestHarness
{

    class TestExecutive
    {
        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern void SetWindowPos(
               IntPtr hwnd, IntPtr order,
               int xpos, int ypos, int width, int height,
               uint flags);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        ////////////////////////////////////////////////////////////////////     


        public TestExecutive()
        {
            SetWindowPos(GetConsoleWindow(), (IntPtr)0, 100, 100, 640, 600, 0);
            Console.Title = "Test Harness";
        }

        public string getLog(string testName) //should return string 
        {
            try
            {
                string text = File.ReadAllText("../../../Repository/TestLogs/" + testName + ".txt");


                return text;
            }
            catch (Exception except)
            {
                Console.Write("\n  {0}\n\n", except.Message);
                return "Failed getting log";
            }
        }


        static void Main(string[] args)
        {
            try
            {
                TestExecutive currentProgram = new TestExecutive();
                MessageServer msgReciever = new MessageServer();
                FileServer fileServer = new FileServer();
                Console.WriteLine("\n(Main thread)Constructing host to download/upload files");
                fileServer.CreateFileServiceHost("http://localhost:4140/ICommService/BasicHttp");
                fileServer.open();

                Console.WriteLine("\n(Main thread)Constructing host to communicate with client and repository");
                msgReciever.CreateMessageServiceHost("http://localhost:8180/ICommService/BasicHttp");
                msgReciever.open();
                while (true)
                {
                    Console.WriteLine("\n(Main thread)Recieveing message from client...");
                    Message msg = msgReciever.TryGetMessage("Client", string.Empty);
                    Console.WriteLine("\n(Main thread)Recieved message from client.");
                    Thread childTH = new Thread(() => { currentProgram.CLMsgProc(msg, msgReciever); });

                    childTH.Start();
                }
            }
            catch (Exception except)
            {
                Console.Write("\n  {0}\n\n", except.Message);
            }
            ///////////////////////////////////////////////////////////////////////////////////////////
        }

        private void CLMsgProc(Message msg, MessageServer msgReciever)
        {
            ExecutiveUtil exeUtil = new ExecutiveUtil();
            MessageClient msgSenderRepo = new MessageClient();
            MessageClient msgSenderCL = new MessageClient();
            string threadName = "Child thread:" + Thread.CurrentThread.ManagedThreadId.ToString();
            string tempPath = string.Empty;
            try
            {
               List<TestInfo> requestInfo = exeUtil.Phase1(threadName, ref tempPath, msg, msgSenderRepo, msgSenderCL, msgReciever);
                exeUtil.Phase2(tempPath, threadName, requestInfo, msgSenderRepo, msgSenderCL, msgReciever);
                
            }
            catch (Exception except)
            {
                Console.Write("\n  {0}\n\n", except.Message);
            }
        }
    }
}
