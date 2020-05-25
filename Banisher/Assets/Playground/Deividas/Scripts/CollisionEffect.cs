using UnityEngine;

public class CollisionEffect : MonoBehaviour {
    public GameObject gameObjectPrefab;
    public float minImpulseRequired = 5f;
    public float maxImpulseRequired = float.MaxValue;

    void OnCollisionEnter(Collision obj) {
        float impact = obj.impulse.magnitude;
        if (impact >= minImpulseRequired && impact <= maxImpulseRequired) {
            GameObject instantiatedGameObject = Instantiate(gameObjectPrefab, obj.contacts[0].point, Quaternion.identity);
            instantiatedGameObject.SetActive(true);
        }
    }
}