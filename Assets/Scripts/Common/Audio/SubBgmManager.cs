using Cysharp.Threading.Tasks;
using Scenes.Ingame.Player;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class SubBgmManager : MonoBehaviour
{
    [SerializeField] private AudioSource _bgmSource1; // 横軸のオーディオソース
    [SerializeField] private AudioSource _bgmSource2; // 縦軸のオーディオソース
    [SerializeField] private List<AudioClip> _bgmAudioClipList = new List<AudioClip>();
    private const float MAXAXISX = 73f; // Maximum X coordinate of the stage
    private const float MAXAXISZ = 73f; // Maximum Z coordinate of the stage
    private const float MINAXISX = -4f; // Minimum X coordinate of the stage
    private const float MINAXISZ = -4f; // Minimum Z coordinate of the stage
    public static SubBgmManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init()
    {
        var bgmName = _bgmAudioClipList[Random.Range(0, _bgmAudioClipList.Count)].name;
        PlayBgm(bgmName);

        var playerTransform = FindFirstObjectByType<PlayerStatus>().transform;
        UpdatePosition(playerTransform);
    }

    private void UpdatePosition(Transform playerTransform)
    {
        this.UpdateAsObservable().Subscribe(_ => AudioSourcePositionUpdate(playerTransform.position)).AddTo(playerTransform);
    }

    private void PlayBgm(string bgmName)
    {
        var audio = _bgmAudioClipList.FirstOrDefault(a => a.name == bgmName);
        if (audio == default || audio == null)
        {
            Debug.LogError($"{bgmName} という音源は設定されていません");
        }
        _bgmSource1.clip = audio;
        _bgmSource2.clip = audio;
        _bgmSource1.Play();
        _bgmSource2.Play();
    }

    private void AudioSourcePositionUpdate(Vector3 playerPosition)
    {
        _bgmSource1.transform.position = new Vector3(playerPosition.x >= (MAXAXISX + MINAXISX) / 2 ? MAXAXISX : MINAXISX,playerPosition.y, playerPosition.z);
        _bgmSource2.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z >= (MAXAXISZ + MINAXISZ) / 2 ? MAXAXISZ : MINAXISZ);
    }
}
