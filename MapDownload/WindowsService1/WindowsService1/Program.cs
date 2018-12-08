using System;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;
using System.Configuration;
using Angels.Application.Service.Host;

namespace WindowsService1
{
    class Program
    {

        private const string ListeningOn = "http://localhost:4244/";

        static void Main(string[] args)
        {
            var TickServiceHost = new TickServiceHost();
            //#if DEBUG
            TickServiceHost.Init().Start("http://*:4244/");
            "MapDownloadService is Listenting....".Print();
            //Process.Start(ListeningOn);


            Console.ReadLine();

            //#else
            //            //When in RELEASE mode it will run as a Windows Service with the code below

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new WinService(TickServiceHost, ListeningOn)
            //};
            //ServiceBase.Run(ServicesToRun);
            //Console.ReadLine();

            //#endif


        }
    }
}
