using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class ClickToBanish : MonoBehaviour {
        public new Camera camera;

        private void Awake() {
            if (!camera) {
                camera = GetComponent<Camera>();
            }
        }

        private void Update() {
            if (Input.GetMouseButtonDown(2) && Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit)) {
                raycastHit.transform.GetComponent<Banishable>()?.ToggleBanishment();
            }
        }
    }
}