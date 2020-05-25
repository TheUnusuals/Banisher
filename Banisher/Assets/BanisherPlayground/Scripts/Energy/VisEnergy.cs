using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy {
    public class VisEnergy : MonoBehaviour {
        [Serializable]
        public class VisEnergyChangeEvent : UnityEvent<VisEnergy, float> { }

        [SerializeField] private float energy = 100;
        [SerializeField] private float minEnergy = 0;
        [SerializeField] private float maxEnergy = 100;

        [SerializeField] private VisEnergyChangeEvent onChange;

        public float Energy {
            get => energy;
            set {
                value = Mathf.Clamp(value, minEnergy, maxEnergy);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (energy != value) {
                    float previousEnergy = energy;

                    energy = value;

                    if (enabled) {
                        onChange.Invoke(this, previousEnergy);
                    }
                }
            }
        }

        public float MinEnergy {
            get => minEnergy;
            set => minEnergy = value;
        }

        public float MaxEnergy {
            get => maxEnergy;
            set => maxEnergy = value;
        }

        public VisEnergyChangeEvent OnChange => onChange;
    }
}