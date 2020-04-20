using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PCLExt.FileStorage;
using PCLExt.FileStorage.Extensions;
using PCLExt.FileStorage.Folders;

namespace M.OBD2
{
    public class Logging
    {
        // Current logging status
        private bool isLoggingActive;
        private StringBuilder sbLogMessage;
        private DateTime LogStartTime;    // Log start time
        private string LogFileName;        // Log full file name
        private const string LOG_FORMAT = "#######.###";    // Log value format specifier
        private long LogEntryCount;
        private IFolder LogFolder;
        private IFile LogFile;
        private BlueToothCmds oBlueToothCmds;

        private const int LOG_RATE = 1000;
        private const string LOG_DELIMIT = "\t";
        private const string LOG_FOLDER = "MOBD2_LOGS";

        public bool GetLogging_Run()
        {
            return isLoggingActive;
        }

        public async void InitLogFile(BlueToothCmds blueToothCmds)
        {
            oBlueToothCmds = blueToothCmds ?? throw new Exception("Invalid Command List");

            if (!await CheckDocumentsFolder())
                throw new Exception("Could not acces document folder");

            StringBuilder sbLogFileName = new StringBuilder();

            if (sbLogMessage == null)
                sbLogMessage = new StringBuilder();

            sbLogMessage.Clear();

            LogEntryCount = 0;
            LogStartTime = DateTime.UtcNow;

            try
            {
                // Build log file name
                sbLogFileName.Append(LogStartTime);
                sbLogFileName.Replace("/", "-");
                sbLogFileName.Replace(":", "-");
                LogFileName = sbLogFileName.ToString();
                sbLogMessage.Append("Logging Started:" + LOG_DELIMIT + LogStartTime.ToShortTimeString() + Environment.NewLine);
                sbLogMessage.Append("Rate(ms):" + LOG_DELIMIT + LOG_RATE + LOG_DELIMIT);
                sbLogMessage.Append(Environment.NewLine);
                //sbLogMessage.Clear();
                sbLogMessage.Append("Time" + LOG_DELIMIT);

                // Iterate and add processes
                oBlueToothCmds.ForEach(x =>
                {
                    if (x.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER
                        || x.Selection_Type == BlueToothCmds.SELECTION_TYPE.USER_PROCESS)
                        sbLogMessage.Append(x.Name + LOG_DELIMIT);
                });

                if (!CreateFile(LogFileName, sbLogMessage.ToString()))
                    throw new Exception("Could not create log file");
            }
            catch (Exception ex) // ToDo: Exception handling
            {

            }
        }

        private async Task<bool> CheckDocumentsFolder()
        {
            try
            {
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
    }
}
