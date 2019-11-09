using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace k163825_Q5
{
    public partial class MailSenderService : ServiceBase
    {
        System.Timers.Timer timer;
        string TestFolder = ConfigurationManager.AppSettings["TestFolder"];
        string fileName = ConfigurationManager.AppSettings["LogPath"];
        DateTime startTime;
        DateTime lastModifiedDate;
        public MailSenderService()
        {
            InitializeComponent();
            Directory.SetLastWriteTime(TestFolder, DateTime.Now);
            startTime = DateTime.Now;
            timer = new System.Timers.Timer();
            timer.Interval = 15 * 60 * 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(WorkProcess);
        }
        private void WorkProcess(object sender, ElapsedEventArgs e)
        {
            string receiverAddr = ConfigurationManager.AppSettings["ReceiverMail"];
            string subjectMail = "File/Folder Update";
            string mailMsgBody = "";
            List<String> fn = new List<string>();
            List<String> fs = new List<string>();
            lastModifiedDate = Directory.GetLastWriteTime(TestFolder);
            int result = DateTime.Compare(lastModifiedDate, startTime);
            if (result == 0)
            {

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
                    FileInfo fsi = new FileInfo(s);
                    int res = DateTime.Compare(fsi.LastWriteTime, startTime);
                    if (res > 0)
                    {
                        Debug.WriteLine("inside loop" + fsi.LastWriteTime.ToString());
                        fn.Add(fileName + " SizeofFile: " + fsi.Length.ToString()+ "\n");
                        fs.Add(fsi.Length.ToString());
                    }


                }
                

                foreach (var s in fn)
                {
                    mailMsgBody += s + "\n";
                }
                fn = null;
                startTime = DateTime.Now;
                Directory.SetLastWriteTime(TestFolder, DateTime.Now);
                genrateEmail(receiverAddr, subjectMail, mailMsgBody);

            }




        }
        private void genrateEmail(string receiverAddr, string subjectMail, string mailMsgBody)
        {
            System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient();
            smtpClient.EnableSsl = true;
            MailMessage mailMesg = new MailMessage();
            System.Net.Mime.ContentType XMLtype = new System.Net.Mime.ContentType("text/html");
            mailMesg.BodyEncoding = System.Text.Encoding.Default;
            mailMesg.To.Add(receiverAddr);
            mailMesg.Priority = System.Net.Mail.MailPriority.High;
            mailMesg.Subject = subjectMail;
            mailMesg.Body = mailMsgBody;
            mailMesg.IsBodyHtml = true;
            System.Net.Mail.AlternateView alternateView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(mailMsgBody, XMLtype);
            smtpClient.Send(mailMesg);

            LogService("Mail Sent");
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
    }
}
