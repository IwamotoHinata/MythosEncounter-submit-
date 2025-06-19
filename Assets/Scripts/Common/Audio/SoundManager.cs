using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _seSource;
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private List<AudioClip> _seAudioClipList = new List<AudioClip>();
    [SerializeField] private List<AudioClip> _bgmAudioClipList = new List<AudioClip>();
    public static SoundManager Instance { get; private set; }
    private string[] damageVoiceNames = { "se_damagevoice00", "se_damagevoice01", "se_damagevoice02" };//damageÇéÛÇØÇΩç€ÇÃSE

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

    public void PlaySe(string seName, Vector3 soundPosition)
    {

        var audio = _seAudioClipList.FirstOrDefault(a => a.name == seName);
        if (audio == default || audio == null)
        {
            Debug.LogError($"{seName} Ç∆Ç¢Ç§âπåπÇÕê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒ");
        }
        if (soundPosition != null)
        {
            transform.position = soundPosition;
        }
        _seSource.PlayOneShot(audio);
    }

    public void PlayDamageSe(Vector3 soundPosition)
    {
        int randomIndex = Random.Range(0, damageVoiceNames.Length);
        PlaySe(damageVoiceNames[randomIndex], soundPosition);
    }

    public void PlayBgm(string bgmName)
    {
        var audio = _bgmAudioClipList.FirstOrDefault(a => a.name == bgmName);
        if (audio == default || audio == null)
        {
            Debug.LogError($"{bgmName} Ç∆Ç¢Ç§âπåπÇÕê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒ");
        }
        _bgmSource.clip = audio;
        _bgmSource.Play();
    }
}
