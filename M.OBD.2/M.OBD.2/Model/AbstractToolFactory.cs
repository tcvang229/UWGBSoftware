namespace M.OBD.Model
{
    public abstract class AbstractToolFactory
    {
        public AbstractToolFactory()
        {
        }

        public abstract AbstractOutput buildOutput();
        public abstract AbstractInput buildInput();

    }
}
