/////////////////////////////////////////////////////////////////////////////
//  Program.cs - Remote client#1                                           //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains the main for the program, methods to set up messages sent to 
 *   test harness and repository.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   IMessageService.cs, HiResTimer.cs, 
 *                       MessageServer.cs, MessageClient.cs, FileServer.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 05 Nov 2016
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MessageService;
using FileService;
using System.Xml.Linq;
using HRTimer;
using System.Reflection;

namespace prototypeClient
{

    public class info_In_One_Request_Folder
    {
        public info_In_One_Request_Folder()
        {
            requestFileName = " ";
            DllNames = new List<string>();
        }

        public string requestFileName;
        public List<string> DllNames;
    }

    class Client
    {
        List<info_In_One_Request_Folder> folderInfoList;
        Message msgToTH;
        Message msgToRepoLoad;
        Message msgToRepoQuery;

        private string[] requestFolder;
        private string[] DllsToSend;

        MessageServer msgReciever;

        MessageClient msgSenderwithRepo;
        MessageClient msgSenderwithTH;
        FileServer fileServer;

        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern void SetWindowPos(
          IntPtr hwnd, IntPtr order,
          int xpos, int ypos, int width, int height,
          uint flags
        );
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();



        public Client()
        {
            int ht = Console.LargestWindowHeight;
            int wd = Console.LargestWindowWidth;
            SetWindowPos(GetConsoleWindow(), (IntPtr)0, 550, 50, 550, 600, 0);
            Console.Title = "Client#1";

            folderInfoList = new List<info_In_One_Request_Folder>();
            msgToRepoLoad = new Message();
            msgToRepoQuery = new Message();
            msgToTH = new Message();
            msgReciever = new MessageServer();

            msgSenderwithRepo = new MessageClient();
            msgSenderwithTH = new MessageClient();
            fileServer = new FileServer();
        }

