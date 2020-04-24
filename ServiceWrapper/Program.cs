using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceWrapper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                       .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\servicewrapper-{Date}.log")
                       .MinimumLevel.Is(Serilog.Events.LogEventLevel.Verbose)
                       .CreateLogger();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServiceWrapper()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
