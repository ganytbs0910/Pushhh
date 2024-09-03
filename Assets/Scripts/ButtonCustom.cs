using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCustom : MonoBehaviour
{
    public bool isButtonClickSoundOn = false;
    public bool isVibrationWeak = false;
    public bool isVibrationMedium = false;
    public bool isVibrationStrong = false;
    Button button

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (isButtonClickSoundOn)
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.button);
            }
            if (isVibrationWeak)
            {
                VibrationController.VibrateLight();
            }
            if (isVibrationMedium)
            {
                VibrationController.VibrateMedium();
            }
            if (isVibrationStrong)
            {
                VibrationController.VibrateHeavy();
            }
        });
    }
}