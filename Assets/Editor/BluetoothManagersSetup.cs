using UnityEngine;
using UnityEditor;

public class BluetoothManagersSetup : Editor
{
    [MenuItem("Tools/Bluetooth/Setup Managers")]
    public static void SetupManagers()
    {
        // Find or create BluetoothManagers object
        var managers = GameObject.Find("BluetoothManagers");
        if (managers == null)
        {
            managers = new GameObject("BluetoothManagers");
            Undo.RegisterCreatedObjectUndo(managers, "Create BluetoothManagers");
        }
        
        // Add components if not exist
        AddComponentIfMissing<BluetoothMessageQueue>(managers);
        AddComponentIfMissing<GameStateSync>(managers);
        AddComponentIfMissing<HeartbeatManager>(managers);
        AddComponentIfMissing<ReconnectManager>(managers);
        AddComponentIfMissing<BluetoothTimeout>(managers);
        AddComponentIfMissing<BluetoothRoleResolver>(managers);
        
        Debug.Log("✅ BluetoothManagers setup complete!");
        Selection.activeGameObject = managers;
    }
    
    private static void AddComponentIfMissing<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() == null)
            go.AddComponent<T>();
    }
}
