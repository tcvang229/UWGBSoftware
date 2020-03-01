using System;
namespace M.OBD.Model
{
    public class OBD2Factory : AbstractToolFactory
    {
        public OBD2Factory()
        {
        }

        public override AbstractInput buildInput()
        {
            return new OB2Input();
        }

        public override AbstractOutput buildOutput()
        {
            return new OB2Output();
        }
    }
}
