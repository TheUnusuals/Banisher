using System.Collections;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.Exsilium;
using UnityEngine;
using UnityEngine.Events;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy {
    public class BanishManager : MonoBehaviour {
        private const string BanisherManagerTag = "BanishManager";

        [SerializeField] private bool banished = false;

        [SerializeField] private UnityEvent onEnterExsilium;
        [SerializeField] private UnityEvent onExitExsilium;

        [SerializeField] private SlowMotionOnBanish slowMotionOnBanish;

        public float AdjustedDeltaTime => slowMotionOnBanish ? slowMotionOnBanish.AdjustedDeltaTime : Time.deltaTime;

        public float AdjustedFixedDeltaTime => slowMotionOnBanish ? slowMotionOnBanish.AdjustedFixedDeltaTime : Time.fixedDeltaTime;

        public bool Banished {
            get => banished;
            set => banished = value;
        }

        public UnityEvent OnEnterExsilium {
            get => onEnterExsilium;
            set => onEnterExsilium = value;
        }

        public UnityEvent OnExitExsilium {
            get => onExitExsilium;
            set => onExitExsilium = value;
        }

        private void Awake() {
            if (!slowMotionOnBanish) slowMotionOnBanish = GetComponent<SlowMotionOnBanish>();
        }

        private IEnumerator Start() {
            yield return null; // wait for next frame
            if (banished) {
                EnterExsilium();
            } else {
                ExitExsilium();
            }
        }

        public void EnterExsilium() {
            banished = true;
            onEnterExsilium.Invoke();
        }

        public void ExitExsilium() {
            banished = false;
            onExitExsilium.Invoke();
        }

        public void ToggleExsilium() {
            if (banished) {
                ExitExsilium();
            } else {
                EnterExsilium();
            }
        }

        public static BanishManager GetInstance() {
            return GameObject
                .FindGameObjectWithTag(BanisherManagerTag)
                .GetComponent<BanishManager>();
        }
    }
}