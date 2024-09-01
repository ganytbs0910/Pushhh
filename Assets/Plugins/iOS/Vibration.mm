#import <AudioToolbox/AudioToolbox.h>

extern "C" {
    void _VibrateLight()
    {
        AudioServicesPlaySystemSound(1519);
    }

    void _VibrateMedium()
    {
        AudioServicesPlaySystemSound(1520);
    }

    void _VibrateHeavy()
    {
        AudioServicesPlaySystemSound(1521);
    }

    void _VibrateSelectionChanged()
    {
        AudioServicesPlaySystemSound(1519);
    }

    void _VibrateNotificationError()
    {
        AudioServicesPlaySystemSound(1521);
    }

    void _VibrateNotificationSuccess()
    {
        AudioServicesPlaySystemSound(1519);
    }

    void _VibrateNotificationWarning()
    {
        AudioServicesPlaySystemSound(1521);
    }
    void _VibrateSoft()
    {
        if (@available(iOS 10.0, *))
        {
            UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
    [generator impactOccurred] ;
}
    }

    void _VibrateRigid()
{
    if (@available(iOS 10.0, *))
    {
        UIImpactFeedbackGenerator* generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
[generator impactOccurred] ;
        }
    }

    void _TapVibrate()
{
    AudioServicesPlaySystemSoundWithCompletion(1352, NULL);
}
void vibratePeek()
{
    AudioServicesPlaySystemSoundWithCompletion(1519, NULL);
}
void vibratePop()
{
    AudioServicesPlaySystemSoundWithCompletion(1520, NULL);
}
void vibrateNope()
{
    AudioServicesPlaySystemSoundWithCompletion(1521, NULL);
}
}
