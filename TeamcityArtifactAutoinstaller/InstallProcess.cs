using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TeamcityArtifactAutoinstaller
{
    public static class InstallProcess
    {
        private static Dictionary<string, string> lastCheckedVersion = new Dictionary<string, string>();
        private static readonly ILog log = LogManager.GetLogger(typeof(InstallProcess));

        public static void CheckAndInstall()
        {
            Console.WriteLine("Check and install");
            log.Debug("CheckAndInstall BEGIN");

            foreach (var project in Properties.Settings.Default.TeamcityProjects)
            {
                log.DebugFormat("Start project {0}", project.TeamCityProjectId);

                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential(project.TeamCityUserName, project.TeamCityPassword);
                var versionUrl = string.Format("{0}/httpAuth/app/rest/buildTypes/id:{1}/builds/status:SUCCESS/number", project.TeamCityBaseUrl, project.TeamCityProjectId);
                var versionString = wc.DownloadString(versionUrl);

                log.DebugFormat("versionString = {0}", versionString);

                if (lastCheckedVersion.ContainsKey(project.TeamCityProjectId))
                {
                    if (lastCheckedVersion[project.TeamCityProjectId] == versionString)
                    {
                        // Nothing has changed
                        log.DebugFormat("Nothing has changed");
                        continue;
                    }
                }
                else
                {
                    // Startup, just remember current version but don't install
                    lastCheckedVersion[project.TeamCityProjectId] = versionString;
                    log.DebugFormat("Startup setting baseline version {0}", versionString);
                    continue;
                }

                // We found a new version for this project

                // Download artifact
                var artifactUrl = string.Format("{0}/repository/downloadAll/{1}/{2}", project.TeamCityBaseUrl, project.TeamCityProjectId, versionString);
                var zipFileName = string.Format("{0}_{1}.zip", project.TeamCityProjectId, versionString);
                var unzipDir = string.Format("{0}_{1}", project.TeamCityProjectId, versionString);
                var zipPath = Path.Combine(project.InstallPath, zipFileName);
                var unzipDirPath = Path.Combine(project.InstallPath, unzipDir);
                wc.DownloadFile(artifactUrl, zipPath);

                log.DebugFormat("File {0} downloaded", artifactUrl);

                ZipFile.ExtractToDirectory(zipPath, unzipDirPath);
                File.Delete(zipPath);

                log.DebugFormat("File unzipped to {0}", unzipDirPath);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = project.InstallPath;
                startInfo.FileName = Path.Combine(project.InstallPath, project.InstallCommand);
                startInfo.Arguments = unzipDir;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;

                log.DebugFormat("Starting {0} with arguments {1}", startInfo.FileName, startInfo.Arguments);

                var process = Process.Start(startInfo);

                process.WaitForExit(5 * 60 * 1000); // wait 5 minutes

                log.Debug("Process execution done");

                var stdOutResponse = process.StandardOutput.ReadToEnd();

                log.Debug("Start sending mails");
                MailMessage m = new MailMessage();
                foreach (var recipient in Properties.Settings.Default.NotificationEmails)
                {
                    m.To.Add(new MailAddress(recipient));
                }
                if (process.ExitCode == 0)
                {
                    m.Subject = string.Format("{0} version {1} automatically deployed", project.TeamCityProjectId, versionString);
                }
                else
                {
                    m.Subject = string.Format("FAILED DEPLOY {0} version {1} failed with ExitCode = {2}", project.TeamCityProjectId, versionString, process.ExitCode);
                }
                m.Body = Properties.Settings.Default.MailBody;
                using (var attachementStream = new MemoryStream(Encoding.UTF8.GetBytes(stdOutResponse)))
                {
                    m.Attachments.Add(new Attachment(attachementStream, "install-log.txt"));

                    SmtpClient smtp = new SmtpClient();
                    smtp.Send(m);
                }

                log.Debug("Mail sending done");
                // Store current version
                lastCheckedVersion[project.TeamCityProjectId] = versionString;
            }

            log.Debug("CheckAndInstall END");
        }
    }
}
