/////////////////////////////////////////////////////////////////////////////
//  AppDomainManager.cs - manages child AppDomains within a thread         //
//  Language:     C#, VS 2015                                              //
//  Platform:     SurfaceBook, Windows 10 Pro                              //
//  Application:  Project4 for CSE681 - Software Modeling & Analysis       //
//  Author:       Jim Fawcett, CST 2-187, Syracuse University              //
//                Weijun Cai                                               //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module contains methods to create child appdomain and unload it 
 *   when executions in child domain complete.
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20 Sep 2016
 */


using System;
using System.Threading;
using System.Security.Policy;


namespace TestHarness
{
    ///////////////////////////////////////////////////////////////////
    // Callback class lives in parent AppDomain and is used to
    // receive messages from child AppDomain

    public class Callback : MarshalByRefObject
        {
            public void answer(string msg)
            {
                Console.Write("\n  reply is: {0}", msg);
            }
        }
        ///////////////////////////////////////////////////////////////////
        // class Creator creates child AppDomain and has brief 
        // conversation with the child

        public class AppdomainManager
        {
            private AppDomain child;


            public AppDomain createChildDomain()
            {
                AppDomainSetup domaininfo = new AppDomainSetup(); //APPDOMAINSETUP: Represents assembly binding information that can be added to an instance of AppDomain.
                domaininfo.ApplicationBase = Environment.CurrentDirectory;  // defines search path for assemblies

                //Create evidence for the new AppDomain from evidence of current

                Evidence adevidence = AppDomain.CurrentDomain.Evidence; //Defines the set of information that constitutes input to security policy decisions. This class cannot be inherited

                // Create Child AppDomain
                child = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);
                return child;
            }

            public void unloadChild(AppDomain ad)
            {
            string threadName = "Child thread:" + Thread.CurrentThread.ManagedThreadId.ToString();
            Console.WriteLine("\n\n({0})Unloading {1}", threadName, ad.FriendlyName);
                AppDomain.Unload(ad);
            }

            //test stub

            //static void Main(string[] args)
            //{


            //    try
            //    {
            //        AppDomain main = AppDomain.CurrentDomain;
            //        Console.Write("\n  Starting in AppDomain {0}\n", main.FriendlyName);

            //        // Create application domain setup information for new AppDomain

            //        AppDomainSetup domaininfo = new AppDomainSetup();    //Represents assembly binding information that can be added to an instance of AppDomain.
            //        domaininfo.ApplicationBase                   //Gets or sets the name of the directory containing the application
            //  = System.Environment.CurrentDirectory;  // defines search path for assemblies

            //        //Create evidence for the new AppDomain from evidence of current

            //        Evidence adevidence = AppDomain.CurrentDomain.Evidence;   //Defines the set of information that constitutes input to security policy decisions.

            //        // Create Child AppDomain

            //        AppDomain ad // ad is the child appdomain
            //  = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);

            //        //////////////////////////////////////////////////////////////////////
            //        //  Way to create ChildDomain using default evidence and domaininfo://
            //        //    AppDomain ad = AppDomain.CreateDomain("ChildDomain", null);   //
            //        //////////////////////////////////////////////////////////////////////

            //        ad.Load("DemoClassLibrary");   //Loads an Assembly given its display name.
            //        showAssemblies(ad);
            //        Console.Write("\n\n");

            //        // create proxy for Hello object in child AppDomain

            //        ObjectHandle oh //pass an object (in a wrapped state) between multiple application domains 
            //                        //without loading the metadata for the wrapped object in each AppDomain through which the ObjectHandle travels
            //          = ad.CreateInstance("DemoClassLibrary", "AppDomainDemo.hello");
            //        //Creates a new instance of the specified type in that domain. Parameters 
            //        //specify the assembly where the type is defined, and the name of the type.

            //        //CreateInstance return:An object that is a wrapper for the new instance specified by typeName. 
            //        //The return value needs to be unwrapped to access the real object.

            //        //CreateInstanceandUnwrap return: An instance of the object specified by typeName.can be used as proxy


            //        object ob = oh.Unwrap();    // unwrap creates proxy to ChildDomain
            //        Console.Write("\n  {0}", ob);        // ob is the proxy(an instance of Hello class)

            //        AppDomain.Unload(ad);
            //        Console.Write("\n\n");
            //    }
            //    catch (Exception except)
            //    {
            //        Console.Write("\n  {0}\n\n", except.Message);
            //    }
            //} }
        }
    }

