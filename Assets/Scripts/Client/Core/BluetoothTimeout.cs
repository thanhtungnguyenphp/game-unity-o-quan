using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Timeout manager for Bluetooth operations
/// </summary>
public class BluetoothTimeout : MonoBehaviour
{
    public static BluetoothTimeout Instance { get; private set; }
    
    public float ScanTimeout = 30f;
    public float ConnectTimeout = 15f;
    public float ResponseTimeout = 10f;
    
    private Coroutine _currentTimeout;
    public event Action OnTimeout;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void StartTimeout(float seconds, Action onTimeout)
    {
        CancelTimeout();
        _currentTimeout = StartCoroutine(TimeoutCoroutine(seconds, onTimeout));
    }
    
    public void StartScanTimeout(Action onTimeout) => StartTimeout(ScanTimeout, onTimeout);
    public void StartConnectTimeout(Action onTimeout) => StartTimeout(ConnectTimeout, onTimeout);
    public void StartResponseTimeout(Action onTimeout) => StartTimeout(ResponseTimeout, onTimeout);
    
    public void CancelTimeout()
    {
        if (_currentTimeout != null)
        {
            StopCoroutine(_currentTimeout);
            _currentTimeout = null;
        }
    }
    
    private IEnumerator TimeoutCoroutine(float seconds, Action onTimeout)
    {
        yield return new WaitForSeconds(seconds);
        Debug.LogWarning($"⏰ Bluetooth timeout after {seconds}s");
        OnTimeout?.Invoke();
        onTimeout?.Invoke();
        _currentTimeout = null;
    }
}
