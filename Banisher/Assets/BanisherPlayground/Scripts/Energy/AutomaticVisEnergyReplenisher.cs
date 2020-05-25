using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy {
    public class AutomaticVisEnergyReplenisher : MonoBehaviour {
        [SerializeField] private VisEnergy visEnergy;

        [SerializeField] private float replenishWaitTime = 10f;
        [SerializeField] private float replenishSpeed = 10f;

        private float timeUntilReplenish;

        private void Awake() {
            if (!visEnergy) visEnergy = GetComponent<VisEnergy>();
        }

        private void Start() {
            if (visEnergy) {
                visEnergy.OnChange.AddListener(OnVisEnergyChange);
            }
        }

        private void Update() {
            if (timeUntilReplenish > 0) {
                timeUntilReplenish -= Time.deltaTime;
            } else if (visEnergy.Energy < visEnergy.MaxEnergy) {
                visEnergy.Energy += replenishSpeed * Time.deltaTime;
            }
        }

        private void OnVisEnergyChange(VisEnergy changedVisEnergy, float previousEnergy) {
            if (changedVisEnergy != visEnergy) {
                return;
            }

            bool energyDecreased = changedVisEnergy.Energy < previousEnergy;

            if (energyDecreased) {
                timeUntilReplenish = replenishWaitTime;
            }
        }
    }
}