using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamcityArtifactAutoinstaller
{
    public class TeamcityProjectConfiguration
    {
        public string TeamCityBaseUrl { get; set; }

        public string TeamCityProjectId { get; set; }

        public string InstallPath { get; set; }

        public string InstallCommand { get; set; }
    }
}
