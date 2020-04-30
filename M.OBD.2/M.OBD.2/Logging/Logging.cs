#region Using Statements
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
        private List<BluetoothCmd> lBluetoothCmds;
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
        private const string TIME_FORMATTER = "T";
        private const int LOG_UPDATE = 1000;        // Log write time in ms
        private const int TIMER_UPDATE = 250;       // Update timer iteration delay in ms

        private static string LogFileName;        // Log full file name
        private static string status_message;
        private string ProcessHeader;
        private readonly Label lblLogFile;
        private static bool isLogging;
        private static bool isError;

        #endregion

        #region Initialization

        public Logging(Label lblLogFile)
        {
            this.lblLogFile = lblLogFile;
            SetLogFileName();
        }

        public bool InitLogging(BlueToothCmds blueToothCmds)
        {
            isError = false;
            try
            {
                lBluetoothCmds = new List<BluetoothCmd>();
                oBlueToothCmds = blueToothCmds ?? throw new Exception("Invalid Command List");
                ProcessHeader = GetProcessHeader(blueToothCmds, lBluetoothCmds);

                CreateLogFile();
                
                return true;
            }
            catch (Exception e)
            {
                isError = true;
                status_message = e.Message;
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
                            WriteFile(dtCurrent);
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

        #endregion

        #region Log File Writing

        private void WriteFile(DateTime dtCurrent)
        {
            try
            {
                if (LogFolder == null || LogFile == null)
                    throw new Exception("Invalid log directory or file");

                LogFile.AppendTextAsync(GetProcessValues(dtCurrent, lBluetoothCmds, sbLogMessage));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error writing log:" + e.Message);
            }
        }

        private static string GetProcessValues(DateTime dtCurrent, List<BluetoothCmd> lbthcmds, StringBuilder sb)
        {
            if (lbthcmds == null || lbthcmds.Count == 0)
                return string.Empty;

            if (sb == null)
                sb = new StringBuilder();
            sb.Clear();

            sb.Append(dtCurrent.ToString(TIME_FORMATTER));
            sb.Append(LOG_DELIMIT);

            lbthcmds.ForEach(x =>
            {
                sb.Append(x.value.ToString(x.GetFormatter()));
                sb.Append(LOG_DELIMIT);
            });
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        #endregion

        #region Log File Creation

        private async void CreateLogFile()
        {
            SetLogFileName();

            if (oBlueToothCmds == null || ProcessHeader == null)
                throw new Exception("Failed to initialize logging");

            if (!await CheckDocumentsFolder())
                throw new Exception("Could not access document folder");

            StringBuilder sbLogFileName = new StringBuilder();

            if (sbLogMessage == null)
                sbLogMessage = new StringBuilder();

            sbLogMessage.Clear();
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

            SetLogFileName();
        }

        private static string GetProcessHeader(BlueToothCmds bthcmds, ICollection<BluetoothCmd> lbthcmds)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Time" + LOG_DELIMIT);

            lbthcmds.Clear();

            bthcmds.ForEach(x =>
            {
                // If valid append to string and add to working list
                if (!string.IsNullOrEmpty(x.Name)
                    && (x.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER
                        || x.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER_PROCESS))
                {
                    sb.Append(x.Name + LOG_DELIMIT);
                    lbthcmds.Add(x);
                }
            });
            sb.Append(Environment.NewLine);

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

        private bool CreateFile(string fileName, string content)
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

        private void SetLogFileName()
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

        #region Gets/Sets

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

        #endregion

    }
}
