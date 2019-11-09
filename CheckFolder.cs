using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using System.IO;

namespace k163825_Q5
{
    public partial class CheckFolder : ServiceBase
    {
        System.Timers.Timer timer;
        string TestFolder = ConfigurationManager.AppSettings["TestFolder"];
        string BackupFolder = ConfigurationManager.AppSettings["Backupfolder"];
        string fileName = ConfigurationManager.AppSettings["LogPath"];
        

        DateTime lastModifiedDate;
        DateTime startTime;
        public CheckFolder()
        {
            InitializeComponent();
            if (!Directory.Exists(TestFolder) && !Directory.Exists(BackupFolder))
            {
                Directory.CreateDirectory(TestFolder);
                Directory.CreateDirectory(BackupFolder);
            }
            else
            {
                // Take an action which will affect the write time.
                Directory.SetLastWriteTime(TestFolder, DateTime.Now);
                startTime = DateTime.Now;
                Debug.WriteLine(startTime);
                
            }
            
            timer = new System.Timers.Timer();
            timer.Interval = 1 * 60 * 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(WorkProcess); 

        }

        private void WorkProcess(object sender, ElapsedEventArgs e)
        {
            lastModifiedDate = Directory.GetLastWriteTime(TestFolder);
            string sourceFile = System.IO.Path.Combine(TestFolder, fileName);
            string dstFile = System.IO.Path.Combine(BackupFolder, fileName);
            int result = DateTime.Compare(lastModifiedDate, startTime);
            if (result == 0)
            {
                SetTimer();
                Debug.WriteLine("Same Time");
            }
            else
            {
                string[] files = System.IO.Directory.GetFiles(TestFolder);
                
                Debug.WriteLine("Different Time");
                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    
                    fileName = System.IO.Path.GetFileName(s);
                    FileSystemInfo fsi = new FileInfo(s);
                    if(fsi.LastWriteTime > startTime)
                    {
                        Debug.WriteLine("inside loop"+ fsi.LastWriteTime.ToString());
                        dstFile = System.IO.Path.Combine(BackupFolder, fileName);
                        System.IO.File.Copy(s, dstFile, true);
                    }
                    
                    
                }
                startTime = DateTime.Now;
                Directory.SetLastWriteTime(TestFolder, DateTime.Now);

            }
        }

        protected override void OnStart(string[] args)
        {
            timer.AutoReset = true;
            timer.Enabled = true;
            LogService("Service Started");
        }

        
        
        protected override void OnStop()
        {
            timer.AutoReset = false;
            timer.Enabled = false;
            LogService("Service Stopped");
            OnStopTime(Directory.GetLastWriteTime(TestFolder).ToString());
        }
        private void SetTimer()
        {
            double inter = GetNextInterval();
            timer.Interval = inter;
            timer.Start();
            Debug.WriteLine("Interval Updated");

        }
        private double GetNextInterval()
        {
            double newTimer = 0;
            if(timer.Interval < 60 *60* 1000)
            {
                timer.Interval = timer.Interval + (2*60*1000);
                newTimer = timer.Interval;

            }
            else
            {
                newTimer = 1*60*1000;
            }
            return newTimer;
        }
        
        private void LogService(string content)
        {
            string logPath = ConfigurationManager.AppSettings["LogPath"];
            //Folder Must Exists
            FileStream fs = new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
        }
        private void OnStopTime(string content)
        {
            string logPath = ConfigurationManager.AppSettings["LogPath"];
            //Folder Must Exists
            FileStream fs = new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();

        }
    }
}
