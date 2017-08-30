﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.IO;
using System.Data.Odbc;
// requires Reference System.configuration
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;       //for struct/union layout

// http://www.codeproject.com/KB/cs/csharpwindowsserviceinst.aspx


namespace SuiteEngNS
{
    public partial class SuiteEng : ServiceBase
    {
        private string DBConStr;
        //private Timer mTimer;
        Thread monitorThread = null;
        private bool monitorThreadStopping = false;
        public Int32 mTime = 10000;         //10 seconds, used in all monitor timer fields.
        public Int32 ExCount = 0;           // count of exceptions, restart after too many

        //database signature variables
        private string CCSig = "";
        private string CCDSig = "";
        private Int64 CCCnt = 0;
        private Int64 CCDCnt = 0;

        public Mutex dbMutex = new Mutex();

        public SuiteEng()
        {
            try
            {
                InitializeComponent();
                GetAppSettings();
            }
            catch (Exception e)
            {
                LogEventLocal("SuiteEng: Exception in InitializeComponent: " + e.ToString());
            }
            LogEventLocal("SuiteEng: InitializeComponent completed");
        }

        protected override void OnStart(string[] args)
        {
            LogEventLocal("SuiteEng: Service Starting...");
            LogEvent("SuiteEng: Service Starting...");

            try
            {
                OnStartThreadCode();
            }
            catch (Exception e)
            {
                LogEventLocal("SuiteEng: Exception in OnStart: " + e.ToString());
            }

            //start monitor timer, this should be last in the start sequence
            StartMonitorThread();

            LogEventLocal("SuiteEng: Service Started");
            LogEvent("SuiteEng: Service Started");
        }
        public void OnStartThreadCode()
        {
            ExCount = 0;        //Only place this can be set, all others increment

            //Get/Start our com channels
            StartNetworks(false, 0, 0);

            //get initial signature of comchannels and comchanneldetails
            GetNetworksSig(ref CCCnt, ref CCSig, ref CCDCnt, ref CCDSig);
            /* Just variables, don't need the signature, don't need to restart. Just call GetDBSettings in the timer.
            GetSettingsSig(ref SetCnt, ref SetSig);*/

#if (DEBUG)
            // This is normally started in the OnStart method, however for debugging we need to start 
            // the monitor thread here, the check for null is so we don't start more than one instance
            if ( monitorThread == null ) StartMonitorThread();
            LogEventLocal("SuiteEng: Service Started");
            LogEvent("SuiteEng: Service Started");
#endif 

        }
        public void GetAppSettings()
        {
            Configuration config;

            string appName = Environment.GetCommandLineArgs()[0];

            string exePath = System.IO.Path.Combine(
                Environment.CurrentDirectory, appName);

            try
            {
                config = ConfigurationManager.OpenExeConfiguration(exePath);
            }
            catch (ConfigurationErrorsException e)
            {
                LogEventLocal("SuiteEng: GetAppSettings: ConfigurationErrorsException: " + e.ToString());
                LogEventLocal(exePath);
            }
            catch (Exception e)
            {
                LogEventLocal("SuiteEng: GetAppSettings: Exception: " + e.ToString());
                LogEventLocal(exePath);
            }

            try
            {
                // Create a custom configuration section
                // having the same name that is used in the 
                // roaming configuration file.
                // This is because the configuration section 
                // can be overridden by lower-level 
                // configuration files. 
                // See the GetRoamingConfiguration() function in 
                // this example.
                /*                string sectionName = "connectionStrings";
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
                ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;
                if (settings != null)
                {
                    /*foreach (ConnectionStringSettings cs in settings)
                    {
                        LogEventLocal(cs.Name);
                        LogEventLocal(cs.ProviderName);
                        LogEventLocal(cs.ConnectionString);
                    }*/
                    ConnectionStringSettings cs = settings[0];
                    /*
                     * Typical ConnectionString
                     *      connectionString="DRIVER={MySQL ODBC 3.51 Driver};SERVER=192.168.2.53;DATABASE=diags;UID=diags;PASSWORD=diags;OPTION=3;" />
                     *      connectionString="DRIVER={MySQL ODBC 5.1 Driver};SERVER=192.168.2.53;DATABASE=diags;UID=diags;PASSWORD=diags;OPTION=3;" />
                     *      
                     *      OPTION=18435;  - BigInt->Int, Compressed Protocol,
                     *      OPTION=16384   - change BigInt to Int
                     *      OPTION=2051    - no BigInt change, with compressed protocol
                     *      
                     * 5-6 second delay connecting to MySql database:
                     *      If something is wrong with the DNS, particularly reverse DNS, the MySql server 
                     *      can delay for 5-6 seconds between the initial connect and the server greeting, 
                     *      trying to resolve the remote address that is connecting. It does not matter 
                     *      that the hosts table only includes IP addresses. 
                     *      
                     *      If the hosts table includes DNS aliases, domain names, etc, then this option 
                     *      can not be enabled. 
                     *      
                     *      Set “skip_name_resolve” in the MySql configuration file, typically “my.cnf”, 
                     *      in the “[mysqld]” section.
                     * 
                     */

                    DBConStr = cs.ConnectionString;
                    LogEventLocal(DBConStr);
                }
                else
                {
                    LogEventLocal("SuiteEng: No Connection Strings Settings");
                }
            }
            catch (ConfigurationErrorsException e)
            {
                LogEventLocal("SuiteEng: ConfigurationErrorsException exception: " + e.ToString());
            }

        }
        protected override void OnStop()
        {
            LogEvent("SuiteEng: Service Stopping");
            LogEventLocal("SuiteEng: Service Stopping");
            try
            {
                StopMonitorThread();
            }
            catch (Exception e)
            {
                LogEvent("SuiteEng: OnStop: StopMonitorThread: Exception: " + e.ToString());
                LogEventLocal("SuiteEng: OnStop: StopMonitorThread: Exception: " + e.ToString());
            }

            // Stop all network threads
            OnStopNetworkThreads();

            LogEventLocal("SuiteEng: Service Stopped");
            LogEvent("SuiteEng: Service Stopped");
        }
        protected void OnStopNetworkThreads()
        {
            try
            {
            }
            catch (Exception e)
            {
                LogEvent("SuiteEng: OnStopNetworkThreads: Network thread check: Exception: " + e.ToString());
                LogEventLocal("SuiteEng: OnStopNetworkThreads: Network thread check: Exception: " + e.ToString());
            }
        }

