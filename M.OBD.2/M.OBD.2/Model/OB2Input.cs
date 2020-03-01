using System;
namespace M.OBD.Model
{
    public class OB2Input : AbstractInput
    {
        public OB2Input()
        {
        }

        //need to put more functionality here
        public override void getInput()
        {
            Console.WriteLine("OB2Input abstract class - get input method called");
        }
    }
}
