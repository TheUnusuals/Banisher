using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy {
    public class VisEnergyReducer : MonoBehaviour {
        [SerializeField] private VisEnergy visEnergy;

        [SerializeField] private float reductionSpeed = 1;

        [SerializeField] private BanishManager banishManager;

        private void Awake() {
            if (!visEnergy) visEnergy = GetComponent<VisEnergy>();
            if (!banishManager) banishManager = BanishManager.GetInstance();
        }

        private void Update() {
            visEnergy.Energy -= reductionSpeed * banishManager.AdjustedDeltaTime;
        }
    }
}