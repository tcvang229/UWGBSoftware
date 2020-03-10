package lab1;

public class BluetoothFactory {
    private String bluetoothType;

    public BluetoothFactory(String bluetoothType) {
        this.bluetoothType = bluetoothType;
    }

    public Bluetooth makeBluetooth() {
        if (bluetoothType.equals("ble")) {
            return new BluetoothLowEnergy();
        } else if (bluetoothType.equals("bt")) {
            return new BluetoothTraditional();
        }

        return null;
    }
}