        static public string SqlEscape(string usString)  // made static again, it should be
        {
            // SQL Encoding for MySQL Recommended here:
            // http://au.php.net/manual/en/function.mysql-real-escape-string.php
            // it escapes \r, \n, \x00, \x1a, baskslash, single quotes, and double quotes

            if (usString == null)
            {
                return null;
            }
            return Regex.Replace(usString, @"[\r\n\x00\x1a\\'""]", @"\$0");
        }

        static private OdbcConnection logConnection = null;  // create a static connection to db for logging events, 
                                                             // more efficent and if db max's out connection, this will still be able to log to the db

        public void LogEvent(string message)
        {
            try
            {
                if (logConnection == null)
                {
                    logConnection = new OdbcConnection(DBConStr);
                    logConnection.Open();
                }
                OdbcCommand Cmd = new OdbcCommand();
                Cmd.Connection = logConnection;
                Cmd.CommandText = "INSERT INTO log (`dbID` ,`Time` ,`Text`) VALUES (NULL ,CURRENT_TIMESTAMP , '" + SqlEscape(message) + "');";
                Cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                LogEventLocal("SuiteEng: LogEvent: OdbcException: " + e.ToString() + "\r\nmessage: " + message.ToString());
                if (logConnection != null)
                {
                    logConnection.Close();
                    logConnection = null; // this force it to open a new connection next a event is logged
                }
            }
            catch (Exception e)
            {
                LogEventLocal("SuiteEng: LogEvent: Exception: " + e.ToString() + "\r\nmessage: " + message.ToString());
            }
        }
        public void LogEventLocal(string message)
        {
            /* If you get an error similar to:
             *      The source was not found, but some or all event logs could not be searched. Inaccessible logs: Security
             * Apparently it creates a event log for this application. Didn't have this problem on WinXP for development.
             * Didn't have this problem on WinXP or Win7 for execution.
             * 
             * Solution seems to be have at least one copy installed. Then debugging works.
             * 
             * Possible solution: C:\Projects\MyProject\bin\Debug>runas /user:domain\username Application.exe
             */
            EventLog.WriteEntry(this.ServiceName, message);
        }
        void LogFileSystemChanges(object sender, FileSystemEventArgs e)
        {
            string log = string.Format("{0} | {1}", e.FullPath, e.ChangeType);
            LogEvent(log);
        }

        void LogFileSystemRenaming(object sender, RenamedEventArgs e)
        {
            string log = string.Format("{0} | Renamed from {1}", e.FullPath, e.OldName);
            LogEvent(log);
        }

        void LogBufferError(object sender, ErrorEventArgs e)
        {
            LogEvent("SuiteEng: Buffer limit exceeded");
        }
        protected override void OnPause()
        {
            LogEvent("SuiteEng: Service Paused");
            LogEventLocal("SuiteEng: Service Paused");
        }

        protected override void OnContinue()
        {
            LogEvent("SuiteEng: Service Continued");
            LogEventLocal("SuiteEng: Service Continued");
        }


