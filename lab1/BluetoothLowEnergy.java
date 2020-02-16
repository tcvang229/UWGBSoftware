package lab1;

public class BluetoothLowEnergy extends Bluetooth{
    public BluetoothLowEnergy() {
        super.setType("Bluetooth Low Energy");
    }

    @Override
    public void connect() {
        // do some connecting with bluetooth API
    }
}