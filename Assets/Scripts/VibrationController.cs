using UnityEngine;
using System.Runtime.InteropServices;

public class VibrationController : MonoBehaviour
{
    public static VibrationController instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private static bool CanVibrate()
    {
        return Settings.instance != null && Settings.instance.IsVibrationOn;
    }

    public static void VibrateLight()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS(ImpactFeedbackStyle.Light);
#endif
    }

    public static void VibrateMedium()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS(ImpactFeedbackStyle.Medium);
#endif
    }

    public static void VibrateHeavy()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS(ImpactFeedbackStyle.Heavy);
#endif
    }

    public static void VibrateSoft()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS(ImpactFeedbackStyle.Soft);
#endif
    }

    public static void VibrateRigid()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS(ImpactFeedbackStyle.Rigid);
#endif
    }

    public static void VibrateSelectionChanged()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS_SelectionChanged();
#endif
    }

    public static void VibrateNotificationError()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS(NotificationFeedbackStyle.Error);
#endif
    }

    public static void VibrateNotificationSuccess()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS(NotificationFeedbackStyle.Success);
#endif
    }

    public static void VibrateNotificationWarning()
    {
        if (!CanVibrate()) return;
#if UNITY_IOS && !UNITY_EDITOR
        Vibration.VibrateIOS(NotificationFeedbackStyle.Warning);
#endif
    }

    public static void Vibrate()
    {
        if (!CanVibrate()) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        Vibration.Init();
        Vibration.Vibrate();
#endif
    }

    public static void VibratePop()
    {
        if (!CanVibrate()) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        Vibration.Init();
        Vibration.VibratePop();
#endif
    }

    public static void VibratePeek()
    {
        if (!CanVibrate()) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        Vibration.Init();
        Vibration.VibratePeek();
#endif
    }

    public static void VibrateNope()
    {
        if (!CanVibrate()) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        Vibration.Init();
        Vibration.VibrateNope();
#endif
    }
}