/////////////////////////////////////////////////////////////////////////////
//  MessageServer.cs - WCF message sending service as a reciever           //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module implements ICommService, providing methods to create host for message service 
 *   sending/recieving messages and parsing load message from repository.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   IMessageService.cs, BlockingQueue.cs
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 05 Nov 2016
 */

using SWTools;
using System;
using System.ServiceModel;
using System.Threading;
using System.Xml.Linq;


namespace MessageService
    {
        [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]

        public class MessageServer : ICommService
        {

            // We want the queue to be shared by all clients and the server,
            // so make it static.

            static BlockingQueue<Message> BlockingQ = null;
            ServiceHost host = null;

            public MessageServer() //set window size and create a queue when initializing a CommService 
            {
                // Only one service, the first, should create the queue

                if (BlockingQ == null)
                    BlockingQ = new BlockingQueue<Message>();
            }



            public void PostMessage(Message msg)
            {
               
                    BlockingQ.enQ(msg);
            }

            // Since this is not a service operation only server can call

            public Message GetMessage()
            {
               return BlockingQ.deQ();
            }


            public void CreateMessageServiceHost(string url)
            {
                // Can't configure SecurityMode other than none with streaming.
                // This is the default for BasicHttpBinding.
                //   BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
                //   BasicHttpBinding binding = new BasicHttpBinding(securityMode);

                WSDualHttpBinding binding1 = new WSDualHttpBinding();
                Uri baseAddress = new Uri(url);
                Type service = typeof(MessageServer);
                host = new ServiceHost(service, baseAddress);
                host.AddServiceEndpoint(typeof(ICommService), binding1, baseAddress);
            }

            public void open()
            {
                host.Open();
            }

            public void close()
            {
                host.Close();
            }

            public InternalMessage Parse(Message msg)
            {
                InternalMessage imsg = new InternalMessage();
                XElement loadmsg = XElement.Parse(msg.fileMessage.xmlLoadMessage);                
                XElement connectmsg = XElement.Parse(msg.xmlConnectMessage);
                imsg.recipient = msg.recipient;
                imsg.connectMessage.fileConnectAddress = connectmsg.Element("FileConnectAddress").Value;
                imsg.connectMessage.MessageConnectAddress = connectmsg.Element("MessageConnectAddress").Value;

                imsg.fileMessage.loadType = loadmsg.Element("LoadType").Value;
                imsg.fileMessage.loadPath = loadmsg.Element("LoadPath").Value;
                foreach(var filename in loadmsg.Element("FileNames").Elements("File"))
                {
                    imsg.fileMessage.fileNames.Add(filename.Value);
                }
                return imsg;
            }

            // Method for server's child thread to run to process messages.
            // It's virtual so you can derive from this service and define
            // some other server functionality.

        public Message TryGetQueryMessage()
        {
            Message msg = new Message();
            while (true)
            {
                msg = GetMessage();
                if (msg.sender != "Client" || msg.recipient != "Query")
                {
                    PostMessage(msg);
                    Thread.Sleep(10);
                    continue;
                }
                else
                    break;
            }
            return msg;
        }

        public Message TryGetMessage()
        {
            Message msg = new Message();
            while (true)
            {
                msg = GetMessage();
                if (msg.recipient == "Query")
                {
                    PostMessage(msg);
                    Thread.Sleep(10);
                    continue;
                }
                else
                    break;
            }
            return msg;
        }



        //TEST STUB

        //public static void Main()
        //{
        //    Console.Write("\n  Communication Server Starting up");
        //    Console.Write("\n ==================================\n");

        //    try
        //    {
        //        MessageServer service = new MessageServer();

        //        // - We're using WSHttpBinding and NetTcpBinding so digital certificates
        //        //   are required.
        //        // - Both these bindings support ordered delivery of messages by default.

        //        BasicHttpBinding binding0 = new BasicHttpBinding();
        //        WSDualHttpBinding binding1 = new WSDualHttpBinding();

        //        Uri address0 = new Uri("http://localhost:4030/ICommService/BasicHttp");
        //        Uri address1 = new Uri("http://localhost:4040/ICommService/WSHttp");


        //        using (service.host = new ServiceHost(typeof(MessageServer), address1))
        //        {
        //            service.host.AddServiceEndpoint(typeof(ICommService), binding0, address0);
        //            service.host.AddServiceEndpoint(typeof(ICommService), binding1, address1);

        //            service.host.Open();

        //            Console.Write("\n  CommService is ready.");


        //            Console.WriteLine();

        //            Thread child = new Thread(new ThreadStart(service.ThreadProc));//recieve msg from client

        //            child.Start();
        //            child.Join();

        //            Console.Write("\n\n  Press <ENTER> to terminate service.\n\n");
        //            Console.ReadLine();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write("\n  {0}\n\n", ex.Message);
        //    }
        //}
    }
    }
