using System;
using System.Collections.Generic;
using System.Text;
using M.OBD2;

namespace M.OBD
{
    /// <summary>
    /// Common initializing and update methods on content page loading and appearing
    /// </summary>

    internal interface IPageLoad
    { 
        // Contructor Methods
        void InitControls();
        void InitUserSettings();
        void InitBluetooth(out Bluetooth bluetooth);

        // Page refresh/update on appear methods
        void Page_Appearing(object sender, EventArgs e);
        void UpdateUserSettings();
        void UpdateControls();
    }
}