package lab1;

public abstract class Bluetooth {
    String btType;

    public void setType(String btType) {
        this.btType = btType;
    }

    public String getType() { 
        return this.btType;
    };

    abstract public void connect();
}