        /* Called on startup, and when a thread has quit, presumably because it detected a database change
         * Part     start a particular com channel
         * ChID     particular com channel ID to start
         * ArrI     index in array previously occupied, so it's put back in same slot
         * 
         * When called on startup,
         *      StartNetworks(false,0);
         * When called to restart a thread, 
         *      StartNetworks(true, NetworksID);
         */
        protected void StartNetworks(bool Part, Int32 ArrI, Int64 ChID)
        {
            dbMutex.WaitOne();
            try
            {
            }
            catch (OdbcException e)
            {
                LogEvent("SuiteEng: StartNetworks: OdbcException: " + e.ToString());
                LogEventLocal("SuiteEng: StartNetworks: OdbcException: " + e.ToString());
            }
            catch (Exception e)
            {
                LogEvent("SuiteEng: StartNetworks: Exception: " + e.ToString());
                LogEventLocal("SuiteEng: StartNetworks: Exception: " + e.ToString());
            }
            dbMutex.ReleaseMutex();
        }


        public bool GetNetworksSig(ref Int64 CCCnt, ref string CCSig, ref Int64 CCDCnt, ref string CCDSig)
        {
            CCCnt = 1;
            CCSig = "abcd";
            CCDCnt = 2;
            CCDSig = "efgj";
            return true;
        }

        public void StartMonitorThread()
        {
           // mTimer = new Timer(new TimerCallback(MonitorTimerProc));
           //mTimer.Change(dueTime, Timeout.Infinite);     // fire off MonitorTimerProc one time in dueTime milliseconds
           monitorThreadStopping = false;
           monitorThread = new Thread(MonitorThread);
           monitorThread.Start();
        }
        public void StopMonitorThread()
        {
            try
            {
                monitorThreadStopping = true;
                monitorThread.Interrupt();  // this will interrupt the thread if it is in the Sleep or any other blocking call
                monitorThread.Join();
            }
            catch (Exception e)
            {
                LogEvent("SuiteEng: StopMonitorThread: Exception: " + e.ToString());
            }
            LogEvent("SuiteEng: MonitorThread stopped");
            LogEventLocal("SuiteEng: MonitorThread stopped");
        }

        //private void MonitorTimerProc(object state)
        private void MonitorThread()
        {
            // the timer is scheduled to only fire once [See StartMonitor()] so it does fire a second time while we are 
            // still inside executing the code in this method, don't forget to reschedule this Proc at the end of itself
            while (!monitorThreadStopping)
            {
                // check if comchannel table has changed.
                // initialize variables differently than global kept copies just in case.

                try
                {
                    Thread.Sleep(10000);   // check every 10 seconds
                }
                catch
                {}

                if (monitorThreadStopping) return;

                //LogEvent("SuiteEngNS: MonitorThread (" + Thread.CurrentThread.ManagedThreadId + ") running health check...");
                try
                {

                    Int64 TCCCnt = -1;
                    string TCCSig = "1";
                    Int64 TCCDCnt = -1;
                    string TCCDSig = "1";
                    GetNetworksSig(ref TCCCnt, ref TCCSig, ref TCCDCnt, ref TCCDSig);
                    if ((CCCnt != TCCCnt ||
                        CCSig != TCCSig ||
                        CCDCnt != TCCDCnt ||
                        CCDSig != TCCDSig) && !monitorThreadStopping)
                    {
                        // table has changed, reload.
                        LogEvent("SuiteEng: MonitorThread: comchannels or comchanneldetails changed. Restarting service.");
                        LogEventLocal("SuiteEng: MonitorThread: comchannels or comchanneldetails changed. Restarting service.");
                        // restart ourselves.
                        OnStopNetworkThreads();
                        OnStartThreadCode();
                        continue;
                    }
                }
                catch (Exception e)
                {
                    LogEventLocal("SuiteEng: MonitorThread: Getting Network Signature: Exception: " + e.ToString());
                }


                if (ExCount > 25 && !monitorThreadStopping)
                {
                    // too many exceptions, reload.
                    //try logging to database first.
                    LogEvent("SuiteEng: MonitorThread: Too many exceptions. Restarting service.");
                    LogEventLocal("SuiteEng: MonitorThread: Too many exceptions. Restarting service.");
                    // restart ourselves.
                    OnStopNetworkThreads();
                    OnStartThreadCode();
                    continue;
                }

                //we aren't restarting, so check/reset DBSettings
                if (!monitorThreadStopping && CheckDbForForcedRestart())
                {
                    //it returned true, so we have to restart.
                    // restart ourselves.
                    LogEvent("SuiteEng: MonitorThread: CheckDbForForcedRestart was true, restarting....");
                    LogEventLocal("SuiteEng: MonitorThread: CheckDbForForcedRestart was true, restarting....");
                    OnStopNetworkThreads();
                    OnStartThreadCode();
                    continue;
                }

                try
                {
                    // check if threads have stopped
                }
                catch (Exception e)
                {
                    LogEvent("SuiteEng: MonitorThread: Network thread check: Exception: " + e.ToString());
                    LogEventLocal("SuiteEng: MonitorThread: Network thread check: Exception: " + e.ToString());
                }

                //LogEvent("SuiteEngNS: MonitorThread (" + Thread.CurrentThread.ManagedThreadId + ") done...next check will run shortly");
            }
            //if (!monitorThreadStopping) mTimer.Change(mTime, Timeout.Infinite);    // schedule to run again
        }

