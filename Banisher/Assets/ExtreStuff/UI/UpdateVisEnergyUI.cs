using TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy;
using TMPro;
using UnityEngine;

namespace TheUnusuals.Banisher.ExtreStuff.UI
{
    // ReSharper disable once InconsistentNaming
    public class UpdateVisEnergyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text tmpText;

        [SerializeField] private VisEnergy visEnergy;

        private void Awake()
        {
            if (!tmpText) tmpText = GetComponent<TMP_Text>();
            if (!visEnergy) visEnergy = GetComponentInParent<VisEnergy>();
        }

        private void Start()
        {
            visEnergy.OnChange.AddListener(OnChangeVisEnergy);
            UpdateUI((int) visEnergy.Energy);
        }

        private void OnChangeVisEnergy(VisEnergy visEnergy, float previousEnergy)
        {
            UpdateUI((int) visEnergy.Energy);
        }

        // ReSharper disable once InconsistentNaming
        private void UpdateUI(int energy)
        {
            tmpText.text = $"Vis: {energy}";
        }
    }
}