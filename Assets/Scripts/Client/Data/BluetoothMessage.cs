using System;
using UnityEngine;

public enum BluetoothMessageType
{
    Move = 0,
    StateSync = 1,
    Heartbeat = 2,
    Ack = 3,
    RequestSync = 4,
    GameOver = 5,
    KeyExchange = 6,
    Ready = 7
}

[Serializable]
public class BluetoothMessage
{
    public int type;
    public int messageId;
    public string payload;
    public long timestamp;
    
    private static int _nextId = 0;
    
    public static BluetoothMessage Create<T>(BluetoothMessageType msgType, T data)
    {
        return new BluetoothMessage
        {
            type = (int)msgType,
            messageId = _nextId++,
            payload = JsonUtility.ToJson(data),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}

[Serializable]
public class MoveMessage
{
    public int cellIndex;
    public int direction;
    public int turn;
    public int moveNumber;
}

[Serializable]
public class StateSyncMessage
{
    public int[] board;
    public int p1Score;
    public int p2Score;
    public int currentTurn;
    public int moveCount;
    public bool quan1Available;
    public bool quan2Available;
}

[Serializable]
public class HeartbeatMessage
{
    public long timestamp;
    public int moveCount;
}

[Serializable]
public class AckMessage
{
    public int messageId;
    public bool success;
}

[Serializable]
public class KeyExchangeMessage
{
    public string key;
}

[Serializable]
public class ReadyMessage
{
    public bool isHost;
    public string deviceId;
}
