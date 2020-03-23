#region Using Statements
using Android.Bluetooth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace M.OBD2
{
    public class Bluetooth : List<BluetoothConnection>
    {
        #region Declarations

        private static string status_message;
        private const char END_CHAR = '>';
        public const string LINE_BREAK = " \r";
        private static readonly string[] REPLACE_VALUES = { "SEARCHING", "\\s", LINE_BREAK, END_CHAR.ToString() };
        private const string CONNECTION_VERIFY = "ELM327";
        private const int MAX_LENGTH = 1000;
        private static bool isDebug, isTest;
        private BluetoothConnection oBluetoothConnection;
        private static string response;
        private readonly List<string> InitCommands;
        private readonly byte[] DTC_CLEAR;
        private const string RX_MESSAGE = "...";
        private static Random oRandom;

        #endregion

        #region Main Control

        public Bluetooth(bool _isDebug, bool _isTest)
        {
            status_message = string.Empty;
            isDebug = _isDebug;
            isTest = _isTest;

            InitCommands = new List<string>
            {
                "?\r",
                "ATZ\r", 
                //"AT@1\r", // Display device identifier
                //"ATE0\r",  // Echo off
                "ATL0\r",  // Linefeed off
                //"ATAT0\r", // Disable adaptive timing
                //"ATST0F", // Set 'nodata' timeout
                "ATSP0", // Search for protocol
                //"ATDP", // Display protocol
                "ATS0", // Remove spaces from ecu responses
                //"ATH1", // Display header info
                //"ATMA", // Display all info
                //"AT@1\r", 
                //"AT@2\r",
                "0100\r" // Keep alive
            };

            DTC_CLEAR = Encoding.ASCII.GetBytes("04" + "\r");
        }

        #endregion

        #region Connection Initialization Routines

        public bool LoadPairedDevices()
        {
            try
            {
                if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
                    throw new Exception("Bluetooth is not enabled");

                AddRange(BluetoothAdapter.DefaultAdapter.BondedDevices.Select(device => new BluetoothConnection(device.Name, device.Address)).ToList());

                SetStatusMessage(string.Format("{0} paired devices found", Count));

                return true;
            }
            catch (Exception e)
            {
                SetErrorMessage("Error Getting Paired Devices", e);
                return false;
            }
        }

        public bool CheckPairedDevices()
        {
            return Count > 0;
        }

        public List<BluetoothConnection> GetPairedDevices()
        {
            return this;
        }

        public async Task<bool> OpenPairedDevice(string name, string address)
        {
            oBluetoothConnection = await GetPairedDevice(name, address);

            return oBluetoothConnection != null;
        }

        private async Task<BluetoothConnection> GetPairedDevice(string name, string address)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address))
            {
                SetStatusMessage("No device parameters");
                return null;
            }

            foreach (BluetoothConnection bc in this.Where(bc => bc.device_name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                if (await OpenDevice(bc))
                {
                    await SendCommandAsync(bc, InitCommands[0]);
                    await SendCommandAsync(bc, InitCommands[1]);
                    if (CheckResponse(response, CONNECTION_VERIFY))
                    {
                        for (int i = 2; i < InitCommands.Count; i++)
                        {
                            if (!await SendCommandAsync(bc, InitCommands[i]))
                            {
                                SetStatusMessage("Failed to Initialize device");
                                return null;
                            }
                        }

                        SetStatusMessage("Device Connected!");
                        return bc;
                    }
                }

                SetStatusMessage("Failed to open device");
                return null;
            }

            SetStatusMessage("Could not find device");
            return null;
        }

        private static bool CheckResponse(string result, string value)
        {
            if (string.IsNullOrEmpty(result) || string.IsNullOrEmpty(value))
                return false;

            return result.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static async Task<bool> OpenDevice(BluetoothConnection bc)
        {
            try
            {
                if (isDebug)
                    Debug.WriteLine("Opening Device Name: {0} Address: {1}", bc.device_name, bc.device_address);

                BluetoothDevice oBthDevice = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(bc.device_address);

                if (oBthDevice == null)
                    throw new Exception("Unable to connect to device");

                Android.OS.ParcelUuid[] puids = oBthDevice.GetUuids();

                if (puids == null || puids.Length == 0)
                    throw new Exception("Invalid device UUID's");

                BluetoothSocket oBthSocket = oBthDevice.CreateRfcommSocketToServiceRecord(puids[0].Uuid);

                if (oBthSocket == null)
                    throw new Exception("Unable to create connection socket");

                await oBthSocket.ConnectAsync();

                if (!oBthSocket.IsConnected)
                    throw new Exception("Unable to connect to socket");

                bc.oBthDevice = oBthDevice;
                bc.oBthSocket = oBthSocket;

                //status_message = string.Format("Device {0} Connected!", bc.device_name);

                return true;
            }
            catch (Exception e)
            {
                SetErrorMessage("Error Opening Device " + bc.device_name, e);
                return false;
            }
        }

        public static bool CheckAdapterPresent()
        {
            if (BluetoothAdapter.DefaultAdapter == null)
            {
                SetStatusMessage("Bluetooth is not available on this device");
                return false;
            }
            return true;
        }

        public static bool CheckAdapterEnabled()
        {
            if (!BluetoothAdapter.DefaultAdapter.IsEnabled)
            {
                SetStatusMessage("Bluetooth is not enabled");
                return false;
            }

            return true;
        }

        #endregion

        #region Test Related

        public bool SendCommandAsync_Test(BluetoothCmd bcmd)
        {
            if (isDebug)
                Debug.WriteLine("Tx: " + bcmd.Cmd);

            bcmd.sbResponse.Clear();

            try
            {
                bcmd.sbResponse.Append(ReadData_Test(bcmd));
                bcmd.tx_good++;

                bool result = ValidateResponse(bcmd);

                if (isDebug)
                    Debug.WriteLine("Rx: {0}  Valid:{1}", response, result);

                return result;
            }
            catch (Exception e)
            {
                status_message = string.Format("{0}: {1}", "Read Error", e.Message);
                bcmd.tx_fail++;

                if (isDebug)
                    Debug.WriteLine(status_message);
                return false;
            }
        }

        private static string ReadData_Test(BluetoothCmd bcmd)
        {
            StringBuilder sb = new StringBuilder();

            if (oRandom == null)
                oRandom = new Random();

            sb.Append(bcmd.Cmd);

            if (!string.IsNullOrEmpty(bcmd.Expression))
            {
                for (int i = 0; i < bcmd.Bytes; i++)
                {
                    sb.Append(oRandom.Next(0, 255).ToString("X2"));
                }
            }
            else
                sb.Append("FF");

            if (sb.Length == 0 || sb.Length > MAX_LENGTH)
                return string.Empty;

            return sb.ToString();
        }

        #endregion

        #region Command Sending and Receiving

        public async Task<bool> SendCommandAsync(BluetoothCmd bcmd)
        {
            BluetoothConnection bc = oBluetoothConnection;

            if (bc == null || !bc.oBthSocket.IsConnected)
                return false;

            if (isDebug)
                Debug.WriteLine("Tx: " + bcmd.Cmd);

            bcmd.sbResponse.Clear();

            try
            {
                await bc.oBthSocket.OutputStream.WriteAsync(bcmd.CmdBytes, 0, bcmd.CmdBytes.Length);
                await bc.oBthSocket.OutputStream.FlushAsync();

                bcmd.sbResponse.Append(ReadData(bc.oBthSocket));
                bcmd.tx_good++;

                bool result = ValidateResponse(bcmd);

                if (isDebug)
                    Debug.WriteLine("Rx: {0}  Valid:{1}", response, result);

                return result;
            }
            catch (Exception e)
            {
                status_message = string.Format("{0}: {1}", "Read Error", e.Message);
                bcmd.tx_fail++;

                if (isDebug)
                    Debug.WriteLine(status_message);
                return false;
            }
        }

        public static async Task<bool> SendCommandAsync(BluetoothConnection bc, string command)
        {
            if (bc == null || !bc.oBthSocket.IsConnected)
                return false;

            if (isDebug)
                Debug.WriteLine("Tx: " + command);

            response = string.Empty;

            byte[] cmd = Encoding.ASCII.GetBytes(command + LINE_BREAK);

            try
            {
                await bc.oBthSocket.OutputStream.WriteAsync(cmd, 0, cmd.Length);
                await bc.oBthSocket.OutputStream.FlushAsync();

                response = ReadData(bc.oBthSocket);

                if (!string.IsNullOrEmpty(response)) // ToDo: process values!
                {

                    if (isDebug)
                        Debug.WriteLine("Rx: " + response);
                }

                return true;
            }
            catch (Exception e)
            {
                status_message = string.Format("{0}: {1}", "Read Error", e.Message);

                if (isDebug)
                    Debug.WriteLine(status_message);
                return false;
            }
        }

        private static string ReadData(BluetoothSocket bs)
        {
            StringBuilder sb = new StringBuilder();
            char c;

            do
            {
                c = (char)bs.InputStream.ReadByte();
                sb.Append(c);
            }
            while (c != END_CHAR);

            if (sb.Length == 0 || sb.Length > MAX_LENGTH)
                return string.Empty;

            foreach (string str in REPLACE_VALUES)
            {
                sb.Replace(str, string.Empty);
            }

            return sb.ToString();
        }

        #endregion

        #region Response Processing

        private static bool ValidateResponse(BluetoothCmd bcmd)
        {
            if (bcmd.sbResponse.Length == 0 || bcmd.sbResponse.Length <= bcmd.Cmd.Length) // If response is empty or length is invalid 
                return false;

            string msg = bcmd.sbResponse.ToString().Substring(0, bcmd.Cmd.Length); // Isolate the expected echoed value
            return msg.Equals(bcmd.Cmd, StringComparison.OrdinalIgnoreCase) && ProcessResponse(bcmd); // Return if echoed portion found and the processing result
        }

        private static bool ProcessResponse(BluetoothCmd bcmd)
        {
            // Trim the returned value portion
            bcmd.Response = bcmd.sbResponse.ToString().Substring(bcmd.Cmd.Length).Trim();

            if (!string.IsNullOrEmpty(bcmd.Response)) // Check if valid
            {
                if (!bcmd.isRxBytes) // If a string message: update counter, return as success
                {
                    bcmd.rx_good++;
                    return true;
                }

                // ToDo: Debug test value
                // bcmd.Response = "41053F";

                if (bcmd.Bytes != 0 && !bcmd.Response.StartsWith(RX_MESSAGE, StringComparison.OrdinalIgnoreCase)) // If we are expecting bytes returned and response is valid
                {
                    // Attempt to parse the hex value
                    if (int.TryParse(bcmd.Response.Substring(bcmd.Response.Length - (bcmd.Bytes * 2)), NumberStyles.HexNumber, null, out int result))
                    {
                        // Store result and call math expression parser
                        bcmd.rxvalue = result;
                        if (bcmd.Calculate()) // On success: update counter and return result
                        {
                            bcmd.rx_good++;
                            return true;
                        }
                    }
                }
            }
            // Response was invalid: update counter and return as failed
            bcmd.rx_fail++;
            return false;
        }

        #endregion

        #region Status Messages

        public static string GetStatusMessage()
        {
            return status_message;
        }

        private static void SetStatusMessage(string message)
        {
            status_message = message;
        }

        private static void SetErrorMessage(string msg, Exception e)
        {
            status_message = string.Format("{0}: {1}", msg, e.Message);

            if (isDebug)
                Debug.WriteLine(status_message);
        }

        #endregion

        #region Misc

        public bool isTestMode()
        {
            return isTest;
        }

        #endregion
    }
}