using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Simple XOR encryption for Bluetooth data
/// </summary>
public static class BluetoothEncryption
{
    private static byte[] _sessionKey;
    
    public static void GenerateSessionKey()
    {
        _sessionKey = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
            rng.GetBytes(_sessionKey);
    }
    
    public static void SetSessionKey(byte[] key) => _sessionKey = key;
    public static byte[] GetSessionKey() => _sessionKey;
    
    public static byte[] Encrypt(byte[] data)
    {
        if (_sessionKey == null) return data;
        var result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
            result[i] = (byte)(data[i] ^ _sessionKey[i % _sessionKey.Length]);
        return result;
    }
    
    public static byte[] Decrypt(byte[] data) => Encrypt(data); // XOR is symmetric
    
    public static string EncryptString(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(Encrypt(bytes));
    }
    
    public static string DecryptString(string encrypted)
    {
        var bytes = Convert.FromBase64String(encrypted);
        return Encoding.UTF8.GetString(Decrypt(bytes));
    }
}
