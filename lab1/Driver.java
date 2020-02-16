package lab1;
import java.util.Scanner;

public class Driver {
    public static void main(String[] args) {
        Scanner myScanner = new Scanner(System.in);
        String myString = myScanner.nextLine();

        BluetoothFactory btFactory = new BluetoothFactory(myString);
        Bluetooth myBT = btFactory.makeBluetooth();
        myBT.connect();
    }
}