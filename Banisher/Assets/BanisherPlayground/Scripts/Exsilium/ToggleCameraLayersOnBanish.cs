using TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Exsilium {
    [RequireComponent(typeof(Camera))]
    public class ToggleCameraLayersOnBanish : MonoBehaviour {
        [SerializeField] private string banishedLayer = "banished";

        [SerializeField] private BanishManager banishManager;
        [SerializeField] private new Camera camera;

        private void Awake() {
            if (!banishManager) banishManager = BanishManager.GetInstance();
            if (!camera) camera = GetComponent<Camera>();
        }

        private void Start() {
            banishManager.OnEnterExsilium.AddListener(OnEnterExsilium);
            banishManager.OnExitExsilium.AddListener(OnExitExsilium);
        }

        private void OnEnterExsilium() {
            camera.cullingMask |= 1 << LayerMask.NameToLayer(banishedLayer);
        }

        private void OnExitExsilium() {
            camera.cullingMask &= ~(1 << LayerMask.NameToLayer(banishedLayer));
        }
    }
}