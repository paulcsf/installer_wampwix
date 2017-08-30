using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
// requires Reference System.configuration
using System.Configuration;
// requires Reference System.Management
using System.Management;
using System.Data.Odbc;





namespace SuiteEngConfNS
{
    public partial class SuiteEngConf : Form
    {
        public string SuiteEngPath;
        
        public SuiteEngConf()
        {
            InitializeComponent();
            txtservicename.Text = "SuiteEng";

            System.Timers.Timer t = new System.Timers.Timer(1000);
            t.Elapsed += new System.Timers.ElapsedEventHandler(UpdateStatus);
            t.Start();

            //serviceCntl.
            serviceCntl.ServiceName = txtservicename.Text;
            try
            {
                txtstatus.Text = this.serviceCntl.Status.ToString();
            }
            catch (Exception e)
            {
                txtstatus.Text = "error or service not installed.";
            }
//            string appName = Environment.GetCommandLineArgs()[0];
//            string exePath = System.IO.Path.Combine(Environment.CurrentDirectory, appName);
//            exePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory)));
//            exePath=exePath+"\\SuiteEng\\bin\\release\\SuiteEng.exe";




            //find path of SuiteEng
            string[] st = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Services").GetSubKeyNames();
            StringBuilder sb = new StringBuilder();
            foreach( string key in st )
            {
                if (key != txtservicename.Text)
                    continue;
                object imagePath;
                object DisplayName;

                if ( (imagePath =Microsoft.Win32.Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Services\\" + key).GetValue( "ImagePath"))== null )
                    continue;

            /*    if ( (DisplayName =Microsoft.Win32.Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Services\\" + key).GetValue( "DisplayName"))== null )
                    DisplayName = "Unknown";

                sb.Append( "Service name=");
                sb.Append( key );
                sb.Append( " | ");

                sb.Append( "DisplayName=");
                sb.Append( DisplayName.ToString() );
                sb.Append( " | ");

                sb.Append( "Service image=");*/
                sb.Append( imagePath.ToString() );

                //sb.Append( Environment.NewLine );
            }
            SuiteEngPath = sb.ToString();
            SuiteEngPath = SuiteEngPath.Trim("\"'".ToCharArray());
            if (SuiteEngPath != null)
                txtfname.Text = SuiteEngPath;

            //now get SuiteEng.exe.config and load values
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(SuiteEngPath);
                /*string sectionName = "connectionStrings";
                Configuration config = ConfigurationManager.OpenExeConfiguration(exePath); 
                ConnectionStringsSection section = config.GetSection(sectionName) as ConnectionStringsSection;
                if (!section.SectionInformation.IsProtected)
                {
                    // Encrypt the section.
                    section.SectionInformation.ProtectSection(
                        "DataProtectionConfigurationProvider");
                    // Save the current configuration.
                    config.Save();
                    ConfigurationManager.RefreshSection(sectionName);
                }*/
                ConnectionStringSettingsCollection settings = config.ConnectionStrings.ConnectionStrings;
                if (settings != null)
                {
                    /*foreach (ConnectionStringSettings cs in settings)
                    {
                        LogEventLocal(cs.Name);
                        LogEventLocal(cs.ProviderName);
                        LogEventLocal(cs.ConnectionString);
                    }*/
                    ConnectionStringSettings cs = settings[0];
                    //cs.ConnectionString contains our parameters
                    //  parse it out to the boxes.
                    //txtip.t
                    string [] split = null;
                    split = cs.ConnectionString.Split(";".ToCharArray());
                    string [] iparr = null;
                    string [] poarr = null;
                    string [] usarr = null;
                    string [] psarr = null;
                    string [] dbarr = null;

                    foreach (string s in split)
                    {
                        if (s.IndexOf("SERVER") >= 0)
                            iparr = s.Split("=".ToCharArray());
                        else if (s.IndexOf("DATABASE") >= 0)
                            dbarr = s.Split("=".ToCharArray());
                        else if (s.IndexOf("PORT") >= 0)
                            poarr = s.Split("=".ToCharArray());
                        else if (s.IndexOf("UID") >= 0)
                            usarr = s.Split("=".ToCharArray());
                        else if (s.IndexOf("PASSWORD") >= 0)
                            psarr = s.Split("=".ToCharArray());
                    }
                    txtip.Text=iparr[1].Trim();
                    txtdbname.Text = dbarr[1].Trim();
                    txtuser.Text = usarr[1].Trim();
                    txtpass.Text = psarr[1].Trim();
                    txtpasshidden.Text = psarr[1].Trim();
                    txtdbport.Text = poarr[1].Trim();

                    //Lbl_error.Text += (DBConStr);
                }
                else
                {
                    Lbl_error.Text += ("No Connection Strings Settings\r\n");
                }
            }
            catch (ConfigurationErrorsException e)
            {
                Lbl_error.Text += ("GetAppSettings: ConfigurationErrorsException: " + e.ToString()+"\r\n");
                Lbl_error.Text += (SuiteEngPath + "\r\n");
            }
            catch (Exception e)
            {
                Lbl_error.Text += ("GetAppSettings: Exception: " + e.ToString()+"\r\n");
                Lbl_error.Text += (SuiteEngPath + "\r\n");
            }
        }

        //didn't work.
        string ServicePath(string serviceName)
        {
            string ret = null;
            ManagementObjectCollection Coll;
            using(ManagementObjectSearcher Searcher = new
            ManagementObjectSearcher("SELECT PathName from Win32_Service where DisplayName =" + "\"" + serviceName + "\""))
            {
                foreach(ManagementObject service in Searcher.Get())
                {
                    ret = service["PathName"].ToString();
                }
            }
            return ret;
        }

        //Timer callback function
        private void UpdateStatus(object sender,System.Timers.ElapsedEventArgs e)
        {
            this.serviceCntl.Refresh();
            try
            {
                txtstatus_update(this.serviceCntl.Status.ToString());
            }
            catch (Exception ex)
            {
                txtstatus_update("error or service not installed.");
            }
        }

        /* If the calling thread is different from the thread that
            created the TextBox control, this method passes in the
            the SetText method to the SetTextCallback delegate and 
            passes in the delegate to the Invoke method.*/
        public delegate void txtstatus_update_Delegate(string a);
        public void txtstatus_update(string a)
        {
            if (txtstatus.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                txtstatus_update_Delegate theDelegate = new txtstatus_update_Delegate(txtstatus_update);
                this.Invoke(theDelegate, new object[] { a }); 
            }
            else
                txtstatus.Text = a;
        }


        private void btn_fname_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "config files (*.config)|*.config|All files (*.*)|*.*";
            dialog.InitialDirectory = Environment.CurrentDirectory;
            dialog.Title = "Select service config file";
//            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : null;
            if (dialog.ShowDialog() == DialogResult.OK)
                txtfname.Text = dialog.FileName;
            else
                txtfname.Text = "";

        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            try
            {
                this.serviceCntl.Start();
                txtstatus.Text = "Starting";
            }
            catch (Exception ex)
            {
            }
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            try
            {
                this.serviceCntl.Stop();
                txtstatus.Text = "Stopping";
            }
            catch (Exception ex)
            {
            }
        }

