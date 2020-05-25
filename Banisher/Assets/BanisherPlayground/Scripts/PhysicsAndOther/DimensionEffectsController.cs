using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class DimensionEffectsController : MonoBehaviour {
        public new Camera camera;
        public PostProcessVolume mainEffects;
        public PostProcessVolume lensEffects;
        public LensDistortion lensDistortionSettings;

        public KeyCode toggleEffectsKey = KeyCode.E;
        public string banishedLayer = "banished";
        public AnimationCurve effectsChangeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public AnimationCurve lensDistortionCurve = new AnimationCurve(
            new Keyframe(0, 0, 0, 0, 0, .3f),
            new Keyframe(.3f, 1f, 0, 0, .2f, .1f),
            new Keyframe(1f, 0, 0, 0, 1f, 0)
        );

        public float effectsChangeTime = 0.2f;

        public float normalFov = -1f;
        public float effectsFov = 70f;

        public float lensDistortionAmount = 30;
        public float lensScaleInAmount = 1.5f;
        public float lensScaleOutAmount = 1.5f;

        public bool effectsEnabled = false;

        public bool effectsChanging = false;
        public float animationTime;

        public float dimensionTimeScale = 0.5f;

        public float defaultFixedDeltaTime;

        private void Awake() {
            camera = GetComponent<Camera>();
            mainEffects = GetComponent<PostProcessVolume>();

            lensDistortionSettings = lensEffects.profile.GetSetting<LensDistortion>();

            if (normalFov <= 0) normalFov = camera.fieldOfView;

            defaultFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void Update() {
            if (Input.GetKeyDown(toggleEffectsKey)) {
                ToggleEffects(!effectsEnabled);
            }

            if (effectsChanging) {
                UpdateEffect(Time.unscaledDeltaTime);
            }
        }

        private void ToggleEffects(bool enabled) {
            if (effectsChanging) return;

            effectsEnabled = enabled;
            animationTime = 0;
            effectsChanging = true;

            if (effectsEnabled) {
                camera.cullingMask |= 1 << LayerMask.NameToLayer(banishedLayer);
                Time.timeScale = dimensionTimeScale;
                Time.fixedDeltaTime = defaultFixedDeltaTime * dimensionTimeScale;
            } else {
                camera.cullingMask &= ~(1 << LayerMask.NameToLayer(banishedLayer));
                Time.timeScale = 1;
                Time.fixedDeltaTime = defaultFixedDeltaTime;
            }
        }

        private void UpdateEffect(float delta) {
            animationTime += delta;

            float currentTime = animationTime / effectsChangeTime;
            float effectsValue = effectsEnabled ? effectsChangeCurve.Evaluate(currentTime) : 1 - effectsChangeCurve.Evaluate(currentTime);
            float lensValue = effectsEnabled ? -lensDistortionCurve.Evaluate(currentTime) : lensDistortionCurve.Evaluate(currentTime);

            mainEffects.weight = effectsValue;
            camera.fieldOfView = normalFov + (effectsFov - normalFov) * effectsValue;
            lensDistortionSettings.intensity.Override(lensValue * lensDistortionAmount);
            lensDistortionSettings.scale.Override(1f + lensValue * (effectsEnabled ? lensScaleInAmount : lensScaleOutAmount));

            if (animationTime >= effectsChangeTime) {
                effectsChanging = false;
            }
        }
    }
}