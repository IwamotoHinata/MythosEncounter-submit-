using UnityEngine;
using UnityEngine.SceneManagement;
namespace Common.UI
{
    public class SceneTransition : MonoBehaviour, IActionUi
    {
        [SerializeField, Tooltip("遷移先のシーン")]
        private SceneObject transitionScene;
        public void Action()
        {
            try
            {
                SceneManager.LoadScene(transitionScene);
            }
            catch (System.Exception)
            {
                Debug.LogError("シーン遷移で問題が発生しました。");
                throw;
            }
        }
    }
}