        private string sqlClean(string txt)
        {
            //return txt.Replace("--", "").Replace(";", "").Replace(".", "");
            return txt.Replace("--", "").Replace(";", "");
        }

        private void btn_testdb_Click(object sender, EventArgs e)
        {
            Lbl_error.Text = "Testing connection.";
            this.Refresh();
            string pass;

            txtpass.Text = sqlClean(txtpass.Text);
            txtpassconf.Text = sqlClean(txtpassconf.Text);

            if (txtpass.Text == txtpassconf.Text)
                pass=txtpass.Text;
            else if (txtpass.Text==txtpasshidden.Text)
                pass=txtpass.Text;
            else if(txtpass.Text!=txtpassconf.Text)
            {
                Lbl_error.Text = "Passwords do not match.";
                return;
            }
            else if (txtpass.Text != txtpasshidden.Text)
            {
                Lbl_error.Text = "Password has been changed.";
                return;
            }

            if (txtip.Text.Length > 0)
            {
                if (txtuser.Text.Length > 0 && txtpass.Text.Length > 0)
                {
                    txtip.Text = sqlClean(txtip.Text);
                    txtpass.Text = sqlClean(txtpass.Text);
                    txtuser.Text = sqlClean(txtuser.Text);
                    txtdbname.Text = sqlClean(txtdbname.Text);
                    txtdbport.Text = sqlClean(txtdbport.Text);
                }
                else
                {
                    return;
                }
                try
                {
                    string DBConStr = "DRIVER={MySQL ODBC 5.2 Unicode Driver};" +
                        "SERVER=" + txtip.Text.Trim() + ";" +
                        "DATABASE=" + txtdbname.Text.Trim() + ";" +
                        "PORT=" + txtdbport.Text.Trim() + ";" +
                        "UID=" + txtuser.Text.Trim() + ";" +
                        "PASSWORD=" + txtpass.Text.Trim() + ";" +
                        "OPTION=3";
                    OdbcConnection Con = new OdbcConnection(DBConStr);
                    Con.Open();
                    if (Con.State == ConnectionState.Open)
                        Lbl_error.Text = "Test successful.";
                    Con.Close();
                }
                catch (Exception ex)
                {
                    Lbl_error.Text=ex.Message.ToString();
                }
            }
        }