        protected bool CheckDbForForcedRestart()
        {
            bool RetVal = false;        

            try
            {
                OdbcConnection Con = new OdbcConnection(DBConStr);
                Con.Open();
                OdbcCommand Cmd = new OdbcCommand();
                Cmd.Connection = Con;
                Cmd.CommandText = "SELECT `value` FROM `settings` WHERE `parameter` = 'ForcePollingEngineRestart'";
                int result = Convert.ToInt32(Cmd.ExecuteScalar());
                if (result == 1)
                {
                    Cmd.CommandText = "UPDATE `settings` SET `value` = '0' WHERE `parameter` = 'ForcePollingEngineRestart'";
                    Cmd.ExecuteNonQuery();
                    RetVal = true;
                }
                Cmd.Dispose();
                Con.Close();
            }
            catch (OdbcException e)
            {
                LogEventLocal("SuiteEng: CheckDbForForcedRestart: OdbcException: " + e.ToString());
            }
            catch (Exception e)
            {
                LogEventLocal("SuiteEng: CheckDbForForcedRestart: Exception: " + e.ToString());
            }

            return RetVal;
        }


        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }



        // CRC-32 generator polynomial.
        //
        //#define POLYNOMIAL           0xedb88320    // little-endian       1110 1101 1011 1000 1000 0011 0010 0000
        //#define POLYNOMIAL           0x04c11db7    // big-endian          0000 0100 1100 0001 0001 1101 1011 0111

        // CRC-32 look-up table
        //
        static readonly UInt32[] crc_table = new UInt32[] {
        0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 
        0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 
        0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 
        0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5, 
        0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 
        0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 
        0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F, 
        0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 
        0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433, 
        0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01, 
        0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 
        0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65, 
        0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 
        0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 
        0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F, 
        0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD, 
        0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683, 
        0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1, 
        0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 
        0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 
        0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 
        0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79, 
        0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236, 0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 
        0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D, 
        0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713, 
        0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21, 
        0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777, 
        0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 
        0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 
        0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 
        0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 
        0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
        };


        //////////////////////////////////////////////////////////////////////////////
        /// Initialize the CRC module.
        //////////////////////////////////////////////////////////////////////////////
        /*
        void crc_init(void) {
           int i, j;
           for (i = 0; i < 256; i++) {
              uint32_t crc = i;
              for (j = 0; j < 8; j++)
                 crc = (crc >> 1) ^ (-(crc & 1) & POLYNOMIAL);
              crc_table[i] = crc;
           }
        }
        extern unsigned long fnCRC32(unsigned long ulSeed, unsigned char *data, unsigned long ulLength)
        {
           register unsigned long ulResult = ulSeed;
           register unsigned long i;
           CRC_STARTING();
           for (i = 0; i < ulLength; i++) {
              ulResult = crc_table[(ulResult ^ data[i]) & 0xff] ^ (ulResult >> 8);
           }
           CRC_COMPLETED();
           return ulResult;
        }
        */
        /*UInt32[] crc_table1;
        public void crc_init()
        {
            UInt32 i,j;
            crc_table1=new UInt32[256];
            for(i=0;i<256;i++)
            {
                UInt32 crc=i;
                UInt32 t = i;
                for (j = 0; j < 8; j++)
                {
                    crc = (crc >> 1) ^ ((UInt32)(-(crc & 1)) & 0xedb88320);
                }
                crc_table1[i]=crc;
            }
        }*/
        public UInt32 fnCRC32(UInt32 ulSeed, byte[] data, UInt32 ulLength)
        {
            UInt32 ulResult = ulSeed;
            UInt32 i;

            for (i = 0; i < ulLength; i++)
            {
                ulResult = crc_table[(ulResult ^ data[i]) & 0xff] ^ (ulResult >> 8);
            }
            return ulResult;
        }
        public static string UTF8toASCII(string text)
        {
            System.Text.Encoding utf8 = System.Text.Encoding.UTF8;
            Byte[] encodedBytes = utf8.GetBytes(text);
            Byte[] convertedBytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, encodedBytes);
            System.Text.Encoding ascii = System.Text.Encoding.ASCII;

            return ascii.GetString(convertedBytes);
        }


    }
}
