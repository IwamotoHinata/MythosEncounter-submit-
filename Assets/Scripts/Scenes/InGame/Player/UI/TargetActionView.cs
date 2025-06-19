using Cysharp.Threading.Tasks;
using Scenes.Ingame.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class TargetActionView : MonoBehaviour
{
    [SerializeField] private Image[] _checkBox;
    [SerializeField] private TextMeshProUGUI[] _checkActionText;
    
    public void Init()
    {
        if (IngameManager.Instance == null) {
            Debug.LogWarning("IngameManager.Instance‚ªNull‚Å‚·");
        }
        else {
            IngameManager.Instance.OnGetJournal.Subscribe(_ => _checkBox[0].gameObject.SetActive(true)).AddTo(this);
            IngameManager.Instance.OnEscapeCount.Subscribe(count => _checkActionText[1].text = $"ƒyƒ“ƒLŠÊ‚ðE‚¤ ({count}/‚S)").AddTo(this);
            IngameManager.Instance.OnOpenEscapePointEvent.Subscribe(_ => _checkBox[1].gameObject.SetActive(true)).AddTo(this);
        }
    }
}
