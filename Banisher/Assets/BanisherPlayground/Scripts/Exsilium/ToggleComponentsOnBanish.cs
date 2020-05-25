using System.Collections.Generic;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Exsilium {
    public class ToggleComponentsOnBanish : MonoBehaviour {
        [SerializeField] private List<Behaviour> activateInExsilium;
        [SerializeField] private List<Behaviour> activateInRealWorld;

        [SerializeField] private BanishManager banishManager;

        private void Awake() {
            if (!banishManager) banishManager = BanishManager.GetInstance();
        }

        private void Start() {
            banishManager.OnEnterExsilium.AddListener(OnEnterExsilium);
            banishManager.OnExitExsilium.AddListener(OnExitExsilium);
        }

        private void OnEnterExsilium() {
            activateInExsilium.ForEach(behaviour => behaviour.enabled = true);
            activateInRealWorld.ForEach(behaviour => behaviour.enabled = false);
        }

        private void OnExitExsilium() {
            activateInExsilium.ForEach(behaviour => behaviour.enabled = false);
            activateInRealWorld.ForEach(behaviour => behaviour.enabled = true);
        }
    }
}