        void GetRequestFiles()
        {
            requestFolder = Directory.GetDirectories(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../TestRequest"));
            foreach (string folder in requestFolder)
            {
                info_In_One_Request_Folder info = new info_In_One_Request_Folder();
                info.requestFileName = Directory.GetFiles(folder, "*.xml")[0];
                DllsToSend = Directory.GetFiles(folder, "*.dll");
                info.DllNames = DllsToSend.ToList();
                folderInfoList.Add(info);
            }
        }



        public void SetupMessageToTH(info_In_One_Request_Folder info, string FileConnectAddress, string MessageConnectAddress)
        {
            XElement connectMessage = new XElement("ConnectMessage");
            connectMessage.Add(new XElement("FileConnectAddress", FileConnectAddress));
            connectMessage.Add(new XElement("MessageConnectAddress", MessageConnectAddress));
            msgToTH.xmlConnectMessage = connectMessage.ToString();

            StreamReader xstream = new StreamReader(info.requestFileName);

            msgToTH.testMessage.xmlRequest = xstream.ReadToEnd();
            msgToTH.sender = "Client";
            msgToTH.recipient = string.Empty;
            Console.WriteLine("\n Displaying the first test request in the form of XML\n");
            Console.WriteLine(msgToTH.testMessage.xmlRequest);
        }


        public void SetupLoadMessageToRepo(info_In_One_Request_Folder info, string FileConnectAddress, string MessageConnectAddress, string loadType)  //store xml requests as
        {
            XElement fileMessage = new XElement("FileMessage");
            XElement connectMessage = new XElement("ConnectMessage");
            connectMessage.Add(new XElement("FileConnectAddress", FileConnectAddress));
            connectMessage.Add(new XElement("MessageConnectAddress", MessageConnectAddress));

            fileMessage.Add(new XElement("LoadType", "Download"));
            fileMessage.Add(new XElement("LoadPath", string.Empty));
            XElement filenames = new XElement("FileNames");
            foreach (string DllfullPath in info.DllNames)
            {
                string DllName = Path.GetFileName(DllfullPath);
                filenames.Add(new XElement("File", DllName));
            }
            msgToRepoLoad.sender = "Client";
            msgToRepoLoad.recipient = "Load";
            fileMessage.Add(filenames);
            msgToRepoLoad.xmlConnectMessage = connectMessage.ToString();
            msgToRepoLoad.fileMessage.xmlLoadMessage = fileMessage.ToString();
        }

        public void SetupQueryMessageToRepo(TestResults tr, string FileConnectAddress, string MessageConnectAddress, string loadType)
        {
            XElement fileMessage = new XElement("FileMessage");
            XElement connectMessage = new XElement("ConnectMessage");
            connectMessage.Add(new XElement("FileConnectAddress", FileConnectAddress));
            connectMessage.Add(new XElement("MessageConnectAddress", MessageConnectAddress));

            fileMessage.Add(new XElement("LoadType", "Upload"));
            fileMessage.Add(new XElement("LoadPath", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\..\\TestResults")));
            XElement filenames = new XElement("FileNames", new XElement("File", tr.LogName + "Summary.txt"));
            fileMessage.Add(filenames);
            msgToRepoQuery.fileMessage.xmlLoadMessage = fileMessage.ToString();
            msgToRepoQuery.sender = "Client";
            msgToRepoQuery.recipient = "Query";
            msgToRepoQuery.xmlConnectMessage = connectMessage.ToString();
            msgToRepoQuery.fileMessage.xmlLoadMessage = fileMessage.ToString();
        }


        public void THMsgProc()
        {
            while (true)
            {
                HiResTimer hrt = new HiResTimer();
                Message msg = msgReciever.TryGetTHMessage();
                Console.WriteLine("\n<--------------------------------------------------------->");
                Console.WriteLine("\nMessage recieved from test harness.");
                if (msg.recipient == "AboutLog")
                {
                    Console.WriteLine("\nDemonstrating test logs and results:");
                    TestResults tr = msgReciever.ParseResultMsg(msg);
                    Console.WriteLine(tr.log);

                    Console.WriteLine("\nDemonstrating querying results from repository:");
                    Console.WriteLine("\nSending message to repository...");
                    SetupQueryMessageToRepo(tr, "http://localhost:4040/ICommService/BasicHttp",
                                          "http://localhost:8080/ICommService/BasicHttp", "Upload");

                    msgSenderwithRepo.channel.PostMessage(msgToRepoQuery);

                    Console.WriteLine("\nWaiting for message from repository...");
                    hrt.Start();
                    Message repoReply = msgReciever.TryGetRepoMessage("Query");
                    hrt.Stop();
                    Console.WriteLine("");
                    Console.WriteLine("\nRecieved message in {0} microsec.", hrt.ElapsedMicroseconds);
                    Console.WriteLine("");
                    TestLoadStatus loadStatus = msgReciever.ParseLoadMsg(repoReply);
                    Console.WriteLine(loadStatus.loadMessage);
                    if (loadStatus.status)
                    {
                        using (FileStream fs = new FileStream(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\..\\TestResults", tr.LogName + "Summary.txt"), FileMode.Open))
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            Console.WriteLine(sr.ReadToEnd());
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\nDemonstrating dll load status in test harness:");
                    Console.WriteLine(msgReciever.ParseLoadMsg(msg).loadMessage);
                }
            }
        }





        static void Main(string[] args)
        {
            object locker = new object();
            lock (locker)
            {
                Client client = new Client();
                client.ClientSetUp(client);
                Thread childTH = new Thread(new ThreadStart(client.THMsgProc));
                childTH.Start();
                try
                {
                    client.GetRequestFiles();
                    foreach (info_In_One_Request_Folder info in client.folderInfoList)
                    {
                        HiResTimer hrt = new HiResTimer();
                        client.SetupLoadMessageToRepo(info, "http://localhost:4040/ICommService/BasicHttp",
                                                  "http://localhost:8080/ICommService/BasicHttp", "Download");
                        client.msgSenderwithRepo.channel.PostMessage(client.msgToRepoLoad);
                        Console.WriteLine("\nWaiting for message from repository");
                        hrt.Start();
                        Message msgFromRepo = client.msgReciever.TryGetRepoMessage("Load");
                        hrt.Stop();
                        TestLoadStatus loadStatus = client.msgReciever.ParseLoadMsg(msgFromRepo);
                        Console.WriteLine("");
                        Console.WriteLine("\nRecieved message from repository in {0} microsec", hrt.ElapsedMicroseconds);
                        Console.WriteLine("");
                        Console.WriteLine(loadStatus.loadMessage);
                        if (loadStatus.status)
                        {
                            Console.WriteLine("\nSending message to TH");
                            client.SetupMessageToTH(info, "http://localhost:4040/ICommService/BasicHttp", "http://localhost:8080/ICommService/BasicHttp");
                            client.msgSenderwithTH.channel.PostMessage(client.msgToTH);
                        }
                    }
                    childTH.Join();
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}", ex.Message);
                }
                Console.Write("\n\n");
            }
        }

        void ClientSetUp(Client client)
        {
            Console.WriteLine("Creating channel to send message to repository...");
            client.msgSenderwithRepo.CreateMessageChannel("http://localhost:8085/ICommService");
            Console.WriteLine("Creating channel to send message to TH...");
            client.msgSenderwithTH.CreateMessageChannel("http://localhost:8180/ICommService/BasicHttp");
            Console.WriteLine("Constructing host to communicate with TH and repository...");
            client.msgReciever.CreateMessageServiceHost("http://localhost:8080/ICommService/BasicHttp");
            client.msgReciever.open();
            Console.WriteLine("Constructing host to download/upload files...");
            client.fileServer.CreateFileServiceHost("http://localhost:4040/ICommService/BasicHttp");
            client.fileServer.open();
        }
    }
}
