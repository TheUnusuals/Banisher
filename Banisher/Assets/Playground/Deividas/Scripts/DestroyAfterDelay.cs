using System.Collections;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour {
    [SerializeField]
    public float delayInSeconds = 1f;


    void Start() {
        StartCoroutine(StartTimer());
    }
    IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(delayInSeconds);
        Destroy(gameObject);
    }
}
