using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace SuiteEngNS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
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
    }
}
