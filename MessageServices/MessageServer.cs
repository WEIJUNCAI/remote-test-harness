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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]

    public class MessageServer : ICommService
    {

        // We want the queue to be shared by all clients and the server,
        // so make it static.
        static int counter = 9999;
        static BlockingQueue<Message> BlockingQ = null; //static so that clients and server can access the same queue 
        ServiceHost host = null;

        public MessageServer() //set window size and create a queue when initializing a CommService 
        {
            // Only one service, the first, should create the queue

            if (BlockingQ == null)
                BlockingQ = new BlockingQueue<Message>();
        }



        public void PostMessage(Message msg)
        {
            //IdentifyClient();
            BlockingQ.enQ(msg);

        }

        // Since this is not a service operation only server can call

        public Message GetMessage()
        {

            var s = BlockingQ.deQ();

            return s;
        }

        public Message TryGetMessage(string sender, string recipient)
        {
            object locker = new object();
            lock (locker)
            {
                Message msg = new Message();
                while (true)
                {
                    msg = GetMessage();
                    if (msg.sender != sender || msg.recipient != recipient)
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


        public string CreateUrl()
        {
            if (counter < 1000)
            {
                counter = 9999;
            }
            string url = "http://localhost:" + (counter--.ToString()) + "/ICommService/BasicHttp";
            return url;
        }

        public TestLoadStatus Parse(Message msg)
        {
            object locker = new object();
            lock (locker)
            {
                XElement reply = XElement.Parse(msg.fileMessage.xmlLoadReply);
                TestLoadStatus tls = new TestLoadStatus();
                tls.status = reply.Element("Status").Value == "true" ? true : false;
                tls.loadMessage = reply.Element("LoadMessage").Value;
                return tls;
            }
        }


        public void open()
        {
            host.Open();
        }

        public void close()
        {
            host.Close();
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

