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

                // Run once
                InstallProcess.CheckAndInstall();

                Console.WriteLine("Done, press any key to quit.");
                Console.ReadKey();
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
