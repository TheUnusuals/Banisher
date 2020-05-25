using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public static class Utils {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }
    }
}