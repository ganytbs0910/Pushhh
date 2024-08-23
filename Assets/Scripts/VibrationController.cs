using UnityEngine;
using System.Runtime.InteropServices;

public class VibrationController : MonoBehaviour
{
    public static void VibrateLight()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS(ImpactFeedbackStyle.Light);
#endif
    }

    public static void VibrateMedium()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS(ImpactFeedbackStyle.Medium);
#endif
    }

    public static void VibrateHeavy()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS(ImpactFeedbackStyle.Heavy);
#endif
    }

    public static void VibrateSoft()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS(ImpactFeedbackStyle.Soft);
#endif
    }

    public static void VibrateRigid()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS(ImpactFeedbackStyle.Rigid);
#endif
    }

    public static void VibrateSelectionChanged()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS_SelectionChanged();
#endif
    }

    public static void VibrateNotificationError()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS(NotificationFeedbackStyle.Error);
#endif
    }

    public static void VibrateNotificationSuccess()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS(NotificationFeedbackStyle.Success);
#endif
    }

    public static void VibrateNotificationWarning()
    {
#if UNITY_IOS && !UNITY_EDITOR
Vibration.VibrateIOS(NotificationFeedbackStyle.Warning);
#endif
    }

    public static void Vibrate()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
Vibration.Init();
            Vibration.Vibrate();
#endif
    }

    public static void VibratePop()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
Vibration.Init();
            Vibration.VibratePop();
#endif
    }

    public static void VibratePeek()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
Vibration.Init();
            Vibration.VibratePeek();
#endif
    }

    public static void VibrateNope()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
Vibration.Init();
            Vibration.VibrateNope();
#endif
    }
}