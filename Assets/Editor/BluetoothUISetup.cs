using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class BluetoothUISetup : EditorWindow
{
    [MenuItem("Game/Setup Bluetooth UI")]
    static void Setup()
    {
        var loading = GameObject.Find("Loading");
        if (loading == null)
        {
            Debug.LogError("Loading not found!");
            return;
        }
        
        // Remove old BluetoothUI if exists
        var oldBtUI = loading.transform.Find("BluetoothUI");
        if (oldBtUI != null) DestroyImmediate(oldBtUI.gameObject);
        
        // Create BluetoothUI
        var btUI = new GameObject("BluetoothUI");
        btUI.transform.SetParent(loading.transform, false);
        var script = btUI.AddComponent<BluetoothUI>();
        
        // === POPUP PANEL (full screen dark overlay) ===
        var popup = CreatePanel(btUI.transform, "PopupPanel", new Color(0, 0, 0, 0.9f));
        script.popupPanel = popup;
        var popupRect = popup.GetComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.sizeDelta = Vector2.zero;
        
        // === MENU CONTENT ===
        var menu = CreateContainer(popup.transform, "MenuContent");
        script.menuContent = menu;
        
        // Title
        CreateText(menu.transform, "Title", "CHƠI QUA BLUETOOTH", 40, new Vector2(0, 180), Color.white);
        
        // Create Room button
        var btnCreate = CreateButton(menu.transform, "BtnCreateRoom", "🏠  TẠO PHÒNG", new Vector2(0, 60), new Color(0.1f, 0.6f, 0.1f), new Vector2(400, 80));
        script.btnCreateRoom = btnCreate.GetComponent<Button>();
        
        // Join Room button
        var btnJoin = CreateButton(menu.transform, "BtnJoinRoom", "🔍  TÌM PHÒNG", new Vector2(0, -40), new Color(0.1f, 0.4f, 0.7f), new Vector2(400, 80));
        script.btnJoinRoom = btnJoin.GetComponent<Button>();
        
        // Close button
        var btnClose = CreateButton(menu.transform, "BtnClose", "✕  ĐÓNG", new Vector2(0, -150), new Color(0.5f, 0.3f, 0.3f), new Vector2(400, 70));
        script.btnClose = btnClose.GetComponent<Button>();
        
        // === WAITING CONTENT ===
        var waiting = CreateContainer(popup.transform, "WaitingContent");
        script.waitingContent = waiting;
        waiting.SetActive(false);
        
        // Status text
        var txtStatus = CreateText(waiting.transform, "TxtStatus", "Đang chờ người chơi...", 32, new Vector2(0, 80), Color.white);
        script.txtStatus = txtStatus.GetComponent<Text>();
        
        // Room name
        var txtRoom = CreateText(waiting.transform, "TxtRoomName", "Phòng: OQuanGame", 28, new Vector2(0, 20), Color.yellow);
        script.txtRoomName = txtRoom.GetComponent<Text>();
        
        // Info text
        CreateText(waiting.transform, "TxtInfo", "Bật Bluetooth trên máy khác\nvà chọn 'Tìm phòng'", 22, new Vector2(0, -50), new Color(0.7f, 0.7f, 0.7f));
        
        // Cancel button
        var btnCancel = CreateButton(waiting.transform, "BtnCancelWait", "HỦY", new Vector2(0, -150), new Color(0.6f, 0.2f, 0.2f), new Vector2(300, 70));
        script.btnCancelWait = btnCancel.GetComponent<Button>();
        
        // === DEVICE LIST CONTENT ===
        var deviceList = CreateContainer(popup.transform, "DeviceListContent");
        script.deviceListContent = deviceList;
        deviceList.SetActive(false);
        
        // Title
        CreateText(deviceList.transform, "TxtTitle", "TÌM PHÒNG", 36, new Vector2(0, 180), Color.white);
        
        // Scanning text
        var txtScan = CreateText(deviceList.transform, "TxtScanning", "Đang tìm...", 24, new Vector2(0, 130), Color.yellow);
        script.txtScanning = txtScan.GetComponent<Text>();
        
        // Scroll view for devices
        var scroll = CreateScrollView(deviceList.transform, new Vector2(0, 0), new Vector2(450, 250));
        script.deviceContainer = scroll.transform.Find("Viewport/Content");
        
        // Device item prefab
        script.deviceItemPrefab = CreateDeviceItemPrefab();
        
        // Back button
        var btnBack = CreateButton(deviceList.transform, "BtnBackFromList", "← QUAY LẠI", new Vector2(0, -170), new Color(0.5f, 0.3f, 0.3f), new Vector2(300, 70));
        script.btnBackFromList = btnBack.GetComponent<Button>();
        
        // Hide popup by default
        popup.SetActive(false);
        
        Debug.Log("✅ Bluetooth UI created with larger size!");
        EditorUtility.SetDirty(btUI);
    }
    
    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        var img = panel.AddComponent<Image>();
        img.color = color;
        return panel;
    }
    
    static GameObject CreateContainer(Transform parent, string name)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return obj;
    }
    
    static GameObject CreateButton(Transform parent, string name, string text, Vector2 pos, Color color, Vector2 size)
    {
        var btn = new GameObject(name);
        btn.transform.SetParent(parent, false);
        
        var rect = btn.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        
        var img = btn.AddComponent<Image>();
        img.color = color;
        
        btn.AddComponent<Button>();
        
        var txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btn.transform, false);
        var txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;
        
        var txt = txtObj.AddComponent<Text>();
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 28;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        return btn;
    }
    
    static GameObject CreateText(Transform parent, string name, string content, int size, Vector2 pos, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(500, 80);
        
        var txt = obj.AddComponent<Text>();
        txt.text = content;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = size;
        txt.color = color;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        return obj;
    }
    
    static GameObject CreateScrollView(Transform parent, Vector2 pos, Vector2 size)
    {
        var scroll = new GameObject("ScrollView");
        scroll.transform.SetParent(parent, false);
        
        var rect = scroll.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        
        var scrollRect = scroll.AddComponent<ScrollRect>();
        
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scroll.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var cRect = content.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0, 1);
        cRect.anchorMax = new Vector2(1, 1);
        cRect.pivot = new Vector2(0.5f, 1);
        cRect.sizeDelta = new Vector2(0, 0);
        
        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(15, 15, 15, 15);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.viewport = vpRect;
        scrollRect.content = cRect;
        
        return scroll;
    }
    
    static GameObject CreateDeviceItemPrefab()
    {
        var item = new GameObject("DeviceItem");
        
        var rect = item.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 70);
        
        var img = item.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.2f);
        
        item.AddComponent<Button>();
        
        var txt = new GameObject("Text");
        txt.transform.SetParent(item.transform, false);
        var tRect = txt.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = new Vector2(-20, 0);
        
        var text = txt.AddComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 24;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        return item;
    }
}
