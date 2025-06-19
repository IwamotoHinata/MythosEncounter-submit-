using UnityEngine;
using Scenes.Ingame.Manager;
using Scenes.Ingame.Player;
namespace Scenes.Ingame.InGameSystem
{
    public class EscapeItem : MonoBehaviour, IInteractable
    {
        IngameManager manager;
        private bool _get = false;
        void Start()
        {
            manager = IngameManager.Instance;
        }

        public void Intract(PlayerStatus status, bool processWithConditionalBypass)
        {
            if (Input.GetMouseButtonDown(1) && !_get)
            {
                SoundManager.Instance.PlaySe("se_getescapeitem00", transform.position);
                _get = true;
                manager.GetEscapeItem();
                Destroy(gameObject, 0.5f);
            }
        }

        public string ReturnPopString()
        {
            return null;
        }
    }
}