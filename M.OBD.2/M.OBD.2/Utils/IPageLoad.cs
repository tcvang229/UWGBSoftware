using System;
using System.Collections.Generic;
using System.Text;
using M.OBD2;

namespace M.OBD
{
    /// <summary>
    /// Common initializing and update methods per content page on loading and appearing
    /// </summary>

    internal interface IPageLoad
    { 
        // Contructor Methods
        void InitControls();
        void InitUserSettings(out UserSetting usersetting);
        void InitBluetooth(out Bluetooth bluetooth);

        // Page refresh/update on appear methods
        void Page_Appearing(object sender, EventArgs e);
        void UpdateUserSettings();
        void UpdateControls();
    }
}