using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy {
    public class VisEnergyCollisionModifier : MonoBehaviour {
        [SerializeField] private float reducedVisEnergyModifier = 1;

        public float ReducedVisEnergyModifier {
            get => reducedVisEnergyModifier;
            set => reducedVisEnergyModifier = value;
        }
    }
}