        private void btn_SaveConfig_Click(object sender, EventArgs e)
        {
            string pass;
            Lbl_error.Text = "";

            txtpass.Text = sqlClean(txtpass.Text);
            txtpassconf.Text = sqlClean(txtpassconf.Text);

            if (txtpass.Text == txtpassconf.Text)
                pass = txtpass.Text;
            else if (txtpass.Text == txtpasshidden.Text)
                pass = txtpass.Text;
            else if (txtpass.Text != txtpassconf.Text)
            {
                Lbl_error.Text = "Passwords do not match.";
                return;
            }
            else if (txtpass.Text != txtpasshidden.Text)
            {
                Lbl_error.Text = "Password has been changed.";
                return;
            }
            else
            {
                Lbl_error.Text = "Unknown password problem.";
                return;
            }

            /*
             * Assumes this is the default:
             * 
             *   <connectionStrings>
             *     <clear />
             *     <add name="Name"
             *     providerName="System.Data.ProviderName"
             *     connectionString="DRIVER={MySQL ODBC 5.1 Driver};SERVER=localhost;DATABASE=diags;UID=DBUser;PASSWORD=DBPass;OPTION=3;" />
             *   </connectionStrings>
             *   */
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(SuiteEngPath);
                ConnectionStringSettings cs = new ConnectionStringSettings();
                cs.Name = "Name";
                cs.ConnectionString="DRIVER={MySQL ODBC 3.51 Driver};SERVER=" + txtip.Text.ToString() + 
                    ";DATABASE=" + txtdbname.Text.ToString() +
                    ";PORT=" + txtdbport.Text.ToString() + 
                    ";UID=" + txtuser.Text.ToString() + 
                    ";PASSWORD=" + pass.ToString() + ";OPTION=3;";
                config.ConnectionStrings.ConnectionStrings.Clear();
                config.ConnectionStrings.ConnectionStrings.Add(cs);
                config.Save();
                Lbl_error.Text += "Configuration saved.\r\n";
            }
            catch (ConfigurationErrorsException ex)
            {
                Lbl_error.Text += ("SaveConfig: ConfigurationErrorsException: " + ex.ToString() + "\r\n");
                Lbl_error.Text += (SuiteEngPath + "\r\n");
            }
            catch (Exception ex)
            {
                Lbl_error.Text += ("SaveConfig: Exception: " + ex.ToString() + "\r\n");
                Lbl_error.Text += (SuiteEngPath + "\r\n");
            }


