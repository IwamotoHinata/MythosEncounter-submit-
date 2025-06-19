using UnityEngine;

public class MasterData : MonoBehaviour
{
    public static MasterData instance;
    [SerializeField, TextArea] private string _masterText;
    public Master master { get; private set; }
    void Awake()
    {
        instance = this;
        master = JsonUtility.FromJson<Master>(_masterText);
    }
}
