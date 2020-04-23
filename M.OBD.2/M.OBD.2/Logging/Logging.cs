using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PCLExt.FileStorage;
using PCLExt.FileStorage.Extensions;
using PCLExt.FileStorage.Folders;
using Xamarin.Forms;

namespace M.OBD2
{
    public class Logging
    {
        // Current logging status
        private bool isLoggingActive;
        private StringBuilder sbLogMessage;
        private DateTime LogStartTime;    // Log start time
        private static string LogFileName;        // Log full file name
        private const string LOG_FORMAT = "#######.###";    // Log value format specifier
        private long LogEntryCount;
        private IFolder LogFolder;
        private IFile LogFile;
        private BlueToothCmds oBlueToothCmds;
        private string ProcessHeader;

        private const string LOG_DELIMIT = "\t";
        private const string LOG_FOLDER = "MOBD2_LOGS";
        private static bool isLogging;
        private const int TIMER_UPDATE = 250;       // Update timer iteration delay in ms
        private const int LOG_UPDATE = 1000;
        private static string status_message;
        private static bool isError;

        private const string LOG_NAME_HEADER = "Log File: ";
        private const string LOG_NAME_NONE = "None";

        #region Initialization

        public Logging()
        {
            ClearLogFileName();
        }

        public bool InitLogging(BlueToothCmds blueToothCmds)
        {
            isError = false;
            try
            {
                oBlueToothCmds = blueToothCmds ?? throw new Exception("Invalid Command List");
                ProcessHeader = GetProcessHeader(blueToothCmds);

                InitLogFile();
                
                return true;
            }
            catch (Exception e)
            {
                isError = true;
                status_message = e.Message;
                return false;
            }
        }

        public async void InitLogFile()
        {
            if (oBlueToothCmds == null || ProcessHeader == null)
                throw new Exception("Failed to initialize logging");

            if (!await CheckDocumentsFolder())
                throw new Exception("Could not access document folder");

            StringBuilder sbLogFileName = new StringBuilder();

            if (sbLogMessage == null)
                sbLogMessage = new StringBuilder();

            sbLogMessage.Clear();

            LogEntryCount = 0;
            LogStartTime = DateTime.UtcNow;

            // Build log file name
            sbLogFileName.Append(LogStartTime);
            sbLogFileName.Replace("/", "-");
            sbLogFileName.Replace(":", "-");
            LogFileName = sbLogFileName.ToString();
            sbLogMessage.Append("Logging Started:" + LOG_DELIMIT + LogStartTime.ToShortTimeString() + Environment.NewLine);
            sbLogMessage.Append("Rate(ms):" + LOG_DELIMIT + TIMER_UPDATE + LOG_DELIMIT);
            sbLogMessage.Append(Environment.NewLine);

            // Add process header
            sbLogMessage.Append(ProcessHeader);

            if (!CreateFile(LogFileName, sbLogMessage.ToString()))
                throw new Exception("Could not create log file");
        }

        private static string GetProcessHeader(BlueToothCmds bthcmds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Time" + LOG_DELIMIT);

            bthcmds.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.Name) 
                    && (x.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER
                    || x.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER_PROCESS))
                    sb.Append(x.Name + LOG_DELIMIT);
            });

            return sb.ToString();
        }

        private async Task<bool> CheckDocumentsFolder()
        {
            try
            {
                // ToDo: change path?
                LogFolder = null;
                DocumentsRootFolder rootFolder = new DocumentsRootFolder();
                LogFolder = await rootFolder.CreateFolderAsync("MOBD2_LOGS", CreationCollisionOption.OpenIfExists);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateFile(string fileName, string content)
        {
            if (LogFolder == null)
                return false;

            // ToDo: change default document path: /data/user/0/uwgb.dylanhoffman.m_obd_2/files/MOBD2_LOGS/"filename"
            try
            {
                LogFile = LogFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting).Result;
                LogFile.WriteAllTextAsync(content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Main Process

        public void RunLogging()
        {
            isLogging = true;

            DateTime dtCurrent = DateTime.UtcNow;
            DateTime dtNext = DateTime.UtcNow;

            Device.StartTimer
            (
                TimeSpan.FromMilliseconds(TIMER_UPDATE), () =>
                {
                    dtCurrent = DateTime.UtcNow;
                    
                    try
                    {
                        if (dtCurrent >= dtNext)
                        {
                            dtNext = dtCurrent.AddMilliseconds(LOG_UPDATE);
                            Debug.WriteLine("Log entry at: " + dtCurrent);
                        }
                    }
                    catch (Exception e)
                    {
                        isError = true;
                        status_message = e.Message;
                        return false;
                    }
                    return isLogging;
                }
            );
        }

        public static bool GetIsLogging()
        {
            return isLogging;
        }

        public static void StartLogging()
        {
            isLogging = true;
        }

        public static void StopLogging()
        {
            isLogging = false;
        }

        public static bool CheckError()
        {
            return isError;
        }

        public static string GetLogFileName()
        {
            return LOG_NAME_HEADER +  (string.IsNullOrEmpty(LogFileName) ? LOG_NAME_NONE : LogFileName);
        }

        private static void ClearLogFileName()
        {
            LogFileName = "None";
        }

        private static void SetLogFileName(string name)
        {
            LogFileName = string.IsNullOrEmpty(name) ? LOG_NAME_NONE : name;
        }
        #endregion
    }
}
