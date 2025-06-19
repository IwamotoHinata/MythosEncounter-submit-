using TMPro;
using UnityEngine;

public class MethodView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _methodText;
    public void Init()
    {
        gameObject.SetActive(false);
    }

    public void ShowPanel(ItemData item)
    {
        gameObject.SetActive(true);
        _methodText.text = item.useMethod;
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        _methodText.text = "";
    }
}
