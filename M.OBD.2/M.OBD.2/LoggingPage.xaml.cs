using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M.OBD._2;
using M.OBD2;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M.OBD
{

    public partial class LoggingPage : ContentPage, IPageLoad
    {
        #region Declarations

        private readonly Bluetooth oBluetooth;

        #endregion

        #region Page Initialization and Update

        public LoggingPage()
        {
            InitializeComponent();

            InitBluetooth(out oBluetooth);
            InitUserSettings();
            InitControls();
        }

        public void InitBluetooth(out Bluetooth bluetooth)
        {
            bluetooth = App.GetBluetooth();
        }

        public void InitControls()
        {
            Appearing += Page_Appearing;
            //btnLogOn.Clicked += btnLogOn_Clicked;
            //btnLogOff.Clicked += btnLogOff_Clicked;
        }

        public void InitUserSettings()
        {

        }

        public void Page_Appearing(object sender, EventArgs e)
        {
            UpdateControls();
            UpdateUserSettings();
        }

        public void UpdateControls()
        {
            //btnLogOn.IsEnabled = App.isBluetoothDisconnected();
            //btnLogOff.IsEnabled = !App.isBluetoothDisconnected();
        }

        public void UpdateUserSettings()
        {

        }

        #endregion


        void ToolbarItem_Clicked(System.Object sender, System.EventArgs e)
        {
            //TODO

        }
    }
}