using System;
using UnityEngine;

/// <summary>
/// Resolves role conflict when both devices try to create room
/// Uses device address comparison - higher address becomes host
/// </summary>
public class BluetoothRoleResolver : MonoBehaviour
{
    public static BluetoothRoleResolver Instance { get; private set; }
    
    private string _myAddress;
    private string _peerAddress;
    private bool _roleResolved;
    
    public event Action<bool> OnRoleResolved; // true = isHost
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void SetMyAddress(string address) => _myAddress = address;
    
    public void OnPeerConnected(string peerAddress)
    {
        _peerAddress = peerAddress;
        if (!_roleResolved) ResolveRole();
    }
    
    private void ResolveRole()
    {
        if (string.IsNullOrEmpty(_myAddress) || string.IsNullOrEmpty(_peerAddress)) return;
        
        // Higher address becomes host (deterministic)
        bool isHost = string.Compare(_myAddress, _peerAddress, StringComparison.OrdinalIgnoreCase) > 0;
        _roleResolved = true;
        
        Debug.Log($"🎯 Role resolved: {(isHost ? "HOST" : "CLIENT")} (my:{_myAddress} vs peer:{_peerAddress})");
        OnRoleResolved?.Invoke(isHost);
    }
    
    public void Reset()
    {
        _peerAddress = null;
        _roleResolved = false;
    }
}
