using System.Collections;
using UnityEngine;

public class ReconnectManager : MonoBehaviour
{
    public static ReconnectManager Instance { get; private set; }
    
    private int maxAttempts = 5;
    private float attemptInterval = 3f;
    private string lastAddress;
    private bool isReconnecting = false;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void SaveConnectionInfo(string address) => lastAddress = address;
    
    public void StartReconnect()
    {
        if (isReconnecting || string.IsNullOrEmpty(lastAddress)) return;
        
        GameManager.instance?.PauseGame();
        StartCoroutine(ReconnectCoroutine());
    }
    
    private IEnumerator ReconnectCoroutine()
    {
        isReconnecting = true;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            BluetoothUI.Instance?.ShowReconnecting(i + 1, maxAttempts);
            BluetoothGameManager.Instance?.ConnectToDevice(lastAddress);
            
            yield return new WaitForSeconds(attemptInterval);
            
            if (BluetoothGameManager.Instance?.isConnected == true)
            {
                OnReconnectSuccess();
                yield break;
            }
        }
        
        OnReconnectFailed();
    }
    
    private void OnReconnectSuccess()
    {
        isReconnecting = false;
        GameStateSync.Instance?.SendCurrentState();
        GameManager.instance?.ResumeGame();
        BluetoothUI.Instance?.Hide();
        HeartbeatManager.Instance?.StartHeartbeat();
        Debug.Log("✅ Reconnected!");
    }
    
    private void OnReconnectFailed()
    {
        isReconnecting = false;
        BluetoothUI.Instance?.ShowReconnectFailed(
            () => StartReconnect(),
            () => {
                GameManager.instance.currentMode = GameMode.Local;
                GameManager.instance?.ResumeGame();
                BluetoothUI.Instance?.Hide();
            }
        );
    }
}
