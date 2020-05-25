using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy {
    public class UnbanishOnLowVisEnergy : MonoBehaviour {
        [SerializeField] private VisEnergy visEnergy;
        [SerializeField] private BanishManager banishManager;

        [SerializeField] private float minEnergyRequired = 20;

        private void Awake() {
            if (!visEnergy) visEnergy = GetComponent<VisEnergy>();
            if (!banishManager) banishManager = BanishManager.GetInstance();
        }

        private void Update() {
            if (banishManager.Banished && visEnergy.Energy < minEnergyRequired) {
                banishManager.ExitExsilium();
            }
        }
    }
}