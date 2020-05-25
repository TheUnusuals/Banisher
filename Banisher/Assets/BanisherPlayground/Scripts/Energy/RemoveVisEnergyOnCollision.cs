using System.Collections.Generic;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy {
    [RequireComponent(typeof(Collider))]
    public class RemoveVisEnergyOnCollision : MonoBehaviour {
        [SerializeField] private VisEnergy visEnergy;

        [SerializeField] private AnimationCurve energyDamageCurve = AnimationCurve.Linear(1, 0, 100, 100);

        [SerializeField] private bool useColliderWhitelist = false;

        [SerializeField] private List<Collider> colliderWhitelist = new List<Collider>();
        [SerializeField] private List<Collider> colliderBlacklist = new List<Collider>();

        private void Awake() {
            if (!visEnergy) visEnergy = GetComponentInParent<VisEnergy>();
        }

        private void OnCollisionEnter(Collision other) {
            if (useColliderWhitelist && !colliderWhitelist.Contains(other.collider)) {
                return;
            }

            if (!useColliderWhitelist && colliderBlacklist.Contains(other.collider)) {
                return;
            }

            float modifier = 1;
            VisEnergyCollisionModifier collisionModifier = other.collider.GetComponent<VisEnergyCollisionModifier>();

            if (collisionModifier) {
                modifier = collisionModifier.ReducedVisEnergyModifier;
            }

            float impulse = other.impulse.magnitude * modifier;
            visEnergy.Energy -= energyDamageCurve.Evaluate(impulse);
        }
    }
}