/////////////////////////////////////////////////////////////////////////////
//  IMessageService.cs - WCF message sending/recieving service             //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module defines the message sending among clients, test harness and repository.
 *   as well as the service contract to post and get message.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 05 Nov 2016
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;


    namespace MessageService
    {
        [ServiceContract(Namespace = "FileTransfer")]
        public interface ICommService
        {

            [OperationContract/*(IsOneWay = true)*/]
            void PostMessage(Message msg);


            // Not a service operation so only server can call

            Message GetMessage();
        }



        [DataContract]
        public class Message  //info for a single request
        {
        public Message()
        {
            xmlConnectMessage = string.Empty;
            sender = string.Empty;
            recipient = string.Empty;
            testMessage = new TestMessage();
            fileMessage = new FileMessage();
        }
        [DataMember]
        public string sender { get; set; }

        [DataMember]
        public string recipient { get; set; }

        [DataMember]
            public string xmlConnectMessage;

            [DataMember]
            public TestMessage testMessage;

            [DataMember]
            public FileMessage fileMessage;

        }



        [DataContract]
        public class TestMessage
        {
            //public TestMessage()
            //{
            //    xmlRequest = string.Empty;
            //    StringResult = string.Empty;

            //}

            [DataMember]
            public string xmlRequest { get; set; }

            [DataMember]
            public string parsedRequest { get; set; }

            [DataMember]
            public string xmlResult { get; set; }


        }


        [DataContract]
        public class FileMessage
        {
            //public FileMessage()
            //{
            //    xmlLoadMessage = string.Empty;
            //    xmlLoadReply = string.Empty;
            //}

            [DataMember]
            public string xmlLoadMessage { get; set; }

            [DataMember]
            public string xmlLoadReply { get; set; }
        }


///////////////////////////////////////////////////////////////

        public class InternalMessage
        {
            public InternalMessage()
            {
                connectMessage = new ConnectMessage();
                loadStatus = new LoadStatus();
                fileMessage = new InternalFileMessage();
                recipient = string.Empty;
            }

           public ConnectMessage connectMessage { get; set; }
           public InternalFileMessage fileMessage { get; set; }
           public LoadStatus loadStatus { get; set; }
           public string recipient { get; set; }
        }



       public class InternalFileMessage
        {
           public InternalFileMessage()
            {
                loadType = string.Empty;
                loadPath = string.Empty;
                fileNames = new List<string>();
            }

            public string loadType { get; set; }
            public string loadPath { get; set; }
            public List<string> fileNames { get; set; }
        } 


        public class ConnectMessage
        {
            public ConnectMessage()
            {
                fileConnectAddress = string.Empty;
                MessageConnectAddress = string.Empty;
            }
            public string fileConnectAddress { get; set; }
            public string MessageConnectAddress { get; set; }
        }

        public class LoadStatus
        {
            public LoadStatus()
            {
                status = new bool();
                loadMessage = string.Empty;
            }
            public bool status { get; set; }
            public string loadMessage { get; set; }
        }

    }
