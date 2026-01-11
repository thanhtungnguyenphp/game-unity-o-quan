using UnityEngine;

public class HeartbeatManager : MonoBehaviour
{
    public static HeartbeatManager Instance { get; private set; }
    
    private float heartbeatInterval = 2f;
    private float timeoutDuration = 6f;
    private float lastHeartbeatSent;
    private float lastHeartbeatReceived;
    private bool isActive = false;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    void Update()
    {
        if (!isActive) return;
        if (BluetoothGameManager.Instance?.isConnected != true) return;
        
        if (Time.time - lastHeartbeatSent >= heartbeatInterval)
        {
            SendHeartbeat();
            lastHeartbeatSent = Time.time;
        }
        
        if (Time.time - lastHeartbeatReceived > timeoutDuration)
        {
            OnConnectionTimeout();
        }
    }
    
    public void StartHeartbeat()
    {
        // Temporarily disabled for stability testing
        isActive = false;
        lastHeartbeatReceived = Time.time;
        lastHeartbeatSent = Time.time;
    }
    
    public void StopHeartbeat() => isActive = false;
    
    public void OnHeartbeatReceived() => lastHeartbeatReceived = Time.time;
    
    private void SendHeartbeat()
    {
        var hb = new HeartbeatMessage
        {
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            moveCount = GameStateSync.Instance?.MoveCount ?? 0
        };
        var msg = BluetoothMessage.Create(BluetoothMessageType.Heartbeat, hb);
        BluetoothGameManager.Instance?.SendMessage(msg);
    }
    
    private void OnConnectionTimeout()
    {
        Debug.LogWarning("⚠️ Connection timeout!");
        isActive = false;
        ReconnectManager.Instance?.StartReconnect();
    }
}
