using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;
using TMPro;

namespace Scenes.Ingame.InGameSystem.UI
{
    public class TitleForthLayerView : MonoBehaviour
    {
        [SerializeField] private Button _nameApplyButton;
        [SerializeField] private GameObject _nameWindow;
        [SerializeField] private SceneObject _lobbyScene;
        [SerializeField] private TMP_InputField _name;

        public IObservable<Unit> NameApplyButtonClick => _nameApplyButton.OnClickAsObservable();

        public void ForthLayerChange(bool value)
        {
            _nameWindow.SetActive(value);
        }

        public void LobbyChange()
        {
            SceneManager.LoadScene(_lobbyScene);
        }
    }
}