            int iSCManagerHandle = 0;
            int iSCManagerLockHandle = 0;
            int iServiceHandle = 0;
            bool bChangeServiceConfig = false;
            bool bChangeServiceConfig2 = false;
            modAPI.SERVICE_DESCRIPTION ServiceDescription;
            modAPI.SERVICE_FAILURE_ACTIONS ServiceFailureActions;
            modAPI.SC_ACTION[] ScActions = new modAPI.SC_ACTION[3];
            //There should be one element for each action. 
            //The Services snap-in shows 3 possible actions.

            bool bCloseService = false;
            bool bUnlockSCManager = false;
            bool bCloseSCManager = false;

            IntPtr iScActionsPointer = new IntPtr();

            try
            {
                //http://www.codeproject.com/KB/cs/csharpwindowsserviceinst.aspx

                //Obtain a handle to the Service Control Manager, 
                //with appropriate rights.
                //This handle is used to open the relevant service.
                iSCManagerHandle = modAPI.OpenSCManagerA(null, null, 
                modAPI.ServiceControlManagerType.SC_MANAGER_ALL_ACCESS);
                //Check that it's open. If not throw an exception.
                if (iSCManagerHandle < 1)
                {
                    Lbl_error.Text += "ServiceOptions: Unable to open the Services Manager.\r\n";
                }
                //Lock the Service Control Manager database.
                iSCManagerLockHandle = modAPI.LockServiceDatabase(iSCManagerHandle);
                //Check that it's locked. If not throw an exception.
                if (iSCManagerLockHandle < 1)
                {
                    Lbl_error.Text += "ServiceOptions: Unable to lock the Services Manager.\r\n";
                }
                //Obtain a handle to the relevant service, with appropriate rights.
                //This handle is sent along to change the settings. The second parameter
                //should contain the name you assign to the service.
                iServiceHandle = modAPI.OpenServiceA(iSCManagerHandle, txtservicename.Text,
                modAPI.ACCESS_TYPE.SERVICE_ALL_ACCESS);
                //Check that it's open. If not throw an exception.
                if (iServiceHandle < 1)
                {
                    Lbl_error.Text += "ServiceOptions: Unable to open the Service for modification.\r\n";
                }
                //Call ChangeServiceConfig to update the ServiceType 
                //to SERVICE_INTERACTIVE_PROCESS.
                //Very important is that you do not leave out or change the other relevant
                //ServiceType settings. The call will return False if you do.
                //Also, only services that use the LocalSystem account can be set to
                //SERVICE_INTERACTIVE_PROCESS.
                bChangeServiceConfig = modAPI.ChangeServiceConfigA(iServiceHandle,
                modAPI.ServiceType.SERVICE_WIN32_OWN_PROCESS | 
                     modAPI.ServiceType.SERVICE_INTERACTIVE_PROCESS,
                     modAPI.SERVICE_NO_CHANGE, modAPI.SERVICE_NO_CHANGE, 
                     null, null, 0, null, null, null, null);
                //If the call is unsuccessful, throw an exception.
                if (bChangeServiceConfig==false)
                {
                    Lbl_error.Text += "ServiceOptions: Unable to change the Service settings.\r\n";
                }
                //To change the description, create an instance of the SERVICE_DESCRIPTION
                //structure and set the lpDescription member to your desired description.
                ServiceDescription.lpDescription = "XW Diagnostics Polling Engine";
                //Call ChangeServiceConfig2 with SERVICE_CONFIG_DESCRIPTION in the second
                //parameter and the SERVICE_DESCRIPTION instance in the third parameter
                //to update the description.
                bChangeServiceConfig2 = modAPI.ChangeServiceConfig2A(iServiceHandle,
                modAPI.InfoLevel.SERVICE_CONFIG_DESCRIPTION,ref ServiceDescription);
                //If the update of the description is unsuccessful it is up to you to
                //throw an exception or not. The fact that the description did not update
                //should not impact the functionality of your service.
                if (bChangeServiceConfig2==false)
                {
                    Lbl_error.Text += "ServiceOptions: Unable to set the Service description.\r\n";
                }
                //To change the Service Failure Actions, create an instance of the
                //SERVICE_FAILURE_ACTIONS structure and set the members to your
                //desired values. See MSDN for detailed descriptions.
                ServiceFailureActions.dwResetPeriod = 600;  //reset failure count after this many seconds
                ServiceFailureActions.lpRebootMsg = "SuiteEng failed! Rebooting...";
                ServiceFailureActions.lpCommand = "";  // "SomeCommand.exe Param1 Param2";
                ServiceFailureActions.cActions = ScActions.Length;
                //The lpsaActions member of SERVICE_FAILURE_ACTIONS is a pointer to an
                //array of SC_ACTION structures. This complicates matters a little,
                //and although it took me a week to figure it out, the solution
                //is quite simple.

                //First order of business is to populate our array of SC_ACTION structures
                //with appropriate values.
                ScActions[0].Delay = 5000;
                ScActions[0].SCActionType = modAPI.SC_ACTION_TYPE.SC_ACTION_RESTART;
                ScActions[1].Delay = 5000;
                ScActions[1].SCActionType = modAPI.SC_ACTION_TYPE.SC_ACTION_RESTART;
                ScActions[2].Delay = 5000;
                ScActions[2].SCActionType = modAPI.SC_ACTION_TYPE.SC_ACTION_RESTART;
                //Once that's done, we need to obtain a pointer to a memory location
                //that we can assign to lpsaActions in SERVICE_FAILURE_ACTIONS.
                //We use 'Marshal.SizeOf(New modAPI.SC_ACTION) * 3' because we pass 
                //3 actions to our service. If you have less 
                //actions change the * 3 accordingly.
                iScActionsPointer = 
                  Marshal.AllocHGlobal(Marshal.SizeOf(new modAPI.SC_ACTION()) * 3);
                //Once we have obtained the pointer for the memory location we need to
                //fill the memory with our structure. We use the CopyMemory API function
                //for this. Please have a look at it's declaration in modAPI.
                modAPI.CopyMemory(iScActionsPointer, 
                  ScActions, Marshal.SizeOf(new modAPI.SC_ACTION()) * 3);
                //We set the lpsaActions member 
                //of SERVICE_FAILURE_ACTIONS to the integer
                //value of our pointer.
                ServiceFailureActions.lpsaActions = iScActionsPointer.ToInt32();
                //We call bChangeServiceConfig2 with the relevant parameters.
                bChangeServiceConfig2 = modAPI.ChangeServiceConfig2A(iServiceHandle,
                      modAPI.InfoLevel.SERVICE_CONFIG_FAILURE_ACTIONS, 
                      ref ServiceFailureActions);
                //If the update of the failure actions 
                //are unsuccessful it is up to you to
                //throw an exception or not. The fact that 
                //the failure actions did not update
                //should not impact the functionality of your service.
                if (bChangeServiceConfig2==false)
                {
                    Lbl_error.Text += "ServiceOptions: Unable to set the Service Failure Actions.\r\n";
                }


                Lbl_error.Text += "ServiceOptions: Updated.";
            }
            catch (Exception ex)
            {
                Lbl_error.Text += ("ServiceOptions: Exception: " + ex.ToString() + "\r\n");
                Lbl_error.Text += (SuiteEngPath + "\r\n");
            }
            finally
            {
                //Close the handles if they are open.
                Marshal.FreeHGlobal(iScActionsPointer);
                if (iServiceHandle > 0)
                {
                    bCloseService = modAPI.CloseServiceHandle(iServiceHandle);
                }
                if (iSCManagerLockHandle > 0)
                {
                    bUnlockSCManager = modAPI.UnlockServiceDatabase(iSCManagerLockHandle);
                }
                if (iSCManagerHandle != 0)
                {
                    bCloseSCManager = modAPI.CloseServiceHandle(iSCManagerHandle);
                }
            }
        }
    }
}
