/////////////////////////////////////////////////////////////////////////////
//  MainWindow.xaml.cs - Remote client with GUI                            //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains the main for the program, methods to set up messages sent to 
 *   test harness and repository as well as the event handlers of GUI event.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   IMessageService.cs, HiResTimer.cs, ClientUtility.cs
 *                       MessageServer.cs, MessageClient.cs, FileServer.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 05 Nov 2016
 */

using FileService;
using HRTimer;
using MessageService;
using Microsoft.Win32;
using prototypeClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.ComponentModel;

namespace ClientGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        ClientUtility clientUtil = new ClientUtility();
       
        Message msgToTH = new Message();
        Message msgToRepoLoad = new Message();
        Message msgToRepoQuery = new Message();
        List<string> testCodeList = new List<string>();
       
        

        MessageServer msgReciever = new MessageServer();

        MessageClient msgSenderwithRepo = new MessageClient();
        MessageClient msgSenderwithTH = new MessageClient();
        FileServer fileServer = new FileServer();
        string subDllFolder = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            btnSendReq.IsEnabled = false;
            Closing += OnClosingWindow;
        }

        private void OnClosingWindow(object sender, CancelEventArgs e)
        {
            if (msgReciever.IsHostStart() && fileServer.IsHostStart())
            {
                msgReciever.close();
                fileServer.close();
            }
        }

        void ShowLoadMsg(string s)
        {
            StringLoadMsg.Text = s; 
        }

        void ShowStringResult(string s)
        {
            StringResult.Text = s;
        }

        private void btnGenerateReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XElement request = new XElement("TestRequest",
                                               new XElement("RequestName", txtRequestName.Text),
                                               new XElement("Author", txtAuthortName.Text));

                foreach (var test in TestsPanel.Children.OfType<StackPanel>())
                {
                    XElement singleTest = new XElement("Test", new XAttribute("Name", test.Children.OfType<DockPanel>().First().Children.OfType<TextBox>().ElementAt(0).Text),
                                                       new XElement("TestDriver", test.Children.OfType<DockPanel>().ElementAt(1).Children.OfType<TextBox>().ElementAt(0).Text));
                    testCodeList.Add(test.Children.OfType<DockPanel>().ElementAt(1).Children.OfType<TextBox>().ElementAt(0).Text);
                    foreach (var testCode in test.Children.OfType<DockPanel>().ElementAt(2).Children.OfType<StackPanel>().First().Children.OfType<TextBox>())
                    {
                        singleTest.Add(new XElement("Library", testCode.Text));
                        testCodeList.Add(testCode.Text);
                    }
                    request.Add(singleTest);
                }


                msgToTH.testMessage.xmlRequest = request.ToString();

                xmlRequest.Text = request.ToString();
                btnSendReq.IsEnabled = true;
            }
            catch 
            {
                MessageBox.Show("Invalid Input", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnSendReq_Click(object sender, RoutedEventArgs e)
        {
            Thread childTH = new Thread(new ThreadStart(THMsgProc));
            childTH.Start();

           
                HiResTimer hrt = new HiResTimer();
                clientUtil.SetupLoadMessageToRepo(msgToRepoLoad, testCodeList, "http://localhost:4244/ICommService/BasicHttp",
                                          "http://localhost:8284/ICommService/BasicHttp", "Download");

                msgSenderwithRepo.channel.PostMessage(msgToRepoLoad);
                Thread childRepo = new Thread(RepoMsgProc);
                childRepo.Start();   
        }

        private void RepoMsgProc()
        {
            Console.WriteLine("\nWaiting for message from repository");
           
            Message msgFromRepo = msgReciever.TryGetRepoMessage("Load");
            
            TestLoadStatus loadStatus = msgReciever.ParseLoadMsg(msgFromRepo);
            Console.WriteLine("");
            Console.WriteLine("\nRecieved message from repository.");
            Console.WriteLine("");
            Console.WriteLine(loadStatus.loadMessage);

            if (loadStatus.status)
            {
                Console.WriteLine("\nSending message to TH");
                clientUtil.SetupMessageToTH(msgToTH, "http://localhost:4244/ICommService/BasicHttp", "http://localhost:8284/ICommService/BasicHttp");
                msgSenderwithTH.channel.PostMessage(msgToTH);
            }
        }

        private void btnAddTest_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel();
            
            sp.Height = 80;
            DockPanel dp1 = new DockPanel();
            Label lbl = new Label { Content = "Test Name" };
            dp1.Children.Add(lbl);
            DockPanel.SetDock(lbl, Dock.Left);
            TextBox tb = new TextBox { Width = 150, Height = 20 };
            dp1.Children.Add(tb);
            DockPanel.SetDock(tb, Dock.Right);
            sp.Children.Add(dp1);

            DockPanel dp2 = new DockPanel();
            Label TDlbl = new Label { Content = "Test Driver" };
            dp2.Children.Add(TDlbl);
            DockPanel.SetDock(TDlbl, Dock.Left);
            Button btnAddTD = new Button { Width = 100, Height = 20,  Content = "Browse File"};
            btnAddTD.Click += btn_addHandlerTD;
            dp2.Children.Add(btnAddTD);
            DockPanel.SetDock(btnAddTD, Dock.Right);
            sp.Children.Add(dp2);

            DockPanel dp3 = new DockPanel();
            Label TClbl = new Label { Content = "Test Code" };
            dp3.Children.Add(TClbl);
            DockPanel.SetDock(TClbl, Dock.Left);
            Button btnAddTC = new Button { Width = 100, Height = 20, Content = "Browse File" };
            btnAddTC.IsEnabled = false;
            btnAddTC.Click += btn_addHandlerTC;
            dp3.Children.Add(btnAddTC);
            DockPanel.SetDock(btnAddTC, Dock.Right);
            sp.Children.Add(dp3);

            TestsPanel.Children.Add(sp);
            TestsPanel.Height += 80;
            TestsPanel.Children.Add(new Separator());
        }

        private void btn_addHandlerTC(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Multiselect = true;
            openDlg.Filter = "DLL Files |*.dll";
            fileServer.setToSendPath(subDllFolder);
            string fullpath = subDllFolder;
            openDlg.InitialDirectory = fullpath;

            if (openDlg.ShowDialog() == true)
            {
                var parent = ((Button)sender).Parent;
                DockPanel dp = (DockPanel)parent;
                StackPanel sp = new StackPanel();
                
                dp.Children.Remove((Button)sender);

                foreach (var testCode in openDlg.SafeFileNames)
                {
                    TextBox tb = new TextBox { Width = 150, Height = 20 };
                    sp.Children.Add(tb);
                    DockPanel.SetDock(tb, Dock.Right);
                    tb.Text = testCode;
                    sp.Height += 20;
                }

                dp.Children.Add(sp);
                DockPanel.SetDock(sp, Dock.Right);
                dp.Height += openDlg.SafeFileNames.Count() * 20; 
            }
        }

        private void btn_addHandlerTD(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "DLL Files |*.dll";
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\..\\ToSend");
            string fullpath = System.IO.Path.GetFullPath(path);
            openDlg.InitialDirectory = fullpath;
          
            if (openDlg.ShowDialog() == true)
            {
                TextBox tb = new TextBox { Width = 150, Height = 20 };
                var parent = ((Button)sender).Parent;
                DockPanel dp = (DockPanel)parent;
                dp.Children.Remove((Button)sender);
                dp.Children.Add(tb);
                DockPanel.SetDock(tb, Dock.Right);

                StackPanel sp = (StackPanel)dp.Parent;
                var testCodebtn = sp.Children.OfType<DockPanel>().ElementAt(2).Children.OfType<Button>().First();
                testCodebtn.IsEnabled = true;


                tb.Text = openDlg.SafeFileName;
                subDllFolder = System.IO.Path.GetDirectoryName(openDlg.FileName);         
            }

        }

        private void btnSetHost_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Constructing host to communicate with TH and repository...");
            msgReciever.CreateMessageServiceHost("http://localhost:8284/ICommService/BasicHttp");
            msgReciever.open();

            Console.WriteLine("Constructing host to download/upload files...");
            fileServer.CreateFileServiceHost("http://localhost:4244/ICommService/BasicHttp");
            fileServer.open();

            Console.WriteLine("Creating channel to send message to repository...");
            msgSenderwithRepo.CreateMessageChannel("http://localhost:8085/ICommService");

            Console.WriteLine("Creating channel to send message to TH...");
            msgSenderwithTH.CreateMessageChannel("http://localhost:8180/ICommService/BasicHttp");
            ((Button)sender).IsEnabled = false;
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
                    clientUtil.SetupQueryMessageToRepo(msgToRepoQuery, tr, "http://localhost:4244/ICommService/BasicHttp",
                                          "http://localhost:8284/ICommService/BasicHttp", "Upload");
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
                    using (FileStream fs = new FileStream(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\..\\TestResults", tr.LogName + "Summary.txt"), FileMode.Open))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        Dispatcher.Invoke(
                           new Action<string>(ShowStringResult),
                           System.Windows.Threading.DispatcherPriority.Background,
                           new string[] { sr.ReadToEnd() });
                    }
                }
                else
                {
                    Console.WriteLine("\nDemonstrating dll load status in test harness:");
                    Dispatcher.Invoke(
                           new Action<string>(ShowLoadMsg),
                           System.Windows.Threading.DispatcherPriority.Background,
                           new string[] { msgReciever.ParseLoadMsg(msg).loadMessage });              
                }
            }
        }
    }
}
