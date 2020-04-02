using System;
using System.Collections.Generic;
using System.Text;

namespace M.OBD2
{
    public class Logging
    {
        // Current logging status
        private bool isLoggingActive;

        public Logging()
        {
            isLoggingActive = false;
        }

        public bool GetLogging_Run()
        {
            return isLoggingActive;
        }
    }
}
