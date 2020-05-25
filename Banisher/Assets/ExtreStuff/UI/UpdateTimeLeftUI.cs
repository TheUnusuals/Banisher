using TMPro;
using UnityEngine;

namespace TheUnusuals.Banisher.ExtreStuff.UI
{
    // ReSharper disable once InconsistentNaming
    public class UpdateTimeLeftUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text tmpText;

        [SerializeField] private Menu menu;

        private void Awake()
        {
            if (!tmpText) tmpText = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            tmpText.text = $"Time left: {menu.TimeLeft:F1}s";
        }
    }
}