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

    }
}
