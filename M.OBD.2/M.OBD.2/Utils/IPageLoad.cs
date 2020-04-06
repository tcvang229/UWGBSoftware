using M.OBD2;
using System;

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