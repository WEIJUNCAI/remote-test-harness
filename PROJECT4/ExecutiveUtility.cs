/////////////////////////////////////////////////////////////////////////////
//  ExecutiveUtility.cs - utility class for test executive                 //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains the helper functions for test executive  
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   InternalMessage.cs, HiResTimer.cs, AppDomainManager.cs,
 *                       Loader.cs, Logger.cs, Parser.cs,
 *                       MessageServer.cs, MessageClient.cs, FileServer.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20 Sep 2016
 *   ver 1.5 : 05 Nov 2016
 */


using HRTimer;
using MessageService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace TestHarness
{
    class ExecutiveUtil
    {


        public Loader injectLoader(AppDomain ad)
        {

            object ob
                  = ad.CreateInstanceAndUnwrap("Loader", "TestHarness.Loader");  //Creates a new instance of the specified type defined in the specified assembly.

            return (Loader)ob;
        }


        public Logger injectLogger(AppDomain ad)
        {

            object ob
                  = ad.CreateInstanceAndUnwrap("Logger", "TestHarness.Logger");  //Creates a new instance of the specified type defined in the specified assembly.

            return (Logger)ob;
        }

        public List<TestInfo> Phase1(string threadName, ref string tempPath, Message msg, MessageClient msgSenderRepo, MessageClient msgSenderCL, MessageServer msgReciever)
        {
            HiResTimer hrt = new HiResTimer();

            Console.WriteLine("\n({0})Parsing test request...", threadName);
            Parser parser = new Parser();
            List<TestInfo> requestInfo = parser.doParse(msg);
            Console.WriteLine("\n({0})The result of parsing:", threadName);
            parser.showParsed();

            Console.WriteLine("\n({0})Creating Temporary directory...", threadName);
            tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\..\\temp" + requestInfo[0].requestName);
            Directory.CreateDirectory(tempPath);

            Console.WriteLine("\n({0})Creating channel to send message to repository...", threadName);

            msgSenderRepo.CreateMessageChannel("http://localhost:8085/ICommService");
            Message msgToRepo = msgSenderRepo.SetupDownLoadMessageToRepo(threadName, requestInfo, tempPath, "http://localhost:4140/ICommService/BasicHttp", "http://localhost:8180/ICommService/BasicHttp");
            Console.WriteLine("\n({0})Sending message to repository...", threadName);

            msgSenderRepo.channel.PostMessage(msgToRepo);
            Console.WriteLine("\n({0})Waiting for message from repository...", threadName);
            hrt.Start();
            Message reply = msgReciever.TryGetMessage("Repository", threadName);
            hrt.Stop();
            TestLoadStatus stat = msgReciever.Parse(reply);
            Console.WriteLine("");
            Console.WriteLine("\n({0})Recieved message from repository in {2} microsec:\n{1}", threadName, stat.loadMessage, hrt.ElapsedMicroseconds);
            Console.WriteLine("");
            Console.WriteLine("\n({0})Creating channel to send message to client...", threadName);
            msgSenderCL.CreateMessageChannel(parser.ParseConnect(msg));
            Message loadMsgToCL = msgSenderCL.SetupLoadMessageToClient(stat);
            Console.WriteLine("\n({0})Sending load message to client...", threadName);
            msgSenderCL.channel.PostMessage(loadMsgToCL);
            Console.WriteLine("\n<---------------------------------------------------------------------->");

            return requestInfo;
        }



        public void SendLogToRepo(string threadName, string tempLogPath, MessageClient msgSenderRepo, MessageServer msgReciever)
        {
            int trycount = 0;
            HiResTimer hrt = new HiResTimer();
            while (trycount <= 5)
            {
                trycount++;
                Console.WriteLine("\n({0})Sending logs to repository...", threadName);
                Message msgToRepo = msgSenderRepo.SetupUploadMessageToRepo(threadName, tempLogPath, "http://localhost:4140/ICommService/BasicHttp", "http://localhost:8180/ICommService/BasicHttp");

                msgSenderRepo.channel.PostMessage(msgToRepo);
                Console.WriteLine("\n({0})Waiting for message from repository...", threadName);
                hrt.Start();
                Message msgFromRepo = msgReciever.TryGetMessage("Repository", threadName);
                hrt.Stop();
                Console.WriteLine("");
                Console.WriteLine("\n({0})Recieved message from repository in {0} microsec.", threadName, hrt.ElapsedMicroseconds);
                Console.WriteLine("");
                TestLoadStatus loadStat = msgReciever.Parse(msgFromRepo);
                Console.WriteLine(loadStat.loadMessage);
                if (loadStat.status)
                    break;
                if (trycount <= 5)
                {
                    Thread.Sleep(500);
                    continue;
                }
                throw new Exception("Failed sending logs to repository");
            }
        }


        public void Phase2(string tempPath, string threadName, List<TestInfo> requestInfo, MessageClient msgSenderRepo, MessageClient msgSenderCL, MessageServer msgReciever)
        {
            HiResTimer hrt = new HiResTimer();
            Console.WriteLine("\n({1})Creating child AppDomain for {0}...", requestInfo[0].requestName, threadName);
            AppdomainManager manager = new AppdomainManager();
            AppDomain childApp = manager.createChildDomain();
            Loader loader = injectLoader(childApp);
            loader.SetTestList(requestInfo);
            Console.WriteLine("\n<------------------------------------------------------------>");
            Console.WriteLine("\n({0})Loading all DLLs required by this test request...", threadName);
            if (loader.LoadTests(tempPath))
            {
                Logger logger = injectLogger(childApp);
                Console.WriteLine("\n({0})Running tests...", threadName);
                hrt.Start();
                var resultList = loader.run();
                hrt.Stop();
                Console.WriteLine("\n({1})Tests execeted in {0} microsec.", hrt.ElapsedMicroseconds, threadName);
                Console.WriteLine("");
                if (resultList != null)
                {
                    logger.SetDataAndResultList(resultList);
                    Console.WriteLine("");
                    Console.WriteLine("\n<------------------------------------------------------------>");
                    Console.WriteLine("\n({0})Demonstrating the test log:\n", threadName);
                    logger.display();
                    string tempLogPath = Path.Combine(tempPath, "Logs");
                    Directory.CreateDirectory(tempLogPath);
                    logger.WriteLog(tempLogPath);
                    logger.writeLogSummary(tempLogPath);

                    SendLogToRepo(threadName, tempLogPath, msgSenderRepo, msgReciever);
                    Console.WriteLine("\n({0})Sending logs to client...", threadName);
                    Message msgToCL = msgSenderCL.SetupResultMessageToClient(logger.WriteToString(), resultList[0].authorName + " " + resultList[0].testTime.ToString("dd-MM-yyyy-HH_mm_ss"));
                    msgSenderCL.channel.PostMessage(msgToCL);

                    Console.WriteLine("");
                    Console.WriteLine("\n<***********************************************************>");
                }
                manager.unloadChild(childApp);
                Console.WriteLine("\n({0})Deleting temporary directory...", threadName);
                Directory.Delete(tempPath, true);
                Console.WriteLine("\n({0})Temporary directory deleted.", threadName);
            }
        }
    }
}
