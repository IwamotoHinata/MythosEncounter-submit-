using UnityEngine;

public class ViewBase : MonoBehaviour
{
    public virtual void Init()
    {

    }
    public virtual void Open()
    {
        gameObject.SetActive(true);
    }
    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}
