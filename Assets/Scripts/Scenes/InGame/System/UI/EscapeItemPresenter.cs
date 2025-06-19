using UnityEngine;
using UniRx;
using Scenes.Ingame.Manager;
using TMPro;

namespace Scenes.Ingame.InGameSystem.UI
{

    public class EscapeItemPresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("脱出アイテム数表示Text")]
        TextMeshProUGUI _socreText;

        void Start()
        {
            IngameManager ingamemanager = IngameManager.Instance;
            ingamemanager.OnEscapeCount.Subscribe(x =>
            {
                _socreText.text = "脱出アイテム(" + x + "/4)";
            }).AddTo(this);
        }
    }
}