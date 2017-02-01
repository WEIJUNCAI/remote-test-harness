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
 *   ICommService interface as a proxy, a method to create communication channel.
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
using System.ServiceModel;
using System.Threading;

namespace MessageService
{
    class MessageClient
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
            channel = factory.CreateChannel();

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
