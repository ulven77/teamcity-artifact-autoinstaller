using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TeamcityArtifactAutoinstaller
{
    public partial class TeamcityArtifactAutoinstaller : ServiceBase
    {
        public TeamcityArtifactAutoinstaller()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ScheduleTask();
        }

        private static void ScheduleTask()
        {
            RunTask();
            Task.Delay(TimeSpan.FromMinutes(1))
            .ContinueWith(_ =>
            {
                ScheduleTask();
            });

        }

        private static void RunTask()
        {
            try
            {
                InstallProcess.CheckAndInstall(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception happened: " + ex.ToString());
            }
        }

        protected override void OnStop()
        {
        }
    }
}
