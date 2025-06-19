using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Common.UI
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text _message;

        public void Init(string message)
        {
            _message.text = message;
        }

        public void OnClose()
        {
            Destroy(this.gameObject);
        }
    }
}
