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
 *   This module contains the main for the program, methods to process file requests
 *   sent from clients or test harness to upload/download files and 
 *   set up reply indicating whether the load is successful.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   IMessageService.cs, HiResTimer.cs, 
 *                       MessageServer.cs, MessageClient.cs, FileClient.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 05 Nov 2016
 */

using HRTimer;
using MessageService;
using System;
using System.Runtime.InteropServices;
using System.Threading;


namespace Repository
{
    class Repository
    {

        [DllImport("USER32.DLL", SetLastError = true)]
        public static extern void SetWindowPos(
          IntPtr hwnd, IntPtr order,
          int xpos, int ypos, int width, int height,
          uint flags
        );
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();



        MessageServer msgReciever = new MessageServer();

        public Repository()
        {

            SetWindowPos(GetConsoleWindow(), (IntPtr)0, 100, 100, 600, 600, 0);
            Console.Title = "Repository";
        }




        public void ThreadProc(InternalMessage imsg) //all file loading exceptions are handled here
        {
            FileClient clnt = new FileClient(); ;
            HiResTimer hrt = new HiResTimer();
            MessageClient msgSender = new MessageClient();
            Message msgToClient = new Message();
            Console.WriteLine("");
            Console.WriteLine("===============================================================");
            try
            {
                clnt.CreateFileChannel(imsg.connectMessage.fileConnectAddress);
                Console.WriteLine("\n Identifying Load request message...");
                if (imsg.fileMessage.loadType == "Download")
                {
                    foreach (string file in imsg.fileMessage.fileNames)
                    {
                        clnt.download(file, imsg.fileMessage.loadPath);
                    }
                    msgToClient = msgSender.SetupMessages(true, "Files successfully downloaded by Repository.", imsg.recipient);
                }
                else
                {
                    foreach (string file in imsg.fileMessage.fileNames)
                    {
                        clnt.upload(file, imsg.fileMessage.loadPath);
                    }
                    msgToClient = msgSender.SetupMessages(true, "Files successfully uploaded from Repository.", imsg.recipient);
                }
            }
            catch (Exception ex)
            {
                msgToClient = msgSender.SetupMessages(false, "\n[Error message]:" + ex.Message, imsg.recipient);
            }
            finally
            {
                Console.WriteLine("\nConnecting to message channel...");
                msgSender.CreateMessageChannel(imsg.connectMessage.MessageConnectAddress);
                Console.WriteLine("\nSending message about loading status back...");
                hrt.Start();
                msgSender.channel.PostMessage(msgToClient);
                hrt.Stop();
                Console.WriteLine("");
                Console.WriteLine("\nMessage sent back in {0} microsec.", hrt.ElapsedMicroseconds);
                Console.WriteLine("");
            }
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Constructing host for message recieving from client and test harness");
            Repository repo = new Repository();
            repo.msgReciever.CreateMessageServiceHost("http://localhost:8085/ICommService");
            repo.msgReciever.open();

            Console.WriteLine("Start processing messages");
            while (true)
            {
                object locker = new object();
                lock (locker)
                {
                    Message msg = repo.msgReciever.GetMessage();
                    //recieve msg from client
                    InternalMessage imsg = repo.msgReciever.Parse(msg);
                    Thread child = new Thread(() => { repo.ThreadProc(imsg); });
                    child.Start();
                }
            }
        }
    }
}

