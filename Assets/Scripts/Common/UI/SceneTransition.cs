using UnityEngine;
using UnityEngine.SceneManagement;
namespace Common.UI
{
    public class SceneTransition : MonoBehaviour, IActionUi
    {
        [SerializeField, Tooltip("�J�ڐ�̃V�[��")]
        private SceneObject transitionScene;
        public void Action()
        {
            try
            {
                SceneManager.LoadScene(transitionScene);
            }
            catch (System.Exception)
            {
                Debug.LogError("�V�[���J�ڂŖ�肪�������܂����B");
                throw;
            }
        }
    }
}