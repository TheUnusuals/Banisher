using System;
using System.Collections.Generic;
using System.Linq;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther;
using UnityEngine;
using UnityEngine.Events;

namespace TheUnusuals.Banisher.ExtreStuff
{
    public class NinjaManager : MonoBehaviour
    {
        public const string NinjaManagerTag = "NinjaManager";

        [SerializeField] private List<ActiveRagdoll> ninjas = new List<ActiveRagdoll>();

        [SerializeField] private UnityEvent onNinjasChange;

        public int CurrentNinjasAlive => ninjas.Count(ninja => ninja.IsAlive);

        public UnityEvent OnNinjasChange => onNinjasChange;

        private void Start()
        {
            foreach (ActiveRagdoll ninja in ninjas)
            {
                ninja.VisEnergy.OnChange.AddListener(OnNinjaVisEnergyChange);
            }
        }

        private void OnNinjaVisEnergyChange(VisEnergy visEnergy, float previousEnergy)
        {
            onNinjasChange.Invoke();
        }

        public static NinjaManager GetInstance()
        {
            return GameObject
                .FindWithTag(NinjaManagerTag)
                .GetComponent<NinjaManager>();
        }
    }
}