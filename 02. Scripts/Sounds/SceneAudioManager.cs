using UnityEngine;

public class SceneAudioManager : Singleton<SceneAudioManager>
{
    private const string BgmVolumeKey = "Audio_BGM_Volume";
    private const string SfxVolumeKey = "Audio_SFX_Volume";
    
    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Cilps")]
    [SerializeField] private AudioClip bgmClip;

    protected override void Awake()
    {
        base.Awake();
        
        LoadVolumeSettings();

        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }
    
    /// <summary>
    /// 특정 효과음 재생
    /// </summary>
    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null) //재생할 효과음이 할당되지 않을경우
        {
            sfxSource.PlayOneShot(clip);
            //Debug.Log("효과음 출력 중임");
        }
    }

    public void SetBgmVolume(float volume)
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(BgmVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }

    public void SetSfxVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SfxVolumeKey, volume);
            PlayerPrefs.Save();
        }
    }
    
    public float GetBgmVolume() => bgmSource != null ? bgmSource.volume : 0f;
    public float GetSfxVolume() => sfxSource != null ? sfxSource.volume : 0f;
    
    private void LoadVolumeSettings()
    {
        if (bgmSource != null)
            bgmSource.volume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);

        if (sfxSource != null)
            sfxSource.volume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
    }
}