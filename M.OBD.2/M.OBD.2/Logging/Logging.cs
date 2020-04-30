﻿#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCLExt.FileStorage;
using PCLExt.FileStorage.Extensions;
using PCLExt.FileStorage.Folders;
using Xamarin.Forms;
#endregion

namespace M.OBD2
{
    public class Logging
    {
        #region Declarations

        private BlueToothCmds oBlueToothCmds;
        private StringBuilder sbLogMessage;
        private static IFolder LogFolder;
        private IFile LogFile;
        private DateTime LogStartTime;    // Log start time
        
        private const string LOG_FILTER = "*.txt"; // File search filter
        private const string LOG_DELIMIT = "\t";
        private const string LOG_FOLDER = "MOBD2_LOGS";
        private const string LOG_FILE_EXT = ".txt";
        private const string LOG_NAME_HEADER = "Log File: ";
        private const string LOG_TITLE = "*** MOBD2 Generate Log ***";
        private const int TIMER_UPDATE = 250;       // Update timer iteration delay in ms
        private const int LOG_UPDATE = 1000;
        
        private static string LogFileName;        // Log full file name
        private static string status_message;
        private string ProcessHeader;
        private readonly Label lblLogFile;
        private long LogEntryCount;
        private static bool isLogging;
        private static bool isError;

        #endregion

        #region Initialization

        public Logging(Label lblLogFile)
        {
            this.lblLogFile = lblLogFile;
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
            ClearLogFileName();

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
            sbLogFileName.Append(LOG_FILE_EXT);
            LogFileName = sbLogFileName.ToString();

            // Build log message
            sbLogMessage.Append(LOG_TITLE + Environment.NewLine);
            sbLogMessage.Append("Logging Started:" + LOG_DELIMIT + LogStartTime.ToShortTimeString() + Environment.NewLine);
            sbLogMessage.Append("Rate(ms):" + LOG_DELIMIT + TIMER_UPDATE + LOG_DELIMIT);
            sbLogMessage.Append(Environment.NewLine);
            sbLogMessage.Append(ProcessHeader);

            if (!CreateFile(LogFileName, sbLogMessage.ToString()))
                throw new Exception("Could not create log file");

            SetLogFileName(LogFileName);
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

        private static async Task<bool> CheckDocumentsFolder()
        {
            try
            {
                // ToDo: change path?
                LogFolder = null;
                DocumentsRootFolder rootFolder = new DocumentsRootFolder();
                LogFolder = await rootFolder.CreateFolderAsync(LOG_FOLDER, CreationCollisionOption.OpenIfExists);
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

            DateTime dtCurrent;
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

        public static void StopLogging()
        {
            isLogging = false;
        }

        public static bool CheckError()
        {
            return isError;
        }

        private void ClearLogFileName()
        {
            SetLogFileName(string.Empty);
        }

        private void SetLogFileName(string name)
        {
            LogFileName = LOG_NAME_HEADER + (string.IsNullOrEmpty(LogFileName) ? string.Empty : LogFileName);
            lblLogFile.Text = LogFileName;
        }
        #endregion

        #region File List Retrieval and Viewing

        public static async Task<List<string>> GetLogFiles()
        {
            if (LogFolder == null && !await CheckDocumentsFolder())
                throw new Exception("Could not open log folder.");

            if (LogFolder == null)
                return null;

            IList<IFile> ifiles = await LogFolder.GetFilesAsync(LOG_FILTER, FolderSearchOption.AllFolders);

            if (ifiles == null || ifiles.Count == 0)
                throw  new Exception("No files found");

            List<string> files = new List<string>();
            files.AddRange(ifiles.Select(f => f.Name));

            return files;
        }

        public static string GetLogFilePath(string fname)
        {
            if (LogFolder == null)
                throw new Exception("Invalid Log Folder");

            return Path.Combine(LogFolder.Path, fname);
        }

        #endregion

    }
}
