using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TeamcityArtifactAutoinstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.FirstOrDefault() == "-c")
            {
                // Console mode

                while (true)
                {
                    // Run once
                    Console.WriteLine("Check and install BEGIN");
                    InstallProcess.CheckAndInstall();
                    Console.WriteLine("Check and install END");

                    Console.WriteLine("Done, 'q' to quit, any other key to check again.");
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Q)
                    {
                        break;
                    }
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new TeamcityArtifactAutoinstaller() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
