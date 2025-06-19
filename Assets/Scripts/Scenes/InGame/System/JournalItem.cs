using Scenes.Ingame.Manager;
using Scenes.Ingame.Player;
using UnityEngine;

public class JournalItem : MonoBehaviour, IInteractable
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
            _get = true;
            manager.GetJournalItem();
            Destroy(gameObject, 0.5f);
        }
    }

    public string ReturnPopString()
    {
        return null;
    }
}