using System;
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
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;       //for struct/union layout
using StackExchange.Redis;
using Newtonsoft.Json;
using TimeZoneConverter;        //https://github.com/mj1856/TimeZoneConverter   v1.1.1 will install with VS2012. PM> Install-Package TimeZoneConverter -Version 1.1.1

// http://www.codeproject.com/KB/cs/csharpwindowsserviceinst.aspx


namespace EngineNS
{

    public enum LogLevels : int { Emerg = 0, Alert = 1, Crit = 2, Err = 3, Warn = 4, Notice = 5, Info = 6, Debug = 7 };

    static class Constants
    {
        public const int ExceptionMax = 25;
        public const int UserSystemWeb = -1;
        public const int UserSystemEngine = -2;
    }



    public partial class Engine : ServiceBase
    {
        //private Timer mTimer;
        Thread monitorThread = null;
        private bool monitorThreadStopping = false;
        public Int32 mTime = 10000;         //10 seconds, used in all monitor timer fields.

        //database signature variables
        private string CCSig = "";
        private string CCDSig = "";
        private Int64 CCCnt = 0;
        private Int64 CCDCnt = 0;
        private string SettingsSig = "";
        private Int64 SettingsCnt = 0;


        //The main thread (this) has it's own database connection for operation and a separate connection for logging.
        //  With a single connection, I have to worry about order of mutex's and logging. 
        //
        private string DBConStr = "";
        private string DBDriverStr = "";
        static private OdbcConnection dbMainCon = null; // create a static connection to db for main thread execution use
        private Int32 ExCount = 0;                      // count of exceptions, restart after too many
        static private Mutex dbMainMutex = new Mutex();

        private string RedisConStr = "";
        public ConnectionMultiplexer Redis;
        public IDatabase RDB = null;

        private string LogPrefix = "SuiteEng: Main: ";
        protected SWTimer REDW = new SWTimer("RedisWrite");
        protected SWTimer REDR = new SWTimer("RedisRead");


        //0 - emergency
        //1 - alert
        //2 - critical
        //3 - error
        //4 - warning
        //5 - notice
        //6 - info
        //7 - debug
        //Can't figure out how to do an array of LimitedQueues.
        private LimitedQueue<LogJSON> Log0 = new LimitedQueue<LogJSON>(100);        //0 - emergency
        private LimitedQueue<LogJSON> Log1 = new LimitedQueue<LogJSON>(100);        //1 - alert
        private LimitedQueue<LogJSON> Log2 = new LimitedQueue<LogJSON>(100);        //2 - critical
        private LimitedQueue<LogJSON> Log3 = new LimitedQueue<LogJSON>(100);        //3 - error
        private LimitedQueue<LogJSON> Log4 = new LimitedQueue<LogJSON>(100);        //4 - warning
        private LimitedQueue<LogJSON> Log5 = new LimitedQueue<LogJSON>(100);        //5 - notice
        private LimitedQueue<LogJSON> Log6 = new LimitedQueue<LogJSON>(100);        //6 - info
        private LimitedQueue<LogJSON> Log7 = new LimitedQueue<LogJSON>(100);        //7 - debug

        public Engine()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                LogEventLocal(LogPrefix + "Exception in InitializeComponent: " + e.ToString());
                Environment.Exit(1);
            }
            //LogEventLocal(LogPrefix + "InitializeComponent completed");

            try
            {
                DBDriverStr = GetOdbcMySQLDriverName();
            }
            catch (Exception e)
            {
                LogEventLocal(LogPrefix + "Quitting, could not get ODBC driver name. Exception: " + e.ToString());
                Environment.Exit(1);
            }

