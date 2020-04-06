
namespace M.OBD2
{
    public class UnitFactory
    {
        private static readonly UnitsMetric unitsMetric = new UnitsMetric();
        private static readonly UnitsImperial unitsImperial = new UnitsImperial();

        public static void InitUnitType(BluetoothCmd bthcmd, UserSetting.UNIT_TYPE Unit_Type)
        {
            if (Unit_Type == UserSetting.UNIT_TYPE.IMPERIAL)
                unitsImperial.InitType(bthcmd);
            else
                unitsMetric.InitType(bthcmd);
        }
    }
}
