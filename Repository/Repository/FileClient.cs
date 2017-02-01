/////////////////////////////////////////////////////////////////////////////
//  FileClient.cs - WCF file streaming as a client                         //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains method to create communication channel to clients or test harness 
 *   and interact file transfer server to download or upload files through a proxy.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   IFileService.cs
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 05 Nov 2016
 */

using FileService;
using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Threading;


namespace Repository
{
    public class FileClient
    {
        IFileService channel;

        string SavePath = string.Empty;
        string ToSendPath = string.Empty;
        int BlockSize = 1024;
        byte[] block;


        public FileClient()
        {
            block = new byte[BlockSize];
            SavePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\SavedFiles");
            ToSendPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\SavedFiles");
        }



        public void CreateFileChannel(string url)
        {
            int tryCount = 0;
            int maxCount = 10;
            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 500000000;
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IFileService> factory
             = new ChannelFactory<IFileService>(binding, address);
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



        public void upload(string filename, string savePath)
        {
            object locker = new object();
            lock (locker)
            {
                string fqname = string.Empty;
                if (filename.EndsWith(".txt"))
                {
                    fqname = Directory.GetFiles(ToSendPath, filename)[0];
                }
                else
                    fqname = Path.Combine(ToSendPath, filename);

                using (var inputStream = new FileStream(fqname, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.filename = filename;
                    msg.savePath = savePath;
                    msg.transferStream = inputStream;
                    channel.upLoadFile(msg); // call service's uploadfile
                }

                Console.Write("\n  Uploaded file \"{0}\"", filename);
            }
        }



        public void download(string filename, string uploadPath)
        {

            object locker = new object();
            lock (locker)
            {
                int totalBytes = 0;

                try
                {
                    Stream strm = channel.downLoadFile(filename, uploadPath);  //call service's downloadfile
                    string rfilename = Path.Combine(SavePath, filename);
                    if (!Directory.Exists(SavePath))
                        Directory.CreateDirectory(SavePath);
                    using (var outputStream = new FileStream(rfilename, Path.GetExtension(filename) == ".txt" ? FileMode.Append : FileMode.Create))
                    {
                        while (true)
                        {
                            int bytesRead = strm.Read(block, 0, BlockSize);
                            totalBytes += bytesRead;
                            if (bytesRead > 0)
                                outputStream.Write(block, 0, bytesRead);
                            else
                                break;
                        }
                    }


                    Console.Write("\n  Received file \"{0}\" of {1} bytes", filename, totalBytes);

                }
                catch (Exception ex)
                {
                    Console.Write("\n  {0}\n", ex.Message);

                    throw ex;
                }
            }




            //TEST STUB

            //static void Main()
            //{
            //    Console.Write("\n  Client of SelfHosted File Stream Service");
            //    Console.Write("\n ==========================================\n");

            //    FileClient clnt = new FileClient();
            //    clnt.channel = CreateFileChannel("http://localhost:8000/StreamService");


            //    Console.Write("\n\n  Press key to terminate client");
            //    Console.ReadKey();
            //    Console.Write("\n\n");
            //    ((IChannel)clnt.channel).Close();
            //}
        }
    }
}

