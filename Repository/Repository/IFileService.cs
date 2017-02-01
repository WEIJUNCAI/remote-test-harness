/////////////////////////////////////////////////////////////////////////////
//  IFileService.cs - WCF file streaming service contract                  //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module defines operation contract for file downloading and uploading operations, 
 *   as well as the SOAP message to send during the operation.
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

using System.IO;
using System.ServiceModel;

    namespace FileService
    {
        [ServiceContract(Namespace = "http://CSE681")]
        public interface IFileService
        {
            [OperationContract(IsOneWay = true)]
            void upLoadFile(FileTransferMessage msg);
            [OperationContract]
            Stream downLoadFile(string filename, string uploadPath);
        }


        [MessageContract]
        public class FileTransferMessage
        {
            [MessageHeader(MustUnderstand = true)]
            public string filename { get; set; }

            [MessageBodyMember(Order = 1)]
            public Stream transferStream { get; set; }

        [MessageHeader(MustUnderstand = true)]
        public string savePath { get; set; }
    }
    }

