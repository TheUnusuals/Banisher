using TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy;
using UnityEngine;
using Valve.VR;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Exsilium {
    public class PlayerBanish : MonoBehaviour {
        [SerializeField] private SteamVR_Action_Boolean banishButton;
        [SerializeField] private KeyCode banishKey = KeyCode.Space;

        [SerializeField] private BanishManager banishManager;

        private void Awake() {
            if (!banishManager) banishManager = BanishManager.GetInstance();
        }

        private void Update() {
            if (Input.GetKeyDown(banishKey) || banishButton.stateDown) {
                banishManager.ToggleExsilium();
            }
        }
    }
}