using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    // UI references
    private Text  _titleText;    // dùng Text thường
    private Button _btnClose;
    private Toggle _sfxToggle;
    private Toggle _musicToggle;
    private Button _btnBgClose;

    /// <summary>
    /// Gọi một lần sau khi Instantiate hoặc Scene load.
    /// </summary>
    public void Init()
    {
        // Lấy tham chiếu tới Title (Text thường)
        _titleText = transform
            .Find("Content/Tittle")
            .GetComponent<Text>();
        _titleText.text = "Cài đặt";

        // Lấy và gán nút Close (icon X)
        _btnBgClose = transform.GetComponent<Button>();
        _btnClose = transform
            .Find("Content/Image/close")
            .GetComponent<Button>();
        _btnClose.onClick.RemoveAllListeners();
        _btnClose.onClick.AddListener(Hide);
        _btnBgClose.onClick.RemoveAllListeners();
        _btnBgClose.onClick.AddListener(Hide);

        // Lấy Toggle SFX
        var sfxT = transform.Find("Content/SFX/Toggle");
        _sfxToggle = sfxT.GetComponent<Toggle>();
        _sfxToggle.isOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
        _sfxToggle.onValueChanged.RemoveAllListeners();
        _sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);

        // Lấy Toggle Music
        var musicT = transform.Find("Content/Music/Toggle");
        _musicToggle = musicT.GetComponent<Toggle>();
        _musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        _musicToggle.onValueChanged.RemoveAllListeners();
        _musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
    }

    /// <summary>
    /// Hiển thị panel Settings
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Ẩn panel Settings
    /// </summary>
    public void Hide() => gameObject.SetActive(false);

    /// <summary>
    /// Xử lý khi bật/tắt SFX
    /// </summary>
    private void OnSfxToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("SFXEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        // Ví dụ: AudioManager.Instance.EnableSFX(isOn);
        Debug.Log($"SFX Enabled: {isOn}");
        SoundManager.Instance.EnableSFX(isOn);
    }

    /// <summary>
    /// Xử lý khi bật/tắt Music
    /// </summary>
    private void OnMusicToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        // Ví dụ: AudioManager.Instance.EnableMusic(isOn);
        Debug.Log($"Music Enabled: {isOn}");
        SoundManager.Instance.EnableMusic(isOn);
    }
}
