using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TeamcityArtifactAutoinstaller
{
    public static class InstallProcess
    {
        private static Dictionary<string, string> lastCheckedVersion = new Dictionary<string, string>();

        public static void CheckAndInstall()
        {
            Console.WriteLine("Check and install");
            foreach (var project in Properties.Settings.Default.TeamcityProjects)
            {
                WebClient wc = new WebClient();
                var versionUrl = string.Format("{0}/httpAuth/app/rest/buildTypes/id:{1}/builds/status:SUCCESS/number", project.TeamCityBaseUrl, project.TeamCityProjectId);
                var versionString = wc.DownloadString(versionUrl);
                if (lastCheckedVersion.ContainsKey(project.TeamCityProjectId))
                {
                    if (lastCheckedVersion[project.TeamCityProjectId] == versionString)
                    {
                        // Nothing has changed
                        continue;
                    }
                }
                else
                {
                    // Startup, just remember current version but don't install
                    lastCheckedVersion[project.TeamCityProjectId] = versionString;
                    continue;
                }

                // We found a new version for this project

                // Download artifact
                var artifactUrl = string.Format("{0}/repository/downloadAll/{1}/{2}", project.TeamCityBaseUrl, project.TeamCityProjectId, versionString);
                var zipFileName = string.Format("{0}_{1}.zip", project.TeamCityProjectId, versionString);
                var unzipDir = string.Format("{0}_{1}", project.TeamCityProjectId);
                var zipPath = Path.Combine(project.InstallPath, zipFileName);
                var unzipDirPath = Path.Combine(project.InstallPath, unzipDir);
                wc.DownloadFile(artifactUrl, zipPath);

                ZipFile.ExtractToDirectory(zipFileName, unzipDirPath);

                // Store current version
                lastCheckedVersion[project.TeamCityProjectId] = versionString;
            }
        }
    }
}
