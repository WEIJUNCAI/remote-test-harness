/////////////////////////////////////////////////////////////////////////////
//  ClientUtility.cs                                                       //                                                          
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains utility methods to set up the message that will be sent to 
 *   test harness or repository
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
using MessageService;
using System.Xml.Linq;

namespace prototypeClient
{


    class ClientUtility
    {

        public ClientUtility()
        {
            
        }


        public void SetupMessageToTH(Message msgToTH, string FileConnectAddress, string MessageConnectAddress)
        {
            XElement connectMessage = new XElement("ConnectMessage");
            connectMessage.Add(new XElement("FileConnectAddress", FileConnectAddress));
            connectMessage.Add(new XElement("MessageConnectAddress", MessageConnectAddress));
            msgToTH.xmlConnectMessage = connectMessage.ToString();

            msgToTH.sender = "Client";
            msgToTH.recipient = string.Empty;
            Console.WriteLine("\n Displaying the first test request in the form of XML\n");
            Console.WriteLine(msgToTH.testMessage.xmlRequest);
        }


        public void SetupLoadMessageToRepo(Message msgToRepoLoad, List<string> info, string FileConnectAddress, string MessageConnectAddress, string loadType)  //store xml requests as
        {
            XElement fileMessage = new XElement("FileMessage");
            XElement connectMessage = new XElement("ConnectMessage");
            connectMessage.Add(new XElement("FileConnectAddress", FileConnectAddress));
            connectMessage.Add(new XElement("MessageConnectAddress", MessageConnectAddress));

            fileMessage.Add(new XElement("LoadType", "Download"));
            fileMessage.Add(new XElement("LoadPath", string.Empty));
            XElement filenames = new XElement("FileNames");
            foreach (string DllName in info)
            {               
                filenames.Add(new XElement("File", DllName));
            }
            msgToRepoLoad.sender = "Client";
            msgToRepoLoad.recipient = "Load";
            fileMessage.Add(filenames);
            msgToRepoLoad.xmlConnectMessage = connectMessage.ToString();
            msgToRepoLoad.fileMessage.xmlLoadMessage = fileMessage.ToString();
        }

        public void SetupQueryMessageToRepo(Message msgToRepoQuery, TestResults tr, string FileConnectAddress, string MessageConnectAddress, string loadType)
        {
            XElement fileMessage = new XElement("FileMessage");
            XElement connectMessage = new XElement("ConnectMessage");
            connectMessage.Add(new XElement("FileConnectAddress", FileConnectAddress));
            connectMessage.Add(new XElement("MessageConnectAddress", MessageConnectAddress));

            fileMessage.Add(new XElement("LoadType", "Upload"));
            fileMessage.Add(new XElement("LoadPath", Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestResults")));
            XElement filenames = new XElement("FileNames", new XElement("File", tr.LogName + "Summary.txt"));
            fileMessage.Add(filenames);
            msgToRepoQuery.fileMessage.xmlLoadMessage = fileMessage.ToString();
            msgToRepoQuery.sender = "Client";
            msgToRepoQuery.recipient = "Query";
            msgToRepoQuery.xmlConnectMessage = connectMessage.ToString();
            msgToRepoQuery.fileMessage.xmlLoadMessage = fileMessage.ToString();
        }


       





        //static void Main(string[] args)
        //{
        //    ClientUtility client = new ClientUtility();

        //    Console.WriteLine("Creating channel to send message to repository...");
        //    client.msgSenderwithRepo.CreateMessageChannel("http://localhost:8085/ICommService");

        //    Console.WriteLine("Creating channel to send message to TH...");
        //    client.msgSenderwithTH.CreateMessageChannel("http://localhost:8180/ICommService/BasicHttp");

           
            

        //    Console.WriteLine("Constructing host to download/upload files...");
        //    client.fileServer.CreateFileServiceHost("http://localhost:4040/ICommService/BasicHttp");
        //    client.fileServer.open();


        //    //try
        //    //{
               

        //    client.GetRequestFiles();

        //    foreach (info_In_One_Request_Folder info in client.folderInfoList)
        //    {
        //        HiResTimer hrt = new HiResTimer();
        //        client.SetupLoadMessageToRepo(info, "http://localhost:4040/ICommService/BasicHttp",
        //                                  "http://localhost:8080/ICommService/BasicHttp", "Download");
                
        //        client.msgSenderwithRepo.channel.PostMessage(client.msgToRepoLoad);
                
        //        Console.WriteLine("\nWaiting for message from repository");
        //        hrt.Start();
        //        Message msgFromRepo = client.msgReciever.TryGetRepoMessage("Load");
        //        hrt.Stop();
        //        TestLoadStatus loadStatus = client.msgReciever.ParseLoadMsg(msgFromRepo);
        //        Console.WriteLine("");
        //        Console.WriteLine("\nRecieved message from repository in {0} microsec", hrt.ElapsedMicroseconds);
        //        Console.WriteLine("");
        //        Console.WriteLine(loadStatus.loadMessage);

        //        if (loadStatus.status)
        //        {
        //            Console.WriteLine("\nSending message to TH");
        //            client.SetupMessageToTH(info, "http://localhost:4040/ICommService/BasicHttp", "http://localhost:8080/ICommService/BasicHttp");
        //            client.msgSenderwithTH.channel.PostMessage(client.msgToTH);
        //        }
        //    }


           
        //    /////////////////////////////////////////////////////////////
        //    // This message would shut down the communication service
        //    // msg.text = "quit";
        //    // Console.Write("\n  sending message: {0}", msg.text);
        //    // client.channel.PostMessage(msg);
        //    Console.ReadKey();
        //        //((ICommunicationObject)client.channel).Close();
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Console.Write("\n\n  {0}", ex.Message);
        //    //}
        //    Console.Write("\n\n");
        //}
    }

    } 
