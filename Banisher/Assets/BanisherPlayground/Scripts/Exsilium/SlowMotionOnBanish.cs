using TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Exsilium {
    public class SlowMotionOnBanish : MonoBehaviour {
        [SerializeField] private float exsiliumTimeScale = 0.1f;
        [SerializeField] private float exsiliumFixedDeltaTimeScale = 0.5f;

        [SerializeField] private float realTimeScale = -1;
        [SerializeField] private float realFixedDeltaTime = -1;

        [SerializeField] private BanishManager banishManager;

        public float AdjustedDeltaTime => banishManager.Banished ? Time.deltaTime / exsiliumTimeScale : Time.deltaTime;
        public float AdjustedFixedDeltaTime => banishManager.Banished ? Time.fixedDeltaTime / exsiliumTimeScale / exsiliumFixedDeltaTimeScale : Time.fixedDeltaTime;

        private void Awake() {
            if (!banishManager) banishManager = BanishManager.GetInstance();
        }

        private void Start() {
            banishManager.OnEnterExsilium.AddListener(OnEnterExsilium);
            banishManager.OnExitExsilium.AddListener(OnExitExsilium);

            if (realTimeScale < 0) realTimeScale = Time.timeScale;
            if (realFixedDeltaTime < 0) realFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void OnEnterExsilium() {
            Time.timeScale = exsiliumTimeScale;
            Time.fixedDeltaTime = realFixedDeltaTime * exsiliumFixedDeltaTimeScale;
        }

        private void OnExitExsilium() {
            Time.timeScale = realTimeScale;
            Time.fixedDeltaTime = realFixedDeltaTime;
        }
    }
}