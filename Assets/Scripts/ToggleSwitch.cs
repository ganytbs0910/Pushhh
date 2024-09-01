using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSwitch : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform handle;
    [SerializeField] private SettingType settingType;
    [NonSerialized] public bool Value;
    private float handlePosX;
    private Sequence sequence;
    private static readonly Color OFF_BG_COLOR = new Color(0.92f, 0.92f, 0.92f);
    private static readonly Color ON_BG_COLOR = new Color(0.2f, 0.84f, 0.3f);
    private const float SWITCH_DURATION = 0.36f;

    public enum SettingType
    {
        Music,
        Sound,
        Vibration,
        Notification,
        AutoSpin
    }

    private void Start()
    {
        handlePosX = Mathf.Abs(handle.anchoredPosition.x);
        Value = GetSettingValue();
        UpdateToggle(0);
    }

    public void SwitchToggle()
    {
        Value = !Value;
        UpdateToggle(SWITCH_DURATION);
        SetSettingValue(Value);
    }

    private void UpdateToggle(float duration)
    {
        var bgColor = Value ? ON_BG_COLOR : OFF_BG_COLOR;
        var handleDestX = Value ? handlePosX : -handlePosX;

        sequence?.Complete();
        sequence = DOTween.Sequence();
        sequence.Append(backgroundImage.DOColor(bgColor, duration))
            .Join(handle.DOAnchorPosX(handleDestX, duration / 2));
    }

    private bool GetSettingValue()
    {
        return settingType switch
        {
            SettingType.Music => Settings.instance.IsMusicOn,
            SettingType.Sound => Settings.instance.IsSoundOn,
            SettingType.Vibration => Settings.instance.IsVibrationOn,
            SettingType.Notification => Settings.instance.IsNotificationOn,
            SettingType.AutoSpin => Settings.instance.IsAutoSpinOn,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void SetSettingValue(bool value)
    {
        switch (settingType)
        {
            case SettingType.Music:
                Settings.instance.IsMusicOn = value;
                break;
            case SettingType.Sound:
                Settings.instance.IsSoundOn = value;
                break;
            case SettingType.Vibration:
                Settings.instance.IsVibrationOn = value;
                break;
            case SettingType.Notification:
                Settings.instance.IsNotificationOn = value;
                break;
            case SettingType.AutoSpin:
                Settings.instance.IsAutoSpinOn = value;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}