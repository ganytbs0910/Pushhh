using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] AudioSource seAudioSource;

    [SerializeField] Slider bgmSilder;
    [SerializeField] Slider seSilder;

    [SerializeField] List<BGMSoundData> bgmSoundDatas;
    [SerializeField] List<SESoundData> seSoundDatas;

    private float masterVolume = 1;
    public float bgmMasterVolume = 1;
    public float seMasterVolume = 1;

    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        bgmMasterVolume = PlayerPrefs.GetFloat("BGMVolume");
        seMasterVolume = PlayerPrefs.GetFloat("SEVolume");

        // スライダーの値を設定する
        bgmSilder.value = bgmMasterVolume;
        seSilder.value = seMasterVolume;

        PlayBGM(BGMSoundData.BGM.normal);
    }

    public void PlayBGM(BGMSoundData.BGM bgm)
    {
        BGMSoundData data = bgmSoundDatas.Find(data => data.bgm == bgm);
        bgmAudioSource.clip = data.audioClip;
        bgmAudioSource.volume = data.volume * bgmMasterVolume * masterVolume;
        bgmAudioSource.Play();
    }

    public void PlaySE(SESoundData.SE se)
    {
        SESoundData data = seSoundDatas.Find(data => data.se == se);
        seAudioSource.volume = data.volume * seMasterVolume * masterVolume;
        seAudioSource.PlayOneShot(data.audioClip);
    }

    //スライダーの値が変更された時に呼ばれる
    public void OnSliderValueChange()
    {
        //スライダーの値をbgmMasterVolumeに代入する
        bgmMasterVolume = bgmSilder.value;
        seMasterVolume = seSilder.value;

        //それを保存する
        PlayerPrefs.SetFloat("BGMVolume", bgmMasterVolume);
        PlayerPrefs.SetFloat("SEVolume", seMasterVolume);
    }
}

[System.Serializable]
public class BGMSoundData
{
    public enum BGM
    {
        normal,
        smallBoss,
        MediumBoss,
        LastBoss,
    }

    public BGM bgm;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}


[System.Serializable]
public class SESoundData
{
    public enum SE
    {
        Attack,
        Damage,
        Hoge,
    }

    public SE se;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}