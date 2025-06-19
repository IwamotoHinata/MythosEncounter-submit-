using System.Collections.Generic;
using System.IO.Pipes;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class OnClickObserver : MonoBehaviour
    {
        private Button _button;
        private IActionUi[] _actionUisl = new IActionUi[0];

        private void Start()
        {
            _button = gameObject.GetComponent<Button>();
            _actionUisl = GetComponentsInChildren<IActionUi>();
            _button.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    foreach (var ui in _actionUisl)
                    {
                        ui.Action();
                    }
                })
                .AddTo(this);
        }
    }
}