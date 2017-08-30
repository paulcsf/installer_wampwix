using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


using System.IO;
using System.Diagnostics;
using System.Reflection;

/*
 * PH Notes
 * http://www.codeproject.com/KB/install/Installation.aspx
 * 
 * Add installerclass to program you want to run after install.
 * Override the base installer class OnAfterInstall function:
 *         protected override void OnAfterInstall(System.Collections.IDictionary savedState)
 *         {
 *             base.OnAfterInstall(savedState);
 *             
 *             //Getting the directory onto which the application is installed. 
 *             Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
 *             //Executing the application in that acquired directory path.      
 *             Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location + "\\SuiteEngConf.exe"));
 *         }
 * 
 * Add the program as a custom action to the setup project.
 * 
 * */

namespace SuiteEngConfNS
{
    [RunInstaller(true)]
    public partial class SuiteEngConfInstaller : Installer
    {
        public SuiteEngConfInstaller()
        {
            InitializeComponent();
        }
        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            //Getting the directory onto which the application is installed. 
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            //Executing the application in that acquired directory path.      
            Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location + "\\SuiteEngConf.exe"));
        }
    }
}