            if (!GetAppSettings())
            {
                LogEventLocal(LogPrefix + "Quitting, could not get AppSettings.");
                Environment.Exit(1);
            }

            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "Main Engine Thread";
            }
            else
            {
                LogEventLocal(LogPrefix + "Unable to name Main Engine Thread");
            }
            try
            {
                StackExchange.Redis.RedisValue RQueue;
                RQueue = RedisStringGet("Engine_Log0");
                if (!RQueue.IsNull)
                {
                    Log0 = new LimitedQueue<LogJSON>(100, JsonConvert.DeserializeObject<List<LogJSON>>(RQueue));
                }
                RQueue = RedisStringGet("Engine_Log1");
                if (!RQueue.IsNull)
                {
                    Log1 = new LimitedQueue<LogJSON>(100, JsonConvert.DeserializeObject<List<LogJSON>>(RQueue));
                }
                RQueue = RedisStringGet("Engine_Log2");
                if (!RQueue.IsNull)
                {
                    Log2 = new LimitedQueue<LogJSON>(100, JsonConvert.DeserializeObject<List<LogJSON>>(RQueue));
                }
                RQueue = RedisStringGet("Engine_Log3");
                if (!RQueue.IsNull)
                {
                    Log3 = new LimitedQueue<LogJSON>(100, JsonConvert.DeserializeObject<List<LogJSON>>(RQueue));
                }
                RQueue = RedisStringGet("Engine_Log4");
                if (!RQueue.IsNull)
                {
                    Log4 = new LimitedQueue<LogJSON>(100, JsonConvert.DeserializeObject<List<LogJSON>>(RQueue));
                }
                RQueue = RedisStringGet("Engine_Log5");
                if (!RQueue.IsNull)
                {
                    Log5 = new LimitedQueue<LogJSON>(100, JsonConvert.DeserializeObject<List<LogJSON>>(RQueue));
                }
                RQueue = RedisStringGet("Engine_Log6");
                if (!RQueue.IsNull)
                {
                    Log6 = new LimitedQueue<LogJSON>(100, JsonConvert.DeserializeObject<List<LogJSON>>(RQueue));
                }
                RQueue = RedisStringGet("Engine_Log7");
                if (!RQueue.IsNull)
                {
                    Log7 = new LimitedQueue<LogJSON>(100, JsonConvert.DeserializeObject<List<LogJSON>>(RQueue));
                }
            }
            catch (Exception e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "Constructor: Exception in Redis Log fetch: " + e.ToString());
            }
        }

        ~Engine()
        {
            try
            {
                dbMainCon.Close();
                dbMainCon = null;
            }
            catch (Exception e)
            {
                LogEventLocal(LogPrefix + "Destructor: Exception(Closing dbMainCon database): " + e.ToString());
            }

            dbMainMutex.Dispose();
        }

        public string GetEngineDBConStr()
        {
            return DBConStr;
        }

        protected override void OnStart(string[] args)
        {
            LogEventLocal(LogPrefix + "Service Starting...");
            LogEvent(LogLevels.Crit, LogPrefix + "Service Starting...");

            try
            {
                OnStartThreadCode();
            }
            catch (Exception e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "Exception in OnStart: " + e.ToString());
            }

            //start monitor timer, this should be last in the start sequence
            MonitorThreadStart();

            LogEventLocal(LogPrefix + "Service Started");
            LogEvent(LogLevels.Info, LogPrefix + "Service Started");
        }
        public void OnStartThreadCode()
        {
            ExCount = 0;            //Only place this can be set, all others increment

            GetDBSettings();        //ignore restart required return value
            EnumToDatabase();       //make sure database has our enums

            //Get/Start our com channels
            StartNetworks(false, 0, 0);

            //get initial signature of comchannels and comchanneldetails
            GetNetworksSig(ref CCCnt, ref CCSig, ref CCDCnt, ref CCDSig);
            GetSettingsSig(ref SettingsCnt, ref SettingsSig);

#if (DEBUG)
            // This is normally started in the OnStart method, however for debugging we need to start 
            // the monitor thread here, the check for null is so we don't start more than one instance
            if (monitorThread == null) MonitorThreadStart();
            LogEventLocal(LogPrefix + "Service Started");
            LogEvent(LogLevels.Info, LogPrefix + "Service Started");
#endif
        }

        public bool OpenCheckDB()
        {
            //MySqlException: MySql.Data.MySqlClient.MySqlException (0x80004005): There is already an open DataReader associated with this Connection which must be closed first.
            //  Happens from not only an open DataReader, but also if a DataReader is open, then you try an ExecuteNonQuery or the like, any other command. 
            //So either make sure no other command can/is happening while a DataReader is open.
            //Or possibly: MultipleActiveResultSets=True in the connection string.
            //
            //Pretty sure the cause was OpenCheckDB, then the mutex protection of the connection. The MonitorThread executing, calling an OpenCheckDB then waiting on a mutex
            //  while a Network was executing UpdateOrInsert_comchanneldetails. Because of the ExecuteNonQuery test in OpenCheckDB prior to the mutex, the UpdateOrInsert 
            //  exceptions out when it tries to build a DataReader. Solution is put the OpenCheckDB inside the mutex protection.
            bool RetVal = false;
            try
            {
                if (dbMainCon == null)
                {
                    if (DBConStr != "")
                    {
                        dbMainCon = new OdbcConnection(DBConStr);
                        dbMainCon.Open();
                        RetVal = true;
                    }
                    else
                    {
                        LogEventLocal(LogPrefix + "OpenCheckDB: DBConStr is empty.");
                    }
                }
                else
                {
                    RetVal = true;
                }

                try
                {
                    OdbcCommand TCmd = new OdbcCommand();
                    TCmd.Connection = dbMainCon;
                    TCmd.CommandText = "DO 0;";
                    TCmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    ExCount++;
                    LogEventLocal(LogPrefix + "OpenCheckDB: Exception: " + e.ToString());
                }

                if (dbMainCon.State == ConnectionState.Broken)
                {
                    dbMainCon.Close();
                }

                if (dbMainCon.State == ConnectionState.Closed)
                {
                    dbMainCon.Open();
                }
            }
            catch (OdbcException e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "OpenCheckDB: OdbcException: " + e.ToString());
                if (dbMainCon != null)
                {
                    try
                    {
                        dbMainCon.Close();
                    }
                    catch { }
                    dbMainCon = null; // this forces it to open a new connection next time it's needed
                }
            }
            catch (Exception e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "OpenCheckDB: Exception: " + e.ToString());
            }
            return RetVal;
        }
        /*public bool OpenCheckDBLog()
        {
            bool RetVal = false;
            try
            {
                if (dbLogCon == null)
                {
                    if (DBConStr != "")
                    {
                        dbLogCon = new OdbcConnection(DBConStr);
                        dbLogCon.Open();
                        RetVal = true;
                    }
                    else
                    {
                        LogEventLocal(LogPrefix + "OpenCheckDBLog: DBConStr is empty.");
                    }
                }
                else
                {
                    RetVal = true;
                }

                try
                {
                    OdbcCommand TCmd = new OdbcCommand();
                    TCmd.Connection = dbLogCon;
                    TCmd.CommandText = "DO 0;";
                    TCmd.ExecuteNonQuery();
                }
                catch { }

                if (dbLogCon.State == ConnectionState.Broken)
                {
                    dbLogCon.Close();
                }

                if (dbLogCon.State == ConnectionState.Closed)
                {
                    dbLogCon.Open();
                }
            }
            catch (OdbcException e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "OpenCheckDBLog: OdbcException: " + e.ToString());
                if (dbLogCon != null)
                {
                    try
                    {
                        dbLogCon.Close();
                    }
                    catch { }
                    dbLogCon = null; // this forces it to open a new connection next time it's needed
                }
            }
            catch (Exception e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "OpenCheckDBLog: Exception: " + e.ToString());
            }
            return RetVal;
        }*/
        public bool OpenCheckRedis()
        {
            bool RetVal = false;
            try
            {
                Redis = ConnectionMultiplexer.Connect(RedisConStr);

                RDB = Redis.GetDatabase();
                String TV = DateTime.UtcNow.ToString("s");
                RedisStringSet("EngineStart", TV, null);

                string Val = RedisStringGet("EngineStart");
                if (TV != Val)
                {
                    LogEventLocal(LogPrefix + "OpenCheckRedis: EngineStart value check failed.");
                }

                RetVal = true;
            }
            catch (Exception e)
            {
                LogEventLocal(LogPrefix + "OpenCheckRedis: Exception: " + e.ToString());
                ExCount++;
            }
            return RetVal;
        }

        public bool GetAppSettings()
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
                ExCount++;
                LogEventLocal(LogPrefix + "GetAppSettings: ConfigurationErrorsException: " + e.ToString());
                LogEventLocal(exePath);
            }
            catch (Exception e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "GetAppSettings: Exception: " + e.ToString());
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
                    foreach (ConnectionStringSettings cs in settings)
                    {
                        if (cs.Name == "MySQL")
                        {
                            DBConStr = cs.ConnectionString;
                            //Now swap out the driver string.
                            DBConStr = Regex.Replace(DBConStr, "DRIVER={MySQL ODBC [a-zA-Z0-9.\\s]* Driver};", "DRIVER={"+DBDriverStr+"};");
                            LogEventLocal(DBConStr);
                        }
                        else if (cs.Name == "Redis")
                        {
                            RedisConStr = cs.ConnectionString;
                            LogEventLocal(RedisConStr);
                        }
                    }
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
                }
                else
                {
                    LogEventLocal(LogPrefix + "GetAppSettings: No Connection Strings Settings");
                }
            }
            catch (ConfigurationErrorsException e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "GetAppSettings: ConfigurationErrorsException exception: " + e.ToString());
            }

            //Now test/open the database
            return OpenCheckDB() && OpenCheckRedis();      //OpenCheckDBLog() &&
        }
        protected override void OnStop()
        {
            LogEvent(LogLevels.Info, LogPrefix + "Service Stopping");
            try
            {
                MonitorThreadStop();
            }
            catch (Exception e)
            {
                ExCount++;
                LogEvent(LogLevels.Err, LogPrefix + "OnStop: StopMonitorThread: Exception: " + e.ToString());
            }

            // Stop all network threads
            OnStopNetworkThreads();

            LogEvent(LogLevels.Crit, LogPrefix + "Service Stopped");
        }
        protected void OnStopNetworkThreads()
        {
            LogEvent(LogLevels.Info, LogPrefix + "OnStopNetworkThreads");
            try
            {
            }
            catch (Exception e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "OnStopNetworkThreads: Network thread check: Exception: " + e.ToString());
                ExCount++;
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


        /*public void LogEvent(string message)
        {
            OpenCheckDBLog();
            try
            {
                if (!dbLogMutex.WaitOne(2000))
                {
                    ExCount++;
                    LogEventLocal(LogPrefix + "LogEvent: Mutex not acquired.\r\n\r\nmessage: " + message.ToString());
                    return;
                }
            }
            catch { }
            try
            {
                OdbcCommand Cmd = new OdbcCommand();
                Cmd.Connection = dbLogCon;
                Cmd.CommandText = "INSERT INTO log (`dbID` ,`Time` ,`Text`) VALUES (NULL ,CURRENT_TIMESTAMP , '" + SqlEscape(message) + "');";
                Cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "LogEvent: OdbcException: " + e.ToString() + "\r\n\r\nmessage: " + message.ToString());
            }
            catch (Exception e)
            {
                ExCount++;
                LogEventLocal(LogPrefix + "LogEvent: Exception: " + e.ToString() + "\r\n\r\nmessage: " + message.ToString());
            }
            try
            {
                dbLogMutex.ReleaseMutex();
            }
            catch { }
        }*/
        public void LogEvent(LogLevels severity, string message)
        {
            try
            {
                if ((int)severity < 0 || (int)severity > 7)
                {
                    LogEvent(LogLevels.Crit, "LogEvent called with incorrect severity. Original message: " + message);
                    return;
                }

                if ((int)severity == 0)
                {
                    Monitor.Enter(Log0);
                    try
                    {
                        Log0.Enqueue(new LogJSON(message));
                        RedisStringSet("Engine_Log0", JsonConvert.SerializeObject(Log0.ToList()), null);
                    }
                    finally
                    {
                        Monitor.Exit(Log0);
                    }
                }
                else if ((int)severity == 1)
                {
                    Monitor.Enter(Log1);
                    try
                    {
                        Log1.Enqueue(new LogJSON(message));
                        RedisStringSet("Engine_Log1", JsonConvert.SerializeObject(Log1.ToList()), null);
                    }
                    finally
                    {
                        Monitor.Exit(Log1);
                    }
                }
                else if ((int)severity == 2)
                {
                    Monitor.Enter(Log2);
                    try
                    {
                        Log2.Enqueue(new LogJSON(message));
                        RedisStringSet("Engine_Log2", JsonConvert.SerializeObject(Log2.ToList()), null);
                    }
                    finally
                    {
                        Monitor.Exit(Log2);
                    }
                }
                else if ((int)severity == 3)
                {
                    Monitor.Enter(Log3);
                    try
                    {
                        Log3.Enqueue(new LogJSON(message));
                        RedisStringSet("Engine_Log3", JsonConvert.SerializeObject(Log3.ToList()), null);
                    }
                    finally
                    {
                        Monitor.Exit(Log3);
                    }
                }
                else if ((int)severity == 4)
                {
                    Monitor.Enter(Log4);
                    try
                    {
                        Log4.Enqueue(new LogJSON(message));
                        RedisStringSet("Engine_Log4", JsonConvert.SerializeObject(Log4.ToList()), null);
                    }
                    finally
                    {
                        Monitor.Exit(Log4);
                    }
                }
                else if ((int)severity == 5)
                {
                    Monitor.Enter(Log5);
                    try
                    {
                        Log5.Enqueue(new LogJSON(message));
                        RedisStringSet("Engine_Log5", JsonConvert.SerializeObject(Log5.ToList()), null);
                    }
                    finally
                    {
                        Monitor.Exit(Log5);
                    }
                }
                else if ((int)severity == 6)
                {
                    Monitor.Enter(Log6);
                    try
                    {
                        Log6.Enqueue(new LogJSON(message));
                        RedisStringSet("Engine_Log6", JsonConvert.SerializeObject(Log6.ToList()), null);
                    }
                    finally
                    {
                        Monitor.Exit(Log6);
                    }
                }
                else if ((int)severity == 7)
                {
                    Monitor.Enter(Log7);
                    try
                    {
                        Log7.Enqueue(new LogJSON(message));
                        RedisStringSet("Engine_Log7", JsonConvert.SerializeObject(Log7.ToList()), null);
                    }
                    finally
                    {
                        Monitor.Exit(Log7);
                    }
                }
            }
            catch (Exception e)
            {
                LogEventLocal(LogPrefix + "LogEvent: Exception: " + e.ToString() + "\r\n\r\nmessage: " + message.ToString());
            }
        }

        private void LogEventLocal(string message)
        {
            /* If you get an error similar to:
             *      The source was not found, but some or all event logs could not be searched. Inaccessible logs: Security
             * Apparently it creates a event log for this application. Didn't have this problem on WinXP for development.
             * Didn't have this problem on WinXP or Win7 for execution.
             * 
             * Solution seems to be have at least one copy installed. Then debugging works.
             * 
             * Possible solution: C:\Projects\MyProject\bin\Debug>runas /user:domain\username Application.exe
             * 
             * 20190907:
             * Make sure that your event source <AppName> is actually registered, i.e. that it appears in the registry under HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\eventlog\Application
             * 
             * AppName is Project Name (?), not Assembly Name / Exe Name when debugging. IE not SuiteEng, but Engine.
             * 
             * Key= HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Application\<Your app name>\
             * String EventMessageFile value=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\EventLogMessages.dll
             * 
             * Possible other values:
             * String EventMessageFile value=C:\Windows\Microsoft.NET\Framework\v2.0.50727\EventLogMessages.dll
             * String EventMessageFile value=C:\Windows\Microsoft.NET\Framework\v4.0.30319\EventLogMessages.dll
             *
             * Alternatively, run Visual Studio as Administrator, debug the project, and it will automatically add the EventLog source to the registry. Subsequent debugging can be done
             *   as a normal user running VS.
             */
            EventLog.WriteEntry(this.ServiceName, message);
        }
        void LogFileSystemChanges(object sender, FileSystemEventArgs e)
        {
            string log = string.Format("{0} | {1}", e.FullPath, e.ChangeType);
            LogEvent(LogLevels.Debug, log);
        }

        void LogFileSystemRenaming(object sender, RenamedEventArgs e)
        {
            string log = string.Format("{0} | Renamed from {1}", e.FullPath, e.OldName);
            LogEvent(LogLevels.Debug, log);
        }

        void LogBufferError(object sender, ErrorEventArgs e)
        {
            LogEvent(LogLevels.Debug, LogPrefix + "Buffer limit exceeded");
        }
        protected override void OnPause()
        {
            LogEvent(LogLevels.Info, LogPrefix + "Service Paused");
            LogEventLocal(LogPrefix + "Service Paused");
        }

        protected override void OnContinue()
        {
            LogEvent(LogLevels.Info, LogPrefix + "Service Continued");
            LogEventLocal(LogPrefix + "Service Continued");
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
            try
            {
                if (!dbMainMutex.WaitOne(2000))
                {
                    ExCount++;
                    LogEvent(LogLevels.Err, LogPrefix + "StartNetworks: Mutex not acquired.");
                    return;
                }
            }
            catch { }
            OpenCheckDB();
            try
            {
            }
            catch (OdbcException e)
            {
                ExCount++;
                LogEvent(LogLevels.Err, LogPrefix + "StartNetworks: OdbcException: " + e.ToString());
            }
            catch (Exception e)
            {
                ExCount++;
                LogEvent(LogLevels.Err, LogPrefix + "StartNetworks: Exception: " + e.ToString());
            }
            try
            {
                dbMainMutex.ReleaseMutex();
            }
            catch { }
        }


        public bool GetNetworksSig(ref Int64 CCCnt, ref string CCSig, ref Int64 CCDCnt, ref string CCDSig)
        {
            CCCnt = 1;
            CCSig = "abcd";
            CCDCnt = 2;
            CCDSig = "efgj";
            return true;
        }
        public bool GetSettingsSig(ref Int64 SetCnt, ref string SetSig)
        {
            try
            {
                if (!dbMainMutex.WaitOne(2000))
                {
                    ExCount++;
                    LogEvent(LogLevels.Err, LogPrefix + "GetSettingsSig: Mutex not acquired.");
                    return false;
                }
            }
            catch { }
            OpenCheckDB();
            try
            {
                OdbcCommand Cmd = new OdbcCommand();
                Cmd.Connection = dbMainCon;
                // returns a variable length string, future idea would be to compute MD5 again of return, or intermediate steps, or 
                //   concatenate rows and MD5 that...
                //
                // @ sign prepended to string allows for multiline string constants.
                Cmd.CommandText = @"SELECT cnt, CONCAT(CONV(ssig1,10,16),
                                                       CONV(ssig2,10,16),
                                                       CONV(ssig3,10,16),
                                                       CONV(ssig4,10,16)) AS sig
                                    FROM ( SELECT SUM(CONV(SUBSTRING(@CRC := MD5( CONCAT_WS( '/##/', COALESCE(`id`,'#NULL#'),
                                                                                                     COALESCE(`parameter`,'#NULL#'),
                                                                                                     COALESCE(`value`,'#NULL#'),
                                                                                                     COALESCE(`Created`,'#NULL#'),
                                                                                                     COALESCE(`CreatedBy`,'#NULL#'),
                                                                                                     COALESCE(`Destroyed`,'#NULL#'),
                                                                                                     COALESCE(`DestroyedBy`,'#NULL#'),
                                                                                                     COALESCE(`Valid`,'#NULL#') ) ),1,8),16,10)) AS ssig1,
                                                  SUM(CONV(SUBSTRING(@CRC,9,8),16,10)) AS ssig2,
                                                  SUM(CONV(SUBSTRING(@CRC,17,8),16,10)) AS ssig3,
                                                  SUM(CONV(SUBSTRING(@CRC,25,8),16,10)) AS ssig4,
                                                  COUNT(*) AS cnt
                                    FROM `settings` ) AS x;";
                OdbcDataReader DbReader = Cmd.ExecuteReader();
                DbReader.Read();
                int cSig = DbReader.GetOrdinal("sig");
                int cCnt = DbReader.GetOrdinal("cnt");
                SetSig = DbReader.GetString(cSig);
                SetCnt = (int)DbReader.GetInt64(cCnt);
                DbReader.Close();

                Cmd.Dispose();
                try
                {
                    dbMainMutex.ReleaseMutex();
                }
                catch { }
                return true;
            }
            catch (OdbcException e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "GetSettingsSig: OdbcException: " + e.ToString());
                ExCount++;
            }
            catch (Exception e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "GetSettingsSig: Exception: " + e.ToString());
                ExCount++;
            }
            try
            {
                dbMainMutex.ReleaseMutex();
            }
            catch { }
            return false;
        }


        public void MonitorThreadStart()
        {
           // mTimer = new Timer(new TimerCallback(MonitorTimerProc));
           //mTimer.Change(dueTime, Timeout.Infinite);     // fire off MonitorTimerProc one time in dueTime milliseconds
           monitorThreadStopping = false;
           monitorThread = new Thread(MonitorThread);
           monitorThread.Start();
        }
        public void MonitorThreadStop()
        {
            try
            {
                monitorThreadStopping = true;
                //wait 150ms for sleep loops and such to exit gracefully.
                Thread.Sleep(150);
                monitorThread.Interrupt();      // this will interrupt the thread if it is in the Sleep or any other blocking call
                monitorThread.Join(500);        //wait until thread is done stopping, or max of 500ms
                //now get rid of everything
                monitorThread = null;
            }
            catch (Exception e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "MonitorThreadStop: Exception: " + e.ToString());
            }
            LogEvent(LogLevels.Info, LogPrefix + "MonitorThread stopped");
        }

        //private void MonitorTimerProc(object state)
        private void MonitorThread()
        {
            try
            {
                /*
                 * Keep a server count of exceptions. If the server exceptions exceed a threshold, then restart the entire server.
                 * 
                 * Keep a count of thread restarts. If the thread restarts exceed a threshold, then restart the entire server.
                 * 
                 * Do not propagate thread exceptions up to here directly. 
                 * 
                 * Let each thread count it's own exceptions, and on exceeding the threshold, the thread quits and gets restarted here.
                 *   By extension too many thread restarts will restart the entire server.
                 */
                // the timer is scheduled to only fire once [See StartMonitor()] so it does fire a second time while we are 
                // still inside executing the code in this method, don't forget to reschedule this Proc at the end of itself
                while (!monitorThreadStopping)
                {
                    // check if comchannel table has changed.
                    // initialize variables differently than global kept copies just in case.

                    try
                    {
                        //sleep 10 seconds
                        for (int i = 0; i < 100 && !monitorThreadStopping; i++)
                            Thread.Sleep(100);
                    }
                    catch
                    { }
                    if (monitorThreadStopping) return;

                    String TV = DateTime.UtcNow.ToString("s");
                    RedisStringSet("EngineMonitorLoop", TV);
                    TimingUpdate();

                    //Check this first, if we have major problems, we get stuck in an exception loop and never clear.
                    if (!monitorThreadStopping && ExCount > Constants.ExceptionMax)
                    {
                        // too many SERVER exceptions, reload.
                        if (ExCount > (Constants.ExceptionMax * 2))
                        {
                            LogEvent(LogLevels.Err, LogPrefix + "MonitorThread: Too many exceptions * 2. Stopping engine.");
                            //incrementing the exception count doesn't do any good here.
                            Environment.Exit(1);
                        }
                        //try logging to database first.
                        LogEvent(LogLevels.Notice, LogPrefix + "MonitorThread: Too many exceptions. Restarting service.");
                        // restart ourselves.
                        OnStopNetworkThreads();
                        OnStartThreadCode();
                        continue;
                    }

                    //LogEvent(LogPrefix + "MonitorThread (" + Thread.CurrentThread.ManagedThreadId + ") running health check...");
                    try
                    {

                        // initialize variables differently than global kept copies just in case.
                        Int64 TCCCnt = -1;
                        string TCCSig = "1";
                        Int64 TCCDCnt = -1;
                        string TCCDSig = "1";
                        GetNetworksSig(ref TCCCnt, ref TCCSig, ref TCCDCnt, ref TCCDSig);
                        if ((CCCnt != TCCCnt ||
                            CCSig != TCCSig ||
                            CCDCnt != TCCDCnt ||
                            CCDSig != TCCDSig)
                            && !monitorThreadStopping)
                        {
                            // table has changed, reload.
                            LogEvent(LogLevels.Notice, LogPrefix + "MonitorThread: comchannels or comchanneldetails changed. Restarting service.");
                            // restart ourselves.
                            OnStopNetworkThreads();
                            OnStartThreadCode();
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        LogEvent(LogLevels.Err, LogPrefix + "MonitorThread: Getting Network Signature: Exception: " + e.ToString());
                        ExCount++;
                    }

                    try
                    {
                        Int64 SCnt = -1;
                        string SSig = "1";
                        GetSettingsSig(ref SCnt, ref SSig);
                        if (!monitorThreadStopping && (SettingsCnt != SCnt || SettingsSig != SSig))
                        {
                            // table has changed, reload.
                            LogEvent(LogLevels.Notice, LogPrefix + "MonitorThread: settings changed. Restarting service.");
                            // restart ourselves.
                            OnStopNetworkThreads();
                            OnStartThreadCode();
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        LogEvent(LogLevels.Err, LogPrefix + "MonitorThread: Getting Settings Signature: Exception: " + e.ToString());
                        ExCount++;
                    }

                    //we aren't restarting, so check/reset DBSettings
                    if (!monitorThreadStopping && GetDBSettings())  //CheckDbForForcedRestart())
                    {
                        //it returned true, so we have to restart.
                        // restart ourselves.
                        LogEvent(LogLevels.Notice, LogPrefix + "MonitorThread: CheckDbForForcedRestart was true, restarting....");
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
                        LogEvent(LogLevels.Err, LogPrefix + "MonitorThread: Network thread check: Exception: " + e.ToString());
                        ExCount++;
                    }

                    //LogEvent(LogPrefix + "MonitorThread (" + Thread.CurrentThread.ManagedThreadId + ") done...next check will run shortly");
                }
                //if (!monitorThreadStopping) mTimer.Change(mTime, Timeout.Infinite);    // schedule to run again
            }
            catch (Exception e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "MonitorThread: Stopping Engine. Exception(Global catch): " + e.ToString());
                //incrementing the exception count doesn't do any good here.
                Environment.Exit(1);
            }
        }



        //Returns true if restart is required.
        protected bool GetDBSettings()
        {
            bool RetVal=false;          //no restart required yet

            try
            {
                if (!dbMainMutex.WaitOne(2000))
                {
                    ExCount++;
                    LogEvent(LogLevels.Err, LogPrefix + "GetDBSettings: Mutex not acquired.");
                    return true;    //Just go ahead and force a restart
                }
            }
            catch { }
            OpenCheckDB();
            try
            {
                OdbcCommand Cmd = new OdbcCommand();
                Cmd.Connection = dbMainCon;

                Cmd.CommandText = "SELECT * FROM `settings` WHERE `Valid`=1;";
                OdbcDataReader DbReader = Cmd.ExecuteReader();

                int cdbID = DbReader.GetOrdinal("id");
                int cParm = DbReader.GetOrdinal("parameter");
                int cValu = DbReader.GetOrdinal("value");
                string Valu;
                while (DbReader.Read())
                {
                    Valu = DbReader.GetString(cValu);
                    if (DbReader.GetString(cParm) == "ForcePollingEngineRestart")
                    {
                        if (Convert.ToInt32(Valu) == 1)        //0=false, 1=true
                        {
                            RetVal = true;      //restart polling engine.
                            //clear this setting in the database, it's just a flag to us.
                            if(!UpdateSetting("ForcePollingEngineRestart", "0"))
                                LogEvent(LogLevels.Info, LogPrefix + "GetDBSettings: ForcePollingEngineRestart update failed.");
                        }
                    }
                    else if (DbReader.GetString(cParm) == "DBVersion")
                    {
                        Int64 Version = Int64.Parse(Valu);
                        if (Version < 20151216)
                        {
                            RetVal = true;      //restart polling engine.
                            LogEvent(LogLevels.Crit, LogPrefix + "GetDBSettings: Too old of database version." + Valu);
                        }
                    }
                }

                DbReader.Close();
                Cmd.Dispose();
            }
            catch (OdbcException e)
            {
                ExCount++;
                LogEvent(LogLevels.Err, LogPrefix + "GetDBSettings: OdbcException: " + e.ToString());
            }
            catch (Exception e)
            {
                ExCount++;
                LogEvent(LogLevels.Err, LogPrefix + "GetDBSettings: Exception: " + e.ToString());
            }
            try
            {
                dbMainMutex.ReleaseMutex();
            }
            catch { }
            return RetVal;
        }

        protected void CheckConstant(int iVal, string iParam)
        {
            UpdateSetting(iParam, iVal.ToString());
        }

        /*
         * No mutex needed, we are under the GetDBSettings or EnumToDatabase mutex
         */ 
        protected bool UpdateSetting(string iParam, string iVal)
        {
            bool RetVal = false;
            OpenCheckDB();

            try
            {
                OdbcCommand Cmd = new OdbcCommand();
                Cmd.Connection = dbMainCon;

                Cmd.CommandText = "SELECT * FROM `settings` WHERE `parameter`=? AND `Valid`=1;";
                Cmd.Prepare();
                Cmd.Parameters.AddWithValue("?", iParam);

                OdbcDataReader DbReader = Cmd.ExecuteReader();

                int cValu = DbReader.GetOrdinal("value");
                string dVal="";
                int RCnt = 0;

                while (DbReader.Read())
                {
                    RCnt++;
                    dVal = DbReader.GetString(cValu);
                }
                DbReader.Close();
                Cmd.Dispose();

                if (RCnt <= 0)
                {
                    //we just need to insert a new one
                    Cmd = new OdbcCommand();
                    Cmd.Connection = dbMainCon;
                    Cmd.CommandText = @"INSERT INTO `settings` (`parameter`, `value`, `Created`, `CreatedBy`, `Destroyed`, `DestroyedBy`, `Valid`) 
                        VALUES (?,?,NOW(),?,?,?,?);";
                    Cmd.Prepare();
                    Cmd.Parameters.AddWithValue("?", iParam);
                    Cmd.Parameters.AddWithValue("?", iVal);
                    Cmd.Parameters.AddWithValue("?", Constants.UserSystemEngine);
                    Cmd.Parameters.AddWithValue("?", "0000-00-00 00:00:00");
                    Cmd.Parameters.AddWithValue("?", 0);
                    Cmd.Parameters.AddWithValue("?", 1);
                    if (Cmd.ExecuteNonQuery() != 1)
                        LogEvent(LogLevels.Err, LogPrefix + "UpdateSetting: <=0: Insert failed.");
                    else
                        RetVal = true;
                    Cmd.Dispose();
                }
                else if (RCnt == 1 && dVal == iVal)
                {
                    //we only have one valid row, and the value matches, so quit
                    RetVal = true;
                }
                else
                {
                    //delete(any/all) and insert
                    Cmd = new OdbcCommand();
                    Cmd.Connection = dbMainCon;
                    Cmd.CommandText = @"UPDATE `settings` SET `Destroyed`=NOW(),`DestroyedBy`=?,`Valid`=0 WHERE `parameter`=? AND `Valid`=1;";
                    Cmd.Prepare();
                    Cmd.Parameters.AddWithValue("?", Constants.UserSystemEngine);
                    Cmd.Parameters.AddWithValue("?", iParam);
                    if (Cmd.ExecuteNonQuery() != 1)
                        LogEvent(LogLevels.Err, LogPrefix + "UpdateSetting: else: Update failed.");
                    Cmd.Dispose();

                    Cmd = new OdbcCommand();
                    Cmd.Connection = dbMainCon;
                    Cmd.CommandText = @"INSERT INTO `settings` (`parameter`, `value`, `Created`, `CreatedBy`, `Destroyed`, `DestroyedBy`, `Valid`) 
                        VALUES (?,?,NOW(),?,?,?,?);";
                    Cmd.Prepare();
                    Cmd.Parameters.AddWithValue("?", iParam);
                    Cmd.Parameters.AddWithValue("?", iVal);
                    Cmd.Parameters.AddWithValue("?", Constants.UserSystemEngine);
                    Cmd.Parameters.AddWithValue("?", "0000-00-00 00:00:00");
                    Cmd.Parameters.AddWithValue("?", 0);
                    Cmd.Parameters.AddWithValue("?", 1);
                    if (Cmd.ExecuteNonQuery() != 1)
                        LogEvent(LogLevels.Err, LogPrefix + "UpdateSetting: else: Insert failed.");
                    else
                        RetVal = true;
                    Cmd.Dispose();
                }
            }
            catch (OdbcException e)
            {
                ExCount++;
                LogEvent(LogLevels.Err, LogPrefix + "UpdateSetting: OdbcException: " + e.ToString());
            }
            catch (Exception e)
            {
                ExCount++;
                LogEvent(LogLevels.Err, LogPrefix + "UpdateSetting: Exception: " + e.ToString());
            }
            return RetVal;
        }


        protected void EnumToDatabase()
        {
            try
            {
                if (!dbMainMutex.WaitOne(2000))
                {
                    ExCount++;
                    LogEvent(LogLevels.Err, LogPrefix + "EnumToDatabase: Mutex not acquired.");
                    return;
                }
            }
            catch { }
            OpenCheckDB();
            try
            {
                CheckConstant((int)LogLevels.Emerg, "LogLevels_Emerg");
                CheckConstant((int)LogLevels.Alert, "LogLevels_Alert");
                CheckConstant((int)LogLevels.Crit, "LogLevels_Crit");
                CheckConstant((int)LogLevels.Err, "LogLevels_Err");
                CheckConstant((int)LogLevels.Warn, "LogLevels_Warn");
                CheckConstant((int)LogLevels.Notice, "LogLevels_Notice");
                CheckConstant((int)LogLevels.Info, "LogLevels_Info");
                CheckConstant((int)LogLevels.Debug, "LogLevels_Debug");
            }
            catch (OdbcException e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "DiagEnumToDatabase: OdbcException: " + e.ToString());
            }
            catch (Exception e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "DiagEnumToDatabase: Exception: " + e.ToString());
            }
            try
            {
                dbMainMutex.ReleaseMutex();
            }
            catch { }
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


        /// <summary>
        /// Gets the ODBC driver names from the registry.
        /// </summary>
        /// <returns>a string array containing the ODBC driver names, if the registry key is present; null, otherwise.</returns>
        public static string[] GetOdbcDriverNames32()
        {
            string[] odbcDriverNames = null;
            using (Microsoft.Win32.RegistryKey localMachineHive = Microsoft.Win32.Registry.LocalMachine)
            using (Microsoft.Win32.RegistryKey odbcDriversKey = localMachineHive.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"))
            {
                if (odbcDriversKey != null)
                {
                    odbcDriverNames = odbcDriversKey.GetValueNames();
                }
            }

            return odbcDriverNames;
        }
        /// <summary>
        /// Gets the ODBC driver names from the registry.
        /// </summary>
        /// <returns>a string array containing the ODBC driver names, if the registry key is present; null, otherwise.</returns>
        public static string[] GetOdbcDriverNames64()
        {
            string[] odbcDriverNames = null;
            using (Microsoft.Win32.RegistryKey localMachineHive = Microsoft.Win32.Registry.LocalMachine)
            using (Microsoft.Win32.RegistryKey odbcDriversKey = localMachineHive.OpenSubKey(@"SOFTWARE\Wow6432Node\ODBC\ODBCINST.INI\ODBC Drivers"))
            {
                if (odbcDriversKey != null)
                {
                    odbcDriverNames = odbcDriversKey.GetValueNames();
                }
            }

            return odbcDriverNames;
        }

        /// <summary>
        /// Gets the name of an ODBC driver for MySQL Server giving preference to the most recent one.
        /// </summary>
        /// <returns>the name of an ODBC driver for MySQL Server, if one is present; null, otherwise.</returns>
        //20180204 - Remove 3.51 as an option.
        public static string GetOdbcMySQLDriverName()
        {
            //When adding to this list, open ODBC Data Source Administrator (32-bit) --OR-- ODBC Data Source Administrator (64-bit) as appropriate, 
            //  select the drivers tab, and retrieve the driver names.
            List<string> driverPrecedence = new List<string>() { 
                "MySQL ODBC 8.0 Unicode Driver", "MySQL ODBC 8.0 ANSI Driver",
                "MySQL ODBC 5.3 Unicode Driver", "MySQL ODBC 5.3 ANSI Driver",
                "MySQL ODBC 5.2 Unicode Driver", "MySQL ODBC 5.2 ANSI Driver",
                "MySQL ODBC 5.1 Unicode Driver", "MySQL ODBC 5.1 ANSI Driver"};
            string[] OdbcDrivers32 = GetOdbcDriverNames32();
            string[] OdbcDrivers64 = GetOdbcDriverNames64();
            string driverName = null;

            string[] OdbcDrivers = new string[OdbcDrivers32.Length + OdbcDrivers64.Length];
            Array.Copy(OdbcDrivers32, OdbcDrivers, OdbcDrivers32.Length);
            Array.Copy(OdbcDrivers64, 0, OdbcDrivers, OdbcDrivers32.Length, OdbcDrivers64.Length);

            if (OdbcDrivers != null)
            {
                driverName = driverPrecedence.Intersect(OdbcDrivers).FirstOrDefault();
            }

            return driverName;
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



        /*public void RedisReadData()  
        {
            string Val = RDB.StringGet("EngineTest");
            LogEventLocal(String.Format("RedisReadData: Value={0}",Val));
            return;
        }  
        public void RedisSaveData()  
        {
            RDB.StringSet("EngineTest", "We are working.", TimeSpan.FromSeconds(25));
            return;
        }*/

        public void RedisStringSet(string key, string val)
        {
            try
            {
                REDW.Start();
                RDB.StringSet(key, val, TimeSpan.FromDays(10));
                REDW.Stop();
            }
            catch (Exception e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "RedisStringSet(key,val): Exception: " + e.ToString());
                ExCount++;
                REDW.StopCancel();
            }
        }
        //TimeSpan for expire time, or null for no expire.
        public void RedisStringSet(string key, string val, TimeSpan? t)
        {
            try
            {
                REDW.Start();
                RDB.StringSet(key, val, t);
                REDW.Stop();
            }
            catch (Exception e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "RedisStringSet(key,val,t): Exception: " + e.ToString());
                ExCount++;
                REDW.StopCancel();
            }
        }
        public RedisValue RedisStringGet(string key)
        {
            try
            {
                RedisValue Ret;
                REDR.Start();
                Ret = RDB.StringGet(key);
                REDR.Stop();
                return Ret;
            }
            catch (Exception e)
            {
                LogEvent(LogLevels.Err, LogPrefix + "RedisStringGet: Exception: " + e.ToString());
                ExCount++;
                REDR.StopCancel();
                return RedisValue.Null;
            }
        }
        public void TimingUpdate()
        {
            RedisStringSet("Engine_TimingStats",
                REDW.Stats() + "\r\n" +
                REDR.Stats());
        }


    }

    public class LimitedQueue<T> : Queue<T>
    {
        public int Limit { get; set; }

        public LimitedQueue(int limit)
            : base(limit)
        {
            Limit = limit;
        }
        public LimitedQueue(int limit, IEnumerable<T> items = null) 
            : base(items)
        {
            Limit = limit;
        }

        public new void Enqueue(T item)
        {
            while (Count >= Limit)
            {
                Dequeue();
            }
            base.Enqueue(item);
        }
    }

    public class LimitedStack<T>
    {
        public readonly int Limit;
        private readonly List<T> _stack;

        public LimitedStack(int limit = 32)
        {
            Limit = limit;
            _stack = new List<T>(limit);
        }

        public void Push(T item)
        {
            if (_stack.Count == Limit) _stack.RemoveAt(0);
            _stack.Add(item);
        }

        public T Peek()
        {
            return _stack[_stack.Count - 1];
        }

        public void Pop()
        {
            _stack.RemoveAt(_stack.Count - 1);
        }

        public int Count
        {
            get { return _stack.Count; }
        }
    }

    public class LogJSON
    {
        public string Msg;
        public string DT;
        public LogJSON(string nMsg)
        {
            Msg = nMsg;
            DT = DateTime.UtcNow.ToString("s");     //u=yyyy-mm-dd hh::mm::ssZ, s=yyyy-mm-ddThh:mm:ss
        }
    }





}
