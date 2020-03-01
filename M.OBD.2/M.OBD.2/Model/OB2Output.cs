using System;
namespace M.OBD.Model
{
    public class OB2Output : AbstractOutput
    {
        public OB2Output()
        {
        }

        //need to put more functionality here
        public override void displayOutput()
        {
            Console.WriteLine("OB2Output abstract class - display output method called");
        }
    }
}
