using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace M.OBD2
{
    public class ProcessItem
    {
        public long id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Units { get; set; }
        public string ImageSource { get; set; }
        public Color NameColor { get; set; }
        public Color ValueColor { get; set; }
        public Color UnitsColor { get; set; }
    }

    public class ProcessItems : List<ProcessItem>
    {
    }
}
