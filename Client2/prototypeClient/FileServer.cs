/////////////////////////////////////////////////////////////////////////////
//  FileServer.cs - WCF file streaming as a server                         //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module implements IFileService interface, provides methods for creating 
 *   or closing host for the file service, file dowloading and uploading.
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

using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;



namespace FileService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class FileServer : IFileService
    {
        string filename = string.Empty;
      
        int BlockSize = 1024;
        byte[] block;
        ServiceHost host = null;

        public FileServer()
        {
            block = new byte[BlockSize];
        }



        public void upLoadFile(FileTransferMessage msg)//client pass in a FileTransferMessage class specifying the stream and file name 
        {
            string s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
           
           string savePath = Path.Combine(s, "..\\..\\..\\TestResults");
                      // which is the info this method uses to copy file from and save in the path specified by host
            int totalBytes = 0;

            filename = msg.filename;
            string rfilename = Path.Combine(savePath, filename);// the save path is hard coded: .\\sendfiles
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize); //read from stream, store in buffer, return the bytes read
                    totalBytes += bytesRead;                                      // read stream source get from FileTransferMessage.transferStream
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }

            Console.Write(
              "\n  Received file \"{0}\" of {1} bytes.",
              filename, totalBytes
            );
        }





        public Stream downLoadFile(string filename, string uploadPath)
        {
            object locker = new object();
            string ToSendPath = string.Empty;
            string sfilename = string.Empty;
            FileStream outStream = null;
            lock (locker)
            {
                string s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ToSendPath = Path.Combine(s, "..\\..\\..\\ToSend");
                sfilename = Path.Combine(ToSendPath, filename);
                //download file from path hard-coded in service: .\\tosendfiles
                if (File.Exists(sfilename))
                {
                    outStream = new FileStream(sfilename, FileMode.Open);
                }
                else
                    throw new Exception("open failed for \"" + sfilename + "\"");

                Console.Write("\n  Sent \"{0}\".", filename);
            }
                return outStream;
        }





        public void CreateFileServiceHost(string url)
        {
            // Can't configure SecurityMode other than none with streaming.
            // This is the default for BasicHttpBinding.
            //   BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            //   BasicHttpBinding binding = new BasicHttpBinding(securityMode);

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 50000000;
            Uri baseAddress = new Uri(url);
            Type service = typeof(FileServer);
            host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IFileService), binding, baseAddress);
        }


        public void open()
        {
            host.Open();
        }

        public void close()
        {
            host.Close();
        }
    }
}

