using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Reflection;

namespace WindowsServiceWrapper
{
    public partial class ServiceWrapper : ServiceBase
    {
        IList<Process> processMap = new List<Process>();
        Timer timer;
        public ServiceWrapper()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Log.Information(Environment.CurrentDirectory);
                var autoEvent = new AutoResetEvent(false);
                string path = ConfigurationManager.AppSettings["applicationpath"]; 
                string applicationPath = Path.GetFullPath(path);
                Log.Information(applicationPath);
                int instanceCount = int.Parse(ConfigurationManager.AppSettings["instancecount"]);
                string exepath = ConfigurationManager.AppSettings["executablepath"];
                Log.Information(applicationPath);
                for (int i = 0; i < instanceCount; i++)
                {
                    var proc = StartApplication(exepath,applicationPath,i);
                    this.processMap.Add(proc);

                    Log.Information("Started process with process id {0}", proc.Id);
                }
                Log.Information("Started {0} instance of application ", instanceCount);

                timer = new Timer(new TimerCallback(MonitorApplication),autoEvent,180000,180000);
                
            }
            catch (Exception exp)
            {
               Log.Error("Error Starting service");
               Log.Error(exp.Message);
               Environment.Exit(-1);
            }
           
        }

        private Process StartApplication(string exepath, string applicationPath, int instanceCount)
        {
            var internalProcess = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = exepath;
            startInfo.Arguments = applicationPath;
            startInfo.WorkingDirectory = Path.GetDirectoryName(applicationPath);
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            internalProcess.StartInfo = startInfo;
            internalProcess.EnableRaisingEvents = true;
            internalProcess.ErrorDataReceived += (object obj, DataReceivedEventArgs eventArgs) =>
            {
                Log.Error("Process error {0}", eventArgs.Data);
            };
            internalProcess.Exited += (object sender, EventArgs e) =>
            {
                int processId = internalProcess.Id;
                Log.Error("Process {0} exited ", internalProcess.Id);
                Log.Error("Error {0}", internalProcess.StandardError.ReadToEnd());
            };
            internalProcess.Start();

            return internalProcess;
        }
        
        private void MonitorApplication(object state)
        {
            Log.Information("Process monitor thread starting ");
            AutoResetEvent autoEvent = (AutoResetEvent)state;
            List<Process> exited = new List<Process>();
            List<Process> created = new List<Process>();
            foreach(var proc in processMap)
            {
                if (proc.HasExited)
                {
                    Log.Information("Process {0} exited. Spawning new process", proc.Id);
                    exited.Add(proc);
                    proc.Start();// this will start a new proc 
                    created.Add(proc);
                    Log.Information("New process {0} created",proc.Id);
                }
            }
            exited.ForEach(x=> processMap.Remove(x));
            created.ForEach(x=> processMap.Add(x));
            Log.Information("Process monitor thread ended ");
        }

        protected override void OnStop()
        {
            try
            {
                foreach (var internalProcess in processMap)
                {
                    if (!internalProcess.HasExited)
                        internalProcess?.Kill();
                }

                timer.Dispose();

                Console.WriteLine("Terminating the application...");
                Log.Information("Stopped process");
            }
            catch(Exception exp)
            {
                Log.Error(exp.Message);
            }

        }
    }
}
