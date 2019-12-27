using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace EngineNS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UEHandler);

#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new Engine() 
			};
            ServiceBase.Run(ServicesToRun);
#else
            Engine service = new Engine();
            service.OnStartThreadCode();
            // Put a breakpoint on the following line to always catch
            // your service when it has finished its work
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif 
        }

        static void UEHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            //Console.WriteLine("MyHandler caught : " + e.Message);
            //Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
            System.Diagnostics.EventLog.WriteEntry(GetSource(),e.Message);
        }

        private static string GetSource()
        {
            //https://stackoverflow.com/a/25725394
            try
            {
                var assembly = System.Reflection.Assembly.GetEntryAssembly();

                // GetEntryAssembly() can return null when called in the context of a unit test project.
                // That can also happen when called from an app hosted in IIS, or even a windows service.

                if (assembly == null)
                {
                    assembly = System.Reflection.Assembly.GetExecutingAssembly();
                }


                if (assembly == null)
                {
                    // From http://stackoverflow.com/a/14165787/279516:
                    assembly = new System.Diagnostics.StackTrace().GetFrames().Last().GetMethod().Module.Assembly;
                }

                if (assembly == null) { return "Unknown"; }

                return assembly.GetName().Name;
            }
            catch
            {
                return "Unknown";
            }
        }

    }
}
