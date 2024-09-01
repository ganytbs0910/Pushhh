using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using TMPro;
using System.Text;

public class Settings : MonoBehaviour
{
    public static Settings instance;
    private const string MusicKey = "IsMusicOn";
    private const string SoundKey = "IsSoundOn";
    private const string VibrationKey = "IsVibrationOn";
    private const string NotificationKey = "IsNotificationOn";
    private const string AutoSpinKey = "IsAutoSpinOn";

    public bool IsMusicOn
    {
        get => PlayerPrefs.GetInt(MusicKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(MusicKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool IsSoundOn
    {
        get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(SoundKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool IsVibrationOn
    {
        get => PlayerPrefs.GetInt(VibrationKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(VibrationKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool IsNotificationOn
    {
        get => PlayerPrefs.GetInt(NotificationKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(NotificationKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool IsAutoSpinOn
    {
        get => PlayerPrefs.GetInt(AutoSpinKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(AutoSpinKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetToDefaults()
    {
        IsMusicOn = true;
        IsSoundOn = true;
        IsVibrationOn = true;
        IsNotificationOn = true;
        IsAutoSpinOn = false;
    }
}