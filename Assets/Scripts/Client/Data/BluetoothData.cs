using System;

[Serializable]
public class MoveData
{
    public int cellIndex;
    public int direction;
    public int turn;
}

[Serializable]
public class DeviceInfo
{
    public string name;
    public string address;
}

public enum GameMode
{
    Local,
    Bluetooth
}
