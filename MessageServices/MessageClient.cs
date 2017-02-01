/////////////////////////////////////////////////////////////////////////////
//  MessageClient.cs - WCF messsage sending as a sender                    //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module defines the behavior of a message sender. It contaions an instance of 
 *   ICommService interface as a proxy, methods to create communication channel and set up
 *   messages to client or repository.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   IMessageService.cs
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 05 Nov 2016
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Xml.Linq;


namespace MessageService
{
    public class MessageClient
        {
            public ICommService channel;

            public void CreateMessageChannel(string url)
            {
                int tryCount = 0;
                int maxCount = 10;
                EndpointAddress address = new EndpointAddress(url);
                WSDualHttpBinding binding = new WSDualHttpBinding();
                ChannelFactory<ICommService> factory
                 = new ChannelFactory<ICommService>(binding, address);
                while (true)
                {
                    try
                    {
                        channel = factory.CreateChannel();
                        tryCount = 0;
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (++tryCount <= maxCount)
                        {
                            Thread.Sleep(500);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }

            }



            public Message SetupDownLoadMessageToRepo(string threadName, List<TestInfo> info, string LoadPath, string FileConnectAddress, string MessageConnectAddress)  //store xml requests as
            {
                Message msgToRepo = new Message();

                XElement FileMessage = new XElement("FileMessage");
                XElement connectMessage = new XElement("ConnectMessage");
                connectMessage.Add(new XElement("FileConnectAddress", FileConnectAddress));
                connectMessage.Add(new XElement("MessageConnectAddress", MessageConnectAddress));
               

                FileMessage.Add(new XElement("LoadType", "Upload"));
                
                FileMessage.Add(new XElement("LoadPath", LoadPath));

                XElement filenames = new XElement("FileNames");
                foreach (TestInfo oneTest in info)
                {                   
                    filenames.Add(new XElement("File",oneTest.testDriverName));
                    foreach(string code in oneTest.testCodeName)
                    {
                        filenames.Add(new XElement("File", code));
                    }
                }
                FileMessage.Add(filenames);
                msgToRepo.sender = "TestHarness";
                msgToRepo.recipient = threadName;
                msgToRepo.xmlConnectMessage = connectMessage.ToString();
                msgToRepo.fileMessage.xmlLoadMessage = FileMessage.ToString();
                return msgToRepo;
            }

            public Message SetupUploadMessageToRepo(string threadName, string path, string FileConnectAddress, string MessageConnectAddress)
            {
                Message msgToRepo = new Message();

                XElement fileMessage = new XElement("FileMessage");
                XElement connectMessage = new XElement("ConnectMessage");
                connectMessage.Add(new XElement("FileConnectAddress", FileConnectAddress));
                connectMessage.Add(new XElement("MessageConnectAddress", MessageConnectAddress));

                fileMessage.Add(new XElement("LoadType", "Download"));
                fileMessage.Add(new XElement("LoadPath", path));
                var filesPath = Directory.GetFiles(path);

                XElement filenames = new XElement("FileNames");
                foreach (string filepath in filesPath)
                {
                    filenames.Add(new XElement("File", Path.GetFileName(filepath))); //send the full path of logs
                }
                fileMessage.Add(filenames);
                msgToRepo.sender = "TestHarness";
                msgToRepo.recipient = threadName;
                msgToRepo.xmlConnectMessage = connectMessage.ToString();
                msgToRepo.fileMessage.xmlLoadMessage = fileMessage.ToString();
                return msgToRepo;
            }

        public Message SetupLoadMessageToClient(TestLoadStatus tls)
        {
            Message msg = new Message();
            msg.sender = "TestHarness";
            msg.recipient = "AboutLoad";

            XElement loadMsg = new XElement("LoadStatus");
            loadMsg.Add(new XElement("Status", tls.status));
            loadMsg.Add(new XElement("LoadMessage", tls.loadMessage));

            msg.fileMessage.xmlLoadReply = loadMsg.ToString();
            return msg;
        }

            public Message SetupResultMessageToClient(string log, string logName)
            {
                Message msg = new Message();
                XElement result = new XElement("TestResult");
                msg.sender = "TestHarness";
                msg.recipient = "AboutLog";

                result.Add(new XElement("LogName", logName));
                result.Add(new XElement("Log", log));
                msg.testMessage.parsedRequest = string.Empty;
                msg.testMessage.xmlResult = result.ToString();
                return msg;
            }


        //TEST STUB

        //static void Main(string[] args)
        //{
        //    if (args.Length == 0)
        //    {
        //        Console.Write("\n\n  Please enter name of machine hosting service\n\n");
        //        return;
        //    }

        //    Console.Write("\n  BasicHttpClient Starting to Post Messages to Service");
        //    Console.Write("\n ======================================================\n");

        //    Client client = new Client();

        //    // We're parameterizing the channel creation process so 
        //    // clients can connect to any ICommService server.

        //    try
        //    {
        //        string url = "http://" + args[0] + ":4030/ICommService/BasicHttp";
        //        Console.Write("\n  connecting to \"{0}\"\n", url);
        //        client.CreateBasicHttpChannel(url);
        //        Message msg = new Message();
        //        msg.command = Message.Command.DoThat;
        //        for (int i = 0; i < 20; ++i)
        //        {
        //            msg.text = "message #" + i.ToString();
        //            client.channel.PostMessage(msg);
        //            Console.Write("\n  sending: {0}", msg.text);

        //            // Sleeping to demonstrate that messages from different
        //            // clients will interleave on server

        //            Thread.Sleep(100);
        //        }

        //      /////////////////////////////////////////////////////////////
        //      // This message would shut down the communication service
        //      // msg.text = "quit";
        //      // Console.Write("\n  sending message: {0}", msg.text);
        //      // client.channel.PostMessage(msg);

        //      ((ICommunicationObject)client.channel).Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write("\n\n  {0}", ex.Message);
        //    }
        //    Console.Write("\n\n");
        //}
    }
}
